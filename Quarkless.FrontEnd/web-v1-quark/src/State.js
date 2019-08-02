import Vue from 'vue'
import Vuex from 'vuex';
import AccountServices from './Services/accountServices';
import TimelineServices from './Services/timelineService';
import Axios from 'axios';
import decoder from 'jwt-decode';
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
      state.TimelineData = data
    },
    failed_to_retrieve_timeline_data_for_user(){
    }
  },
  getters: {
    IsLoggedIn: state => !!state.token,
    AuthStatus: state => state.status,
    GetInstagramAccounts:state => {
       return state.AccountData;
    },
    UserRole:state=>{return state.role},
    UserTimeline:state=> { return state.TimelineData}
  },
  actions: {
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
    login({commit}, user){
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