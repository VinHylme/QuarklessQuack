import axios from 'axios'

export default(withAuth) => {
  if(withAuth!==undefined){
    return axios.create({
      baseURL: `http://localhost:51518/api`,
      withCredentials: false,
      headers: {
          'Accept': 'application/json',
          'Content-Type': 'application/json',
          'Authorization': withAuth.token
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