import time
import requests
import detector as dt
import cv2

if __name__ == "__main__":
    t = 30  # seconds a person can leave the room for
    t0 = time.time()
    time.clock()    
    elapsed = 0
    stream = "http://217.128.254.187:8083/mjpg/video.mjpg"

    detector = dt.Detector(stream)
    music_playing = False
        
    while True:
        elapsed = time.time() - t0
        if elapsed > t and music_playing:
            r = requests.get("http://192.168.178.53/stop")
            if r.status_code == 200:
                music_playing = False

        result, img = detector.detect()
        if result and not music_playing:
            r = requests.get("http://192.168.178.53/play")
            if r.status_code == 200:
                music_playing = True
                t0 = time.time()
        cv2.imshow("preview", img) # cv2.destroyWindow("preview")
        time.sleep(1)
        
