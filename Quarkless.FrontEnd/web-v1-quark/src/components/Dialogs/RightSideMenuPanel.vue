<template>
  <transition name="slide-in">
    <div class="right-side-panel-container" v-if="isActive">
         <div class="panel-bg" @click="Close()"></div>
         <div id="panel" class="right-panel slide-in">
           <div class="menu-option">
              <b-tabs @change="onChangeType" animated type="is-toggle-rounded">
                  <b-tab-item label="Send By Post" icon="camera">
                    <div class="b-tab-item-sub">
                      <div class="field has-addons">
                        <p class="control">
                          <a @click="GetMediaFromLibary" class="button" :disabled="medias.isLoading">
                            <span class="icon is-small">
                              <i class="fas fa-book"></i>
                            </span>
                            <span>From Library</span>
                          </a>
                        </p>
                        <p class="control">
                          <a @click="GetUserMedia" class="button" :disabled="medias.isLoading">
                            <span class="icon is-small">
                              <i class="fas fa-images"></i>
                            </span>
                            <span>My Posts</span>
                          </a>
                        </p>
                        <p class="control">
                          <a class="button" :disabled="medias.isLoading">
                            <span class="icon is-small">
                              <i class="fas fa-upload"></i>
                            </span>
                            <span>From Computer</span>
                            <d-drop acceptFile="image/*|video/*" :isHidden="true" :isMulti="false" class="dropStyle" @readyUpload="onUpload"></d-drop> 
                          </a>
                        </p>
                      </div>
                     </div>
                    <div class="media-contain">
                      <div class="media-results" v-if="HasSelectedMedia">
                        <b-notification style="background:transparent; height:150px; margin:0 auto;" v-if="medias.isLoading" :closable="false">
                            <b-loading :is-full-page="false" :active.sync="medias.isLoading" :can-cancel="false"></b-loading>
                        </b-notification>
                        <div @click="SelectMedia(index)" class="media-item" v-for="(item,index) in this.medias.searchResults" :key="index">
                          <ImageItem v-if="item.mediaUrl && (item.mediaType === 1 || item.mediaType === 8)" :source="item.mediaUrl" width="155px" height="155px" :isRounded="false"/>    
                          <video v-else-if="item.mediaUrl && item.mediaType === 2" width="155px" height="155px" controls="controls" preload="metadata"  :src="item.mediaUrl+'#t=0.5'"></video>
                          <ImageItem v-else-if="item.mediaType===1" :source="item.mediaBytes" width="155px" height="155px" :isRounded="false" />
                          <video v-else-if="item.mediaBytes && item.mediaType === 2" width="155px" height="155px" controls="controls" preload="metadata"  :src="item.mediaBytes+'#t=0.5'"></video>
                        </div>
                      </div>
                      <div v-else class="selected-media">
                        <img v-if="medias.selectedMedia.mediaUrl && (medias.selectedMedia.mediaType === 1 || medias.selectedMedia.mediaType === 8)" class="media-holder" :src="medias.selectedMedia.mediaUrl" alt="">
                        <video v-else-if="medias.selectedMedia.mediaUrl && medias.selectedMedia.mediaType === 2" class="media-holder" controls="controls" preload="metadata"  :src="medias.selectedMedia.mediaUrl+'#t=0.5'"></video>
                        <img v-else-if="medias.selectedMedia.mediaBytes && medias.selectedMedia.mediaType === 1" class="media-holder" :src="medias.selectedMedia.mediaBytes" alt="">
                        <video v-else-if="medias.selectedMedia.mediaBytes && medias.selectedMedia.mediaType === 2" class="media-holder" controls="controls" preload="metadata"  :src="medias.selectedMedia.mediaBytes+'#t=0.5'"></video>
                        <br>
                        <br>
                        <div class="buttons has-addons is-centered">
                          <span @click="GoBack" class="button is-medium is-danger">
                            <span class="icon is-small">
                              <i class="fas fa-history"></i>
                            </span>
                            <span>
                            Back
                            </span>
                            </span>
                          <span @click="SendPost" class="button is-medium is-success">
                            <span class="icon is-small">
                              <i class="fas fa-inbox"></i>
                            </span>
                            <span>
                            Send Post
                            </span>
                            </span>
                        </div>
                      </div>
                    </div>
                  </b-tab-item>
                  <b-tab-item label="Send By Text" icon-pack="fas" icon="envelope">
                    <div class="new-compose" style="float:left;">
                      <a @click="SelectMessageAction" class="button is-light" :disabled="messages.isLoading">
                        <span class="icon">
                          <i v-if="HasSelectedMessage" class="fas fa-lg fa-plus"></i>
                          <i v-else class="fas fa-lg fa-history"></i>
                        </span>
                        <span v-if="HasSelectedMessage">Compose</span>
                        <span v-else>Go Back</span>
                      </a>
                    </div>
                    <div class="message-results" v-if="HasSelectedMessage">
                        <b-notification style="background:transparent; height:150px; margin:0 auto;" v-if="messages.isLoading" :closable="false">
                            <b-loading :is-full-page="false" :active.sync="messages.isLoading" :can-cancel="false"></b-loading>
                        </b-notification>
                        <div v-for="(item, index) in messages.searchResults" :key="index">
                          <TextCard @click-select="SelectMessage" width="600px" :rows="MessageRowLength(item.entity)" :isArray="false" :data="item" :allowDelete="false" :allowEdit="false" icon="envelope-open" :date="FormatedDate(item.dateAdded)" :link="item.entity.link" :message="MessageFormat(item.entity)"/>                   
                       </div>
                    </div>
                    <div v-else class="compose-editor">
                        <b-field label="Compose Message">
                          <textarea type="is-light" style="resize:none;" maxlength="1000" rows="20" v-model="messages.selectedMessage.entity.message" class="textarea side" placeholder="e.g. This right here is my saved caption"></textarea>
                        </b-field>
                        <b-switch v-model="IsLinkIncluded" size="is-default" style="float:left;">Include a link?</b-switch>
                        <br>
                        <b-field style="margin-top:.5em;" v-if="IsLinkIncluded" label="Link">
                          <input class="input" type="text" v-model="messages.selectedMessage.entity.link" placeholder="e.g. www.mycoolwebsite.com">
                        </b-field>
                          <div style="margin-top:1em;" class="buttons has-addons is-centered">
                          <span @click="SendMessage" class="button is-medium is-success">
                            <span class="icon is-small">
                              <i class="fas fa-inbox"></i>
                            </span>
                            <span>
                              Send Message
                            </span>
                          </span>
                        </div>
                    </div>
                  </b-tab-item>
              </b-tabs>
           </div>
         </div>
    </div>
  </transition>
