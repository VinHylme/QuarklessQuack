<template>
  <transition class="event_view_dialog_container" name="zoom-out">
        <div class="v-cal-dialog" v-if="isActive">
          <div class="v-cal-dialog__bg" @click="cancel"></div>
              <div class="v-cal-dialog-card is-big">
                <header class="v-cal-dialog-card__header">
                  <p class="is-large">Posting at: </p>
                    <b-field label="">
                        <b-clockpicker
                            style="width:150px; padding:0 !important; margin-top:0.7em; margin-left:0.7em"
                            v-model="time"
                            rounded
                            :placeholder="'Posting at '+ time"
                            icon="clock"
                            type="is-info"
                            :hour-format="format">                 
                        </b-clockpicker>
                    </b-field>
                    <button type="button" class="v-cal-dialog__close" @click="cancel"></button>
                </header>
                <div class="columns is-mobile">
                  <div class="column is-7">
                      <div v-if="type === 1">
                        <section class="v-cal-dialog-card__body">
                          <div class="body-container-view" style="overflow:hidden; padding:0">
                            <div class="media-section-view">
                              <img class="media-container" id="image" v-bind:src="medias[0].url">
                            </div>
                          </div>
                        </section>
                      </div>
                      <div v-else-if="type === 2">
                        <section class="v-cal-dialog-card__body" style="overflow:hidden;">
                          <div class="body-container-view" style="overflow:hidden; padding:0">
                            <div class="media-section-view">
                              <video class="media-container" autoplay controls v-bind:src="medias[0].url">
                                  The “video” tag is not supported by your browser. Click [here] to download the video file.
                              </video>
                            </div>
                          </div>
                        </section>
                      </div>
                      <div v-else-if="type === 3">
                         <section class="v-cal-dialog-card__body" style="overflow:hidden;">
                          <div class="body-container-view" style="overflow:hidden; padding:0">
                            <div class="media-section-view">
                            <carousel :perPage="1">
                              <slide v-for="(item,index) in medias" v-bind:key="index">
                                <img v-if="item.type === 1" class="media-container" :src="item.url" alt="">
								<video v-else-if="item.type === 2" controls="controls" preload="metadata" :src="item.url+'#t=0.5'" class="media-container"/>
                              </slide>
                            </carousel>
                            </div>
                          </div>
                         </section>
                      </div>
                      <div class="media_info">
                        <b-taglist v-if="credit" style="padding:0.3em;" attached>
                            <b-tag size="is-medium" type="is-light"><b-icon icon="crown"></b-icon></b-tag>
                            <b-tag size="is-medium" type="is-danger">{{credit}}</b-tag>
                        </b-taglist>
                        <b-taglist v-if="location.Name" style="padding:0.3em; margin-top:-1.45em" attached>
                            <b-tag size="is-medium" type="is-light"><b-icon  pack="fas" icon="map-marker-alt"></b-icon></b-tag>
                            <b-tag size="is-medium" type="is-info">{{location.Name}}</b-tag>
                        </b-taglist>
                      </div>
                    </div>
                  <div class="column is-5" >               
                    <div class="caption-section-view">
                      <div class="caption-container">
                          <b-field label="Caption" class="is-left">
                              <b-input v-model="caption" type="textarea"
                                  minlength="1"
                                  maxlength="200"
                                  placeholder="Please write something about your post">
                              </b-input>
                          </b-field>
                          <div class="emoji-container"> 
                              <Emoji class="emj_item" set="messanger" emoji="heart" :size="32" @click="emojiFallback" :tooltip="true"/>
                              <Emoji class="emj_item"  set="facebook" emoji="heart_eyes" :size="32" @click="emojiFallback" />
                              <Emoji class="emj_item"  set="facebook" emoji="kissing_heart" :size="32" @click="emojiFallback" /> 
                              <Emoji class="emj_item"  set="facebook" emoji="grin" :size="32" @click="emojiFallback" /> 
                              <Emoji class="emj_item"  set="facebook" emoji="sob" :size="32" @click="emojiFallback"/> 
                              <Emoji class="emj_item"  set="facebook" emoji="sweat_smile" :size="32" @click="emojiFallback" /> 
                              <Emoji class="emj_item"  set="facebook" emoji="scream" :size="32" @click="emojiFallback" /> 
                              <Emoji class="emj_item"  set="facebook" emoji="stuck_out_tongue" :size="32" @click="emojiFallback" /> 
                              <Emoji class="emj_item"  set="facebook" emoji="thumbsup" :size="32" @click="emojiFallback" /> 
                              <Emoji class="emj_item"  set="facebook" emoji="sunglasses" :size="32" @click="emojiFallback"/> 
                          </div>
                          <!-- <emoji-picker set="facebook"/>  -->
                              
                          <div class="hashtag-container">
                              <b-field label="Hashtags" class="is-left">
                                  <b-taginput type="is-twitter" size="is-default" style="height:200px; overflow:auto; background:transparent!important;"
                                  :on-paste-separators="[',','@','-',' ']"
                                  :confirm-key-codes="[32, 13]"
                                  :before-adding="evaluateTags"
                                  maxlength="25"
                                  maxtags="30"
                                  v-model="hashtags">
                                  </b-taginput>
                              </b-field>
                              <b-switch :disabled="isFetchingHashtags" v-model="autoSuggest" @input="getSuggestedTags" type="is-twitter">Suggest Hashtags?</b-switch>
                          </div>
                      </div>
                     </div>  
                   <hr class="hr-sep-view">
                  <div class="footer-section-view ">
                    <div class="field has-addons is-right">
                      <p class="control">
                        <a @click="deleteEvent" class="button is-default is-danger">
                          <span class="icon">
                            <i class="fas fa-archive"></i>
                          </span>
                          <span>Delete</span>
                        </a>
                      </p>
                      <p class="control">
                        <a @click="enqueueEvent" class="button is-default is-light">
                          <span class="icon">
                            <i class="fas fa-camera-retro"></i>
                          </span>
                          <span>Post Now</span>
                        </a>
                      </p>
                      <p class="control">
                      <a @click="updateEvent" class="button is-default is-light">
                        <span class="icon">
                          <i class="fas fa-edit"></i>
                        </span>
                        <span>Update</span>
                      </a>
                    </p>
                    </div> 
                  </div>
                  </div>
                </div>
            </div>       
          </div>
    </transition>
