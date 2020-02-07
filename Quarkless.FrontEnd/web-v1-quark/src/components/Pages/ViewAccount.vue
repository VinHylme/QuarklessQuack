<template>
<div class="timeline_layout">
  <div class="columns is-mobile is-gapless">
    <div :class="$store.state.showingLogs ?'column is-4':'column is-0'" class="activity-log-container">
        <p class="title">Activity</p>
      <div v-for="(timelineLog,index) in timelineLogs" :key="timelineLog+'_'+index" class="card-acticity-log">
        <div class="card-activity-header">
          <b-icon v-if="timelineLog.actionType === 8" icon="user-plus" pack="fas" type="is-success" size="is-default"/>
          <b-icon v-if="timelineLog.actionType === 9" icon="user-minus" pack="fas" type="is-warning" size="is-default"/>
          <b-icon v-if="timelineLog.actionType === 10" icon="heart" pack="fas" type="is-danger" size="is-default"/>
          <b-icon v-if="timelineLog.actionType === 12" icon="thumbs-up" pack="fas" type="is-info" size="is-default"/>
          <b-icon v-if="timelineLog.actionType === 1" icon="camera" pack="fas" type="is-twitter" size="is-default"/>
          <b-icon v-if="timelineLog.actionType === 3" icon="comment" pack="fas" class="is-purple" size="is-default"/>
          <b-icon v-if="timelineLog.actionType === 5" icon="book" pack="fas" class="is-gold" size="is-default"></b-icon>
          <b-icon v-if="timelineLog.actionType === 20" icon="inbox" pack="fas" class="is-success" size="is-default"></b-icon>
          <b-icon v-if="timelineLog.actionType === 21" icon="inbox" pack="fas" class="is-success" size="is-default"></b-icon>
          <b-icon v-if="timelineLog.actionType === 22" icon="inbox" pack="fas" class="is-success" size="is-default"></b-icon>
          <b-icon v-if="timelineLog.actionType === 23" icon="inbox" pack="fas" class="is-success" size="is-default"></b-icon>
          <b-icon v-if="timelineLog.actionType === 24" icon="inbox" pack="fas" class="is-success" size="is-default"></b-icon>
          <b-icon v-if="timelineLog.actionType === 25" icon="inbox" pack="fas" class="is-success" size="is-default"></b-icon>
        </div>
        <div class="card-activity-content">
          {{timelineLog.message}}
        </div>
        <div class="card-activity-footer">
          <p>{{formatDate(timelineLog.dateAdded)}}</p>
        </div>
      </div>
    </div>
    <div :class="$store.state.showingLogs ?'column is-8':'column is-12'">
      <div class="timeline_container">
        <vue-scheduler @CreatePost="OnCreatePost" :events="this.$store.getters.UserTimeline" :event-display="eventDisplay"/>
        <div id="scheduler" class="overlay_timeline">
          <p class="subtitle is-5">Drop your media to schedule your post</p>     
        </div>
      </div>
    </div>
  </div>
  <b-notification style="background-color:transparent; width:0; height:0;" :closable="false">
      <b-loading :is-full-page="isFullPage" :active.sync="isLoading" :can-cancel="false"></b-loading>
  </b-notification>
</div>  
</template>

