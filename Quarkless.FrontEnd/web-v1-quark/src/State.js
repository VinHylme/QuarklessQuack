import Vue from 'vue'
import Vuex from 'vuex';
import AccountServices from './Services/accountServices';
import TimelineServices from './Services/timelineService';
import Axios from 'axios';
import decoder from 'jwt-decode';
import moment from 'moment';

Vue.use(Vuex);
export default new Vuex.Store({
  state:{
    AccountData:{
      InstagramAccounts:[],
      Profiles:[],
      TimelineData:[]
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
      state.AccountData = value;
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
    failed_to_update_event(){}
    
  },
  getters: {
    IsLoggedIn: state => !!state.token,
    AuthStatus: state => state.status,
    GetInstagramAccounts:state => {
       return state.AccountData;
    },
    UserRole:state=>{return state.role},
    UserTimeline:state=> { return state.AccountData.TimelineData}
  },
  actions: {
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
      this.state.AccountData.TimelineData = [];
      return new Promise((resolve, reject)=>{
        TimelineServices.GetUserTimeline(id).then(resp=>{
         // this.state.AccountData.TimelineData = [];
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
    LinkInstagramAccount({
      commit
    }) {
      commit('ADD_ACCOUNT', this.state.accounts[0]);

      //todo
      //make sure the account is verified
      //if it is
      //call api function to populate the accounts

    }
  }
});