import Vue from 'vue'
import Vuex from 'vuex';
import AccountServices from './Services/accountServices';
import TimelineServices from './Services/timelineService';
import QueryServices from './Services/queryServices';
import Axios from 'axios';
import decoder from 'jwt-decode';
import moment from 'moment';

Vue.use(Vuex);
export default new Vuex.Store({
  state:{
    AccountData:{
      InstagramAccounts:[],
      Profiles:[],
      TimelineData:[],
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

    }
  },
  getters: {
    IsLoggedIn: state => !!state.token,
    AuthStatus: state => state.status,
    UserRole:state=> state.role,
    GetInstagramAccounts:state => {return state.AccountData.InstagramAccounts},
    UserTimeline: state => {return state.AccountData.TimelineData},
    UserProfiles: state => {return state.AccountData.Profiles},
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
    SimilarSearch({commit},data){
      return new Promise((resolve,reject)=>{
        QueryServices.SimilarImageSearch(data.urls,data.limit, data.offset).then(resp=>{
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