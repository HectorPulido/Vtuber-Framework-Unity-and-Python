import cv2
import numpy as np
import torch
from PIL import Image
from torchvision import transforms

from models.emotion_model import EmotionModel
from visualize.grad_cam import BackPropagation, GradCAM, GuidedBackPropagation
from utils.video_reader import VideoReader


class EmotionDetector:
    faceCascade = cv2.CascadeClassifier(
        "./visualize/haarcascade_frontalface_default.xml"
    )
    shape = (48, 48)
    classes = ["Angry", "Disgust", "Fear", "Happy", "Sad", "Surprised", "Neutral"]

    def __init__(self, model_name="private_model_233_66.t7", gpu=True):
        device = torch.device("cuda" if torch.cuda.is_available() and gpu else "cpu")

        self.net = EmotionModel(num_classes=len(self.classes))
        self.checkpoint = torch.load(model_name, map_location=device)
        self.net.load_state_dict(self.checkpoint["net"])
        self.net.eval()

    def preprocess(self, image):
        transform_test = transforms.Compose([transforms.ToTensor()])
        faces = self.faceCascade.detectMultiScale(
            image,
            scaleFactor=1.1,
            minNeighbors=5,
            minSize=(1, 1),
            flags=cv2.CASCADE_SCALE_IMAGE,
        )

        if len(faces) == 0:
            face = cv2.resize(image, self.shape)
        else:
            (x, y, w, h) = faces[0]
            face = image[y : y + h, x : x + w]
            face = cv2.resize(face, self.shape)

        img = Image.fromarray(face).convert("L")
        inputs = transform_test(img)
        return inputs

    def guided_backprop(self, image):
        target = self.preprocess(image)
        img = torch.stack([target])
        bp = BackPropagation(model=self.net)
        _, ids = bp.forward(img)
        actual_emotion = ids[:, 0]

        return self.classes[actual_emotion.data]
