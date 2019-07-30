import Vue from 'vue'
import Vuex from 'vuex';
import AccountServices from './Services/accountServices';
import Axios from 'axios';
Vue.use(Vuex);
export default new Vuex.Store({
  state:{
    AccountData:{
      InstagramAccounts:[],
      Profiles:[]
    },
    status:'',
    token: localStorage.getItem('token') || '',
    user: {}
  },

  mutations: {
    auth_request(state){
      state.status = 'loading'
    },
    auth_success(state, token, user){
      state.status = 'success'
      state.token = token
      state.user = user
    },
    auth_error(state){
      state.status = 'error'
    }, 
    logout(state){
      state.status = ''
      state.token = ''
    },
    ADD_ACCOUNT(state, value) {
      state.accounts.push(value);
    },
    account_details_retrieved(state, value){
      state.AccountData = value;
    },
    failed_to_retrive_account_details(state){
      state.AccountData = null;
    }
  },
  getters: {
    IsLoggedIn: state => !!state.token,
    AuthStatus: state => state.status,
    GetInstagramAccounts:state => {
       return state.AccountData;
    }
  },
  actions: {
    AccountDetails({commit}, payload){
      return new Promise((resolve, reject)=>{
        AccountServices.GetInstagramAccountsForUser({accountId:payload.userId},payload.token).then(resp=>{
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
          commit('auth_success',token, user);
          resolve(resp)
          window.location.reload();
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
      // eslint-disable-next-line no-console
      console.log(commit);
      //todo
      //make sure the account is verified
      //if it is
      //call api function to populate the accounts

    }
  }
});