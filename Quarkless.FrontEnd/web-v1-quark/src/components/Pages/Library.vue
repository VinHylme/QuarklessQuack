<template>
  <div class="container-fluid" style="background:#232323; width:100%;">
    <div class="columns is-mobile" >
      <div class="column is-9" style="padding-left:7em !important; padding-top:3em !important; padding-right:2em !important;">
        <div class="header-lib">
        <p class="title is-4" style="color:#d9d9d9; padding-bottom:1em">Media Library</p>
        <!-- <a class="button is-success is-right">Save Changes</a> -->
        <b-notification :closable="false" v-if="isLoading" style="background:transparent;">
            <b-loading :is-full-page="true" :active.sync="isLoading" :can-cancel="true"></b-loading>
        </b-notification>
        </div>
            <div class="media-section-lib b-tabs-media">
              <b-tabs type="is-toggle"  expanded>
                  <b-tab-item style="margin-left:3.5em;" label="Saved Medias" icon-pack="fas" icon="images">
                    <div class="current-list-container">
                      <p class="subtitle"></p>
                      <div class="image_container zoomable" v-for="(image,index) in mediaLibrary.items" :key=index>
                          <ImageItem
                            v-if="image.mediaBytes"
                            :source="image.mediaBytes"
                            width="300px" 
                            height="300px"
                            :isRounded=true
                            @click="selectMedia(index)"
                            v-bind:style="mediaLibrary.selectedMedia.index == index ? 'border: 2px solid turquoise;' : 'border:none'"
                          />
                          <div class="cross-delete" @click="deleteMedia(index)">
                            <span class="icon">
                              <i class="far fa-2x fa-times-circle"></i>
                            </span>
                          </div>
                      </div>
                    </div>
                  </b-tab-item>
                  <b-tab-item label="Saved Captions" icon-pack="fas" icon="pen">

                  </b-tab-item>
                  <b-tab-item label="Saved Hashtags" icon-pack="fas" icon="hashtag">

                  </b-tab-item>
              </b-tabs>
        </div>
      </div>
       <div class="column is-3">
          <div class="media-section-side-panel">
            <div class="uploader-section">
              <d-drop accept="image/x-png, image/jpeg" :isHidden="false" :isMulti="true" class="dropStyle" @readyUpload="onUpload"></d-drop> 
            </div>
            <hr class="hr-an">
          </div>
      </div>
    </div>
  </div>
</template>

<script>
import ImageItem from '../../components/Objects/ImageItem';
import DropZone from '../../components/Objects/DropZone';
export default {
  components:{
    ImageItem,
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
      uploadedMedia:{
        items:[]
      },
      isLoading:false
    }
  },
  mounted(){
    this.isLoading = true;
    let data = this.$store.dispatch('GetSavedMedias', {accountId: this.$store.getters.User, instagramAccountId: this.$route.params.id}).then(resp=>{
      if(resp.data!==undefined){
        this.mediaLibrary.items = resp.data.data;
      }
      this.isLoading = false;
    }).catch(err=>{
      this.isLoading = false;
    })
  },
  methods:{
    isFileImage(file) {
      return file && file['type'].split('/')[0] === 'image';
    },
    selectMedia(position){
      this.mediaLibrary.selectedMedia = {index: position, item: this.mediaLibrary.items[position]};
    },
    deleteMedia(position){
      let item = this.mediaLibrary.items[position];
      let deleteData = {
        accountId: this.$store.getters.User,
        instagramAccountId: item.instagramAccountId,
        data : item,
      }
      this.mediaLibrary.items = [];
      this.$store.dispatch('DeleteSavedMedia', deleteData).then(resp=>{
        this.mediaLibrary.items = this.$store.getters.UserLibrary(this.$route.params.id);
      })
    },
    uuidv4() 
    {
        return ([1e7]+-1e3+-4e3+-8e3+-1e11).replace(/[018]/g, c =>
          (c ^ crypto.getRandomValues(new Uint8Array(1))[0] & 15 >> c / 4).toString(16)
        )
    },
    async onUpload(data){
      var xl = 0;
      for(; xl < data.requestData.length; xl++){
        await this.readFile(data.requestData[xl]).then(resp=>{
          this.mediaLibrary.items.push({url: resp});
          let mediaData = {
            instagramAccountId : this.$route.params.id,
            groupName : this.uuidv4(),
            mediaType : this.isFileImage(data.requestData[xl]) ? 0 : 1,
            dateAdded : new Date(),
            mediaBytes : resp
          };
          let submitData = {
            accountId : this.$store.getters.User,
            instagramAccountId: mediaData.instagramAccountId,
            data : mediaData
          }
          this.$store.dispatch('SetSavedMedias', submitData).then(resp=>{
            this.mediaLibrary.items = this.$store.getters.UserLibrary(mediaData.instagramAccountId);
            console.log(this.$store.getters.UserLibrary(mediaData.instagramAccountId))
          })
        })            
      }
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

<style lang="scss" >
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
        color:wheat;
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
  background:#212121 !important;
  width:100%;
  height:100%; 
  border: 1px solid #313131;
  box-shadow: -0.01rem 0 .7rem rgba(0, 0, 0, 0.19);
}
.current-list-container{
  width:100%;
  height: 100%;
  display: flex;
  margin:0 auto;
  flex-flow: row wrap;
  align-items: center;
  background:transparent;
  margin-left:3.5em;
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