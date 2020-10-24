import socket
import json
from utils.pose_estimator import PoseEstimator
from utils.video_reader import VideoReader

estimator = PoseEstimator()
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

while True:
    # Wait for a connection
    print("waiting for a connection")
    connection, client_address = sock.accept()
    try:
        print("connection from", client_address)

        # Receive the data in small chunks and retransmit it
        while True:
            pose_data = estimator.run_demo()
            data_to_send = str.encode(json.dumps(pose_data))

            try:
                connection.sendall(data_to_send)
            except:
                print("Connection closed")
                connection.close()
                break

    finally:
        print("Connection closed")
        connection.close()
