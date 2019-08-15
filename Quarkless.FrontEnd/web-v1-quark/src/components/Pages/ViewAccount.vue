<template>
<div class="timeline_layout">
  <div class="columns is-mobile is-gapless">
    <div class="column is-0" style="background:#323232;">
      <div class="media_container" style="text-align:center;">   
        <b-tabs position="is-centered" type="is-toggle" v-model="activeTab">
            <b-tab-item label="Upload Media"  icon="upload">
            </b-tab-item>
            <b-tab-item label="Reverse Search" pack="fas" icon="book-open">
              <d-drop accept="image/x-png,image/jpeg, image/bmp" :isMulti="false" class="dropStyle" @readyUpload="onUpload"></d-drop>
              <br>
              <div class="uploadSearchResults">
                 <b-notification v-if="uploadMethodData.isLoading" style="width:100%; height:100px; background:#323232;" :closable="false">
                    <b-loading :is-full-page="false" :active.sync="uploadMethodData.isLoading" :can-cancel="false">
                        <b-icon
                          pack="fas"
                          icon="circle-notch"
                          size="is-large"
                          custom-class="fa-spin">
                        </b-icon>
                    </b-loading>
                </b-notification>
                <div id="items" class="uploadSearchResults">
                 <div v-for="(image,index) in uploadMethodData.searchMediaItems" :key="image+'_'+index" class="image_container zoomable dropitem" >
                   <ImageItem v-if="image.url" :source="image.url" width="120px" height="120px" />
                   <div class="overlayTick">
                      <span class="icon has-text-info">
                        <i class="fas fa-lg fa-check-circle"></i>
                      </span>
                   </div>
                  </div>
                  </div>
              </div>
            </b-tab-item>
            <b-tab-item label="Google" pack="fas" icon="google-drive">
              
            </b-tab-item>
        </b-tabs>
      </div>
    </div>
    <div class="column is-12">
      <div class="timeline_container">
        <vue-scheduler :events="this.$store.getters.UserTimeline" :event-display="eventDisplay"/>
        <div id="scheduler" class="overlay_timeline">
          <p class="subtitle is-5">Drop your media to schedule your post</p>     
        </div>
      </div>
    </div>
  </div>
  <b-notification style="background-color:#121212; width:0; height:0;" :closable="false">
      <b-loading :is-full-page="isFullPage" :active.sync="isLoading" :can-cancel="false"></b-loading>
  </b-notification>
</div>  
</template>

