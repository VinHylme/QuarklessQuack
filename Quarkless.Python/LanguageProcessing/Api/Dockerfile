FROM python:slim-buster
LABEL image for language detection api
WORKDIR /app
COPY LanguageProcessing/Api .
RUN pip3 install flask flask-restplus langdetect spacy babel langid chardet spacy_langdetect
CMD [ "python3", "main.py" ]
