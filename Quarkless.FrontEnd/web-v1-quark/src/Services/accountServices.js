import Api from './Api'
import axios from 'axios';
const base_url = 'http://localhost:51518';
export default {
  Login(params){
    return Api(false).post('auth/loginaccount', params)
  },
  RefreshToken(params){
    return Api(false).post('auth/refreshState', params)
  },
  ResendConfirm(username){
    return Api(false).put('auth/resendConfirmation/'+username);
  },
  ChangeProfilePicture(instagramAccountId, formData){
    return axios.put(base_url+'/api/account/changepp/', formData.formData, {
      headers:{
        'Content-Type': 'multipart/form-data',
        'Authorization': 'Bearer '+  axios.defaults.headers.common['Authorization'],
        'FocusInstaAccount' : instagramAccountId 
      }
    })
  },
  ChangeBiography(instagramAccountId, biography){
    return Api(true, instagramAccountId).put('account/changeBio', {Biography: biography.text});
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
    return axios.put(base_url+'/api/storage/upload/'+instaId+'/'+profileId, formData, 
    {
      headers:{
        'Content-Type': 'multipart/form-data',
        'Authorization': 'Bearer '+  axios.defaults.headers.common['Authorization']
        }
    })
  },
  CreateSession(ptype, curr, sauce, accid){
		return Api(true).post('account/session',{
			chargeType: ptype,
			source: sauce,
			currency: curr,
			accountId: accid
		});
  }
}