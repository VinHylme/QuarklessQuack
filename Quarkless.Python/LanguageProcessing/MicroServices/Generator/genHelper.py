from textgenrnn import textgenrnn

def initialise_textgen(model_name, trainType):
  return textgenrnn(weights_path='Data/{}/{}/{}_weights.hdf5'.format(trainType, model_name, model_name),
        vocab_path='Data/{}/{}/{}_vocab.json'.format(trainType, model_name, model_name),
        config_path='Data/{}/{}/{}_config.json'.format(trainType, model_name, model_name),
        name='Data/{}/{}/{}'.format(trainType, model_name, model_name))

def loadModel(model_name, trainType):
  return textgenrnn('Data/{}/{}/{}_weights.hdf5'.format(trainType, model_name, model_name))

def create_textgen(model_name, trainType):
  return textgenrnn(name='Data/{}/{}/{}'.format(trainType, model_name, model_name))

def train_textgen(textgen : textgenrnn, texts, modelName, trainType, newModel = False):
   textgen.train_on_texts(texts, gen_epochs=3, new_model=newModel, num_epochs=30)
   textgen.save('Data/{}/{}/{}_weights.hdf5'.format(trainType, modelName, modelName))

def sample(model_name, trainType, n = 3, top_n = 3, temperatures=[0.2, 0.5, 1.0], prefix = None, returnAsList = False, interactive = False):
  try:
    modelGen = initialise_textgen(model_name, trainType)
    return modelGen.generate_samples(n = n, top_n = top_n, temperatures = temperatures, return_as_list = returnAsList, prefix = prefix, interactive = interactive)
  except Exception as e:
    print(e)