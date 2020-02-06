import axios from 'axios'
import store from '../State'
import router from '../Route'
import { Calling } from './constants'
let defaultHeaders = {
  'Accept': 'application/json',
  'Content-Type': 'application/json',
}
var instance = axios.create({
  baseURL: Calling.base_path + '/api',
  withCredentials:false,
  headers: defaultHeaders
});
instance.interceptors.request.use((config) => {
  // eslint-disable-next-line no-console
  //console.log(config);
  return config;
});
// Response
instance.interceptors.response.use((response) => {
  if(response.status === 201){
    
  }
  return response 
}, (error) => {
  const originalRequest = error.config
  if (error.response.status === 401 && error.config && !error.config.__isRetryRequest) {
    originalRequest._retry = true   
    store.dispatch('refreshToken').then((response) => {
    let token = response.data.idToken
    let headerAuth = 'Bearer ' + token;
    originalRequest.headers['Authorization'] = headerAuth
	Promise.resolve(error.config)
	window.location.reload();
    return axios(originalRequest)
 }).catch((error) => {
   store.dispatch('logout').then(() => {
      router.push('/')
   }).catch(() => {
      router.push('/')
   })
 })
}
return Promise.reject(error)
});
export default(withAuth, instagramAccount) => {
  if(withAuth){
    if(instagramAccount){
      instance.defaults.headers = {
        'Accept': 'application/json',
        'Content-Type': 'application/json',
        'Authorization': 'Bearer '+  axios.defaults.headers.common['Authorization'],
        'FocusInstaAccount' : instagramAccount 
      }
      return instance;
    }
    else{
      instance.defaults.headers = {
        'Accept': 'application/json',
        'Content-Type': 'application/json',
        'Authorization': 'Bearer '+  axios.defaults.headers.common['Authorization']
      }
      return instance;
    }
  }
  else{
    return instance;
  }
}