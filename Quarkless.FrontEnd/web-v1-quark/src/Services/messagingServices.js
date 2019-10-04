import Api from './Api';
import { Calling } from './constants';

export default{
	GetThread(instaAccount, threadId, limit){
		return Api(true, instaAccount).get(Calling["messaging_thread"]+threadId+'/'+limit)
	},
	SendDM(instaAccount, type, message){
		return Api(true,instaAccount).post(Calling["messaging_post_text"]+type, message);
	},
	DeleteComment(instaAccount, media, comment){
		return Api(true, instaAccount).delete(Calling["messaging_delete_comment"]+media+'/'+comment)
	},
	LikeComment(instaAccount, comment){
		return Api(true,instaAccount).post(Calling["messaging_like_comment"]+comment)
	},
	UnLikeComment(instaAccount, comment){
		return Api(true,instaAccount).put(Calling["messaging_unlike_comment"]+comment)
	},
	ReplyComment(instaAccount, media, comment, message){
		return Api(true,instaAccount).post(Calling["messaging_reply_comment"]+media+'/'+comment, message)
	},
	CreateComment(instaAccount, media, message){
		return Api(true,instaAccount).post(Calling["messaging_create_comment"]+media, message)
	}
}