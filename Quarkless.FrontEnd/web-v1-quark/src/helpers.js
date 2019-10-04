function ReadFile(file) {
  return new Promise((resolve, reject) => {
      let fr = new FileReader();
      fr.onload = x => resolve(fr.result);
      fr.readAsDataURL(file) // or readAsText(file) to get raw content
  })
}

function GetMediaType(file){
  return new Promise(async (resolve, reject)=>{
    var media_type = file.split('/')[0].split(':')[1];
    if(media_type === "video"){
      await ReadVideoLength(file).then(res=>{
        if(res === false)
        {
          resolve({type:2, err:false});
        }
        else {
          resolve({type:2, err: true});
        }
      });
    }
    else if(media_type === "image"){
      resolve({type:1, err:false});
    }
    else {
      resolve({type:1, err:true});
    }
  })
}

function ReadVideoLength(resp){
  return new Promise((resolve, reject)=>{
    let video = document.createElement('video');
    video.preload = 'metadata';
    video.onloadedmetadata = function(){
      window.URL.revokeObjectURL(video.src);
      var duration = video.duration;
      if(duration > 65)
        resolve(true);
      else
        resolve(false);
    };
    var byteCharacters = atob(resp.slice(resp.indexOf(',') + 1)); 
    var byteNumbers = new Array(byteCharacters.length);
    for (var i = 0; i < byteCharacters.length; i++) {            
      byteNumbers[i] = byteCharacters.charCodeAt(i);            
    }
    var byteArray = new Uint8Array(byteNumbers);
    var blob = new Blob([byteArray], {type: 'video/ogg'});
    video.src = URL.createObjectURL(blob);          
  })
}

function UUIDV4() {
  return ([1e7]+-1e3+-4e3+-8e3+-1e11).replace(/[018]/g, c =>
    (c ^ crypto.getRandomValues(new Uint8Array(1))[0] & 15 >> c / 4).toString(16)
  )
}

async function AnalyseFiles(data, accountId, instagramAccountId){
  var xl = 0;
  var items = [];
  for(; xl < data.requestData.length; xl++){
    await ReadFile(data.requestData[xl]).then(async resp=>{
      var mediaType = await GetMediaType(resp);
      if(mediaType.err === true){
        items.push(mediaType);
      }
      else{
        items.push({
          groupName: UUIDV4(),
          mediaType: mediaType.type,
          dateAdded: new Date(),
          mediaBytes: resp,
          instagramAccountId: instagramAccountId,
          accountId: accountId
        })
      }
    });
  }
  return items;
}

async function AnalyseFile(data, accountId, instagramAccountId){
  var response = {}
  await ReadFile(data.requestData).then(async resp=>{
    var mediaType = await GetMediaType(resp);
    if(mediaType.err === true){
      response = mediaType;
    }
    else {
      response =  {
        groupName: UUIDV4(),
        mediaType: mediaType.type,
        dateAdded: new Date(),
        mediaBytes: resp,
        instagramAccountId: instagramAccountId,
        accountId: accountId
      }
    }
  });
  return response;
}

function ValidateUrl(value) {
  return /^(?:(?:(?:https?|ftp):)?\/\/)(?:\S+(?::\S*)?@)?(?:(?!(?:10|127)(?:\.\d{1,3}){3})(?!(?:169\.254|192\.168)(?:\.\d{1,3}){2})(?!172\.(?:1[6-9]|2\d|3[0-1])(?:\.\d{1,3}){2})(?:[1-9]\d?|1\d\d|2[01]\d|22[0-3])(?:\.(?:1?\d{1,2}|2[0-4]\d|25[0-5])){2}(?:\.(?:[1-9]\d?|1\d\d|2[0-4]\d|25[0-4]))|(?:(?:[a-z\u00a1-\uffff0-9]-*)*[a-z\u00a1-\uffff0-9]+)(?:\.(?:[a-z\u00a1-\uffff0-9]-*)*[a-z\u00a1-\uffff0-9]+)*(?:\.(?:[a-z\u00a1-\uffff]{2,})))(?::\d{2,5})?(?:[/?#]\S*)?$/i.test(value);
}

function Base64ToBuffer(dataUrl) {
  var BASE64_MARKER = ';base64,';
  var base64Index = dataUrl.indexOf(BASE64_MARKER) + BASE64_MARKER.length;
  var base64 = dataUrl.substring(base64Index);
  var raw = window.atob(base64);
  var rawLength = raw.length;
  var array = new Uint8Array(new ArrayBuffer(rawLength));

  for(var i = 0; i < rawLength; i++) {
    array[i] = raw.charCodeAt(i);
  }
  return array;
}

const _ReadFile = ReadFile;
export { _ReadFile as ReadFile };
const _GetMediaType = GetMediaType;
export { _GetMediaType as GetMediaType };
const _ReadVideoLength = ReadVideoLength;
export { _ReadVideoLength as ReadVideoLength };
const _AnalyseFiles = AnalyseFiles;
export { _AnalyseFiles as AnalyseFiles };
const _AnalyseFile = AnalyseFile;
export { _AnalyseFile as AnalyseFile };
const _UUIDV4 = UUIDV4;
export { _UUIDV4 as UUIDV4 };
const _Base64ToBuffer = Base64ToBuffer;
export { _Base64ToBuffer as Base64ToBuffer }
const _ValidateUrl = ValidateUrl;
export { _ValidateUrl as ValidateUrl }