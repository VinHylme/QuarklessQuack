<template>
  <div class="container-fluid" style="background:#232323; width:100%;">
    <div class="columns is-mobile" >
      <div class="column is-10" style="padding-left:7em !important; padding-top:1.8em !important; padding-right:2em !important;">
        <div class="header-lib">
        <!-- <a class="button is-success is-right">Save Changes</a> -->
        <b-notification :closable="false" v-if="isLoading" style="background:transparent;">
            <b-loading :is-full-page="true" :active.sync="isLoading" :can-cancel="true"></b-loading>
        </b-notification>
        <!-- <b-notification :closable="false" v-if="isDeleting" style="background:transparent;">
            <b-loading :is-full-page="true" :active.sync="isDeleting" :can-cancel="true"></b-loading>
        </b-notification> -->
        </div>
            <div class="media-section-lib b-tabs-media">
              <b-tabs :animated="false" @change="changedTab" type="is-toggle"  expanded>
                  <b-tab-item style="margin-left:3.5em;" label="Saved Medias" icon-pack="fas" icon="images">
                    <div class="current-list-container">
                      <p class="subtitle"></p>
                      <div class="image_container zoomable" v-for="(media,index) in this.filterMedia" :key=index>
                          <ImageItem
                            v-if="media.mediaBytes && media.mediaType === 0"
                            :source="media.mediaBytes"
                            width="300px" 
                            height="300px"
                            :isRounded=true
                            @click="selectMedia(index)"
                            v-bind:style="mediaLibrary.selectedMedia.index == index ? 'border: 2px solid turquoise;' : 'border:none'"
                          />
                          <video width="300px" height="300px" controls="controls" preload="metadata" v-else-if="media.mediaBytes && media.mediaType === 1" :src="media.mediaBytes+'#t=0.5'"></video>
                          <div class="cross-delete" @click="deleteMedia(index)">
                            <span class="icon">
                              <i class="far fa-2x fa-times-circle"></i>
                            </span>
                          </div>
                      </div>
                    </div>
                  </b-tab-item>
                  <b-tab-item label="Saved Captions" icon-pack="fas" icon="pen">
                    <div class="cap_container">
                      <div v-for="(caption,index) in captionLibrary.items" :key="index">
                        <TextCard width="600px" :rows="4" :isArray="false" :data="caption" :allowDelete="true" :allowEdit="true" @click-delete="onClickDelete" @click-edit="onClickEdit" icon="book-open" :date="formatedDate(caption.dateAdded)" :message="caption.caption"/>
                      </div>
                    </div>
                  </b-tab-item>
                  <b-tab-item label="Saved Hashtags" icon-pack="fas" icon="hashtag">
                    <div class="hash_container">
                      <div v-for="(hashtag,index) in hashtagsLibrary.items" :key="index">
                        <TextCard width="600px" type="is-light" :isArray="true" :data="hashtag" :allowDelete="true" :allowEdit="true" @click-delete="onClickDeleteHashtag" @click-edit="onClickEditHashtag" icon="hashtag" :date="formatedDate(hashtag.dateAdded)" :messageArray="hashtag.hashtag"/>
                      </div>
                    </div>
                  </b-tab-item>
                  <b-tab-item label="Saved Messages" icon-pack="fas" icon="inbox">
                    <div class="cap_container">
                       <div v-for="(message,index) in messageLibrary.items" :key="index">
                        <TextCard width="600px" :rows="8" :isArray="false" :data="message" :allowDelete="true" :allowEdit="true" @click-delete="onClickDeleteMessage" @click-edit="onClickEditMessage" icon="envelope-open" :date="formatedDate(message.dateAdded)" :link="message.entity.link" :message="messageFormat(message.entity)"/>
                      </div>
                    </div>
                  </b-tab-item>
              </b-tabs>
        </div>
      </div>
       <div class="column is-2" style="padding-top:1.9em !important; margin-left:-1.2em !important;">
          <div class="media-section-side-panel">
            <div ref="media_section" v-if="selectedSection === 0" >
              <div class="uploader-section">
                <d-drop accept="image/x-png, image/jpeg" :isHidden="false" :isMulti="true" swidth="240px" sheight="180px" class="dropStyle" @readyUpload="onUpload"></d-drop> 
              </div>
              <hr class="hr-an">
              <div class="sort_media_type">
                <b-field label="Sort By Media Type">
                  <select v-model="mediaFilterObject.mediaType" class="select is-default" @change="onChangeMediaType">
                      <option value="-1" selected>All</option>
                      <option value="0">Image</option>
                      <option value="1">Video</option>
                  </select>
                </b-field>
              </div>
            </div>
            <div class="cap_sec" ref="caption_section" style="padding-top:1em;" v-else-if="selectedSection === 1">
              <b-field label="Add Caption">
                <textarea type="is-light" style="resize:none;" maxlength="2100" rows="10" v-model="captionObject.caption" class="textarea side" placeholder="e.g. This right here is my saved caption"></textarea>
              </b-field>
              <div class="emoji-container"> 
                <Emoji class="emj_item" set="messanger" emoji="heart" :size="24" @click="emojiFallback" :tooltip="true"/>
                <Emoji class="emj_item"  set="facebook" emoji="heart_eyes" :size="24" @click="emojiFallback" />
                <Emoji class="emj_item"  set="facebook" emoji="kissing_heart" :size="24" @click="emojiFallback" /> 
                <Emoji class="emj_item"  set="facebook" emoji="grin" :size="24" @click="emojiFallback" /> 
                <Emoji class="emj_item"  set="facebook" emoji="sob" :size="24" @click="emojiFallback"/> 
                <Emoji class="emj_item"  set="facebook" emoji="sweat_smile" :size="24" @click="emojiFallback" /> 
                <Emoji class="emj_item"  set="facebook" emoji="scream" :size="24" @click="emojiFallback" /> 
                <Emoji class="emj_item"  set="facebook" emoji="stuck_out_tongue" :size="24" @click="emojiFallback" /> 
                <Emoji class="emj_item"  set="facebook" emoji="thumbsup" :size="24" @click="emojiFallback" /> 
                <Emoji class="emj_item"  set="facebook" emoji="sunglasses" :size="24" @click="emojiFallback"/> 
              </div>
              <br>
              <a class="button is-info" @click="addCaption">Save Caption</a>
              <hr class="hr-an">
            </div>
            <div class="hash_sec" ref="hashtag_section" v-else-if="selectedSection === 2">
                <b-field label="Add Hashtags" class="is-left">
                    <b-taginput type="is-twitter" size="is-default" class="hash-tag-input"
                    :on-paste-separators="[',','@','-',' ']"
                    :confirm-key-codes="[32, 13]"
                    :before-adding="evaluateTags"
                    icon="hashtag"
                    icon-pack="fas"
                    maxlength="100"
                    maxtags="30"
                    v-model="hashtagsObject.hashtags">
                    </b-taginput>
                </b-field>
              <br>
              <a class="button is-info" @click="addHashtags">Save Hashtags</a>
              <hr class="hr-an">
            </div>
            <div class="cap_sec" ref="message_section" style="padding-top:1em;" v-else-if="selectedSection === 3">
              <b-field label="Add Message">
                <textarea type="is-light" style="resize:none;" maxlength="10000" rows="15" v-model="messageObject.entity.message" class="textarea side" placeholder="e.g. This right here is my saved caption"></textarea>
              </b-field>
              <b-switch v-model="messageObject.includelink"  size="is-default" style="margin-left:-12em;">Include a link? </b-switch>
              <br>
              <b-field v-if="messageObject.includelink" label="Link">
                <input class="input" type="text" v-model="messageObject.entity.link" placeholder="e.g. www.mycoolwebsite.com">
              </b-field>
              <br>
              <div class="emoji-container"> 
                <Emoji class="emj_item" set="messanger" emoji="heart" :size="24" @click="emojiFallbackMSG" :tooltip="true"/>
                <Emoji class="emj_item"  set="facebook" emoji="heart_eyes" :size="24" @click="emojiFallbackMSG" />
                <Emoji class="emj_item"  set="facebook" emoji="kissing_heart" :size="24" @click="emojiFallbackMSG" /> 
                <Emoji class="emj_item"  set="facebook" emoji="grin" :size="24" @click="emojiFallbackMSG" /> 
                <Emoji class="emj_item"  set="facebook" emoji="sob" :size="24" @click="emojiFallbackMSG"/> 
                <Emoji class="emj_item"  set="facebook" emoji="sweat_smile" :size="24" @click="emojiFallbackMSG" /> 
                <Emoji class="emj_item"  set="facebook" emoji="scream" :size="24" @click="emojiFallbackMSG" /> 
                <Emoji class="emj_item"  set="facebook" emoji="stuck_out_tongue" :size="24" @click="emojiFallbackMSG" /> 
                <Emoji class="emj_item"  set="facebook" emoji="thumbsup" :size="24" @click="emojiFallbackMSG" /> 
                <Emoji class="emj_item"  set="facebook" emoji="sunglasses" :size="24" @click="emojiFallbackMSG"/> 
              </div>
              <br>
              <a class="button is-info" @click="addMessage">Save Message</a>
              <hr class="hr-an">
            </div>
          </div>
      </div>
    </div>
  </div>
