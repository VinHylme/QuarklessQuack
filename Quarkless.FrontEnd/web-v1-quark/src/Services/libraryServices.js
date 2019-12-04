import Api from './Api'
import { Calling } from './constants'

export default {
  GetSavedCaptions(accountId){
    return Api(true).get(Calling.library_get_captions + accountId)
  },
  GetSavedMedias(accountId){
    return Api(true).get(Calling.library_get_medias + accountId)
  },
  GetSavedHashtags(accountId){
    return Api(true).get(Calling.library_get_hashtags + accountId)
  },
  GetSavedMessages(accountId){
    return Api(true).get(Calling.library_get_messages + accountId)
  },

  GetSavedCaptionsForUser(instagramAccountId){
    return Api(true).get(Calling.library_get_captions_for_user + instagramAccountId)
  },
  GetSavedMediasForUser(instagramAccountId){
    return Api(true).get(Calling.library_get_medias_for_user + instagramAccountId)
  },
  GetSavedHashtagsForUser(instagramAccountId){
    return Api(true).get(Calling.library_get_hashtags_for_user + instagramAccountId)
  },
  GetSavedMessagesForUser(instagramAccountId){
    return Api(true).get(Calling.library_get_messages_for_user + instagramAccountId)
  },

  SetSavedCaptions(data){
    return Api(true).post(Calling.library_set_captions,data)
  },
  SetSavedMedias(data){
    return Api(true).post(Calling.library_set_medias, data)
  },
  SetSavedHashtags(data){
    return Api(true).post(Calling.library_set_hashtags, data)
  },
  SetSavedMessages(data){
    return Api(true).post(Calling.library_set_messages, data)
  },

  UpdateSavedCaption(data){
    return Api(true).put(Calling.library_update_captions, data)
  },
  UpdateSavedHashtags(data){
    return Api(true).put(Calling.library_update_hashtags, data)
  },
  UpdateSavedMessages(data){
    return Api(true).put(Calling.library_update_messages, data)
  },
  
  DeleteSavedCaptions(data){
    return Api(true).put(Calling.library_delete_captions, data)
  },
  DeleteSavedMedias(data){
    return Api(true).put(Calling.library_delete_medias, data)
  },
  DeleteSavedHashtags(data){
    return Api(true).put(Calling.library_delete_hashtags, data)
  },
  DeleteSavedMessages(data){
    return Api(true).put(Calling.library_delete_messages, data)
  }
}