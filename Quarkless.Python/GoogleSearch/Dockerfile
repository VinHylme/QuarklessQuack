FROM python:slim-buster
LABEL image for google_images_download flask
WORKDIR /app
COPY GoogleSearch/ .
RUN pip3 install Werkzeug==0.16.0 flask flask-restplus google_images_download
CMD [ "python3", "main.py" ]
