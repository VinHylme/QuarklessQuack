import Api from './Api'

export default {
  GetUserTimeline(id){
    return Api(true).get('schedule/timeline/'+id)
  },
  DeleteEventFromTimeline(eventId){
    return Api(true).delete('timeline/delete/'+eventId);
  },
  EnqueueNow(eventId){
    return Api(true).put('timeline/enqueue/'+eventId);
  },
  UpdateEvent(event){
    return Api(true).put('timeline/update/', event);
  }
}