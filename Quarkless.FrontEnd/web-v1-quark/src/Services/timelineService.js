import Api from './Api'
import { Calling } from './constants'
export default {
  GetUserTimeline(id){
    return Api(true).get(Calling.timeline_get_user_timeline + id)
  },
  DeleteEventFromTimeline(eventId){
    return Api(true).delete(Calling.timeline_delete_event + eventId);
  },
  EnqueueNow(eventId){
    return Api(true).put(Calling.timeline_enqueue_event + eventId);
  },
  UpdateEvent(event){
    return Api(true).put(Calling.timeline_update_event, event);
  },
  CreatePost(id, event){
    return Api(true, id).post(Calling.timeline_create_post + id, event);
  },
  CreateMessage(type, id, messages){
    return Api(true,id).post(Calling.timeline_create_message + type + '/' + id, messages)
  },
  GetEventLogs(instagramAccountId, limit){
    return Api(true).get(Calling.timeline_get_event_logs + instagramAccountId + '/' + limit);
  },
  GetAllEventLogsForUser(instagramAccountId, limit){
    return Api(true, instagramAccountId).get(Calling.timeline_get_event_logs_for_user + limit)
  }
}