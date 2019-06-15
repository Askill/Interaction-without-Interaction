
### usage

git clone https://github.com/Askill/UI

cd UI

docker build -t ui-server .

docker run -ti --rm -p 80:5000 ui-server:latest

