import cv2


class VideoReader(object):
    def __init__(self, file_name, scale=75):
        self.scale = scale
        self.file_name = file_name
        try:  # OpenCV needs int to read from webcam
            self.file_name = int(file_name)
        except ValueError:
            pass

    def rescale_frame(self, frame):
        width = int(frame.shape[1] * self.scale/ 100)
        height = int(frame.shape[0] * self.scale/ 100)
        dim = (width, height)
        return cv2.resize(frame, dim, interpolation =cv2.INTER_AREA)

    def __iter__(self):
        self.cap = cv2.VideoCapture(self.file_name)
        if not self.cap.isOpened():
            raise IOError("Video {} cannot be opened".format(self.file_name))
        return self

    def __next__(self):
        was_read, img = self.cap.read()
        if not was_read:
            raise StopIteration
        return self.rescale_frame(img)
