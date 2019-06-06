import json
import time
import requests
import detector as dt
import cv2
import _thread
from flask import Flask, jsonify, Response

#########           ###########
###         Init            ###
#########           ###########

application = Flask(__name__)

clients = []
cams = []
with open("./server/clients.json",  'r', encoding='utf-8') as f:
    array = f.read()
    clients = json.loads(array)

with open("./server/cams.json",  'r', encoding='utf-8') as f:
    array = f.read()
    cams = json.loads(array)

class VideoCamera(object):
    """Video stream object"""
    def __init__(self, url):
        self.video = cv2.VideoCapture(url)

    def __del__(self):
        self.video.release()
    
    def get_frame(self):
        success, image = self.video.read()
        ret, jpeg = cv2.imencode('.jpg', image)
        return jpeg.tobytes()

def gen(camera):
    """Video streaming generator function."""
    while True:
        frame = camera.get_frame()
        yield (b'--frame\r\n'
               b'Content-Type: image/jpeg\r\n\r\n' + frame + b'\r\n')

#########           ###########
###         Core            ###
#########           ###########

def main():
    for cam in cams:
        t = 1  # seconds a person can leave the room for
        t0 = time.time()
        time.clock()    
        elapsed = 0
        stream = cam["ip"]
        detector = dt.Detector(stream)
        music_playing = client_ip = clients[cam["client_id"]]["status"]
        client_ip = "http://" + clients[cam["client_id"]]["ip"]

        while True:
            elapsed = time.time() - t0
            if elapsed > t and music_playing:
                r = requests.get(client_ip + "/stop")
                if r.status_code == 200:
                    clients[cam["client_id"]]["status"] = False
            tmp = time.time()
            img = detector.detect()
            print(time.time()-tmp)
            if img is not None and not music_playing:
                r = requests.get(client_ip + "/play")
                if r.status_code == 200:
                    clients[cam["client_id"]]["status"] = True
                    t0 = time.time()
            # TODO noch nicht sicher wie ich das verarbeirtete Bild ausgebe 
            #cv2.imshow("preview", img)
            #cv2.waitKey(1)

#########           ###########
###         Routes          ###
#########           ###########

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

@application.route('/cam/<num>/stream')
def cam_stream(num):
    return Response(gen(VideoCamera(cams[int(num)]["ip"])),
                    mimetype='multipart/x-mixed-replace; boundary=frame')


#########           ###########
###         Start           ###
#########           ###########

if __name__ == '__main__':

    _thread.start_new_thread(main, () )


    application.run(host='127.0.0.1', port=80)
