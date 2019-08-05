import Api from './Api'

export default {
  Login(params){
    return Api(false).post('/auth/loginaccount', params)
  },
  GetInstagramAccountsForUser(params){
    return Api(true).get('/insta/' + params.accountId)
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
  RefreshState(id){
    return Api(true).get('insta/refreshLogin/'+id)
  }
}