</template>

<script>
import moment from 'moment';
import { Picker } from 'emoji-mart-vue';
import { Emoji } from 'emoji-mart-vue';
import { Carousel, Slide } from 'vue-carousel';
import state from '../../../../src/State';
import route from '../../../../src/Route';

export default {
  props:{
    id:String,
    title: String,
    caption: String,
    hashtags: Array,
    credit: String,
    location: Object,
    medias:Array,
    mediaTopic:Object,
    startTime: Object,
    type: Number
  },
  components:{
    Carousel,
    Slide ,
    'emoji-picker': Picker,
    Emoji
  },
  data() {
      return {
          isActive: false,
          event: {},
          isTag1Active: true,
          time: Date,
          isFetchingHashtags:false,
          autoSuggest:false,
          profile:{}
      }
  },
  beforeMount(){
    document.body.appendChild(this.$el);
    this.time = new Date(moment(this.startTime).format("YYYY-MM-DD HH:mm:ss")); 
    this.profile = state.getters.UserProfile(route.app.$route.params.id);
  },
  mounted() {
    this.isActive = true;
  },
  computed: {
      format() {
        return this.isAmPm ? '12' : '24'
      }
  },
  methods:{
    getSuggestedTags(e){
        if(e===true){
          this.isFetchingHashtags = true;
          const request = {
            profileTopic: this.profile.profileTopic,
            mediaTopic: this.mediaTopic,
            pickAmount: 28,
            mediaUrls: this.medias.map(res=>res.url)
          }
          state.dispatch('BuildTags', request).then(resp=>{
              this.hashtags = [];
              resp.data.forEach((item)=>this.hashtags.push(item));
              this.autoSuggest = false;
              this.isFetchingHashtags = false;
          }).catch(err=>{
              this.autoSuggest = false;
              this.isFetchingHashtags = false;
          })
        }
      },
     evaluateTags(hashtag){
        if(hashtag.length<2) return false;
        if(hashtag.includes("#")){
            return true;
        }
        else{
            hashtag = '#' + hashtag;
            this.postDataBuild.hashtags.push(hashtag);
            return false;
        }
    },
    emojiFallback(emoji){
      this.caption+=emoji.native;
    },
    updateEvent(){
      var object_to_update = {
        id: this.id,
        caption: this.caption,
        time: this.time,
        hashtags: this.hashtags,
        location: this.location,
        credit: this.credit,
        type: this.type
      };
      this.$emit('update-event', {object_to_update, done : ()=>{
      } })
      this.close();
    },
    enqueueEvent(){
      this.$emit('enqueue-event', this.id);
      this.close();
    },
    deleteEvent(){
      this.$emit('event-deleted', this.id);
      this.close();
    },
    toBase64(arr){
       return function (buffer) {
        var binary = '';
        var bytes = new Uint8Array(buffer);
        var len = bytes.byteLength;
        for (var i = 0; i < len; i++) {
            binary += String.fromCharCode(bytes[i]);
        }
        return window.btoa(binary);
    };
    },
    cancel() {
      this.close();
    }, 
    close() {
      this.isActive = false;
      // Timeout for the animation complete before destroying
      setTimeout(() => {
          this.$destroy();
          this.$el.remove();
      }, 150);
    },
    }
}
</script>

