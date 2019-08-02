<template>
<div class="timeline_layout">
  <div class="columns is-mobile is-gapless">
    <div class="column is-3">
      <div class="media_container">   
      </div>
    </div>
    <div class="column is-9">
      <div class="timeline_container">
        <vue-scheduler :events="TimelineData" :event-display="eventDisplay"/>
      </div>
    </div>
  </div>
</div>  
</template>

<script>
import moment from 'moment';
export default {
  data(){
      return {
        IsAdmin:false,
        TimelineData:[],
        timer:''
      }
  },
  created(){
     this.timer = setInterval(this.loadData, 60000)
     this.loadData();
  },
  mounted(){
      this.IsAdmin = this.$store.getters.UserRole == 'Admin';
  },
  methods: {
    loadData(){
      this.TimelineData = [];
      this.$store.dispatch('GetUsersTimeline',this.$route.params.id).then(res=>{
       if(res.data!==undefined || res.data !==null){
          for(var i = 0; i < res.data.length; i++){
            var item = res.data[i];
            var moment_enqueued = moment(item.response.enqueueTime);
            var enqueueTime = new Date(moment_enqueued.format('YYYY-MM-DD HH:mm:ss'));
            this.TimelineData.push({
              id: item.response.itemId,
              startTime: enqueueTime.getHours()+":"+ enqueueTime.getMinutes(),
              endTime: enqueueTime.getHours() + ":" + enqueueTime.getMinutes(),
              actionObject:{
                actionName:item.response.actionName.split('_')[0],
                body:item.response.body,
                targetId:item.response.targetId
              }
            })
          }
       }
      });
    },
    eventDisplay(event) {
      return  event.actionObject.actionName;
    }
  },
  beforeDestroy() {
    clearInterval(this.timer)
  }
}
</script>

<style lang="scss">
@import "../../../references/v-calendar-scheduler/scss/variables";

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
$v-cal-time-block-hover-bg	:rgba(255, 236, 64, 0.0511);

$v-cal-times-bg	: #242424;
$v-cal-times-font-size	:0.88rem;
$v-cal-times-border-color	:#242424;

$v-cal-event-bg	:#3B86FF;
$v-cal-event-color	: $white;
$v-cal-event-border-radius	: 0px;

$v-cal-button-bg	: #ddd;
$v-cal-button-shadow	:0 2px 3px rgba(0,0,0,.05);
$v-cal-button-padding	: 8px 18px;
$v-cal-button-border-color	:#242424;
$v-cal-button-border-radius	:50px;
$v-cal-button-active-bg	:#3B86FF;
$v-cal-button-active-color	:$white;
$v-cal-button-hover-bg	:#fcfcfc;
$v-cal-button-hover-color	:#3B86FF;
$v-cal-button-disabled-bg	:#f0f0f0;
$v-cal-button-disabled-color	:#d0d0d0;
$v-cal-button-disabled-cursor	:not-allowed;
// .v-cal-hour, .v-cal-day__hour-block{
//   padding:50px !important;
// }

// .v-cal-content .v-cal-event-item{
//   text-align: center !important;
//   margin-left:10px !important;
//   width:100px!important;
//   padding:0.4em !important;
//   flex-grow: 0 !important;
// }
//  Import the rest
@import "../../../references/v-calendar-scheduler/scss/main";
.media_container{
  padding:0.4em;
  color:white;
  width:100%;
  height:100%;
  background-color:#2d2d2d;
}
.timeline_container{
  overflow: auto;
  color: white;
  width:100%;
  height:915px;
  background-color: #222;
}
html,body{
 overflow: hidden;
}
</style>
