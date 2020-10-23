
import socket
import json
import cv2
import numpy as np
import torch

from models.with_mobilenet import PoseEstimationWithMobileNet
from modules.keypoints import extract_keypoints, group_keypoints
from modules.load_state import load_state
from modules.pose import Pose, track_poses
from val import normalize, pad_width

from datetime import datetime


class VideoReader(object):
    def __init__(self, file_name):
        self.file_name = file_name
        try:  # OpenCV needs int to read from webcam
            self.file_name = int(file_name)
        except ValueError:
            pass

    def __iter__(self):
        self.cap = cv2.VideoCapture(self.file_name)
        if not self.cap.isOpened():
            raise IOError('Video {} cannot be opened'.format(self.file_name))
        return self

    def __next__(self):
        was_read, img = self.cap.read()
        if not was_read:
            raise StopIteration
        return img


class PoseEstimator():

    def __init__(self):
        self.checkpoint_path = 'checkpoint_iter_370000.pth'
        self.video = '0'

        self.height_size = 256
        self.cpu = False
        self.track = 1
        self.smooth = True
        self.stride = 8
        self.upsample_ratio = 4
        self.num_keypoints = Pose.num_kpts
        self.previous_poses = []

        self.net = PoseEstimationWithMobileNet()
        checkpoint = torch.load(self.checkpoint_path, map_location='cpu')
        load_state(self.net, checkpoint)

        self.net = self.net.eval()
        if not self.cpu:
            self.net = self.net.cuda()
        self.delay = 33

        self.frame_provider = iter(VideoReader(self.video))


    def infer_fast(self, img, pad_value=(0, 0, 0), img_mean=(128, 128, 128), img_scale=1/256):
        height, width, _ = img.shape
        scale = self.height_size / height

        scaled_img = cv2.resize(img, (0, 0), fx=scale, fy=scale, interpolation=cv2.INTER_CUBIC)
        scaled_img = normalize(scaled_img, img_mean, img_scale)
        min_dims = [self.height_size, max(scaled_img.shape[1], self.height_size)]
        padded_img, pad = pad_width(scaled_img, self.stride, pad_value, min_dims)

        tensor_img = torch.from_numpy(padded_img).permute(2, 0, 1).unsqueeze(0).float()
        if not self.cpu:
            tensor_img = tensor_img.cuda()

        stages_output = self.net(tensor_img)

        stage2_heatmaps = stages_output[-2]
        heatmaps = np.transpose(stage2_heatmaps.squeeze().cpu().data.numpy(), (1, 2, 0))
        heatmaps = cv2.resize(heatmaps, (0, 0), fx=self.upsample_ratio, fy=self.upsample_ratio, interpolation=cv2.INTER_CUBIC)

        stage2_pafs = stages_output[-1]
        pafs = np.transpose(stage2_pafs.squeeze().cpu().data.numpy(), (1, 2, 0))
        pafs = cv2.resize(pafs, (0, 0), fx=self.upsample_ratio, fy=self.upsample_ratio, interpolation=cv2.INTER_CUBIC)

        return heatmaps, pafs, scale, pad


    def run_demo(self):
        img = next(self.frame_provider)
        orig_img = img.copy()
        heatmaps, pafs, scale, pad = self.infer_fast(img)

        total_keypoints_num = 0
        all_keypoints_by_type = []
        for kpt_idx in range(self.num_keypoints):  # 19th for bg
            total_keypoints_num += extract_keypoints(heatmaps[:, :, kpt_idx], all_keypoints_by_type, total_keypoints_num)

        pose_entries, all_keypoints = group_keypoints(all_keypoints_by_type, pafs, demo=True)
        for kpt_id in range(all_keypoints.shape[0]):
            all_keypoints[kpt_id, 0] = (all_keypoints[kpt_id, 0] * self.stride / self.upsample_ratio - pad[1]) / scale
            all_keypoints[kpt_id, 1] = (all_keypoints[kpt_id, 1] * self.stride / self.upsample_ratio - pad[0]) / scale
        current_poses = []
        for n in range(len(pose_entries)):
            if len(pose_entries[n]) == 0:
                continue
            pose_keypoints = np.ones((self.num_keypoints, 2), dtype=np.int32) * -1
            for kpt_id in range(self.num_keypoints):
                if pose_entries[n][kpt_id] != -1.0:  # keypoint was found
                    pose_keypoints[kpt_id, 0] = int(all_keypoints[int(pose_entries[n][kpt_id]), 0])
                    pose_keypoints[kpt_id, 1] = int(all_keypoints[int(pose_entries[n][kpt_id]), 1])
            pose = Pose(pose_keypoints, pose_entries[n][18])
            current_poses.append(pose)

        return current_poses[-1].keypoints.tolist()

estimator = PoseEstimator()
HOST = '127.0.0.1'  # Standard loopback interface address (localhost)
PORT = 65432        # Port to listen on (non-privileged ports are > 1023)

with socket.socket(socket.AF_INET, socket.SOCK_STREAM) as s:
    s.bind((HOST, PORT))
    s.listen()
    conn, addr = s.accept()
    with conn:
        print('Connected by', addr)
        while True:
            # data = conn.recv(1024)
            # if not data:
            #     break

            now = datetime.now().time() # time object
            print("now =", now)

            pose_data = estimator.run_demo()

            now = datetime.now().time() # time object
            print("now =", now)


            data_to_send = str.encode(json.dumps(pose_data))

            conn.sendall(data_to_send)

