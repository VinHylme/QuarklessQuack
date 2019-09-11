from nltk.corpus import words as nltk_words
from nltk import corpus, wordpunct_tokenize

def IsEnglishWord(word):
    # creation of this dictionary would be done outside of 
    #     the function because you only need to do it once.
    dictionary = dict.fromkeys(nltk_words.words(), None)
    try:
        x = dictionary[word]
        return True
    except KeyError:
        return False

def IsLatinCharacters(s):
    try:
        s.encode(encoding='utf-8').decode('ascii')
    except UnicodeDecodeError:
        return False
    else:
        return True
        
def RemoveNonEnglishWords(sent):
  words = set(corpus.words.words())
  return " ".join(w for w in wordpunct_tokenize(sent) \
         if w.lower() in words or not w.isalpha())