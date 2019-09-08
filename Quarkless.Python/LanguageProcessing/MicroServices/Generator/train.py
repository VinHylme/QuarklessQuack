import genHelper, mongoHelper, collections, os, re, nltkHelper
from itertools import groupby

def OnCaptionsWithTopic():
  for topic in mongoHelper.GetCaptionTopics():
    print('----- Started Caption Training ----- Current: {}'.format(topic))
    captions = list(filter(None,mongoHelper.GetCaptionsByTopic(topic)))
    print('Collected {} Captions'.format(len(captions)))
    removeTrash = list(set([re.sub(r'(_+|=+|\.+|\n+)',"", x["Caption"] if nltkHelper.IsLatinCharacters(x["Caption"]) else "" , re.M) for x in captions]))
    print(removeTrash)
    try:
      gen = genHelper.initialise_textgen(topic, "captions")
      genHelper.train_textgen(gen, removeTrash, topic, trainType = "captions")
    except OSError as o:
      print(o)
      os.makedirs('Data/captions/{}'.format(topic))
      gen = genHelper.create_textgen(topic, trainType = "captions")
      genHelper.train_textgen(gen, removeTrash, topic, trainType = "captions", newModel = True)
    except Exception as e:
      print(e)

def OnCommentsWithTopic():
  for topic in mongoHelper.GetCommentTopics():
    print('----- Started Caption Training ----- Current: {}'.format(topic))
    comments = list(filter(None,mongoHelper.GetCommentsByTopic(topic)))
    #print('Collected {} Comments'.format(len(comments)))
    removeTrash = list(set([re.sub(r'(_+|=+|\.+|\n+)',"",x["Comments"] if nltkHelper.IsLatinCharacters(x["Comments"]) else "", re.M) for x in comments]))
    try:
      gen = genHelper.initialise_textgen(topic, "comments")
      genHelper.train_textgen(gen, removeTrash, topic, trainType = "comments")
    except OSError as o:
      print(o)
      os.makedirs('Data/comments/{}'.format(topic))
      gen = genHelper.create_textgen(topic, trainType = "comments")
      genHelper.train_textgen(gen, removeTrash, topic, trainType = "comments", newModel = True)
    except Exception as e:
      print(e)

def OnCaptions(iskip, ilimit):
  print('----- Started Caption Training ----- Current: {}'.format(iskip))
  captionsCursor = mongoHelper.GetCaptions(iskip,ilimit)
  dictg = {k: [c["Caption"] for c in g] for k, g in groupby((captionsCursor), lambda x: x['Topic'])}
  for key in dictg.keys():
    filteredList = list(filter(None,dictg[key]))
    removeTrash = list(set([re.sub(r'(_+|=+|\.+|\n+)',"",x, re.M) for x in filteredList]))
    try:
      #check if exists
      gen = genHelper.initialise_textgen(key, "captions")
      genHelper.train_textgen(gen, removeTrash, key, trainType = "captions")
    except OSError as e:
      print(e)
      os.makedirs('Data/captions/{}'.format(key))
      gen = genHelper.create_textgen(key, trainType = "captions")
      genHelper.train_textgen(gen, removeTrash, key, trainType = "captions", newModel=True)

def OnComments(iskip, ilimit):
  print('----- Started Comment Training ----- Current: {}'.format(iskip))
  commentCursor = mongoHelper.GetComments(iskip,ilimit)
  dictg = {k: [c["Comment"] for c in g] for k, g in groupby((commentCursor), lambda x: x['Topic'])}
  for key in dictg.keys():
    #print(key)
    filteredList = list(filter(None,dictg[key]))
    #print(filteredList)
    removeTrash = list(set([re.sub(r'(_+|=+|\.+|\n+)',"",x, re.M) for x in filteredList]))
    try:
      #check if exists
      gen = genHelper.initialise_textgen(key, "comments")
      genHelper.train_textgen(gen, removeTrash, key, trainType="comments")
    except OSError as e:
      print(e)
      os.makedirs('Data/comments/{}'.format(key))
      gen = genHelper.create_textgen(key, trainType = "comments")
      genHelper.train_textgen(gen, removeTrash, key, trainType = "comments", newModel=True)
    except Exception as e2:
      print(e2)

