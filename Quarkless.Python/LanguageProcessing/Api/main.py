from langdetect import detect, DetectorFactory
DetectorFactory.seed = 0
import spacy, babel, langid, chardet, json
from spacy_langdetect import LanguageDetector
from flask import Flask
from flask_restplus import fields, Api, Resource

def LangDetect(texts, method):
  words = []
  if texts is None:
    return words
  else:
    try:
      if method == 0:
        for word in texts:
          words.append(langid.classify(word)[0])
      elif method == 1:
        for word in texts:
          words.append(detect(word))
      elif method == 2:
        for word in texts:
          detected = chardet.detect(word.encode('cp1251'))
          print(detected)
          lang = babel.Locale.parse(detected["Language"])
          words.append(lang)
      elif method == 3:
        for word in texts:
          nlp = spacy.load("en")
          nlp.add_pipe(LanguageDetector(), name="language_detector", last=True)
          words.append(nlp(word)._.language['language'])
      else:
        return words
    except Exception as e:
      print(e)
    return words

app = Flask(__name__)
api = Api(app)

language_model = {
  "texts":fields.List(fields.String)
}

model = api.model('Model', language_model)

@api.route('/detectLanguage')
class LanguageDetections(Resource):
  @api.expect(model)
  def post(self):
    texts = api.payload["texts"]
    detections = LangDetect(texts, 0)
    if detections is None:
      print('trying method 2')
      detections = LangDetect(texts, 1)
    return {
      "detections" : detections
    }

if __name__ == '__main__':
    app.run(debug=True, port=5006)