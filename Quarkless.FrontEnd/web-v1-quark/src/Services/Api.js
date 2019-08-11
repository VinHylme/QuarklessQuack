import axios from 'axios'
const url = 'http://localhost:51518/api';
export default(withAuth, instagramAccount) => {
  if(withAuth){
    if(instagramAccount){
      return axios.create({
        baseURL: url,
        withCredentials: false,
        headers: {
            'Accept': 'application/json',
            'Content-Type': 'application/json',
            'Authorization': 'Bearer '+  axios.defaults.headers.common['Authorization'],
            'FocusInstaAccount' : instagramAccount 
        }
      })
    }
    else{
      return axios.create({
        baseURL: url,
        withCredentials: false,
        headers: {
            'Accept': 'application/json',
            'Content-Type': 'application/json',
            'Authorization': 'Bearer '+  axios.defaults.headers.common['Authorization']
        }
      })
    }
  }
  else{
    return axios.create({
        baseURL: url,
        withCredentials: false,
        headers: {
            'Accept': 'application/json',
            'Content-Type': 'application/json',
        }
    })
  }
}