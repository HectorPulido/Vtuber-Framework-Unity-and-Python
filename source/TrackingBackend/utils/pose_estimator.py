import cv2
import math
import numpy as np
import torch

from emotion_models.with_mobilenet import PoseEstimationWithMobileNet
from pose_models.keypoints import extract_keypoints, group_keypoints
from pose_models.load_state import load_state
from pose_models.pose import Pose, track_poses
from utils.video_reader import VideoReader


class PoseEstimator:
    def __init__(self, checkpoint_path="checkpoints/checkpoint_iter_370000.pth"):

        self.height_size = 256 / 2
        self.cpu = False
        self.stride = 8
        self.upsample_ratio = 4
        self.num_keypoints = Pose.num_kpts
        self.previous_poses = []

        self.net = PoseEstimationWithMobileNet()
        checkpoint = torch.load(checkpoint_path, map_location="cpu")
        load_state(self.net, checkpoint)

        self.net = self.net.eval()

        if not self.cpu:
            self.net = self.net.cuda()

    def infer_fast(
        self, img, pad_value=(0, 0, 0), img_mean=(128, 128, 128), img_scale=1 / 256
    ):
        height, _, _ = img.shape
        scale = self.height_size / height

        scaled_img = cv2.resize(
            img, (0, 0), fx=scale, fy=scale, interpolation=cv2.INTER_CUBIC
        )
        scaled_img = self.normalize(scaled_img, img_mean, img_scale)
        min_dims = [self.height_size, max(scaled_img.shape[1], self.height_size)]
        padded_img, pad = self.pad_width(scaled_img, self.stride, pad_value, min_dims)

        tensor_img = torch.from_numpy(padded_img).permute(2, 0, 1).unsqueeze(0).float()
        if not self.cpu:
            tensor_img = tensor_img.cuda()

        stages_output = self.net(tensor_img)

        stage2_heatmaps = stages_output[-2]
        heatmaps = np.transpose(stage2_heatmaps.squeeze().cpu().data.numpy(), (1, 2, 0))
        heatmaps = cv2.resize(
            heatmaps,
            (0, 0),
            fx=self.upsample_ratio,
            fy=self.upsample_ratio,
            interpolation=cv2.INTER_CUBIC,
        )

        stage2_pafs = stages_output[-1]
        pafs = np.transpose(stage2_pafs.squeeze().cpu().data.numpy(), (1, 2, 0))
        pafs = cv2.resize(
            pafs,
            (0, 0),
            fx=self.upsample_ratio,
            fy=self.upsample_ratio,
            interpolation=cv2.INTER_CUBIC,
        )

        return heatmaps, pafs, scale, pad

    def get_pose_data(self, img):
        heatmaps, pafs, scale, pad = self.infer_fast(img)

        total_keypoints_num = 0
        all_keypoints_by_type = []
        for kpt_idx in range(self.num_keypoints):  # 19th for bg
            total_keypoints_num += extract_keypoints(
                heatmaps[:, :, kpt_idx], all_keypoints_by_type, total_keypoints_num
            )

        pose_entries, all_keypoints = group_keypoints(
            all_keypoints_by_type, pafs, demo=True
        )
        for kpt_id in range(all_keypoints.shape[0]):
            all_keypoints[kpt_id, 0] = (
                all_keypoints[kpt_id, 0] * self.stride / self.upsample_ratio - pad[1]
            ) / scale
            all_keypoints[kpt_id, 1] = (
                all_keypoints[kpt_id, 1] * self.stride / self.upsample_ratio - pad[0]
            ) / scale
        current_poses = []
        for n in range(len(pose_entries)):
            if len(pose_entries[n]) == 0:
                continue
            pose_keypoints = np.ones((self.num_keypoints, 2), dtype=np.int32) * -1
            for kpt_id in range(self.num_keypoints):
                if pose_entries[n][kpt_id] != -1.0:  # keypoint was found
                    pose_keypoints[kpt_id, 0] = int(
                        all_keypoints[int(pose_entries[n][kpt_id]), 0]
                    )
                    pose_keypoints[kpt_id, 1] = int(
                        all_keypoints[int(pose_entries[n][kpt_id]), 1]
                    )
            pose = Pose(pose_keypoints, pose_entries[n][18])
            current_poses.append(pose)
            break

        track_poses(self.previous_poses, current_poses, smooth=True)
        self.previous_poses = current_poses
        for pose in current_poses:
            pose.draw(img)

        if len(current_poses) == 0:
            return []

        return current_poses[0].keypoints.tolist()

    def normalize(self, img, img_mean, img_scale):
        img = np.array(img, dtype=np.float32)
        img = (img - img_mean) * img_scale
        return img

    def pad_width(self, img, stride, pad_value, min_dims):
        h, w, _ = img.shape
        h = min(min_dims[0], h)
        min_dims[0] = math.ceil(min_dims[0] / float(stride)) * stride
        min_dims[1] = max(min_dims[1], w)
        min_dims[1] = math.ceil(min_dims[1] / float(stride)) * stride
        pad = []
        pad.append(int(math.floor((min_dims[0] - h) / 2.0)))
        pad.append(int(math.floor((min_dims[1] - w) / 2.0)))
        pad.append(int(min_dims[0] - h - pad[0]))
        pad.append(int(min_dims[1] - w - pad[1]))
        padded_img = cv2.copyMakeBorder(
            img, pad[0], pad[2], pad[1], pad[3], cv2.BORDER_CONSTANT, value=pad_value
        )
        return padded_img, pad