import Api from './Api'
import axios from 'axios';
import { Calling } from './constants';

export default {
  Login(params){
    return Api(false).post(Calling['account_login'], params)
  },
  AddUserDetails(userId, data){
    console.log(data)
    return Api(true).post(Calling['account_detail'] + userId, data)
  },
  RefreshToken(params){
    return Api(false).post(Calling['account_refresh'], params)
  },
  ResendConfirm(username){
    return Api(false).put(Calling['account_confirmation']+username);
  },
  ChangeProfilePicture(instagramAccountId, formData){
    return axios.put(Calling.base_path+Calling['account_changepp'], formData.formData, {
      headers:{
        'Content-Type': 'multipart/form-data',
        'Authorization': 'Bearer '+  axios.defaults.headers.common['Authorization'],
        'FocusInstaAccount' : instagramAccountId 
      }
    })
  },
  ChangeBiography(instagramAccountId, biography){
    return Api(true, instagramAccountId).put(Calling['account_changebio'], {Biography: biography.text});
  },
  GetInstagramAccountsForUser(params){
    return Api(true).get(Calling['account_get_instagrams_account'] + params.accountId)
  },
  GetProfilesForUser(userId){
    return Api(true).get(Calling['account_get_profiles'] + userId)
  },
  UpdateProfile(profileId, profileData){
    return Api(true).put(Calling['account_update_profile']+profileId,profileData);
  },
  AddProfileTopics(topicsRequest){
    return Api(true).post(Calling['account_add_profile_topics'], topicsRequest);
  },
  GetInstagramAccount(params){
    return Api(true).get(Calling['account_get_user_instagram_account']+ params.accountId + '/' + params.instagramAccountId)
  },
  UpdateAgentState(params){
    return Api(true).put(Calling['account_update_agent_state'] + params.instagramAccountId + '/' + params.newState)
  },
  LinkInstagramAccount(params){
    return Api(true).post(Calling['account_add_instagram'],params)
  },
  SubmitCodeForChallange(code,data){
    return Api(true).put(Calling['account_instagram_challenge']+code,data);
  },
  RefreshState(id){
    return Api(true).get(Calling['account_instagram_refresh']+id)
  },
  UploadFile(instaId,profileId,formData){
    return axios.put(Calling.base_path+Calling['account_upload']+instaId+'/'+profileId, formData, 
    {
      headers:{
        'Content-Type': 'multipart/form-data',
        'Authorization': 'Bearer '+  axios.defaults.headers.common['Authorization']
        }
    })
  },
  CreateSession(ptype, curr, sauce, accid){
		return Api(true).post(Calling['account_session'],{
			chargeType: ptype,
			source: sauce,
			currency: curr,
			accountId: accid
		});
  }
}