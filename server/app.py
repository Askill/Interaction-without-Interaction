import io
import json
import time
import requests
import detector as dt
import cv2
import _thread
from flask import Flask, jsonify, Response, send_file

#########           ###########
###         Init            ###
#########           ###########

application = Flask(__name__)

clients = []
cams = []
lastImages = list(range(0,10))
with open("./clients.json", 'r', encoding='utf-8') as f:
    clients = json.loads(f.read())

with open("./cams.json", 'r', encoding='utf-8') as f:
    cams = json.loads(f.read())

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

def gen_processed(num):
    """Video streaming generator function."""
    while True:
        frame = cv2.imencode('.jpg', lastImages[int(num)])[1].tobytes()
        yield (b'--frame\r\n'
               b'Content-Type: image/jpeg\r\n\r\n' + frame + b'\r\n')

#########           ###########
###         Core            ###
#########           ###########

def main():
    detector = dt.Detector()
    max_absence_time = 5  # seconds a person can leave the room for

    for cam in cams:
        cam["last_detection"] = 0

    while True:
        for cam in cams:
            stream = cam["ip"]
            
            clientStatus = clients[cam["client_id"]]["status"]
            clientIp = clients[cam["client_id"]]["ip"]

            elapsed = time.time() - cam["last_detection"]
            if elapsed > max_absence_time and clientStatus:
                try:    
                    r = requests.get(clientIp + "/stop")
                    #if r.status_code == 200:
                    clients[cam["client_id"]]["status"] = False
                except:
                    print("request error")

            tmp = time.time()
            try:
                img, detected = detector.detect(stream) 
            except:
                continue

            print(cam["id"], detected, time.time()-tmp)
            lastImages[cam["id"]] = img

            if detected:
                cam["status"] = True
                if not clientStatus:
                    try:      
                        r = requests.get(clientIp + "/play")
                        if r.status_code == 200:
                            clients[cam["client_id"]]["status"] = True
                            cam["last_detection"] = time.time()
                    except:
                        print("request error")
            else:
                cam["status"] = False


#########           ###########
###         Routes          ###
#########           ###########

@application.route('/client/')
def client_list():
    return jsonify(clients)

@application.route('/client/<num>/info')
def client_info(num):
    return jsonify(clients[int(num)])

@application.route('/cam/')
def cam_list():
    return jsonify(cams)

@application.route('/cam/<num>/info')
def cam_info(num):
    return jsonify(cams[int(num)])

@application.route('/cam/<num>/stream')
def cam_stream(num):
    return Response(gen(VideoCamera(cams[int(num)]["ip"])), mimetype='multipart/x-mixed-replace; boundary=frame')

@application.route('/cam/<num>/processed')
def cam_stream_processed(num):
    #frame = cv2.imencode('.jpg', lastImages[int(num)])[1]
    #return send_file(io.BytesIO(frame), mimetype='image/jpeg')
    return Response(gen_processed(num), mimetype='multipart/x-mixed-replace; boundary=frame')


#########           ###########
###         Start           ###
#########           ###########

if __name__ == '__main__':

    _thread.start_new_thread(main, () )

    application.run(host='0.0.0.0', port=5000, threaded=True)
