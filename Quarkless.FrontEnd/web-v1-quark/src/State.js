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
    token: localStorage.getItem('token') || ''
  },

  mutations: {
    auth_request(state){
      state.status = 'loading'
    },
    auth_error(state){
      state.status = 'error'
    },
    DELETE_ACCOUNT(state, value) {
      state.accounts[value].isActive = false;
    },
    ADD_ACCOUNT(state, value) {
      state.accounts.push(value);
    }
  },
  getters: {
    GetInstagramAccounts:state => (userId) => {
      if(state.AccountData.InstagramAccounts.length>0){
        return state.AccountData
      }
      else{
        state.AccountData = AccountServices.GetInstagramAccountsForUser({accountId: userId});
        return state.AccountData;
      }
    }
  },
  actions: {
    login({commit}, user){
      return new Promise((resolve, reject) => {
        commit('auth_request')

        AccountServices.Login(user).then(resp=>{
          const token = resp.data.idToken
          localStorage.setItem('token',token)
          Axios.defaults.headers.common['Authorization'] = token;
          commit('auth_request',token,user);
          resolve(resp)
        })
        .catch(err=>{
          commit('auth_error')
          localStorage.removeItem('token')
          reject(err)
        })
      })
    },
    DeleteAccount({
      state,
      commit
    }, value) {
      if (state.accounts) {
        commit('DELETE_ACCOUNT', value);
      }
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