from textgenrnn import textgenrnn
textgen = textgenrnn('textgenrnn_weights.hdf5',vocab_path='textgenrnn_vocab.json',config_path='textgenrnn_config.json')

#textgen.reset()
#textgen.train_from_file("_commentsp.csv", new_model=True, num_epochs = 1, gen_epochs = 1, context=True, Word_level=True)
answer = textgen.generate(1,prefix="art",return_as_list=True,temperature=0.3)
print(answer)