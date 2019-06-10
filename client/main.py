from flask import Flask, request
import os
import simpleaudio as sa

app = Flask(__name__)

p = None
w = sa.WaveObject.from_wave_file("./rave.wav")
playing = False

@app.route('/play')
def index():
    global playing, p, w
    if playing:
        return 406
    else:
        playing = True
        p = w.play()
        return 200

@app.route('/stop')
def test():
    global playing, p
    if playing:
        return 200
    else:
        playing = False
        p.stop()
        return 406


port = int(os.environ.get('PORT', 81))
app.run(host='0.0.0.0', port=port)


    


