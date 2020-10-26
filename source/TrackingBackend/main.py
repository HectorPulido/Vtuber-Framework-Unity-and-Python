import socket
import json
from utils.pose_estimator import PoseEstimator
from utils.video_reader import VideoReader
from utils.emotion_dection import EmotionDetector

video = "0"
HOST = "127.0.0.1"  # Standard loopback interface address (localhost)
PORT = 65432  # Port to listen on (non-privileged ports are > 1023)

# Create a TCP/IP socket
sock = socket.socket(socket.AF_INET, socket.SOCK_STREAM)

# Bind the socket to the port
server_address = (HOST, PORT)
print("starting up on {} port {}".format(*server_address))
sock.bind(server_address)

# Listen for incoming connections
sock.listen(1)


frame_provider = iter(VideoReader(video))
emotion_detector = EmotionDetector()
pose_estimator = PoseEstimator()

while True:
    # Wait for a connection
    print("waiting for a connection")
    connection, client_address = sock.accept()
    try:
        print("connection from", client_address)

        while True:
            img = next(frame_provider)

            emotion_data = emotion_detector.guided_backprop(img)
            pose_data = pose_estimator.get_pose_data(img)

            data_to_send = {"pose_data": pose_data, "emotion_data": emotion_data}

            data_to_send = str.encode(json.dumps(data_to_send))

            try:
                connection.sendall(data_to_send)
            except:
                print("Connection closed")
                connection.close()
                break

    finally:
        print("Connection closed")
        connection.close()