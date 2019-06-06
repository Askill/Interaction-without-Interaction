import os
import random
import json
from importlib import import_module
import cv2
from flask import Flask, jsonify, Response

application = Flask(__name__)
clients = []
cams = []
with open("./clients.json",  'r', encoding='utf-8') as f:
    array = f.read()
    clients = json.loads(array)

with open("./cams.json",  'r', encoding='utf-8') as f:
    array = f.read()
    cams = json.loads(array)


class VideoCamera(object):
    def __init__(self, url):
        self.video = cv2.VideoCapture(url)

    def __del__(self):
        self.video.release()
    
    def get_frame(self):
        success, image = self.video.read()
        # We are using Motion JPEG, but OpenCV defaults to capture raw images,
        # so we must encode it into JPEG in order to correctly display the
        # video stream.
        ret, jpeg = cv2.imencode('.jpg', image)
        return jpeg.tobytes()

@application.route('/client/')
def client_list():
    json = clients
    return jsonify(json)

@application.route('/client/<num>/info')
def client_info(num):
    json = clients[int(num)]
    return jsonify(json)

@application.route('/cam/')
def cam_list():
    json = cams
    return jsonify(json)

@application.route('/cam/<num>/info')
def cam_info(num):
    json = cams[int(num)]
    return jsonify(json)

def gen(camera):
    """Video streaming generator function."""
    while True:
        frame = camera.get_frame()
        yield (b'--frame\r\n'
               b'Content-Type: image/jpeg\r\n\r\n' + frame + b'\r\n')

@application.route('/cam/<num>/stream')
def cam_stream(num):

    return Response(gen(VideoCamera(cams[int(num)]["ip"])),
                    mimetype='multipart/x-mixed-replace; boundary=frame')

if __name__ == '__main__':
    print(clients[0])
    application.run(host='127.0.0.1', port=80)
