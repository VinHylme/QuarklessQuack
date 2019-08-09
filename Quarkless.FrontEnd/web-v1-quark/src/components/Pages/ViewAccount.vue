<template>
<div class="timeline_layout">
  <div class="columns is-mobile is-gapless">
    <div class="column is-4">
      <div class="media_container">   
      </div>
    </div>
    <div class="column is-8">
      <div class="timeline_container">
        <vue-scheduler :events="this.$store.getters.UserTimeline" :event-display="eventDisplay"/>
      </div>
    </div>
  </div>
  <b-notification style="background-color:#141414;" :closable="false">
      <b-loading :is-full-page="isFullPage" :active.sync="isLoading" :can-cancel="false"></b-loading>
  </b-notification>
</div>  
</template>

<script>
import moment from 'moment';
export default {
  data(){
      return {
        IsAdmin:false,
        TimelineData:[],
        timer:'',
        isLoading: false,
        isFullPage: true
      }
  },
  created(){
      this.isLoading = true
      this.$store.dispatch('GetUsersTimeline',this.$route.params.id).then(res=> { this.isLoading = false });
      this.TimelineData = this.$store.getters.UserTimeline;
      this.timer = setInterval(this.loadData, 15000)
  },
  mounted(){
      this.IsAdmin = this.$store.getters.UserRole == 'Admin';
  },
  methods: {
    loadData(){
      this.$store.dispatch('GetUsersTimeline',this.$route.params.id).then(res=> 
      { 
        if(res){
          this.TimelineData = this.$store.getters.UserTimeline;
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
  height:100%;
  background-color: #222;
  ::-webkit-scrollbar {
      display: none;
  }
}
html,body{
 overflow: auto;
  ::-webkit-scrollbar {
      display: none;
  }
}
</style>
