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
  },
  CreatePost(id, event){
    return Api(true, id).post('timeline/post/'+ id, event);
  },
  CreateMessage(type, id, messages){
    return Api(true,id).post('timeline/messaging/'+type+'/'+ id, messages)
  },
  GetEventLogs(instagramAccountId, limit){
    return Api(true).get('timeline/log/'+instagramAccountId+'/'+limit);
  },
  GetAllEventLogsForUser(limit){
    return Api(true).get('timeline/log/'+limit)
  }
}