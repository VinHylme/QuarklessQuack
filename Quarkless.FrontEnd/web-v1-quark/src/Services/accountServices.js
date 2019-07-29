import Api from './Api'

export default {
  Login(params){
    // eslint-disable-next-line no-console
    console.log(params);   
    return Api().post('/auth/loginaccount',{Username: params.Username, Password: params.Password})
  },
  GetInstagramAccountsForUser(params){
    return Api().get('/insta/' + params.accountId)
  },
  GetInstagramAccount(params){
    return Api().get('insta/state/'+ params.accountId + '/' + params.instagramAccountId)
  },
  LinkInstagramAccount(params){
    return Api().post('insta/add',params)
  }
}