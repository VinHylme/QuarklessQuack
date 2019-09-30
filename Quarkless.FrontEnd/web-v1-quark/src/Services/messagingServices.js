import Api from './Api';
import { Calling } from './constants';

export default{
	GetThread(instaAccount, threadId, limit){
		return Api(true, instaAccount).get(Calling["messaging_thread"]+threadId+'/'+limit)
	},
	SendDM(instaAccount, type, message){
		return Api(true,instaAccount).post(Calling["messaging_post_text"]+type, message);
	}
}