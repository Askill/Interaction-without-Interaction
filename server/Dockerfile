FROM python:3.7

RUN apt-get update
RUN apt-get install -y gcc libevent-dev python-dev

COPY ./requirements.txt /
RUN pip install -r /requirements.txt

COPY ./ /app

WORKDIR /app
EXPOSE 5000

CMD ["gunicorn", "-b", "0.0.0.0:5000", "app"]