/* eslint-disable no-unused-vars */
import AccountServices from '../Services/accountServices';
import TimelineServices from '../Services/timelineService';
import NotificationServices from '../Services/notificationServices';
import QueryServices from '../Services/queryServices';
import LibraryServices from '../Services/libraryServices';
import MessagingServices from '../Services/messagingServices';
import {GetUserDetails} from '../localHelpers'
import Axios from 'axios';
import decoder from 'jwt-decode';

export default {
  UpdateUserProxy({commit}, proxyData){
    return new Promise((resolve,reject)=>
    {
      AccountServices.UpdateUserProxy(proxyData).then(resp=>{
        resolve(resp)
      }).catch(err=>{
        reject(err)
      })
    })
  },
  ReAssignUserProxy({commit}, proxyData){
    return new Promise((resolve,reject)=>
    {
      AccountServices.ReAssignUserProxy(proxyData).then(resp=>{
        resolve(resp)
      }).catch(err=>{
        reject(err)
      })
    })
  },
  TestProxyConnectivity({commit}, proxyData){
    return new Promise((resolve,reject)=>
    {
      AccountServices.TestProxyConnectivity(proxyData).then(resp=>{
        resolve(resp)
      }).catch(err=>{
        reject(err)
      })
    })
  },
  GetUserProxy({commit}, data){
    return new Promise((resolve,reject)=>{
      AccountServices.GetUserProxy(data.instagramAccountId).then(resp=>{
        resolve(resp)
      }).catch(err=>reject(err))
    })
  },
  SaveProfileStepSection({commit}, data){
    localStorage.setItem('profile-active-step-'+ data.profile, data.step)
  },
	DeleteComment({commit}, data){
		return new Promise((resolve, reject) =>{
			MessagingServices.DeleteComment(data.instagramAccountId, data.media, data.comment).then(resp=>{
				resolve(resp)
			}).catch(err=>reject(err))
		})
	},
	LikeComment({commit}, data){
		return new Promise((resolve, reject) =>{
			MessagingServices.LikeComment(data.instagramAccountId, data.comment).then(resp=>{
				resolve(resp)
			}).catch(err=>reject(err))
		})
	},
	UnLikeComment({commit}, data){
		return new Promise((resolve, reject) =>{
			MessagingServices.UnLikeComment(data.instagramAccountId, data.comment).then(resp=>{
				resolve(resp)
			}).catch(err=>reject(err))
		})
	},
	ReplyComment({commit}, data){
		return new Promise((resolve, reject) =>{
			MessagingServices.ReplyComment(data.instagramAccountId, data.media, data.comment, data.message).then(resp=>{
				resolve(resp)
			}).catch(err=>reject(err))
		})
	},
	CreateComment({commit}, data){
		return new Promise((resolve, reject) =>{
			MessagingServices.CreateComment(data.instagramAccountId, data.media, data.message).then(resp=>{
				resolve(resp)
			}).catch(err=>reject(err))
		})
	},
	GetRecentComments({commit},data){
		return new Promise((resolve, reject)=>{
			QueryServices.GetRecentComments(data.instagramAccountId, data.topic).then(resp=>{
				resolve(resp);
			}).catch((err)=>{
				reject(err);
			})
		})
	},
	DMMessage({commit}, data){
		return new Promise((resolve, reject)=>{
			MessagingServices.SendDM(data.id, data.type, data.message).then(resp=>{
				resolve(resp)
			}).catch(err=>reject(err))
		})
	},
	GetThread({commit}, data){
		return new Promise((resolve, reject)=>{
			MessagingServices.GetThread(data.instagramAccountId, data.threadId, data.limit).then(resp=>{
				commit('success_thread', resp.data)
				resolve(resp);
			}).catch((err)=>{
				commit('failed_thread')
				reject(err);
			})
		})
	},
  SearchByTopic({commit}, data){
      return new Promise((resolve, reject)=>{
        QueryServices.SearchByTopic(data.query, data.instagramAccountId, data.limit).then(resp=>{
          resolve(resp);
        }).catch((err)=>{
          reject(err);
        })
      })
    },
    SearchByLocation({commit}, data){
      return new Promise((resolve, reject)=>{
        QueryServices.SearchByLocation(data.query, data.instagramAccountId, data.limit).then(resp=>{
          resolve(resp);
        }).catch((err)=>{
          reject(err);
        })
      })
    },
    GetUserMedias({commit}, data){
      return new Promise((resolve, reject)=>{
        QueryServices.GetUserMedias(data.instagramAccountId, data.topic).then(resp=>{
          resolve(resp);
        }).catch((err)=>{
          reject(err);
        })
      })
	},
	GetUserInbox({commit}, data){
		return new Promise((resolve, reject)=>{
			QueryServices.GetUserInbox(data.instagramAccountId, data.topic).then(resp=>{
				resolve(resp);
			}).catch((err)=>{
				reject(err);
			})
		})
	},
    GetUserFeed({commit}, data){
      return new Promise((resolve, reject)=>{
        QueryServices.GetUserFeed(data.instagramAccountId, data.topic).then(resp=>{
          resolve(resp);
        }).catch((err)=>{
          reject(err);
        })
      })
    },
    GetMediasByLocation({commit}, data){
      return new Promise((resolve, reject)=>{
        QueryServices.GetMediasByLocation(data.instagramAccountId, data.topic).then(resp=>{
          resolve(resp);
        }).catch((err)=>{
          reject(err);
        })
      })
    },
    GetUserFollowerList({commit}, data){
      return new Promise((resolve, reject)=>{
        QueryServices.GetUserFollowerList(data.instagramAccountId, data.topic).then(resp=>{
          resolve(resp);
        }).catch((err)=>{
          reject(err);
        })
      })
    },
    GetUserFollowingList({commit}, data){
      return new Promise((resolve, reject)=>{
        QueryServices.GetUserFollowingList(data.instagramAccountId, data.topic).then(resp=>{
          resolve(resp);
        }).catch((err)=>{
          reject(err);
        })
      })
    },
    GetUserFollowingSuggestionList({commit}, data){
      return new Promise((resolve, reject)=>{
        QueryServices.GetUserFollowingSuggestions(data.instagramAccountId, data.topic).then(resp=>{
          resolve(resp);
        }).catch((err)=>{
          reject(err);
        })
      })
    },
    GetUserTargetLocation({commit}, data){
      return new Promise((resolve, reject)=>{
        QueryServices.GetUserTargetLocation(data.instagramAccountId, data.topic).then(resp=>{
          resolve(resp);
        }).catch((err)=>{
          reject(err);
        })
      })
    },
    GetUsersTargetList({commit}, data){
      return new Promise((resolve, reject)=>{
        QueryServices.GetUsersTargetList(data.instagramAccountId, data.topic).then(resp=>{
          resolve(resp);
        }).catch((err)=>{
          reject(err);
        })
      })
    },
    ReleatedTopicByParent({commit}, parentId)
    {
      return new Promise((resolve, reject)=>{
        QueryServices.ReleatedTopicByParent(parentId).then(resp=>{
          resolve(resp);
        }).catch((err)=>{
          reject(err);
        })
      })
    },
    GetEventLogs({commit}, data){
      return new Promise((resolve, reject)=>{
        NotificationServices.GetEventLogs(data.instagramAccountId, data.limit).then(resp=>{
          commit('retrieved_event_logs_in',resp.data);
          resolve(resp);
        }).catch((err)=>{
          commit('failed_to_retrieve_event_logs_in',err);
          reject(err);
        })
      })
    },
    GetAllEventLogsForUser({commit}, data){
      return new Promise((resolve, reject)=>{
        NotificationServices.GetAllEventLogsForUser(data.instagramAccountId, data.limit).then(resp=>{
          commit('retrieved_event_logs',resp.data);
          resolve(resp);
        }).catch((err)=>{
          commit('failed_to_retrieve_event_logs');
          reject(err);
        })
      })
    },
    CreatePost({commit}, data){
      return new Promise((resolve, reject)=>{
        TimelineServices.CreatePost(data.id, data.event).then(resp=>{
          commit('create_post', data);
          resolve(resp);
        }).catch((err)=>{
          commit('failed_to_create_post', err);
          reject(err);
        })
      })
    },
    CreateMessage({commit}, data){
      return new Promise((resolve, reject)=>{
        TimelineServices.CreateMessage(data.type, data.id, data.messages).then(resp=>{
          resolve(resp);
        }).catch(err=>{
          reject(err);
        })
      })
    },
    SetSavedMedias({commit}, medias){
      return new Promise((resolve, reject)=>{
        LibraryServices.SetSavedMedias(medias).then(resp=>{
          commit('set_saved_medias', {request: medias, response: resp});
          resolve(resp);
        }).catch((err)=>{
          commit('failed_to_set_saved_medias', { request: medias, error: err })
          reject(err);
        })
      })
    }, 
    //#region SavedCaptions
    SetSavedCaption({commit}, caption){
      return new Promise((resolve, reject)=>{
        LibraryServices.SetSavedCaptions(caption).then(resp=>{
          commit('set_saved_caption', {request: caption, response: resp});
          resolve(resp);
        }).catch((err)=>{
          commit('failed_to_set_saved_caption', { request: caption, error: err })
          reject(err);
        })
      })
    },
    UpdateSavedCaption({commit}, caption){
      return new Promise((resolve, reject)=>{
        LibraryServices.UpdateSavedCaption(caption).then(resp=>{
          //commit('update_saved_caption', {request: caption, response: resp});
          resolve(resp);
        }).catch((err)=>{
          //commit('failed_to_update_saved_caption', { request: caption, error: err })
          reject(err);
        })
      })
    },
    //#endregion
    SetSavedHashtags({commit}, hashtags){
      return new Promise((resolve, reject)=>{
        LibraryServices.SetSavedHashtags(hashtags).then(resp=>{
          commit('set_saved_hashtags', {request: hashtags, response: resp});
          resolve(resp);
        }).catch((err)=>{
          commit('failed_to_set_saved_hashtags', { request: hashtags, error: err })
          reject(err);
        })
      })
    },
    UpdateSavedHashtags({commit}, hashtags){
      return new Promise((resolve, reject)=>{
        LibraryServices.UpdateSavedHashtags(hashtags).then(resp=>{
          //commit('update_saved_caption', {request: caption, response: resp});
          resolve(resp);
        }).catch((err)=>{
          //commit('failed_to_update_saved_caption', { request: caption, error: err })
          reject(err);
        })
      })
    },
    DeleteSavedHashtags({commit}, hashtags){
      return new Promise((resolve, reject)=>{
        LibraryServices.DeleteSavedHashtags(hashtags).then(resp=>{
          commit('delete_saved_hashtags', {request: hashtags, response: resp});
          resolve(resp);
        }).catch((err)=>{
          //commit('failed_to_update_saved_caption', { request: caption, error: err })
          reject(err);
        })
      })
    },
    SetSavedMessages({commit}, message){
      return new Promise((resolve, reject)=>{
        LibraryServices.SetSavedMessages(message).then(resp=>{
          commit('set_saved_message', {request: message, response: resp});
          resolve(resp);
        }).catch((err)=>{
          //commit('failed_to_set_saved_messages', { request: message, error: err })
          reject(err);
        })
      })
    },
    UpdateSavedMessages({commit}, message){
      return new Promise((resolve, reject)=>{
        LibraryServices.UpdateSavedMessages(message).then(resp=>{
          //commit('update_saved_caption', {request: caption, response: resp});
          resolve(resp);
        }).catch((err)=>{
          //commit('failed_to_update_saved_caption', { request: caption, error: err })
          reject(err);
        })
      })
    },
    DeleteSavedMessages({commit}, message){
      return new Promise((resolve, reject)=>{
        LibraryServices.DeleteSavedMessages(message).then(resp=>{
          commit('delete_saved_message', {request: message, response: resp});
          resolve(resp);
        }).catch((err)=>{
          //commit('failed_to_update_saved_caption', { request: caption, error: err })
          reject(err);
        })
      })
    },
    GetSavedMessages({commit}, accountId){
      return new Promise((resolve, reject)=>{
        LibraryServices.GetSavedMessages(accountId).then(resp=>{
          commit('get_saved_messages', {request: accountId, response: resp});
          resolve(resp);
        }).catch((err)=>{
          commit('failed_to_get_saved_message')
          reject(err);
        })
      })
    },
    GetSavedHashtags({commit}, accountId){
      return new Promise((resolve, reject)=>{
        LibraryServices.GetSavedHashtags(accountId).then(resp=>{
          commit('get_saved_hashtags', {request: accountId, response: resp});
          resolve(resp);
        }).catch((err)=>{
          commit('failed_to_get_saved_hashtags')
          reject(err);
        })
      })
    },
    DeleteSavedCaption({commit}, caption){
      return new Promise((resolve, reject)=>{
        LibraryServices.DeleteSavedCaptions(caption).then(resp=>{
          commit('delete_saved_caption', {request: caption, response: resp});
          resolve(resp);
        }).catch((err)=>{
          //commit('failed_to_update_saved_caption', { request: caption, error: err })
          reject(err);
        })
      })
    },
    GetSavedMedias({commit}, accountId){
      return new Promise((resolve, reject)=>{
        LibraryServices.GetSavedMedias(accountId).then(resp=>{
          commit('get_saved_medias', {request: accountId, response: resp});
          resolve(resp);
        }).catch((err)=>{
          commit('failed_to_get_saved_medias')
          reject(err);
        })
      })
    },
    GetSavedCaption({commit}, accountId){
      return new Promise((resolve, reject)=>{
        LibraryServices.GetSavedCaptions(accountId).then(resp=>{
          commit('get_saved_caption', {request: accountId, response: resp});
          resolve(resp);
        }).catch((err)=>{
          commit('failed_to_get_saved_caption')
          reject(err);
        })
      })
    },
    GetSavedMediasForUser({commit}, instagramAccountId){
      return new Promise((resolve, reject)=>{
        LibraryServices.GetSavedMediasForUser(instagramAccountId).then(resp=>{
          commit('get_saved_medias_for_user', {request: instagramAccountId, response: resp});
          resolve(resp);
        }).catch((err)=>{
          commit('failed_to_get_saved_medias_for_user')
          reject(err);
        })
      })
    },
    DeleteSavedMedia({commit}, media){
      return new Promise((resolve, reject)=>{
        LibraryServices.DeleteSavedMedias(media).then(resp=>{
          commit('delete_saved_medias', {request: media, response: resp});
          resolve(resp);
        }).catch((err)=>{
          commit('failed_to_delete_saved_medias')
          reject(err);
        })
      })
    },
    BuildTags({commit}, data){
      return new Promise((resolve, reject)=>{
        QueryServices.BuildTags(data).then(resp=>{
          resolve(resp);
        }).catch((err)=>reject(err));
      })
    },
    ReleatedTopics({commit}, data)
    {
      return new Promise((resolve,reject)=>{
        QueryServices.ReleatedTopic(data.instaId, data.topic).then(resp=>{
          resolve(resp);
        }).catch((err)=>reject(err))
      })
    },
    UploadFileForProfile({commit}, data){
      return new Promise((resolve, reject)=>{
        AccountServices.UploadFile(data.instaId, data.profileId, data.formData).then(resp=>{
          commit('profile_uploaded_files',data);
          resolve(resp);
        }).catch((err)=>reject(err))
      })
    },
    ChangeBiography({commit}, data){
      return new Promise((resolve, reject)=>{
        AccountServices.ChangeBiography(data.instagramAccountId, data.biography).then(resp=>{
          commit('update_biography', {request: data, response: resp.data})
          resolve(resp);
        }).catch((err)=>reject(err));
      });
    },
    ChangeProfilePicture({commit}, data){
      return new Promise((resolve,reject)=>{
        AccountServices.ChangeProfilePicture(data.instagramAccountId, data.image).then(resp=>{
          commit('update_profile_picture', data);
          resolve(resp);
        }).catch((err)=> reject(err))
      })
    },
    SimilarSearch({commit},data){
      return new Promise((resolve,reject)=>{
        QueryServices.SimilarImageSearch(data.urls,data.limit, data.offset, data.moreAccurate).then(resp=>{
          resolve(resp);
        }).catch((err)=>reject(err));
      })
    },
    GooglePlacesSearch({commit},query){
      return new Promise((resolve,reject)=>{
        QueryServices.GooglePlaceSearch(query).then(resp=>{
          resolve(resp);
        }).catch(err=>{
          reject(err);
        })
      })
    },
    GooglePlacesAutoCompleteSearch({commit},queryObject){
      return new Promise((resolve, reject)=>{
        QueryServices.GooglePlacesAutocomplete(queryObject.query, queryObject.radius).then(resp=>{
          resolve(resp);
        }).catch(err=>reject(err));
      })
    },
    GetProfileConfig({commit}){
      return new Promise((resolve,reject)=>{
        QueryServices.GetProfileConfig().then(resp=>{
          commit('profile_config_retrieved', resp.data);
          resolve(resp);
        }).catch(err=>{
          commit('failed_to_retrieve_profile_config');
          reject(err);
        })
      })
    },
    AddProfileTopics({commit}, addProfileTopicRequest){
      return new Promise((resolve, reject)=>{
        AccountServices.AddProfileTopics(addProfileTopicRequest).then(resp=>{
           resolve(resp);
        }).catch(err=>{
          reject(err);
        }) 
      })
    },
    UpdateProfile({commit}, profileData){
      return new Promise((resolve, reject)=>{
        AccountServices.UpdateProfile(profileData._id, profileData).then(resp=>{
          commit('profile_updates', {profileData: profileData, response:resp});
          resolve(resolve);
        }).catch(err=>{
          commit('failed_profile_update',profileData);
          reject(err);
        })
      });
    },
    GetProfiles({commit}, userId){
      return new Promise((resolve, reject)=>{
        AccountServices.GetProfilesForUser(userId).then(resp=>{
          commit('profiles_retrieved', resp.data)
          resolve(resp);
        }).catch(err=>{
          commit('failed_to_retieve_profiles')
          reject(err);
        })
      })
    },
    UpdateEvent({commit}, event){
      return new Promise((resolve, reject)=>{
        TimelineServices.UpdateEvent(event).then(resp=>{
          commit('updated_event', {event, newid: resp.data });
          resolve(resp);
        }).catch(err=>{
          console.log(err)
          commit('failed_to_update_event')
          reject(err);
        })
      })
    },
    EnqueueEventNow({commit}, eventId){
      return new Promise((resolve,reject)=>{
        TimelineServices.EnqueueNow(eventId).then(resp=>{
          commit('event_enqueued',eventId);
          resolve(resp);
        }).catch(err=>{
          commit('failed_to_enqueue_event');
          reject(err);
        })
      })
    },
    DeleteEvent({commit}, eventId){
      return new Promise((resolve,reject)=>{
        TimelineServices.DeleteEventFromTimeline(eventId).then(resp=>{
          commit('event_deleted',eventId);
          resolve(resp);
        }).catch(err=>{
          commit('failed_to_delete_event');
          reject(err);
        })
      })
    },
    GetUsersTimeline({commit},id){
      return new Promise((resolve, reject)=>{
        if(id === undefined || id === null){
          resolve('no id')
        }
        TimelineServices.GetUserTimeline(id).then(resp=>{
          commit('retrieved_timeline_data_for_user',resp.data);
          resolve(resp)
        }).catch(err=>{
          commit('failed_to_retrieve_timeline_data_for_user')
          reject(err)
        })
      })
    },
    RefreshState({commit}, id){
      return new Promise((resolve,reject)=>{
        AccountServices.RefreshState(id).then(resp=>{
          commit('refreshed_state');
          resolve(resp)
        }).catch(err=>{
          commit('failed_to_refresh_state');
          reject(err);
        })
      })
    },
    ChangeState({commit}, newstate){
      return new Promise((resolve,reject)=>{
        AccountServices.UpdateAgentState({instagramAccountId: newstate.instaId, newState: parseInt(newstate.state)}).then(resp=>{
          commit('updated_agent_state');
          resolve(resp);
        }).catch(err=>{
          commit('failed_to_update_agent_state');
          if(err.message.includes('401')){
            this.dispatch('logout');
          }
          reject(err);
        })
      })
    },
    AccountDetails({commit}, payload){
      return new Promise((resolve, reject)=>{
        AccountServices.GetInstagramAccountsForUser({accountId:payload.userId}).then(resp=>{
          commit('account_details_retrieved',resp.data);
          resolve(resp);
        })
        .catch(err=>{
          commit('failed_to_retrive_account_details');
          if (err.message.includes('401')) {
            this.dispatch('logout');
          }
          reject(err);
        })
      })
    },
    resendConfirmation({commit}, username){
      return new Promise((resolve, reject)=>{
        AccountServices.ResendConfirm(username).then(resp=>{
          resolve(resp);
        }).catch(err=>reject(err))
      })
    },
    refreshToken({commit}){
      return new Promise((resolve, reject)=>{
        AccountServices.RefreshToken({refreshToken: localStorage.getItem('refresh'), username: this.getters.User}).then(resp=>{
          const token = resp.data.idToken;
          //const refreshToken = resp.data.refreshToken;
          localStorage.setItem('token', token)
         // localStorage.setItem('refresh', refreshToken);
          Axios.defaults.headers.common['Authorization'] = token;
          var decoded = decoder(token);
          var role = decoded["cognito:groups"][0];
          //localStorage.setItem('user', this.getters.User)
          //localStorage.setItem('role', role)
          commit('auth_success', 
          { 
            Token: token, 
            User: { 
              Username: this.getters.User,
              Role: role
            }
          });
          decoded = null;
          resolve(resp);
        }).catch(err=>{
          commit('auth_refresh_error')
          localStorage.removeItem('token')
          this.state.UserAuthenticated = false;
          reject(err);
        })
      });
    },
    registerAccount({commit},user){
      return new Promise((resolve,reject)=>{
        AccountServices.Register(user).then(resp=>{
          resolve(resp)
        }).catch(err=>{
          
        })
      })
    },
    confirmAccount({commit}, confirmData){
      return new Promise((resolve,reject)=>{
        AccountServices.Confirm(confirmData).then(resp=>{
          resolve(resp)
        }).catch(err =>{
          reject(err)
        })
      })
    },
    async login({commit}, user){
      return new Promise((resolve, reject) => {
        commit('auth_request')
        AccountServices.Login(user).then(resp=>{
          const token = resp.data.idToken
          const refreshToken = resp.data.refreshToken;
          localStorage.setItem('token', token)
          localStorage.setItem('refresh', refreshToken);
          Axios.defaults.headers.common['Authorization'] = token;
          var decoded = decoder(token);
          var role = decoded["cognito:groups"][0];
          localStorage.setItem('user', user.Username)
          localStorage.setItem('role', role)
          commit('auth_success', 
          { 
            Token: token, 
            User: { 
              Username: user.Username,
              Role: role
            }
          });
          decoded = null;
          resolve(resp)
        })
        .catch(err=>{
          commit('auth_error')
          localStorage.removeItem('token')
          localStorage.removeItem('refresh')
          this.state.UserAuthenticated = false;
          reject(err)
        })
      })
    },
    logout({commit}){
      return new Promise((resolve) => {
        commit('logout')
        localStorage.removeItem('token')
        delete Axios.defaults.headers.common['Authorization']
        resolve()
      })
    },
    LinkInstagramAccount({commit}, data) {
      return new Promise((resolve,reject)=>{
        AccountServices.LinkInstagramAccount(data).then(resp=>{
          resolve(resp);
        }).catch(err=>{
          reject(err);
        })
      })
    },
    DeleteInstagramAccount({commit}, instagramAccountId){
      return new Promise((resolve,reject)=>{
        AccountServices.DeleteInstagramAccount(instagramAccountId).then(resp=>{
          commit('insta_acc_deleted', instagramAccountId)
          resolve(resp)
        }).catch(err=>{
          reject(err)
        })
      })
    },
    SubmitCodeForChallange({commit}, data){
      return new Promise((resolve,reject)=>{
        AccountServices.SubmitCodeForChallange(data.code, data.instagramAccountId).then(resp=>{
          resolve(resp)
        }).catch(err=>reject(err));
      })
    },
    SubmitPhoneForChallange({commit}, data){
      return new Promise((resolve,reject)=>{
        AccountServices.SubmitPhoneForChallange(data.phoneNumber, data.instagramAccountId).then(resp=>{
          resolve(resp)
        }).catch(err=>reject(err));
      })
    },
	CreateSession({commit}, data){
		return new Promise((resolve,reject)=>{
			AccountServices.CreateSession(data.ptype,data.curr,data.sauce,data.accid).then(resp=>{
				resolve(resp)
			}).catch(err=> reject(err))
		})
  },
  AddUserDetails({commit}, data){
    return new Promise((resolve,reject)=>{
      AccountServices.AddUserDetails(data.userId, data.userInformation).then(res=>{
        resolve(res)
      }).catch(err=>reject(err))
    })
  }
}