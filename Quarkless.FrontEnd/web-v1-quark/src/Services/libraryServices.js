import Api from './Api'

export default {
  GetSavedCaptions(accountId){
    return Api(true).get('library/savedCaptions/' + accountId)
  },
  GetSavedMedias(accountId){
    return Api(true).get('library/savedMedias/' + accountId)
  },
  GetSavedHashtags(accountId){
    return Api(true).get('library/savedHashtags/' + accountId)
  },
  GetSavedMessages(accountId){
    return Api(true).get('library/savedMessages/' + accountId)
  },

  GetSavedCaptionsForUser(instagramAccountId){
    return Api(true).get('library/savedCaptionsForUser/' + instagramAccountId)
  },
  GetSavedMediasForUser(instagramAccountId){
    return Api(true).get('library/savedMediasForUser/' + instagramAccountId)
  },
  GetSavedHashtagsForUser(instagramAccountId){
    return Api(true).get('library/savedHashtagsForUser/' + instagramAccountId)
  },
  GetSavedMessagesForUser(instagramAccountId){
    return Api(true).get('library/savedMessagesForUser/' + instagramAccountId)
  },

  SetSavedCaptions(data){
    return Api(true).post('library/savedCaptions',data)
  },
  SetSavedMedias(data){
    return Api(true).post('library/savedMedias', data)
  },
  SetSavedHashtags(data){
    return Api(true).post('library/savedHashtags', data)
  },
  SetSavedMessages(data){
    return Api(true).post('library/savedMessages', data)
  },

  UpdateSavedCaption(data){
    return Api(true).put('library/savedCaptions', data)
  },
  UpdateSavedHashtags(data){
    return Api(true).put('library/savedHashtags', data)
  },
  UpdateSavedMessages(data){
    return Api(true).put('library/savedMessages', data)
  },
  
  DeleteSavedCaptions(data){
    return Api(true).put('library/delete/savedCaptions', data)
  },
  DeleteSavedMedias(data){
    return Api(true).put('library/delete/savedMedias', data)
  },
  DeleteSavedHashtags(data){
    return Api(true).put('library/delete/savedHashtags', data)
  },
  DeleteSavedMessages(data){
    return Api(true).put('library/delete/savedMessages', data)
  }
}