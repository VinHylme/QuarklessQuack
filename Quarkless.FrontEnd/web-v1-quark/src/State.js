import Vue from 'vue'
import Vuex from 'vuex';
import AccountServices from './Services/accountServices';
import TimelineServices from './Services/timelineService';
import QueryServices from './Services/queryServices';
import LibraryServices from './Services/libraryServices';
import Axios from 'axios';
import decoder from 'jwt-decode';
import moment from 'moment';
import helper from './helpers';
Vue.use(Vuex);
export default new Vuex.Store({
  state:{
    showingLogs:false,
    AccountData:{
      InstagramAccounts:[],
      Profiles:[],
      Library:{
        MediaLibrary:[],
        CaptionLibrary:[],
        HashtagLibrary:[],
        MessagesLibrary:[]
      },
      TimelineData:[],
      TimelineLogData:[],
      ProfileConfg:{}
    },
    status:'',
    token: localStorage.getItem('token') || '',
    user: localStorage.getItem('user') || '',
    role: localStorage.getItem('role') || ''
  },

  mutations: {
    auth_request(state){
      state.status = 'loading'
    },
    auth_success(state, data){
      state.status = 'success'
      state.token = data.Token
      state.user = data.User
      state.role = data.Role
    },
    auth_error(state){
      state.status = 'error'
      state.token = ''
      state.user = ''
      state.role = ''
    }, 
    logout(state){
      state.status = ''
      state.token = ''
      state.user = ''
      state.role = ''
    },
    ADD_ACCOUNT(state, value) {
      state.accounts.push(value);
    },
    account_details_retrieved(state, value){
      state.AccountData.InstagramAccounts = value;
    },
    failed_to_retrive_account_details(state){
      state.AccountData = null;
    },
    failed_to_update_agent_state(){

    },
    updated_agent_state(){

    },
    failed_to_refresh_state(){

    },
    refreshed_state(){},
    retrieved_timeline_data_for_user(state, data){
      state.AccountData.TimelineData = [];
      for(var i = 0; i < data.length; i++){
        var item = data[i];
        var moment_enqueued = moment(item.enqueueTime);
        var enqueueTime = new Date(moment_enqueued.format('YYYY-MM-DD HH:mm:ss'));
        if(enqueueTime===undefined){continue;}
        state.AccountData.TimelineData.push({
          id: item.itemId,
          startTime: enqueueTime.getHours()+":"+ enqueueTime.getMinutes(),
          endTime: enqueueTime.getHours() + ":" + enqueueTime.getMinutes(),
          actionObject:{
            actionName:item.actionName.split('_')[0],
            actionType:item.actionName.split('_')[1],
            body:item.body,
            targetId:item.targetId
          }
        })
      }
    },
    failed_to_retrieve_timeline_data_for_user(){
    },
    failed_to_delete_event(){},
    event_deleted(state, eventId){
      state.AccountData.TimelineData = state.AccountData.TimelineData.filter((obj)=> obj.id !== eventId);
    },
    event_enqueued(state, eventId){
      state.AccountData.TimelineData = state.AccountData.TimelineData.filter((obj)=> obj.id !== eventId);
    },
    failed_to_enqueue_event(){},
    updated_event(state, {event, newid}){
      /*
        caption: this.caption,
        time: this.time,
        hashtags: this.hashtags,
        location: this.location,
        credit: this.credit,
        type: this.type 

        const caption = bodyResp.MediaInfo.Caption;
        const hashtags = bodyResp.MediaInfo.Hashtags;
        const credit = bodyResp.MediaInfo.Credit;
        const location = bodyResp.Location;

      */
      const index = state.AccountData.TimelineData.findIndex((obj=>obj.id == event.id));
      var tojsonObject = JSON.parse(state.AccountData.TimelineData[index].actionObject.body);
      state.AccountData.TimelineData[index].id = newid;
      tojsonObject.MediaInfo.Caption = event.caption;
      tojsonObject.MediaInfo.Hashtags = event.hashtags;
      tojsonObject.Location = event.location;
      tojsonObject.MediaInfo.Credit = event.credit;
      state.AccountData.TimelineData[index].actionObject.body = JSON.stringify(tojsonObject);
},
    failed_to_update_event(){},
    profiles_retrieved(state, profiles){
      state.AccountData.Profiles = profiles;
    },
    failed_to_retieve_profiles(state){
      state.AccountData.Profiles = []
    },
    profile_config_retrieved(state, data){
      state.AccountData.ProfileConfg = data;
    },
    failed_to_retrieve_profile_config(state){
      state.AccountData.ProfileConfg = {}
    },
    profile_uploaded_files(state, data){
      //todo: make sure that the profile data files are upto date
    },
    profile_updates(state, data){
    },
    failed_profile_update(state, profileData){

    },
    set_saved_medias(state, data){
      if(data.request !== undefined || data.request !== null){
        var index = 0;
        for(index; index < data.request.length; index++)
        {
          state.AccountData.Library.MediaLibrary.push(data.request[index])
        }
      }
    },
    failed_to_set_saved_medias(state, data){

    },
    get_saved_medias(state, data){
      state.AccountData.Library.MediaLibrary = data.response.data.data;
    },
    get_saved_medias_for_user(state, data){
      if(data.response.data.data !== undefined || data.response.data.data !== null){
        var index = 0;
        for(index; index < data.response.data.data.length; index++){
          //state.AccountData.Library.push()
        }
      }
    },
    failed_to_get_saved_medias(state){
      state.AccountData.Library = null;
    },
    delete_saved_medias(state, data){
      let position = -1;
      state.AccountData.Library.MediaLibrary.forEach((item,index) => 
      {
        if(item._id === data.request._id)
          position = index;
      });
      state.AccountData.Library.MediaLibrary.splice(position,1);
    },
    failed_to_delete_saved_medias(state){
      state.AccountData.Library = null;
    },
    create_post(state,event){},
    failed_to_create_post(state){},
    retrieved_event_logs(state, logs){
      state.AccountData.TimelineLogData = logs;
    },
    failed_to_retrieve_event_logs(state)
    {
      state.AccountData.TimelineLogData = []
    },
    async update_profile_picture(state, data){
      await helper.readFile(data.image.requestData[0]).then(res=>{
        state.AccountData.InstagramAccounts.filter((item)=>{
          if(item.id == data.instagramAccountId)
            item.profilePicture = res
        })
      })
    },
    update_biography(state, data){
      state.AccountData.InstagramAccounts.filter((item)=>{
        if(item.id == data.instagramAccountId)
          item.userBiography = data.request.biography
      })
    },
    set_saved_hashtags(state, data){
      if(data.request !== undefined || data.request !== null){
        state.AccountData.Library.HashtagLibrary.push(data.request)      
      }
    },
    failed_to_set_saved_hashtags(state, data){

    },
    set_saved_caption(state, data){
      if(data.request !== undefined || data.request !== null){
        state.AccountData.Library.CaptionLibrary.push(data.request)      
      }
    },
    failed_to_set_saved_caption(state, data){

    },
    get_saved_caption(state, data){
      state.AccountData.Library.CaptionLibrary = data.response.data.data;
    },
    get_saved_hashtags(state, data){
      state.AccountData.Library.HashtagLibrary = data.response.data.data;
    },
    failed_to_get_saved_hashtags(state){
      state.AccountData.Library.HashtagLibrary = [];
    },
    failed_to_get_saved_caption(state){
      state.AccountData.Library.CaptionLibrary = [];
    },
    delete_saved_caption(state, data){
      let position = -1;
      state.AccountData.Library.CaptionLibrary.forEach((item,index)=>{
        if(item._id == data.request._id){
          position = index;
        }
      });
      state.AccountData.Library.CaptionLibrary.splice(position,1);
    },
    delete_saved_hashtags(state, data){
      let position = -1;
      state.AccountData.Library.HashtagLibrary.forEach((item,index)=>{
        if(item._id == data.request._id){
          position = index;
        }
      });
      state.AccountData.Library.HashtagLibrary.splice(position,1);
    },
    delete_saved_message(state, data){
      let position = -1;
      state.AccountData.Library.MessagesLibrary.forEach((item,index)=>{
        if(item._id == data.request._id){
          position = index;
        }
      });
      state.AccountData.Library.MessagesLibrary.splice(position,1);
    },
    get_saved_messages(state, data){
      state.AccountData.Library.MessagesLibrary = data.response.data.data;
    },
    failed_to_get_saved_message(state){
      state.AccountData.Library.MessagesLibrary = [];
    },
    set_saved_message(state, data){
      if(data.request !== undefined || data.request !== null){
        state.AccountData.Library.MessagesLibrary.push(data.request)      
      }
    }
  },
  getters: {
    User: state => state.user,
    IsLoggedIn: state => !!state.token,
    AuthStatus: state => state.status,
    UserRole:state=> state.role,
    GetInstagramAccounts:state => {return state.AccountData.InstagramAccounts},
    UserTimeline: state => {return state.AccountData.TimelineData},
    UserTimelineLogs: state => state.AccountData.TimelineLogData,
    UserTimelineLogForUser: state => instaId => {
      return state.AccountData.TimelineLogData.filter(item=>item.instagramAccountID === instaId);
    },
    UserProfiles: state => {return state.AccountData.Profiles},
    UserLibraries: state => {return state.AccountData.Library},
    UserMediaLibrary: state => (instagramAccountId) => {
      return state.AccountData.Library.MediaLibrary.filter(item=>item.instagramAccountId === instagramAccountId);
    },
    UserCaptionLibrary: state => (instagramAccountId) => {
      return state.AccountData.Library.CaptionLibrary.filter(item=>item.instagramAccountId === instagramAccountId);
    },
    UserHashtagsLibrary: state => (instagramAccountId) => {
      return state.AccountData.Library.HashtagLibrary.filter(item=>item.instagramAccountId === instagramAccountId);
    },
    UserMessageLibrary: state => (instagramAccountId) => {
      return state.AccountData.Library.MessagesLibrary.filter(item=>item.instagramAccountId === instagramAccountId);
    },  
    UserProfile: state => (instaId) => 
    {
      let profile = state.AccountData.Profiles[state.AccountData.Profiles.findIndex(_=>_.instagramAccountId ==instaId)];
      if(profile!==undefined)
        return profile; 
    },
    InstagramProfilePicture: state => id => {
      var elment = state.AccountData.InstagramAccounts[state.AccountData.InstagramAccounts.findIndex(_=>_.id==id)];
      if(elment !== undefined){
        return elment.profilePicture;
      }
    },
    GetProfileConfig:state=> state.AccountData.ProfileConfg,
    MenuState: () => localStorage.getItem("menu_state")
  },
  actions: {
    GetEventLogs({commit}, data){
      return new Promise((resolve, reject)=>{
        TimelineServices.GetEventLogs(data.instagramAccountId, data.limit).then(resp=>{
          commit('retrieved_event_logs_in',resp.data);
          resolve(resp);
        }).catch((err)=>{
          commit('failed_to_retrieve_event_logs_in',err);
          reject(err);
        })
      })
    },
    GetAllEventLogsForUser({commit},limit){
      return new Promise((resolve, reject)=>{
        TimelineServices.GetAllEventLogsForUser(limit).then(resp=>{
          commit('retrieved_event_logs',resp.data);
          resolve(resp);
        }).catch((err)=>{
          commit('failed_to_retrieve_event_logs');
          reject(err);
        })
      })
    },
    CreatePost({commit}, data){
      return new Promise((resolve, reject)=>{
        TimelineServices.CreatePost(data.id, data.event).then(resp=>{
          commit('create_post', data);
          resolve(resp);
        }).catch((err)=>{
          commit('failed_to_create_post', err);
          reject(err);
        })
      })
    },
   
    SetSavedMedias({commit}, medias){
      return new Promise((resolve, reject)=>{
        LibraryServices.SetSavedMedias(medias).then(resp=>{
          commit('set_saved_medias', {request: medias, response: resp});
          resolve(resp);
        }).catch((err)=>{
          commit('failed_to_set_saved_medias', { request: medias, error: err })
          reject(err);
        })
      })
    }, 
    //#region SavedCaptions
    SetSavedCaption({commit}, caption){
      return new Promise((resolve, reject)=>{
        LibraryServices.SetSavedCaptions(caption).then(resp=>{
          commit('set_saved_caption', {request: caption, response: resp});
          resolve(resp);
        }).catch((err)=>{
          commit('failed_to_set_saved_caption', { request: caption, error: err })
          reject(err);
        })
      })
    },
    UpdateSavedCaption({commit}, caption){
      return new Promise((resolve, reject)=>{
        LibraryServices.UpdateSavedCaption(caption).then(resp=>{
          //commit('update_saved_caption', {request: caption, response: resp});
          resolve(resp);
        }).catch((err)=>{
          //commit('failed_to_update_saved_caption', { request: caption, error: err })
          reject(err);
        })
      })
    },
    //#endregion
    SetSavedHashtags({commit}, hashtags){
      return new Promise((resolve, reject)=>{
        LibraryServices.SetSavedHashtags(hashtags).then(resp=>{
          commit('set_saved_hashtags', {request: hashtags, response: resp});
          resolve(resp);
        }).catch((err)=>{
          commit('failed_to_set_saved_hashtags', { request: hashtags, error: err })
          reject(err);
        })
      })
    },
    UpdateSavedHashtags({commit}, hashtags){
      return new Promise((resolve, reject)=>{
        LibraryServices.UpdateSavedHashtags(hashtags).then(resp=>{
          //commit('update_saved_caption', {request: caption, response: resp});
          resolve(resp);
        }).catch((err)=>{
          //commit('failed_to_update_saved_caption', { request: caption, error: err })
          reject(err);
        })
      })
    },
    DeleteSavedHashtags({commit}, hashtags){
      return new Promise((resolve, reject)=>{
        LibraryServices.DeleteSavedHashtags(hashtags).then(resp=>{
          commit('delete_saved_hashtags', {request: hashtags, response: resp});
          resolve(resp);
        }).catch((err)=>{
          //commit('failed_to_update_saved_caption', { request: caption, error: err })
          reject(err);
        })
      })
    },
    SetSavedMessages({commit}, message){
      return new Promise((resolve, reject)=>{
        LibraryServices.SetSavedMessages(message).then(resp=>{
          commit('set_saved_message', {request: message, response: resp});
          resolve(resp);
        }).catch((err)=>{
          //commit('failed_to_set_saved_messages', { request: message, error: err })
          reject(err);
        })
      })
    },
    UpdateSavedMessages({commit}, message){
      return new Promise((resolve, reject)=>{
        LibraryServices.UpdateSavedMessages(message).then(resp=>{
          //commit('update_saved_caption', {request: caption, response: resp});
          resolve(resp);
        }).catch((err)=>{
          //commit('failed_to_update_saved_caption', { request: caption, error: err })
          reject(err);
        })
      })
    },
    DeleteSavedMessages({commit}, message){
      return new Promise((resolve, reject)=>{
        LibraryServices.DeleteSavedMessages(message).then(resp=>{
          commit('delete_saved_message', {request: message, response: resp});
          resolve(resp);
        }).catch((err)=>{
          //commit('failed_to_update_saved_caption', { request: caption, error: err })
          reject(err);
        })
      })
    },
    GetSavedMessages({commit}, accountId){
      return new Promise((resolve, reject)=>{
        LibraryServices.GetSavedMessages(accountId).then(resp=>{
          commit('get_saved_messages', {request: accountId, response: resp});
          resolve(resp);
        }).catch((err)=>{
          commit('failed_to_get_saved_message')
          reject(err);
        })
      })
    },
    GetSavedHashtags({commit}, accountId){
      return new Promise((resolve, reject)=>{
        LibraryServices.GetSavedHashtags(accountId).then(resp=>{
          commit('get_saved_hashtags', {request: accountId, response: resp});
          resolve(resp);
        }).catch((err)=>{
          commit('failed_to_get_saved_hashtags')
          reject(err);
        })
      })
    },
    DeleteSavedCaption({commit}, caption){
      return new Promise((resolve, reject)=>{
        LibraryServices.DeleteSavedCaptions(caption).then(resp=>{
          commit('delete_saved_caption', {request: caption, response: resp});
          resolve(resp);
        }).catch((err)=>{
          //commit('failed_to_update_saved_caption', { request: caption, error: err })
          reject(err);
        })
      })
    },
    GetSavedMedias({commit}, accountId){
      return new Promise((resolve, reject)=>{
        LibraryServices.GetSavedMedias(accountId).then(resp=>{
          commit('get_saved_medias', {request: accountId, response: resp});
          resolve(resp);
        }).catch((err)=>{
          commit('failed_to_get_saved_medias')
          reject(err);
        })
      })
    },
    GetSavedCaption({commit}, accountId){
      return new Promise((resolve, reject)=>{
        LibraryServices.GetSavedCaptions(accountId).then(resp=>{
          commit('get_saved_caption', {request: accountId, response: resp});
          resolve(resp);
        }).catch((err)=>{
          commit('failed_to_get_saved_caption')
          reject(err);
        })
      })
    },
    GetSavedMediasForUser({commit}, instagramAccountId){
      return new Promise((resolve, reject)=>{
        LibraryServices.GetSavedMediasForUser(instagramAccountId).then(resp=>{
          commit('get_saved_medias_for_user', {request: instagramAccountId, response: resp});
          resolve(resp);
        }).catch((err)=>{
          commit('failed_to_get_saved_medias_for_user')
          reject(err);
        })
      })
    },
    DeleteSavedMedia({commit}, media){
      return new Promise((resolve, reject)=>{
        LibraryServices.DeleteSavedMedias(media).then(resp=>{
          commit('delete_saved_medias', {request: media, response: resp});
          resolve(resp);
        }).catch((err)=>{
          commit('failed_to_delete_saved_medias')
          reject(err);
        })
      })
    },
    BuildTags({commit}, data){
      return new Promise((resolve, reject)=>{
        QueryServices.BuildTags(data.topic, data.subcat, data.lang, data.limit, data.pickRate).then(resp=>{
          resolve(resp);
        }).catch((err)=>reject(err));
      })
    },
    ReleatedTopics({commit}, data)
    {
      return new Promise((resolve,reject)=>{
        QueryServices.ReleatedTopic(data.instaId, data.topic).then(resp=>{
          resolve(resp);
        }).catch((err)=>reject(err))
      })
    },
    HideunHideMenu({commit}, value){
        localStorage.setItem("menu_state", value);
    },
    UploadFileForProfile({commit}, data){
      return new Promise((resolve, reject)=>{
        AccountServices.UploadFile(data.instaId, data.profileId, data.formData).then(resp=>{
          commit('profile_uploaded_files',data);
          resolve(resp);
        }).catch((err)=>reject(err))
      })
    },
    ChangeBiography({commit}, data){
      return new Promise((resolve, reject)=>{
        AccountServices.ChangeBiography(data.instagramAccountId, data.biography).then(resp=>{
          commit('update_biography', {request: data, response: resp.data})
          resolve(resp);
        }).catch((err)=>reject(err));
      });
    },
    ChangeProfilePicture({commit}, data){
      return new Promise((resolve,reject)=>{
        AccountServices.ChangeProfilePicture(data.instagramAccountId, data.image).then(resp=>{
          commit('update_profile_picture', data);
          resolve(resp);
        }).catch((err)=> reject(err))
      })
    },
    SimilarSearch({commit},data){
      return new Promise((resolve,reject)=>{
        QueryServices.SimilarImageSearch(data.urls,data.limit, data.offset, data.moreAccurate).then(resp=>{
          resolve(resp);
        }).catch((err)=>reject(err));
      })
    },
    GooglePlacesSearch({commit},query){
      return new Promise((resolve,reject)=>{
        QueryServices.GooglePlaceSearch(query).then(resp=>{
          resolve(resp);
        }).catch(err=>{
          reject(err);
        })
      })
    },
    GooglePlacesAutoCompleteSearch({commit},queryObject){
      return new Promise((resolve, reject)=>{
        QueryServices.GooglePlacesAutocomplete(queryObject.query, queryObject.radius).then(resp=>{
          resolve(resp);
        }).catch(err=>reject(err));
      })
    },
    GetProfileConfig({commit}){
      return new Promise((resolve,reject)=>{
        QueryServices.GetProfileConfig().then(resp=>{
          commit('profile_config_retrieved', resp.data);
          resolve(resp);
        }).catch(err=>{
          commit('failed_to_retrieve_profile_config');
          reject(err);
        })
      })
    },
    UpdateProfile({commit}, profileData){
      return new Promise((resolve, reject)=>{
        AccountServices.UpdateProfile(profileData._id, profileData).then(resp=>{
          commit('profile_updates', {profileData: profileData, response:resp});
          resolve(resolve);
        }).catch(err=>{
          commit('failed_profile_update',profileData);
          reject(err);
        })
      });
    },
    GetProfiles({commit}, userId){
      return new Promise((resolve, reject)=>{
        AccountServices.GetProfilesForUser(userId).then(resp=>{
          commit('profiles_retrieved', resp.data)
          resolve(resp);
        }).catch(err=>{
          commit('failed_to_retieve_profiles')
          reject(err);
        })
      })
    },
    UpdateEvent({commit}, event){
      return new Promise((resolve, reject)=>{
        TimelineServices.UpdateEvent(event).then(resp=>{
          commit('updated_event', {event, newid: resp.data });
          resolve(resp);
        }).catch(err=>{
          commit('failed_to_update_event')
          reject(err);
        })
      })
    },
    EnqueueEventNow({commit}, eventId){
      return new Promise((resolve,reject)=>{
        TimelineServices.EnqueueNow(eventId).then(resp=>{
          commit('event_enqueued',eventId);
          resolve(resp);
        }).catch(err=>{
          commit('failed_to_enqueue_event');
          reject(err);
        })
      })
    },
    DeleteEvent({commit}, eventId){
      return new Promise((resolve,reject)=>{
        TimelineServices.DeleteEventFromTimeline(eventId).then(resp=>{
          commit('event_deleted',eventId);
          resolve(resp);
        }).catch(err=>{
          commit('failed_to_delete_event');
          reject(err);
        })
      })
    },
    GetUsersTimeline({commit},id){
      return new Promise((resolve, reject)=>{
        TimelineServices.GetUserTimeline(id).then(resp=>{
          commit('retrieved_timeline_data_for_user',resp.data);
          resolve(resp)
        }).catch(err=>{
          commit('failed_to_retrieve_timeline_data_for_user')
          reject(err)
        })
      })
    },
    RefreshState({commit}, id){
      return new Promise((resolve,reject)=>{
        AccountServices.RefreshState(id).then(resp=>{
          commit('refreshed_state');
          resolve(resp)
        }).catch(err=>{
          commit('failed_to_refresh_state');
          reject(err);
        })
      })
    },
    ChangeState({commit}, newstate){
      return new Promise((resolve,reject)=>{
        AccountServices.UpdateAgentState({instagramAccountId: newstate.instaId, newState: parseInt(newstate.state)}).then(resp=>{
          commit('updated_agent_state');
          resolve(resp);
        }).catch(err=>{
          commit('failed_to_update_agent_state');
          if(err.message.includes('401')){
            this.dispatch('logout');
          }
          reject(err);
        })
      })
    },
    AccountDetails({commit}, payload){
      return new Promise((resolve, reject)=>{
        AccountServices.GetInstagramAccountsForUser({accountId:payload.userId}).then(resp=>{
          commit('account_details_retrieved',resp.data);
          resolve(resp);
        })
        .catch(err=>{
          commit('failed_to_retrive_account_details');
          if (err.message.includes('401')) {
            this.dispatch('logout');
          }
          reject(err);
        })
      })
    },
    resendConfirmation({commit}, username){
      return new Promise((resolve, reject)=>{
        AccountServices.ResendConfirm(username).then(resp=>{
          resolve(resp);
        }).catch(err=>reject(err))
      })
    },
    async login({commit}, user){
      return new Promise((resolve, reject) => {
        commit('auth_request')
        AccountServices.Login(user).then(resp=>{
          const token = resp.data.idToken
          localStorage.setItem('token',token)
          Axios.defaults.headers.common['Authorization'] = token;
          var decoded = decoder(token);
          var role = decoded["cognito:groups"][0];
          localStorage.setItem('user', user.Username)
          localStorage.setItem('role', role)
          commit('auth_success', 
          { 
            Token: token, 
            User: { 
              Username: user.Username,
              Role: role
            }
          });
          decoded = null;
          resolve(resp)
        })
        .catch(err=>{
          commit('auth_error')
          localStorage.removeItem('token')
          this.state.UserAuthenticated = false;
          reject(err)
        })
      })
    },
    logout({commit}){
      return new Promise((resolve) => {
        commit('logout')
        localStorage.removeItem('token')
        delete Axios.defaults.headers.common['Authorization']
        resolve()
      })
    },
    LinkInstagramAccount({commit}, data) {
      return new Promise((resolve,reject)=>{
        AccountServices.LinkInstagramAccount(data).then(resp=>{
          resolve(resp);
        }).catch(err=>{
          reject(err);
        })
      })
    },
    SubmitCodeForChallange({commit},data){
      return new Promise((resolve,reject)=>{
        AccountServices.SubmitCodeForChallange(data.code, data.account).then(resp=>{
          resolve(resp)
        }).catch(err=>reject(err));
      })
    }
  }
});