<style lang="scss" scoped>
$width: 45vw;
$height:25vw;

.VueCarousel-pagination{
	margin-top:-2.75em !important;
}

textarea{
  resize: none !important;
  box-shadow: none;
  background:#121212 !important;
}
.body-container-view{
    position: absolute;
    height:620px;
    width: 550px;
    top: 100px;
    left: calc(50% - 500px);
    display: flex;
    margin-left:0.3em;
}
.media-section-view{
    width:550px;
    height:520px;
    .media-container{
        height: 500px;
        width: 500px;
        object-fit:contain;
		margin: 0 auto;
    }
    //background:#23232338;
    border-radius:0.7em;
   // box-shadow: -0.3rem 0 1rem #000;
    margin-top:.5em;
    margin-left:-.5em;
    .position{
        text-align: left !important;
        margin-left:0em;
    }
}
.hr-sep-view{
    position: absolute;
    left:2em;
    right:2em;
    bottom:3.7em;
    height: 1.25px;
    background:#232323;
}
.media_info{
  position: absolute;
  bottom: 0.5em;
  left:0;
  margin-left:1.15em;
  margin-top:1em;
  display: inline-flex;
  flex-flow: row wrap;
  align-items: center;
}
.column{
  background:transparent;
  padding-right:2em;
}

.is-centered{
  margin:0 auto;
  margin-left:2.1em;
  margin-top:1em;
}
.footer-section-view{
    position:absolute;
    bottom:2em;
    right:2em;
    width:100%;
    height: 40px;
    .is-right{
        float:right;
    }
    .is-left{
        float:left;
        margin-left:-5.7em;
    }
}
.caption-section-view{
    position:absolute;
    top:6.7em;
    right:2em;
    width:450px;
    height:520px;
    background:#23232338;
    border-radius:0.7em;
    box-shadow: -0.01rem 0 .3rem #000;
    .label{
        &.is-left{
            text-align: left !important;
        }
    }
    .caption-container{
        padding:.8em;      
    }
    .emoji-container{
        padding:0;
        margin-top:-.7rem;
        .emj_item{
            padding-left:.5rem;
            &:hover{
                opacity:.8;
                cursor: pointer;
            }
        }
    }
    .hashtag-container{
        .taginput-container{
            background:#323232 !important;
        }
        .autocomplete{
            input{
                background:#323232!important;
                &:hover{
                    background:#232323 !important;
                }
            }
        }
    }
}
.is-big{
  border-radius:0.7em;
  width:55% !important;
  height:77% !important;
  max-width: 55%;
  padding-top:1em;
  padding-right:2em !important;
  padding-left:2em !important;
 // box-shadow: 10px 2px 3px 14px rgba(30, 30, 30, 0.1) !important;
}
.is-large{
  font-size: 16px !important;
  font-weight: bold;
}

.media_container{
  padding:0 !important;
  outline: none !important;
  border: none !important;
  float:center !important;
  height: $height !important;
  width: $width/1.5 !important;
  object-fit:contain !important;
  background-color: transparent !important; 
  &.isImage{
    width:$width;
    object-fit: contain;
  }
}
.v-cal-dialog .v-cal-dialog-card__body{
  padding:0;
}
.v-cal-dialog .v-cal-dialog-card__header{
  padding:0em;
  padding-top:0.1em;
  padding-bottom:1em;
  padding-left:1em;
  padding-right:1em;
}
.zoom-out-enter -active,
.zoom-out-leave-active {
    transition: opacity 150ms ease-out;
}
.zoom-out-enter-active .animation-content,
.zoom-out-enter-active .animation-content,
.zoom-out-leave-active .animation-content,
.zoom-out-leave-active .animation-content {
    transition: transform 150ms ease-out;
}

.zoom-out-enter,
.zoom-out-leave-active {
    opacity: 0;
}
.zoom-out-enter .animation-content,
.zoom-out-enter .animation-content,
.zoom-out-leave-active .animation-content,
.zoom-out-leave-active .animation-content {
    transform: scale(1.05);
}
</style>