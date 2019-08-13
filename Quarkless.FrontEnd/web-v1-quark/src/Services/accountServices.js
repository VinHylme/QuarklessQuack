import Api from './Api'
import axios from 'axios';
export default {
  Login(params){
    return Api(false).post('/auth/loginaccount', params)
  },
  GetInstagramAccountsForUser(params){
    return Api(true).get('insta/' + params.accountId)
  },
  GetProfilesForUser(userId){
    return Api(true).get('profiles/' + userId)
  },
  UpdateProfile(profileId, profileData){
    return Api(true).put('profiles/partial/'+profileId,profileData);
  },
  GetInstagramAccount(params){
    return Api(true).get('insta/state/'+ params.accountId + '/' + params.instagramAccountId)
  },
  UpdateAgentState(params){
    return Api(true).put('insta/agent/' + params.instagramAccountId + '/' + params.newState)
  },
  LinkInstagramAccount(params){
    return Api(true).post('insta/add',params)
  },
  SubmitCodeForChallange(code,data){
    return Api(true).put('insta/challange/submitCode/'+code,data);
  },
  RefreshState(id){
    return Api(true).get('insta/refreshLogin/'+id)
  },
  UploadFile(instaId,profileId,formData){
    return axios.put('http://localhost:51518/api/storage/upload/'+instaId+'/'+profileId, formData, 
    {
      headers:{
        'Content-Type': 'multipart/form-data',
        'Authorization': 'Bearer '+  axios.defaults.headers.common['Authorization']
        }
    })
  }
}