<script>
import moment from 'moment';
import DropZone from '../Objects/DropZone';
import ImageItem from '../Objects/ImageItem';
import draggable from 'vuedraggable';
import { Sortable, MultiDrag } from 'sortablejs';
import Vue from 'vue';
Sortable.mount(new MultiDrag());
export default {
  components:{
    'd-drop': DropZone,
    'd-drag': draggable,
    ImageItem
  },
  data(){
    return {
      IsAdmin:false,
      TimelineData:[],
      timer:'',
      isLoading: false,
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
      this.isLoading = true
      this.$store.dispatch('GetUsersTimeline',this.$route.params.id).then(res=> { this.isLoading = false });
      this.TimelineData = this.$store.getters.UserTimeline;
      this.timer = setInterval(this.loadData, 15000)
  },
  mounted(){
      this.IsAdmin = this.$store.getters.UserRole == 'Admin';
      this.profile = this.$store.getters.UserProfile;
      var vm = this;
      this.$nextTick(() => {
        let count = 0;
        const sortable = Sortable.create(document.getElementById('items') , {
          group:{
            name:'items'
          },
          multiDrag: true,
          selectedClass: "selected",
          dragClass: "isDragOn",  // Class name for the dragging item
          animation:100,
          onSelect: function(evt) {
            evt.item;
          },
          onDeselect: function(evt) {
            evt.item;
          },
          onStart:function(evt){
            evt.oldIndex;  // element index within parent
            document.getElementById("scheduler").style.display = "block";
          },
          onEnd:function(evt){
            var itemEl = evt.item;  // dragged HTMLElement
            evt.to;    // target list
            evt.from;  // previous list
            evt.oldIndex;  // element's old index within old parent
            evt.newIndex;  // element's new index within new parent
            evt.oldDraggableIndex; // element's old index within old parent, only counting draggable elements
            evt.newDraggableIndex; // element's new index within new parent, only counting draggable elements
            evt.clone // the clone element
            evt.pullMode;  // when item is in another sortable: `"clone"` if cloning, `true` if moving
            document.getElementById("scheduler").style.display = "none";
          },
          onMove:function(/**Event*/evt, /**Event*/originalEvent){
            var currentSearchItems = getValues(sortable);
          }
        });

        const othersort = Sortable.create(document.getElementById('scheduler'), {
          group: {
            name: 'dropzone_',
            put: ['items']
          },
          sort:false,
          animation: 150,
          dragoverBubble:false,
          onAdd:function(evt){
            tryToAdd(evt);
          }
        });
        function tryToAdd(evt){
          var currentListToUpload = getValues(othersort);          
          if(currentListToUpload.length > 8){
            Vue.prototype.$toast.open({
                message: 'You can currently only upload upto 8 images or videos',
                type: 'is-info'
              });      
              othersort.option("revert",true);
              sortable.option("revert", true);
          }
          else{
            vm.$emit('newPost', { Items: currentListToUpload, Context:evt }); 
          }
        }

        this.$on('newPost', function(value) {
          this.prePostData = value.Items;
          this.handlePost(value.Context);
        });

        this.$on('failedPost', function(value){
          othersort.option("revert", true);
          sortable.option("revert", true);   
        });
        this.$on('successPost', function(data){
          data.context.item.parentNode.removeChild(data.context.item);
        });

        document.getElementById("scheduler").style.display = "none";
        function getValues(element){
          var elementsFound = []
          var childs = element.el.childNodes;
          var i = 0;
          for(i; i < childs.length; i++){
            var child = childs[i];
            if(child.firstChild!==undefined && child.firstChild.nodeName == "FIGURE"){
              let arr = [...child.firstChild.children];
              var j = 0;
              for(j; j < arr.length; j++){
                if(arr[j]!==undefined && arr[j].nodeName == "IMG"){
                  elementsFound.push(arr[j].dataset.url);
                }
              }
            }
          }
          return elementsFound;
        }
      });
  },
  watch:{
    prePostChange(value){
    }
  },
  methods: {
    handlePost(evt){
      console.log(this.prePostData);
      this.$emit('failedPost', { context: evt, response: true } );
    },
    selectMediaFromSearch(index){
      if(this.uploadMethodData.searchMediaItems.reduce((a, c) => c.isSelected ? ++a : a, 0)<=8 || this.uploadMethodData.searchMediaItems[index].isSelected===true){
        this.uploadMethodData.searchMediaItems[index].isSelected = !this.uploadMethodData.searchMediaItems[index].isSelected;
      }
      else{
          Vue.prototype.$toast.open({
            message: 'You can currently only upload upto 8 images or videos',
            type: 'is-info'
          })
      }
    },
     onUpload(e){
      this.uploadMethodData.currentPage = 1;
      const data = {instaId: this.profile.instagramAccountId, profileId: this.profile._id, formData:e};
      this.$store.dispatch('UploadFileForProfile',data).then(resp=>{
        this.uploadMethodData.urls = resp.data.urls;
        this.searchImage(this.uploadMethodData.urls);
      })
    },
    searchImage(data){
      this.uploadMethodData.isLoading = true;
      this.$store.dispatch('SimilarSearch',{urls:this.uploadMethodData.urls, limit:this.uploadMethodData.perPage, offset:this.uploadMethodData.currentPage}).then(
        res=> {
          this.uploadMethodData.searchMediaItems = []
          res.data.medias.forEach((item=> this.uploadMethodData.searchMediaItems.push(
          {
            url:item.mediaUrl[0],
            isSelected:false,
            objectData:item
          })));

          this.uploadMethodData.isLoading = false;
          this.uploadMethodData.finishedSearching = true;
          this.uploadMethodData.originalSet = this.uploadMethodData.searchMediaItems;
        }).catch(err=>{this.isLoading = false})
    },
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

@import "../../../references/v-calendar-scheduler/scss/main";
.media_container{
  width:100% !important;
  height:100vh !important;
  background-color:#323232 !important;
  padding-top:2em;
  .b-tabs{
    color:white;
    .tabs{
      color:white;
      a{
        background:#d9d9d9;
      }
      li{
        &.is-active{
          a{
            background:#4CAF50;
            border:none;
          }
        }
      }
    }
  }
}
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
html,body{
 overflow: auto;
  ::-webkit-scrollbar {
      display: none;
  }
}
.dropStyle{
  margin-left:0em;
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
