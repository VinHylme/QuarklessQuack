/* eslint-disable no-unused-vars */
import Vue from 'vue'
import Vuex from 'vuex';
import actions from './State/actions';
import getters from './State/getters';
import mutations from './State/mutations';

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
  mutations,
  getters,
  actions
});