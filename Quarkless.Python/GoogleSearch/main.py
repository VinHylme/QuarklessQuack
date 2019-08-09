from collections import OrderedDict
from flask import Flask
from flask_restplus import fields, Api, Resource
from google_images_download import google_images_download
import json
app = Flask(__name__)
api = Api(app)
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
    "related_images":fields.Boolean(False),
    "similar_images": fields.String(None),
    "format": fields.String(None),
    "color_type": fields.String(None),
    "usage_rights": fields.String(None),
    "size": fields.String(None),
    "exact_size": fields.String(None),
    "proxy": fields.String(None),
    "offset":fields.Integer(0)
}

model = api.model('Model',paramsModel)
googleApiClient = google_images_download.googleimagesdownload()  # class instantiation

@api.route('/relatedKeywords')
class SearchRelated(Resource):
    @api.expect(model)
    def post(self):
        print(api.payload)
        path = googleApiClient.download(api.payload)        
        return path;

@api.route('/searchImages')
class SearchImage(Resource):
    @api.expect(model)
    def post(self):
        print(api.payload)
        path = googleApiClient.download(api.payload)
        mediaResponse  = []

        for k,v in path[0].items():
            for n in v:
                mediaResponse.append({"Topic":k, "MediaUrl": n})

        result = {
            "MediasObject" : mediaResponse,
            "errors" : path[1]
        }
        return result

if __name__ == '__main__':
    app.run(debug=True)
