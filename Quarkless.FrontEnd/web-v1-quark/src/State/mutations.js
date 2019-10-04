/* eslint-disable no-unused-vars */
import moment from 'moment';
import {ReadFile} from '../helpers';

export default {
	success_thread(state,thread){
	},
	failed_thread(state){
		
	},
    auth_request(state){
      state.status = 'loading'
    },
    auth_success(state, data){
      state.status = 'success'
      state.token = data.Token
      state.user = data.User
      state.role = data.Role
    },
    auth_error(state){
      state.status = 'error'
      state.token = ''
      state.user = ''
      state.role = ''
    },
    auth_refresh_error(state){
      state.token = ''
      state.status = 'error'
    },
    logout(state){
      state.status = ''
      state.token = ''
      state.user = ''
      state.role = ''
    },
    ADD_ACCOUNT(state, value) {
      state.accounts.push(value);
    },
    account_details_retrieved(state, value){
      state.AccountData.InstagramAccounts = value;
    },
    failed_to_retrive_account_details(state){
      state.AccountData = null;
    },
    failed_to_update_agent_state(){

    },
    updated_agent_state(){

    },
    failed_to_refresh_state(){

    },
    refreshed_state(){},
    retrieved_timeline_data_for_user(state, data){
      state.AccountData.TimelineData = [];
      for(var i = 0; i < data.length; i++){
        var item = data[i];
        var moment_enqueued = moment(item.enqueueTime);
        var enqueueTime = new moment(moment_enqueued.format('YYYY-MM-DD HH:mm:ss'));
        if(enqueueTime===undefined){continue;}
        state.AccountData.TimelineData.push({
          id: item.itemId,
          startTime: enqueueTime,
          endTime: enqueueTime.add(30,'minutes'),
          actionObject:{
            actionName:item.actionName.split('_')[0],
            actionType:item.actionName.split('_')[1],
            body:item.body,
            targetId:item.targetId
          }
        })
      }
    },
    failed_to_retrieve_timeline_data_for_user(){
    },
    failed_to_delete_event(){},
    event_deleted(state, eventId){
      state.AccountData.TimelineData = state.AccountData.TimelineData.filter((obj)=> obj.id !== eventId);
    },
    event_enqueued(state, eventId){
      state.AccountData.TimelineData = state.AccountData.TimelineData.filter((obj)=> obj.id !== eventId);
    },
    failed_to_enqueue_event(){},
    updated_event(state, {event, newid}){
      /*
        caption: this.caption,
        time: this.time,
        hashtags: this.hashtags,
        location: this.location,
        credit: this.credit,
        type: this.type 

        const caption = bodyResp.MediaInfo.Caption;
        const hashtags = bodyResp.MediaInfo.Hashtags;
        const credit = bodyResp.MediaInfo.Credit;
        const location = bodyResp.Location;

      */
      const index = state.AccountData.TimelineData.findIndex((obj=>obj.id == event.id));
      var tojsonObject = JSON.parse(state.AccountData.TimelineData[index].actionObject.body);
      state.AccountData.TimelineData[index].id = newid;
      tojsonObject.MediaInfo.Caption = event.caption;
      tojsonObject.MediaInfo.Hashtags = event.hashtags;
      tojsonObject.Location = event.location;
      tojsonObject.MediaInfo.Credit = event.credit;
      state.AccountData.TimelineData[index].actionObject.body = JSON.stringify(tojsonObject);
},
    failed_to_update_event(){},
    profiles_retrieved(state, profiles){
      state.AccountData.Profiles = profiles;
    },
    failed_to_retieve_profiles(state){
      state.AccountData.Profiles = []
    },
    profile_config_retrieved(state, data){
      state.AccountData.ProfileConfg = data;
    },
    failed_to_retrieve_profile_config(state){
      state.AccountData.ProfileConfg = {}
    },
    profile_uploaded_files(state, data){
      //todo: make sure that the profile data files are upto date
    },
    profile_updates(state, data){
    },
    failed_profile_update(state, profileData){

    },
    set_saved_medias(state, data){
      if(data.request !== undefined || data.request !== null){
        var index = 0;
        for(index; index < data.request.length; index++)
        {
          state.AccountData.Library.MediaLibrary.push(data.request[index])
        }
      }
    },
    failed_to_set_saved_medias(state, data){

    },
    get_saved_medias(state, data){
      state.AccountData.Library.MediaLibrary = data.response.data.data;
    },
    get_saved_medias_for_user(state, data){
      if(data.response.data.data !== undefined || data.response.data.data !== null){
        var index = 0;
        for(index; index < data.response.data.data.length; index++){
          //state.AccountData.Library.push()
        }
      }
    },
    failed_to_get_saved_medias(state){
      state.AccountData.Library = null;
    },
    delete_saved_medias(state, data){
      let position = -1;
      state.AccountData.Library.MediaLibrary.forEach((item,index) => 
      {
        if(item._id === data.request._id)
          position = index;
      });
      state.AccountData.Library.MediaLibrary.splice(position,1);
    },
    failed_to_delete_saved_medias(state){
      state.AccountData.Library = null;
    },
    create_post(state,event){},
    failed_to_create_post(state){},
    retrieved_event_logs(state, logs){
      state.AccountData.TimelineLogData = logs;
    },
    failed_to_retrieve_event_logs(state)
    {
      state.AccountData.TimelineLogData = []
    },
    async update_profile_picture(state, data){
      await ReadFile(data.image.requestData[0]).then(res=>{
        state.AccountData.InstagramAccounts.filter((item)=>{
          if(item.id == data.instagramAccountId)
            item.profilePicture = res
        })
      })
    },
    update_biography(state, data){
      state.AccountData.InstagramAccounts.filter((item)=>{
        if(item.id == data.instagramAccountId)
          item.userBiography = data.request.biography
      })
    },
    set_saved_hashtags(state, data){
      if(data.request !== undefined || data.request !== null){
        state.AccountData.Library.HashtagLibrary.push(data.request)      
      }
    },
    failed_to_set_saved_hashtags(state, data){

    },
    set_saved_caption(state, data){
      if(data.request !== undefined || data.request !== null){
        state.AccountData.Library.CaptionLibrary.push(data.request)      
      }
    },
    failed_to_set_saved_caption(state, data){

    },
    get_saved_caption(state, data){
      state.AccountData.Library.CaptionLibrary = data.response.data.data;
    },
    get_saved_hashtags(state, data){
      state.AccountData.Library.HashtagLibrary = data.response.data.data;
    },
    failed_to_get_saved_hashtags(state){
      state.AccountData.Library.HashtagLibrary = [];
    },
    failed_to_get_saved_caption(state){
      state.AccountData.Library.CaptionLibrary = [];
    },
    delete_saved_caption(state, data){
      let position = -1;
      state.AccountData.Library.CaptionLibrary.forEach((item,index)=>{
        if(item._id == data.request._id){
          position = index;
        }
      });
      state.AccountData.Library.CaptionLibrary.splice(position,1);
    },
    delete_saved_hashtags(state, data){
      let position = -1;
      state.AccountData.Library.HashtagLibrary.forEach((item,index)=>{
        if(item._id == data.request._id){
          position = index;
        }
      });
      state.AccountData.Library.HashtagLibrary.splice(position,1);
    },
    delete_saved_message(state, data){
      let position = -1;
      state.AccountData.Library.MessagesLibrary.forEach((item,index)=>{
        if(item._id == data.request._id){
          position = index;
        }
      });
      state.AccountData.Library.MessagesLibrary.splice(position,1);
    },
    get_saved_messages(state, data){
      state.AccountData.Library.MessagesLibrary = data.response.data.data;
    },
    failed_to_get_saved_message(state){
      state.AccountData.Library.MessagesLibrary = [];
    },
    set_saved_message(state, data){
      if(data.request !== undefined || data.request !== null){
        state.AccountData.Library.MessagesLibrary.push(data.request)      
      }
    }
  }