</template>

<script>
import store from '../../State';
import ImageItem from '../Objects/ImageItem';
import TextCard from '../Objects/TextCard';
import moment from 'moment';
import DropZone from '../Objects/DropZone';
import { ReadFile, AnalyseFile, UUIDV4 } from '../../helpers'
export default {
  components:{
    TextCard,
    ImageItem,
    'd-drop': DropZone
  },
  props: {
    data:Object
  },
  data(){
    return {
      isActive:false,
      selectedType:0,
      medias:{
        searchResults:[],
        selectedMedia:{},
        isLoading: false
      },
      messages:{
        searchResults:[],
        selectedMessage:{},
        isLoading:false,
        includelink:false
      }
    }
  },
  beforeMount(){
    this.data = this.$options.propsData
    document.body.appendChild(this.$el);
  },
  mounted(){
    this.isActive = true;
    this.GetUserMedia();
  },
  computed:{
    EntityHasLink(){
      if(this.messages.selectedMessage.entity.link)
        return true;
      else
        return false;
    },
    IsLinkIncluded:{
      get(){
        if(this.EntityHasLink && this.messages.includelink === true){
          this.messages.includelink = true;
        }
        return this.messages.includelink;
      },
      set(value){
        this.messages.includelink = value;
      }
    },
    HasSelectedMessage(){
      return this.messages.selectedMessage.entity===undefined;
    },
    HasSelectedMedia(){
      return this.medias.selectedMedia.mediaUrl === undefined && this.medias.selectedMedia.mediaBytes === undefined;
    }
  },
  methods:{
    SelectMessageAction(){
      if(!this.HasSelectedMessage)
        this.GoBack();
      else
        this.ComposeNew();
    },
    ComposeNew(){
      this.messages.selectedMessage = {
        _id: null,
        instagramAccountId: this.data.selectedAccount.account.id,
        accountId: this.data.selectedAccount.account.accountId,
        groupName: UUIDV4(),
        entity:{
          message:'',
          link:''
        },
        type:0,
        dateAdded: new Date()
      }
    },
    MessageRowLength(entity){
      if(entity.message === undefined || entity.message === null || entity.message.split(' ').length < 40)
        return 2;
      return (entity.message.split(' ').length / 22);
    },
    MessageFormat(entity){
      return entity.message + '\n';
    },
    FormatedDate(e){
      return moment(e).format("YYYY-MM-DD HH:mm:ss");
    },
    async onUpload(media){
      let data = await AnalyseFile(media, undefined, undefined);
      this.medias.selectedMedia = data
    },
    SendMessage(){
      var local = this.messages.selectedMessage;
      if(!this.IsLinkIncluded)
        local.entity.link = ''
      this.$emit('send-message', local);
      this.Close();
    },
    SendPost(){
      this.$emit('send-post', this.medias.selectedMedia);
      this.Close();
    },
    GoBack(){
      this.medias.selectedMedia = {};
      this.messages.selectedMessage = {};
      this.messages.includelink = false;
    },
    SelectMessage(e){
      this.messages.selectedMessage = e;
      this.messages.includelink = this.EntityHasLink;
    },
    GetSavedMessages(){
      this.messages.selectedMessage = {};
      if(this.messages.isLoading === true)
        return;
      this.messages.searchResults = [];
      this.messages.searchResults = store.getters.UserMessageLibrary(this.data.selectedAccount.account.id);
      if(this.messages.searchResults === null || this.messages.searchResults === undefined || this.messages.searchResults.length<=0){
        this.messages.isLoading = true;
        store.dispatch('GetSavedMessages', this.data.selectedAccount.account.accountId).then(res=>{
        this.messages.searchResults = store.getters.UserMessageLibrary(this.data.selectedAccount.account.id);
        this.messages.isLoading = false;
        }).catch(err=>{
        this.messages.isLoading = false;
        })
      }     
    },
    SelectMedia(e){
      this.medias.selectedMedia = this.medias.searchResults[e];
    },
    GetMediaFromLibary(){
      let _self = this;
      this.medias.selectedMedia = {}
      this.medias.searchResults = []

      setTimeout(function(){
        if(_self.medias.isLoading === true)
          return;
        
        _self.medias.searchResults = store.getters.UserMediaLibrary(_self.data.selectedAccount.account.id);

        if(_self.medias.searchResults === null || _self.medias.searchResults === undefined || _self.medias.searchResults.length<=0){
          _self.medias.isLoading = true;
          store.dispatch('GetSavedMedias', _self.data.selectedAccount.account.accountId).then(res=>{
            _self.medias.searchResults = store.getters.UserMediaLibrary(_self.data.selectedAccount.account.id);
            _self.medias.isLoading = false;
          }).catch(err=>{
            _self.medias.isLoading = false;
          })
        }
      },50);    
    },
    GetUserMedia(){
      this.medias.selectedMedia = {}
      this.medias.searchResults = []
      let _self = this;
      setTimeout(function(){
        if(_self.medias.isLoading === true)
          return;
        _self.medias.isLoading = true;
        store.dispatch('GetUserMedias', {
          instagramAccountId: _self.data.selectedAccount.account.id, 
          topic: _self.data.selectedAccount.profile.profileTopic
        }).then(res=>{
          _self.medias.searchResults = res.data
          _self.medias.isLoading = false;
        }).catch(err=>{
          console.log(err);
          _self.medias.isLoading = false;
        })
      }, 50);
    },
    onChangeType(e){
      switch(e){
        case 0:
          this.medias.selectedMedia = {}
          break;
        case 1:
          if(this.messages.searchResults.length<=0)
            this.GetSavedMessages();
          this.messages.selectedMessage = {};
          break;
      }
      this.selectedType = e;
    },
    Close(){
      document.getElementById('panel').classList.remove('slide-in')
      document.getElementById('panel').classList.add('slide-out')
       setTimeout(() => {
          this.isActive = false;
          this.$destroy();
          this.$el.remove();
        }, 650);
    }
  }
}
</script>

