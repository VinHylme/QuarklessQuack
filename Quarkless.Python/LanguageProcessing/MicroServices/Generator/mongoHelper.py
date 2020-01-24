from pymongo import MongoClient
from pprint import pprint
import yaml

#connect
client = MongoClient(yaml.load(open('application.yml'), Loader=yaml.FullLoader)['application']['connectionString'])
#get the database
db = client["ContentAgent"]

def GetCaptions(start, limit, language = 'en'):
  try:
    collection = db["CMedias"]
    return collection.find(
    {
      "Caption":
      {
        '$regex': r'^(?!.*@\S*|#\S*|\d{7,}).*', '$options':'si'
      },
      "Language":language
    },
    { 
      "Caption": 1, "Topic": 1 , "_id": 0
    }, skip = start, limit = limit).sort('Topic')
  except Exception as e:
    print(e)

def GetComments(start, limit, language = 'en'):
  try:
    collection = db["CComments"]
    return collection.find(
    {
      "Comment":
      {
        '$regex': r'^(?!.*@\S*|#\S*|\d{7,}).*', '$options':'si'
      },
      "Language":language
    },
    { 
      "Comment": 1, "Topic": 1 , "_id": 0
    }, skip = start, limit = limit).sort('Topic')
  except Exception as e:
    print(e)

def GetCaptionsByTopic(topic, language = 'en'):
  try:
    collection = db["CMedias"]
    return collection.find(
    {
      "Topic" : topic,
      "Caption":
      {
        '$regex': r'^(?!.*@\S*|#\S*|\d{7,}).*', '$options':'si'
      },
      "Language":language
    },
    { 
      "Caption": 1, "Topic": 1 , "_id": 0
    })
  except Exception as e:
    print(e)

def GetCommentsByTopic(topic, language = 'en'):
  try:
    collection = db["CComments"]
    return collection.find(
    {
      "Topic" : topic,
      "Comment":
      {
        '$regex': r'^(?!.*@\S*|#\S*|\d{7,}).*', '$options':'si'
      },
      "Language":language
    },
    { 
      "Comment": 1, "Topic": 1 , "_id": 0
    })
  except Exception as e:
    print(e)

#was going to use this to start multiple topics at the same time, but not enough power to handle that
#get all unique topics, then call GetCaptionByTopic and start training on the text
def GetCaptionTopics():
  try:
    return db["CMedias"].find().distinct('Topic')
  except Exception as e:
    print(e)

#was going to use this to start multiple topics at the same time, but not enough power to handle that
#get all unique topics, then call GetCommentsByTopic and start training on the text
def GetCommentTopics():
  try:
    return db["CComments"].find().distinct('Topic')
  except Exception as e:
    print(e)