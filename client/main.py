from flask import Flask, request
import os
import simpleaudio as sa

#----------------------------------------------------------------------------#
# App Config.
#----------------------------------------------------------------------------#

app = Flask(__name__)

#----------------------------------------------------------------------------#
# Controllers.
#----------------------------------------------------------------------------#

p = None
w = None
playing = False

@app.route('/play')
def index():
    global p, playing
    playing = True
    print("yes")
    p = w.play()
    if playing:
        return 406
    if not playing:
        return 200

@app.route('/stop')
def test():
    global p, playing
    playing = False
    p.stop()
    if playing:
        return 200
    if not playing:
        return 406

if __name__ == '__main__':
    port = int(os.environ.get('PORT', 80))
    global p
    global w
    w = sa.WaveObject.from_wave_file("./rave.wav")
   
    app.run(host='0.0.0.0', port=port)


    



