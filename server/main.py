import time
import requests
import detector as dt
import cv2

if __name__ == "__main__":
    t = 1  # seconds a person can leave the room for
    t0 = time.time()
    time.clock()    
    elapsed = 0
    #stream = "https://192.168.178.56:8080/video"
    stream = "http://217.128.254.187:8083/mjpg/video.mjpg"
    detector = dt.Detector(stream)
    music_playing = False

    #cv2.startWindowThread()
    #cv2.namedWindow("preview")

    while True:
        elapsed = time.time() - t0
        if elapsed > t and music_playing:
            r = requests.get("http://192.168.178.53/stop")
            if r.status_code == 200:
                music_playing = False
        tmp = time.time()
        img = detector.detect()
        print(time.time()-tmp)
        if img is not None and not music_playing:
            r = requests.get("http://192.168.178.53/play")
            if r.status_code == 200:
                music_playing = True
                t0 = time.time()
        

        cv2.imshow("preview", img)
        cv2.waitKey(1)
        
