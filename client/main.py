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
        return str(406)
    else:
        playing = True
        p = w.play()
        return str(200)

@app.route('/stop')
def test():
    global playing, p
    if playing:
        playing = False
        p.stop()
        return str(200)
    else:
        return str(406)


port = int(os.environ.get('PORT', 81))
app.run(host='0.0.0.0', port=port)


    



