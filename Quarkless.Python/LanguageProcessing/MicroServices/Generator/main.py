from threading import Thread
import argparse, train, genHelper, nltkHelper

def str2bool(v):
    if isinstance(v, bool):
       return v
    if v.lower() in ('yes', 'true', 't', 'y', '1'):
        return True
    elif v.lower() in ('no', 'false', 'f', 'n', '0'):
        return False
    else:
        raise argparse.ArgumentTypeError('Boolean value expected.')

parser = argparse.ArgumentParser(description="sample parser")
parser.add_argument("--sample", type=str2bool, nargs='?',
  const=True, default=False,
  help="Sampling...")

parser.add_argument("--type", type=str, nargs='?',
  const=True, default="captions",
  help="which collection do you want to sample from?")

parser.add_argument("--topic", type=str, nargs='?',
  const=True, default="actor",
  help="what topic are you looking to sample?")

parser.add_argument("--prefix", type=str, nargs='?',
  const=True, default=None,
  help="Add a prefix ")


args = parser.parse_args()

if args.sample:
  genHelper.sample(model_name = args.topic, trainType = args.type, prefix=args.prefix, n=1)
  #lm = mh.GetCaptionsByTopic(0,100,'art')
  #print(list(lm))
else:
  #print(nltkHelper.IsSentenceEnglish('le france andiamo wow my laldkada'))
  train.OnCaptionsWithTopic()
  #print(train.RemoveNonEnglishWords('le france andiamo wow my laldkada'))
  # skip = 0
  # limit = 10000
  # for times in range(1):
  #   train.OnCaptions(skip,limit)
    # p1 = Thread(target= train.OnComments, args=(skip, limit))
    # p2 = Thread(target= train.OnCaptions, args=(skip, limit))
    # p1.start()
    # p2.start()
    # p1.join()
    # p2.join()
    #skip+=limit
