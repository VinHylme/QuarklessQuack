import axios from 'axios'
export default(withAuth) => {
  if(withAuth){
    return axios.create({
      baseURL: `http://localhost:51518/api`,
      withCredentials: false,
      headers: {
          'Accept': 'application/json',
          'Content-Type': 'application/json',
          'Authorization': 'Bearer '+  axios.defaults.headers.common['Authorization']
      }
    })
  }
  else{
    return axios.create({
        baseURL: `http://localhost:51518/api`,
        withCredentials: false,
        headers: {
            'Accept': 'application/json',
            'Content-Type': 'application/json',
        }
    })
  }
}