<style lang="scss">
.compose-editor{
  margin:0 auto;
  margin-top:3em;
  label{
    color:#d9d9d9;
    text-align: center !important;
  }
  input{
    border:none;

  }
  input::placeholder{
    color:#d9d9d98a !important;
  }
  textarea{
    transition: background 0.5s ease;
    background:#232323 !important;
    border:none;
    color:#d9d9d9;
    &:hover{
      transition: all .2s ease-in-out;
      background:#111 !important;
    }
  }
  textarea::placeholder{
    color:#d9d9d98a;
  }
}
.right-side-panel-container {
  .loading-background{
    background:transparent !important;
  }
  position: fixed;
  width: 100%;
  height: 100%;
  top:0;
  z-index: 999;
  box-sizing: border-box;
  * {
    box-sizing: inherit;
  }
  background:transparent;
  .right-panel{
    position: absolute;
    top:0;
    right:0;
    height:100%;
    width:40%;
    background:#141414;
    transform: translateX(100%);
    -webkit-transform: translateX(100%);
    box-shadow: -0.1rem 0 .8rem rgb(5, 5, 5);
    .media-holder{
        width:500px;
        height:500px;
        //margin-left:7em !important;
        margin-left:-.5em !important;
        margin-top:2em;
        transition: all 0.5s ease;
        object-fit: fill !important;
        &:hover{
          transition: all .5s ease-in-out;
          transform: scale(1.1);
        }
      }
    .menu-option{
      .message-results{
        width:100%;
        display: flex;
        margin:0 auto;
        flex-flow: row wrap;
        align-items: center;
        background:transparent;
        margin-left:0.5em;
        max-height:750px;
        //height:750px;
        overflow: auto;
        margin-left:4em;
      }
      margin-top:1em;
      .b-tabs{
        border:none !important;
        .tabs
        {
          border:none!important;
          ul
          {
            margin: 0em auto;
            margin-left:15em;
            li
            { 
              &.is-active{
                a{
                  background:black !important;
                }
              }
              &:hover{
                a{
                  background:black !important;
                  -webkit-transition: background-color 800ms linear;
                  -ms-transition: background-color 800ms linear;
                  transition: background-color 800ms linear;
                }
              }
              a{
                border:none;
                background:#232323;
                color:#d9d9d9;
              }
            }
          }
        }
      }
    }
    .media-results{
      video {
          object-fit: fill !important;
          width:155px;
          height: 145px;
          margin-top:.25em; 
        }
      .media-item{
        object-fit: scale-down !important;
        transition: all 0.5s ease;
        &:hover{
          transition: all .2s ease-in-out;
          cursor: pointer;
            transform: scale(1.1);
            border-radius:0.25em;
        }
      }
      width:100%;
      display: flex;
      margin:0 auto;
      flex-flow: row wrap;
      align-items: center;
      background:transparent;
      margin-left:0.5em;
      max-height:750px;
      //height:750px;
      overflow: auto;
      margin-left:4em;
    }
    .post-footer{
      margin-top:2em;
      margin-left:10em;
      
    }
  }
  .panel-bg{
    background-color: rgba(0, 0, 0, 0.392);
    position: absolute;
    width: 100%;
    height: 100%;
  }
}
.tab-content{
  padding-top:1em !important;
  flex-direction: column !important;
  text-align: center !important;
}
.b-tab-item-sub{
  width:100%;
  margin:0;
  padding:0;
 // background:black;
  
  .button{
    border:1px dashed #cac8c8;
    background: #323232;
    color:#fff;
    &.is-active{
      background:#121212;
    }
    &:active{
      background:#121212;
    }
    &:hover{
      background:#121212;
      -webkit-transition: background-color 400ms linear;
      -ms-transition: background-color 400ms linear;
      transition: background-color 400ms linear;
    }
  }
  margin-bottom:2em;
  margin-left:10em;
}
.slide-in {
    animation: slide-in 0.7s forwards;
    -webkit-animation: slide-in 0.7s forwards;
}

.slide-out {
    animation: slide-out 0.7s forwards;
    -webkit-animation: slide-out 0.7s forwards;
}
 
@keyframes slide-in {
    100% { transform: translateX(0%); }
}

@-webkit-keyframes slide-in {
    100% { -webkit-transform: translateX(0%); }
}
    
@keyframes slide-out {
    0% { transform: translateX(0%); }
    100% { transform: translateX(100%); }
}

@-webkit-keyframes slide-out {
    0% { -webkit-transform: translateX(0%); }
    100% { -webkit-transform: translateX(-100%); }
}
</style>