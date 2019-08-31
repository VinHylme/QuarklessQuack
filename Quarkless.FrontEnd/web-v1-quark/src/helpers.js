function readFile(file) {
  return new Promise((resolve, reject) => {
      let fr = new FileReader();
      fr.onload = x => resolve(fr.result);
      fr.readAsDataURL(file)
  })
}

module.exports.readFile = readFile;