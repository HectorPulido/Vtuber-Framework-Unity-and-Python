
import torch
import cv2

class DepthEstimator():

    def __init__(self):
        self.midas = torch.hub.load("intel-isl/MiDaS", "MiDaS")
        self.device = torch.device("cuda") if torch.cuda.is_available() else torch.device("cpu")

        self.midas.to(self.device)
        self.midas.eval()

        midas_transforms = torch.hub.load("intel-isl/MiDaS", "transforms")
        self.transform = midas_transforms.default_transform

    def estimateFromImage(self, img):
        with torch.no_grad():
            img = cv2.cvtColor(img, cv2.COLOR_BGR2RGB)
            input_batch = self.transform(img).to(self.device)
            prediction = self.midas(input_batch)
            prediction = torch.nn.functional.interpolate(
                prediction.unsqueeze(1),
                size=img.shape[:2],
                mode="bicubic",
                align_corners=False,
            ).squeeze()
                
            pred = prediction.cpu().numpy()
            pred /= pred.max()
            return pred
    
    def estimatePose(self, img, pose):
        estimation = []
        for point in pose:
            if point[1] >= img.shape[0]:
                estimation.append(0)
                continue
            if point[0] >= img.shape[1]:
                estimation.append(0)
                continue

            estimation.append(img[point[1], point[0]].tolist())
        return estimation