<script>
import Vue from 'vue';
import moment from 'moment';
export default {
  data(){
    return {
      IsAdmin:false,
      timelineLogs:[],
      timer:'',
      timerLog:'',
      isLoading: false,
      isLoadingLogs: false,
      isFullPage: true,
      activeTab:0,
      profile:{},
      uploadMethodData:{
        urls:[],
        searchMediaItems:[],
        isLoading:false,
        currentPage:1,
        perPage:25,
        originalSet:[]
      },
      prePostData:[]
    }
  },
  created(){
     
  },
  beforeMount(){
      this.IsAdmin = this.$store.getters.UserRole == 'Admin';
      this.profile = this.$store.getters.UserProfile;
      this.$emit('selectedAccount',this.$route.params.id);
      this.isLoading = true;   
      this.isLoadingLogs = true;
      this.$store.dispatch('GetUsersTimeline',this.$route.params.id).then(res=> 
      { 
        this.isLoading = false;
      }).catch(err=>{
          this.isLoading = false;
      });
      this.$store.dispatch('GetAllEventLogsForUser', {instagramAccountId: this.$route.params.id, limit:100}).then(resp=>{
          this.timelineLogs = this.$store.getters.UserTimelineLogForUser(this.$route.params.id);
          this.isLoadingLogs = false;
        }).catch(err=>{
          this.isLoadingLogs = false;
      });
      this.timer = setInterval(this.loadData, 10000);
	  this.timerLog = setInterval(this.loadLogs, 5000);
  },
  mounted(){

  },
  computed:{
  },
  methods: {
    formatDate(date) {
     return moment(date).format('llll');
  },
    OnCreatePost(event){
	  this.isLoading = true;
      this.$store.dispatch('CreatePost', event).then(resp=>{
		  this.isLoading = false;
         Vue.prototype.$toast.open({
            message: 'Post Successfuly Scheduled',
            type: 'is-success'
		  })
      }).catch(err=>{
		  this.isLoading = false;
        Vue.prototype.$toast.open({
            message: 'Oops looks like something went wrong: ' + err.message,
            type: 'is-danger'
          })
      })
    },
    handlePost(evt){
      this.$emit('failedPost', { context: evt, response: true } );
    },
    loadData(){
      this.$store.dispatch('GetUsersTimeline',this.$route.params.id).then(res=> 
      { 
        if(res){
          this.TimelineData = this.$store.getters.UserTimeline;
        }
      });
    },
    loadLogs(){
      this.isLoadingLogs = true;
      this.$store.dispatch('GetAllEventLogsForUser', {instagramAccountId: this.$route.params.id, limit:100}).then(resp=>{
          this.timelineLogs = this.$store.getters.UserTimelineLogForUser(this.$route.params.id);
          this.isLoadingLogs = false;
        }).catch(err=>{
          this.isLoadingLogs = false;
      });
    },
    eventDisplay(event) {
		if(event.actionObject.actionType === "Image"){
			return JSON.parse(event.actionObject.body).Image.Uri;
		}
		else if(event.actionObject.actionType === "Video"){
			return JSON.parse(event.actionObject.body).Video.VideoThumbnail.Uri;
		}
		else if(event.actionObject.actionType === "Carousel"){
			let carousel = JSON.parse(event.actionObject.body).Album[0];
			if(carousel.ImageToUpload === null && carousel.ImageToUpload === undefined)
				return carousel.VideoToUpload.VideoThumbnail.Uri;
			else 
				return carousel.ImageToUpload.Uri
		}
		else{
	  		return  event.actionObject.actionName;
		}
    }
  },
  beforeDestroy() {
    clearInterval(this.timer)
  }
}
</script>

<style lang="scss">
@import "../../../references/calendar-scheduler/scss/variables";
@import url('https://fonts.googleapis.com/css?family=Roboto:300&display=swap');

//  Set your variables
$v-cal-event-bg: salmon; //  Event background color
$v-cal-event-border-radius: 1px; //  Events border radius (all 4 corners)
$v-cal-body-bg:#242424;
$v-cal-font-color	: $white;
$v-cal-title-font-size	:22px;
$v-cal-body-shadow	: 0 2px 6px rgba(0,0,0,.04);
$v-cal-content-border-color	: #242424;
$v-cal-days-head-bg	:#242424;
$v-cal-days-head-border-color	:#242424;
$v-cal-days-head-color	:$white;
$v-cal-days-head-font-size	:.9rem;
$v-cal-days-head-font-weight	:bold;
$v-cal-day-bg	: #242424;
$v-cal-day-border-color	:#242424;
$v-cal-day-diff-month-color	:rgba(67, 66, 93, .3);
$v-cal-day-today-bg	: transparent;
$v-cal-day-disabled-bg	:#242424;
$v-cal-day-disabled-color	:#b0b0b0;