</template>

<script>
import { Emoji } from 'emoji-mart-vue';
import ImageItem from '../../components/Objects/ImageItem';
import DropZone from '../../components/Objects/DropZone';
import TextCard from '../Objects/TextCard';
import moment from 'moment';
import Vue from 'vue';
export default {
  components:{
    TextCard,
    ImageItem,
    Emoji,
    'd-drop' : DropZone
  },
  data(){
    return{
      mediaLibrary:{
        items:[],
        selectedMedia:{
          item:null,
          index:-1
        }
      },
      hashtagsLibrary:{
        items:[]
      },
      hashtagsObject:{
        hashtags:[]
      },
      captionLibrary:{
        items:[]
      },
      messageLibrary:{
        items:[]
      },
      messageObject:{
        entity:{
          message:'',
          link:'',
          mediaBytes:'',
          profileIdShare:'',
          mediaIdShare:''
        },
        includelink:false,
        type:0
      },
      uploadedMedia:{
        items:[]
      },
      captionObject:{
        caption:''
      },
      isLoading:false,
      isDeleting:false,
      selectedSection:0,
      mediaFilterObject:{
        mediaType:-1
      }
    }
  },
  beforeMount(){
    this.$emit('unSelectAccount');
    this.loadData();
  },
  computed:{
    filterMedia(){
      if(this.mediaFilterObject.mediaType == -1) return this.mediaLibrary.items;
      return this.mediaLibrary.items.filter(res=>{
        if(res.mediaType == this.mediaFilterObject.mediaType)
          return res;
      })
    }
  },
  methods:{
    messageFormat(entity){
      return entity.message + '\n';
    },
    loadData(){
      this.captionLibrary.items = this.$store.getters.UserCaptionLibrary(this.$route.params.id);
      if(this.captionLibrary.items === undefined || this.captionLibrary.items === null){
        this.$store.dispatch('GetSavedCaption', this.$store.getters.User).then(resp=>{
          this.captionLibrary.items = this.$store.getters.UserCaptionLibrary(this.$route.params.id);
        });
      };

      this.hashtagsLibrary.items = this.$store.getters.UserHashtagsLibrary(this.$route.params.id);
      if(this.hashtagsLibrary.items === undefined || this.hashtagsLibrary.items === null){
        this.$store.dispatch('GetSavedHashtags', this.$store.getters.User).then(resp=>{
            this.hashtagsLibrary.items = this.$store.getters.UserHashtagsLibrary(this.$route.params.id);
        });
      };
     
      this.messageLibrary.items = this.$store.getters.UserMessageLibrary(this.$route.params.id);
      if(this.messageLibrary.items === undefined || this.messageLibrary.items === null){
        this.$store.dispatch('GetSavedMessages', this.$store.getters.User).then(resp=>{
          this.messageLibrary.items = this.$store.getters.UserMessageLibrary(this.$route.params.id);
        });
      };
      
      this.mediaLibrary.items = this.$store.getters.UserMediaLibrary(this.$route.params.id);
      if(this.mediaLibrary.items === undefined || this.mediaLibrary.items === null){
        //this.isLoading = true;
        this.$store.dispatch('GetSavedMedias', this.$store.getters.User).then(resp=>{
          this.mediaLibrary.items = this.$store.getters.UserMediaLibrary(this.$route.params.id);
          //this.isLoading = false;
        }).catch(err=>{
          //this.isLoading = false;
        })
      };
    },
    evaluateTags(hashtag){
      if(hashtag.length<2) return false;
        if(hashtag.includes("#")){
          if(!this.hashtagsObject.hashtags.includes(hashtag))
            return true;
          else
            return false;
        }
        else{
          hashtag = '#' + hashtag;
          if(!this.hashtagsObject.hashtags.includes(hashtag)){
            this.hashtagsObject.hashtags.push(hashtag);
            return false;
          }
          else
            return false;
      }
    },
    emojiFallback(emoji){
      this.captionObject.caption+=emoji.native;
    },
    emojiFallbackMSG(emoji){
      this.messageObject.entity.message += emoji.native;
    },
    formatedDate(e){
      return moment(e).format("YYYY-MM-DD HH:mm:ss");
    },
    addHashtags(){
      if(this.hashtagsObject.hashtags.length<=0){
        Vue.prototype.$toast.open({
          message: 'Sorry, please populate the hashtag field first.',
          type: 'is-danger'
        });
        return;
      }
      let hashtagData = {
        instagramAccountId: this.$route.params.id,
        accountId: this.$store.getters.User,
        groupName: this.uuidv4(),
        hashtag: this.hashtagsObject.hashtags,
        dateAdded: new Date()
      };
      this.$store.dispatch('SetSavedHashtags', hashtagData).then(resp=>{
       Vue.prototype.$toast.open({
            message: 'Saved Hashtags!',
            type: 'is-success'
        });
        this.hashtagsLibrary.items = this.$store.getters.UserHashtagsLibrary(this.$route.params.id);
        this.hashtagsObject.hashtags = [];
      }).catch(err=>
      {
         Vue.prototype.$toast.open({
            message: 'Failed to save hashtags',
            type: 'is-danger'
        });
      })
    },
    addCaption(){
      if(this.captionObject.caption === ''){
         Vue.prototype.$toast.open({
            message: 'Sorry, please populate the caption field first.',
            type: 'is-danger'
          });
        return;
      }
      let captionData = {
        instagramAccountId: this.$route.params.id,
        accountId: this.$store.getters.User,
        groupName: this.uuidv4(),
        caption: this.captionObject.caption,
        dateAdded: new Date()
      };
      this.$store.dispatch('SetSavedCaption', captionData).then(resp=>{
       Vue.prototype.$toast.open({
            message: 'Saved Caption!',
            type: 'is-success'
        });
        this.captionLibrary.items = this.$store.getters.UserCaptionLibrary(this.$route.params.id);
        this.captionObject.caption = ''
      }).catch(err=>
      {
         Vue.prototype.$toast.open({
            message: 'Failed to save caption',
            type: 'is-danger'
        });
      })
    },
    addMessage(){
      if(this.messageObject.includelink){
        this.messageObject.type = 1;
        if(this.messageObject.entity.link === ''){
           Vue.prototype.$toast.open({
            message: 'Sorry, please populate the link field first.',
            type: 'is-danger'
          });
          return;
        }
      }
      if(this.messageObject.entity.message === ''){
         Vue.prototype.$toast.open({
            message: 'Sorry, please populate the message field first.',
            type: 'is-danger'
          });
        return;
      }
      let messageData = {
        instagramAccountId: this.$route.params.id,
        accountId: this.$store.getters.User,
        groupName: this.uuidv4(),
        entity: this.messageObject.entity,
        type: this.messageObject.type,
        dateAdded: new Date()
      };
      this.$store.dispatch('SetSavedMessages', messageData).then(resp=>{
       Vue.prototype.$toast.open({
            message: 'Saved Message!',
            type: 'is-success'
        });
        this.messageLibrary.items = this.$store.getters.UserMessageLibrary(this.$route.params.id);
        this.messageObject.message = '';
      }).catch(err=>
      {
         Vue.prototype.$toast.open({
            message: 'Failed to save message',
            type: 'is-danger'
        });
      })
    },
    changedTab(e){
      this.selectedSection = e;
    },
    onChangeMediaType(e){
      console.log(e);
    },
    onClickDelete(e){
      this.$store.dispatch('DeleteSavedCaption', e).then(resp=>{
        Vue.prototype.$toast.open({
            message: 'Deleted!',
            type: 'is-success'
        });
        this.captionLibrary.items = this.$store.getters.UserCaptionLibrary(this.$route.params.id);
      }).catch(err=>{
        Vue.prototype.$toast.open({
            message: 'Failed to delete caption',
            type: 'is-danger'
          });
      })
    },
    onClickEdit(e){
      if(e.message!==undefined && e.message!= '' && e.message!==e.originalData.caption){
        e.originalData.caption = e.message;
        this.$store.dispatch('UpdateSavedCaption', e.originalData).then(resp=>{
          Vue.prototype.$toast.open({
            message: 'Updated!',
            type: 'is-success'
          });
        }).catch(err=>{
          Vue.prototype.$toast.open({
            message: 'Failed to update caption',
            type: 'is-danger'
          });
        })
      }
      else{

      }
    },
    onClickDeleteMessage(e){
      this.$store.dispatch('DeleteSavedMessages', e).then(resp=>{
        Vue.prototype.$toast.open({
            message: 'Deleted!',
            type: 'is-success'
        });
        this.messageLibrary.items = this.$store.getters.UserMessageLibrary(this.$route.params.id);
      }).catch(err=>{
        Vue.prototype.$toast.open({
            message: 'Failed to delete message',
            type: 'is-danger'
          });
      })
    },
    onClickEditMessage(e){
      console.log(e);
      if(e.message!==undefined && e.message!= '' && e.message!==e.originalData.entity.message){
        e.originalData.entity.message = e.message;
        e.originalData.entity.link = e.link;
        this.$store.dispatch('UpdateSavedMessages', e.originalData).then(resp=>{
          Vue.prototype.$toast.open({
            message: 'Updated!',
            type: 'is-success'
          });
        }).catch(err=>{
          Vue.prototype.$toast.open({
            message: 'Failed to update message',
            type: 'is-danger'
          });
        })
      }
      else{

      }
    },
    onClickDeleteHashtag(e){
      this.$store.dispatch('DeleteSavedHashtags', e).then(resp=>{
        Vue.prototype.$toast.open({
            message: 'Deleted!',
            type: 'is-success'
        });
        this.hashtagsLibrary.items = this.$store.getters.UserHashtagsLibrary(this.$route.params.id);
      }).catch(err=>{
        Vue.prototype.$toast.open({
            message: 'Failed to delete hashtags',
            type: 'is-danger'
          });
      })
    },
    onClickEditHashtag(e){
      if(e.message!==undefined && e.message!= []){
        e.originalData.hashtag = e.message;
        this.$store.dispatch('UpdateSavedHashtags', e.originalData).then(resp=>{
          Vue.prototype.$toast.open({
            message: 'Updated!',
            type: 'is-success'
          });
        }).catch(err=>{
          Vue.prototype.$toast.open({
            message: 'Failed to update hashtags',
            type: 'is-danger'
          });
        })
      }
      else{

      }
    },
    isFileImage(file) {
      return file && file['type'].split('/')[0] === 'image';
    },
    selectMedia(position){
      this.mediaLibrary.selectedMedia = {index: position, item: this.mediaLibrary.items[position]};
    },
    deleteMedia(position){
      this.isDeleting = true;
      let item = this.mediaLibrary.items[position];
      this.mediaLibrary.items = [];
      this.$store.dispatch('DeleteSavedMedia', item).then(resp=>{
        this.mediaLibrary.items = this.$store.getters.UserMediaLibrary(this.$route.params.id);
        this.isDeleting = false;
      }).catch(err=>{
        this.isDeleting = false;
      })
    },
    uuidv4() {
        return ([1e7]+-1e3+-4e3+-8e3+-1e11).replace(/[018]/g, c =>
          (c ^ crypto.getRandomValues(new Uint8Array(1))[0] & 15 >> c / 4).toString(16)
        )
    },
    async onUpload(data){
      this.isLoading = true;
      var _this = this;
      var submitMediaData = []
      var xl = 0;
      for(; xl < data.requestData.length; xl++){
        var canAdd = false;
        await this.readFile(data.requestData[xl]).then(async resp=>{
         var media_type = resp.split('/')[0].split(':')[1];
            if(media_type === "video"){
              //check length
              await this.readVideoLength(resp).then(res=>{
                if(res){
                  Vue.prototype.$toast.open({
                    message: 'Sorry, please ensure your video is no longer than 1 minute',
                    type: 'is-danger'
                  });
                }
                else{
                  canAdd = true;
                }
              });      
            }
            else if(media_type === "image"){
             canAdd = true;
            }
            if(canAdd){
              var mediaData = {
                instagramAccountId : _this.$route.params.id,
                accountId: _this.$store.getters.User,
                groupName : _this.uuidv4(),
                mediaType : media_type === "video" ? 1 : 0,
                dateAdded : new Date(),
                mediaBytes : resp
              };
              submitMediaData.push(mediaData);
              _this.mediaLibrary.items.push({url: resp});
          }
        });
      }
      this.$store.dispatch('SetSavedMedias', submitMediaData).then(resp=>{
        this.mediaLibrary.items = this.$store.getters.UserMediaLibrary(this.$route.params.id);
        this.isLoading = false;
      }).catch(err=>{
        this.isLoading = false;
      })
    },
    readVideoLength(resp){
      return new Promise((resolve, reject)=>{
        let video = document.createElement('video');
        video.preload = 'metadata';
        video.onloadedmetadata = function(){
          window.URL.revokeObjectURL(video.src);
          var duration = video.duration;
          if(duration > 65)
            resolve(true);
          else
            resolve(false);
        };
        var byteCharacters = atob(resp.slice(resp.indexOf(',') + 1)); 
        var byteNumbers = new Array(byteCharacters.length);
        for (var i = 0; i < byteCharacters.length; i++) {            
          byteNumbers[i] = byteCharacters.charCodeAt(i);            
        }
        var byteArray = new Uint8Array(byteNumbers);
        var blob = new Blob([byteArray], {type: 'video/ogg'});
        video.src = URL.createObjectURL(blob);          
      })
    },
    readFile(file) {
      return new Promise((resolve, reject) => {
          let fr = new FileReader();
          fr.onload = x=> resolve(fr.result);
          fr.readAsDataURL(file)
      })
    },
  }
}
</script>

