import Api from './Api'
import { Calling } from './constants'

export default{
    GetEventLogs(instagramAccountId, limit){
        return Api(true).get(Calling.notifications_get_events + instagramAccountId + '/' + limit);
    },
    GetAllEventLogsForUser(instagramAccountId, limit){
        return Api(true, instagramAccountId).get(Calling.notifications_get_event_for_user + limit)
    }
}