$v-cal-time-block-bg	: transparent;
$v-cal-time-block-hover-bg	:rgba(64, 217, 255, 0.051);

$v-cal-times-bg	: #242424;
$v-cal-times-font-size	:0.88rem;
$v-cal-times-border-color	:#242424;

$v-cal-event-bg	:#13b94d;
$v-cal-event-color	: $white;
$v-cal-event-border-radius	: 0px;

$v-cal-button-bg	: #ddd;
$v-cal-button-shadow	:0 2px 3px rgba(0,0,0,.05);
$v-cal-button-padding	: 8px 18px;
$v-cal-button-border-color	:#242424;
$v-cal-button-border-radius	:5px;
$v-cal-button-active-bg	:#13b94d;
$v-cal-button-active-color	:$white;
$v-cal-button-hover-bg	:#fcfcfc;
$v-cal-button-hover-color	:#13b94d;
$v-cal-button-disabled-bg	:#f0f0f0;
$v-cal-button-disabled-color	:#d0d0d0;
$v-cal-button-disabled-cursor	:not-allowed;

@import "../../../references/calendar-scheduler/scss/main";
.timeline_container{
  overflow: auto;
  color: white;
  width:100% !important;
  height:100% !important;
  background-color: #242424;
  ::-webkit-scrollbar {
     display: none;
  }
}
.activity-log-container {
	background:#1f1f1f; 
	color:white; 
	display:inline-block;
	.title{
		margin-left:2em; 
		margin-top:2em; 
		color:#d9d9d9;
		text-align:center;
	}
}
html,body{
 overflow: auto;
  ::-webkit-scrollbar {
      display: none;
  }
}
.dropStyle{
  margin-left:0em;
}
.card-acticity-log{
  background:#292929;
  padding:1em;
  margin:1em;
  margin-left:5em;
  height:125px;
  box-shadow: -0.1rem 0 .3rem rgba(0,0,0,.2);
  border-radius: .7em;
  .card-activity-header{
    float:left;
    margin:.5em;
  }
  .card-activity-footer{
    float:right;
    margin-top:1em;
    font-size: 14px;
    color:#b0b0b0;
  }
  .card-activity-content{
    margin-left:4em;
  }
}
.uploadSearchResults
{
  width:100%;  
  height:300px;
  display: flex;
  margin:0 auto;
  flex-flow: row wrap;
  align-items: center;
  background:transparent;
  margin-left:0.5em;
}
.isSelected{
  img{
    border: 4px solid rgb(0, 195, 255) !important;
  }
}
.selected{
   img{
    border: 4px solid rgb(0, 195, 255) !important;
  }
}
.isDragOn{
  img{
    border: 4px solid #13b94d !important;
  }
}
.is-purple{
  color:#ff74dc;
}
.is-gold{
  color:gold;
}
.image_container{
  width:120px;
  height: 120px;
  position: relative;
  transition: all .2s ease-in-out;
  &:hover{
    display: block;
      &.zoomable{
        transform: scale(1.1);
        border-radius:0.25em;
        cursor: pointer;
    }
    .title {
      top: 90px;
    }
    .is-overlayed{
      opacity: 1;
    }
  }
  img{
    margin:0 auto;
  }
  margin:0.4em;
}
.overlayTick{
  display: none;
  position:absolute;
  top:1.3em;
  left:1.3em;
}
.loading-overlay .loading-background{
  background: transparent;
}
#scheduler{
  display: none;
}
.overlay_timeline{
  border-radius: .4em;
  position: absolute;
  top:18%;
  right:10%;
  width:45%;
  height:15%;
  background:rgba(228, 228, 228, 0.2);
  border: 2px dashed rgb(0, 195, 255);
  .subtitle{
    //font-size: 17px;
    font-family: 'Roboto', sans-serif !important;
    font-style: 'light' !important;
    text-align: center;
    font-weight: normal;
    color:rgb(0, 195, 255);
    opacity: 0.9;
    margin-top:2.5em;
  }
}
</style>
