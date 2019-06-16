from collections import OrderedDict
from flask import Flask
from flask_restplus import fields, Api, Resource
from google_images_download import google_images_download
from textgenrnn import textgenrnn
import json
app = Flask(__name__)
api = Api(app)
textgen = textgenrnn()
paramsModel = {
    "prefix":fields.String(),
    "prefix_keywords": fields.String(None),
    "keywords": fields.String(),
    "suffix_keywords": fields.String(None),
    "limit": fields.Integer(1),
    "print_urls": fields.Boolean(True),
    "color": fields.String(None),
    "type": fields.String(None),
    "no_download": fields.Boolean(True),
    "related_images": fields.String(None),
    "format": fields.String(None),
    "color_type": fields.String(None),
    "usage_rights": fields.String(None),
    "size": fields.String(None),
    "exact_size": fields.String(None),
    "proxy": fields.String(None),
}
caption_model_params = {
    "Topic" : fields.String(),
    "Language" : fields.String(),
}
caption_model = api.model('Caption_Model',caption_model_params)
model = api.model('Model',paramsModel)

googleApiClient = google_images_download.googleimagesdownload()  # class instantiation

@api.route('/searchImages')
class SearchImage(Resource):
    @api.expect(model)
    def post(self):
        print(api.payload)
        path = googleApiClient.download(api.payload)
        response  = []
        for key,value in path[0].items():
            response.append({"Topic" : key, "MediaUrl" : value})

        result = {
            "Medias" : response,
            "errors" : path[1]
        }
        return result
@api.route('/generateCaption')
class GenerateCaption(Resource):
    @api.expect(caption_model)
    def post(self):
        print(api.payload)
        result = textgen.generate()
        return result

if __name__ == '__main__':
    app.run(debug=True)
