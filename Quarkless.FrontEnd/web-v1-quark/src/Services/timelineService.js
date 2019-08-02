import Api from './Api'

export default {
  GetUserTimeline(id){
    return Api(true).get('schedule/timeline/'+id)
  }
}