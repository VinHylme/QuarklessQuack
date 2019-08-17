import Api from './Api'

export default {
  GetSavedCaptions(accountId, instagramId){
    return Api(true).get('library/savedCaptions/' + accountId + '/' + instagramId)
  },
  GetSavedMedias(accountId, instagramId){
    return Api(true).get('library/savedMedias/' + accountId + '/' + instagramId)
  },
  GetSavedHashtags(accountId, instagramId){
    return Api(true).get('library/savedHashtags/' + accountId + '/' + instagramId)
  },
  SetSavedCaptions(accountId, instagramId, data){
    return Api(true).post('library/savedCaptions/' + accountId + '/' + instagramId, data)
  },
  SetSavedMedias(accountId, instagramId, data){
    return Api(true).post('library/savedMedias/' + accountId + '/' + instagramId, data)
  },
  SetSavedHashtags(accountId, instagramId, data){
    return Api(true).post('library/savedHashtags/' + accountId + '/' + instagramId, data)
  },
  DeleteSavedCaptions(accountId, instagramId, data){
    return Api(true).put('library/savedCaptions/' + accountId + '/' + instagramId, data)
  },
  DeleteSavedMedias(accountId, instagramId, data){
    return Api(true).put('library/savedMedias/' + accountId + '/' + instagramId, data)
  },
  DeleteSavedHashtags(accountId, instagramId, data){
    return Api(true).put('library/savedHashtags/' + accountId + '/' + instagramId, data)
  },
}