<style lang="scss">
.emoji-container{
    padding:0;
    margin-top:-0rem;
    .emj_item{
        padding-left:.5rem;
        &:hover{
            opacity:.8;
            cursor: pointer;
        }
    }
}
.hash_sec{
  padding-top:1.2em;
  label{
    font-size:14px;
    color:#d9d9d9;
  }
  .hash-tag-input{
    //height:250px; 
    overflow:auto;
    background:transparent!important;
    margin-right:.8em;
    margin-left:.5em;
    .taginput-container{
      //height: 250px;
      color:#fefefe;
      border:none;
      background:#323232 !important;
    }
    input{
      color:#fefefe;
      background:#323232 !important;
    }
  }
}
.cap_sec{
  color:#d9d9d9 !important;
  label{
    font-size:14px;
    color:#d9d9d9;
  }
  .input{
    border-radius: 0;
    background:#323232 !important;
    border:none;
    color:#d9d9d9 !important;
    &::placeholder{
      color:#d0d0d0;
    }
  }
  .textarea{
    border:none;
    margin-right:2em;
    color:#d9d9d9;
    &.side{
      background:#323232 !important;
    }
    &::placeholder{
      color:#d0d0d0;
    }
  }
}

.sort_media_type{
  text-align: center !important;
  margin-left:-1.5em;
  label{
    text-align: center;
    //margin-left:-6em;
    color:#d9d9d9;
    font-size: 14px;
  }
  select{
    width:200px !important;
    background:#121212 !important;
    border:none !important;
    color:#d9d9d9 !important;
    transition: background 0.5s ease;
    &:focus{
      color:#d9d9d9 !important;
      box-shadow: 0 !important;
      border:none !important;
    }
    &:hover{
      transition: all .2s ease-in-out;
      background:#282828 !important;
    }
    option{
      background:#212121 !important;
      color:#d9d9d9 !important;
    }
  }
}
video {
  object-fit: fill !important;
  width:300px;
  height: 290px;
  margin-top:.25em; 
}
.header-lib{
  display: flex;
  flex-flow: row wrap;
  .button{
    &.is-right{
      text-align: right;
      margin-left:2em;
    }
  }
}
.b-tabs-media{
  .b-tabs{
    color:white;
    .tabs{
      a{
        border:none !important;
        background:#4e4e4e;
        color:#d9d9d9;
      }
      li{
          a{
           border-radius: 0 !important;
          }
        &.is-active{
          a{
            background:#23d160;
            border:none;
          }
        }
      }
    }
  }
}
.media-section-lib{
  background:#202020 !important;
  width:100%;
  height:100%; 
  border: 1px solid #313131;
  box-shadow: -0.01rem 0 .7rem rgba(0, 0, 0, 0.19);
}
.cap_container{
  width:100%;
  height: 100%;
  display: flex;
  margin:0 auto;
  flex-flow: row wrap;
  align-items: center;
  margin-left:6em;
}
.hash_container{
  width:100%;
  height: 100%;
  display: flex;
  margin:0 auto;
  flex-flow: row wrap;
  align-items: center;
  margin-left:6em;
}
.current-list-container{
  width:100%;
  height: 100%;
  display: flex;
  margin:0 auto;
  flex-flow: row wrap;
  align-items: center;
  background:transparent;
  margin-left:-1.5em;
  .image_container{
    position: relative;
    margin: 25px; 
    margin-top: 25px;
    width: 300px;
    height: 300px;
    transition: all .2s ease-in-out;
    .cross-delete{
      display:none;
      width:50px;
      height:50px;
      position:absolute;
      top:-5px;
      padding:1em;
      right:3px;
      overflow:hidden;
      text-shadow: .5px .3px .4px #1f1f1f; 
      &:hover{
        color:#cc0000;
      }
    }
    &:hover{
      .cross-delete{
        display: block;
      }
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
  }
}

.media-section-side-panel{
  //margin-top:6.15em;
  text-align: center;
  background:#1f1f1f;
  width:100%;
  height: 1000px;
  border: 1px solid #313131;
  .hr-an{
    height: 1px;
    background: #313131;  
  }
  .uploader-section{
    margin:0 auto;
    width:100%;
    text-align: center;
    padding-top:3em;
    margin-left:-.4em;
    color:#d9d9d9;
    .file-cta{
      width:100% !important;
    }
  }
}

</style>