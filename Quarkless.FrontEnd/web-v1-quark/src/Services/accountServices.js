import Api from './Api'

export default {
  Login(params){
    return Api(undefined).post('/auth/loginaccount',params)
  },
  GetInstagramAccountsForUser(params,token){
    return Api(token).get('/insta/' + params.accountId)
  },
  GetInstagramAccount(params,token){
    return Api(token).get('insta/state/'+ params.accountId + '/' + params.instagramAccountId)
  },
  LinkInstagramAccount(params,token){
    return Api(token).post('insta/add',params)
  }
}