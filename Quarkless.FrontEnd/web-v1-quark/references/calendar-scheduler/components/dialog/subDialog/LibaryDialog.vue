<template>
  <transition name="zoom-out">
    <div class="v-cal-dialog" v-if="isActive">
    <div class="v-cal-dialog__bg" @click="cancel"></div>
      <div class="is-media-libary" :style="type===2 ? 'max-width:100%' : ''">
         <header class="v-cal-dialog-card__header">
            <h5 class="v-cal-dialog__title">{{ title }}</h5>
          </header>
          <section class="body-container" :style="type===2 ? 'max-width:100%; width:90%; margin-left:-13em;' : ''">
           <div class="similarImages_Container is-long ">
                 <div v-for="(media,index) in mediaLibary" :key="media+'_'+index" 
				 :class="type===2 ? 'image_container is-normal zoomable' : 'image_container is-small zoomable'">
                        <ImageItem
                          v-if="media.mediaBytes && media.mediaType === 1"
                          :source="media.mediaBytes"
                          :width="'100px'" 
                          :height="'100px'" 
                          :isRounded=false
                          @click="AddToSelectList(media)"
                          v-bind:style="ItemIsSelected(media) ? 'border: 2px solid turquoise;' : 'border:none'"
                        />
						<video @click="AddToSelectList(media)" 
						v-bind:style="ItemIsSelected(media) ? [borderStyle,videoStyle] : [videoStyle]" 
						v-else-if="media.mediaType === 2" controls="controls" preload="metadata" :src="media.mediaBytes+'#t=0.5'"></video>
                    </div>
              </div>
			 	<a v-if="type===2"  class="button is-success is-bottom-right-button" 
				 :disabled="SelectedList.length <= 0" @click="emitMedia">Confirm</a>
          </section>
      </div>
       <div v-if="type !==2" class="is-media-searcher">
         <header class="v-cal-dialog-card__header">
            <h5 class="v-cal-dialog__title">Image Search</h5>
            <b-tooltip label="Search will be slightly slower, but you will get more accurate results"
            type="is-danger"
            position="is-top">
              <b-switch :rounded="false" :outlined="true" type="is-warning" style="margin-left:2em;" v-model="isMoreAccurate">More Accurate Results?</b-switch>
            </b-tooltip>
            <button type="button" class="v-cal-dialog__close" @click="cancel"></button>
          </header>
          <section class="v-cal-dialog-card__body">
              <b-notification style="background:transparent; margin-top:2em; height:90px" :closable="false" v-if="searchMediaResults.isSearching">
                  <b-loading :is-full-page="false" :active.sync="searchMediaResults.isSearching" :can-cancel="false">
                      <b-icon
                          pack="fas"
                          icon="sync-alt"
                          size="is-large"
                          custom-class="fa-spin">
                      </b-icon>
                  </b-loading>
              </b-notification>
              <d-drop v-if="!searchMediaResults.finishedSearching" acceptFile="image/x-png, image/jpeg" :isHidden="false" :isMulti="true" @readyUpload="onUpload"></d-drop> 
              <div class="similarImages_Container">
                 <div v-for="(image,index) in searchMediaResults.displayableList" :key="image+'_'+index" class="image_container is-normal zoomable">
						<ImageItem
                          v-if="image.url"
                          :source="image.url"
                          width="200px" 
                          height="200px"
                          :isRounded=false
                          @click="AddToSelectList(MapSearchItem(image.url))"
                          v-bind:style="ItemIsSelected(MapSearchItem(image.url)) ? [borderStyle] : []"
                        />
                    </div>
              </div>
          </section>
          <hr class="hr-sep-lib">
            <b-pagination
              v-if="searchMediaResults.finishedSearching"
              class="pagination-style"
              @change="updatePage"
              :total="searchMediaResults.total"
              :current.sync="searchMediaResults.currentPage"
              :range-before="searchMediaResults.rangeBefore"
              :range-after="searchMediaResults.rangeAfter"
              order="is-centered"
              size="is-medium"
              :simple="false"
              :rounded="false"
              :per-page="searchMediaResults.perPage"
              aria-next-label="Next page"
              aria-previous-label="Previous page"
              aria-page-label="Page"
              aria-current-label="Current page">
            </b-pagination> 
            <a class="button is-success is-centered-button" :disabled="SelectedList.length<=0" @click="emitMedia">Confirm</a>
      </div> 
    </div>
  </transition>
</template>

<script>
import DropZone from '../../../../../src/components/Objects/DropZone';
import ImageItem from '../../../../../src/components/Objects/ImageItem';
import state from '../../../../../src/State';
import route from '../../../../../src/Route';

import Vue from 'vue';
export default {
  components:{
    'd-drop' : DropZone,
    ImageItem
  },
  props:{
    title:String,
	profile:Object,
	type: Number,
  },
  data(){
    return {
	  borderStyle:{
		  border: '2px solid turquoise'
	  },
	  videoStyle:{
		  objectFit:'fill',
		  width: this.type === 2 ? '200px' : '100px',
		  height: this.type === 2 ? '200px' : '100px'
	  },
      isActive: false,
	  isMoreAccurate:false,
	  retreiveData:[],
      searchMediaResults:{
        finishedSearching:false,
        isSearching:false,
        items:[],
        rangeBefore:5,
        rangeAfter:5,
        currentPage:0,
        displayableList:[],
        perPage:25,
        total:100,
        uploadedUrls:[]
	  },
	  selectedMedias:[],
      styleBorder:{
        border: '2px solid turquoise'
      }
    }
  },
  beforeMount(){
    document.body.appendChild(this.$el);
  },
  mounted(){
    this.isActive = true;
	let id = route.app.$route.params.id;
	state.dispatch('GetSavedMedias', state.getters.User).then(resp=>{
	if(resp.data!==undefined){
		this.retreiveData = resp.data.data;
	}
	// this.isLoading = false;
	}).catch(err=>{
	// this.isLoading = false;
	})
  },
  computed:{
	  mediaLibary:{
		  get(){
			  if(this.type > 2)
				  return this.retreiveData;
			  else
			  	 return this.retreiveData.filter(x=>x.mediaType === this.type);
		  },
		  set(value){
			  return value;
		  }
	  },
	  SelectedList(){
		  return this.selectedMedias;
	  },
	  AllowedMax(){
		  switch(this.type){
			  case 1: return 1
			  case 2: return 1
			  case 3: return 10
		  }
	  }
  },
  methods:{
	MapSearchItem(item){
		return {
			mediaUrl: item,
			mediaType: 1
		}
	},
	ItemIsSelected(item){
		if(this.SelectedList.length<=0) return false;
		return this.SelectedList.findIndex(res => {
			if(item.mediaUrl!==null && item.mediaUrl !== undefined)
				return res.mediaUrl === item.mediaUrl
			else
				return res.mediaBytes === item.mediaBytes
		}) > -1
	},
	AddToSelectList(item){
		if(this.ItemIsSelected(item)){
			this.selectedMedias = this.selectedMedias.filter(res=>{
				if(res.mediaUrl !== item.mediaUrl)
					return res;
				if(res.mediaBytes !== item.mediaBytes)
					return res;
			})
		}
		else{
			if(this.SelectedList.length > this.AllowedMax - 1)
				this.selectedMedias.pop(item)
			this.selectedMedias.push(item);
		}
	},
    updatePage(e){
      this.searchMediaResults.currentPage = e;
      this.searchMediaResults.displayableList = [];
      var millisecondsToWait = 250;
      let _this = this;
      this.searchMediaResults.isSearching = true;
      setTimeout(function() {
        _this.NextLoad();
      }, millisecondsToWait);
    },
    NextLoad(){
      let currentSet =  this.searchMediaResults.currentPage * this.searchMediaResults.perPage;
      this.searchMediaResults.displayableList = this.searchMediaResults.items.slice(currentSet, currentSet + this.searchMediaResults.perPage)
      this.searchMediaResults.isSearching = false;
    },
    emitMedia(){
      this.$emit('media-omitted', this.SelectedList.map(res=> { 
		  return {
			type: res.mediaType,
			url: res.mediaUrl === null || res.mediaUrl === undefined ? res.mediaBytes : res.mediaUrl
	  	  }
	  }));
      this.close();
	},
    onUpload(e){
      this.searchMediaResults.currentPage = 1;
      const data = { instaId: this.profile.instagramAccountId, profileId: this.profile._id, formData:e.formData };
      state.dispatch('UploadFileForProfile', data).then(resp=>{
        this.searchMediaResults.uploadedUrls = resp.data.urls;
        this.searchImage(this.searchMediaResults.uploadedUrls);
      }).catch(err=>{
        Vue.prototype.$toast.open({
            message: 'Oops, looks like something went wrong on our end',
            type: 'is-danger'
        })
      })
    },
    searchImage(data){
      this.searchMediaResults.isSearching = true;
      state.dispatch('SimilarSearch',{urls:data, limit:-1, offset:0, moreAccurate:this.isMoreAccurate}).then(
        res=> {
          this.searchMediaResults.items = []
          res.data.medias.forEach((item=> this.searchMediaResults.items.push({url:item.mediaUrl[0]})));
          let currentSet =  this.searchMediaResults.currentPage * this.searchMediaResults.perPage;
          this.searchMediaResults.displayableList = this.searchMediaResults.items.slice(currentSet, currentSet + this.searchMediaResults.perPage)
          this.searchMediaResults.isSearching = false;
          this.searchMediaResults.finishedSearching = true;
        }).catch(err=>{this.searchMediaResults.isSearching = false})
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
.dropStyle{
  margin: 0 auto;
}
.body-container{
  overflow: auto;
  height:680px; 
  max-width: 400px;
  background:transparent; 
  margin-left: 11.5em;
  color:#d9d9d9;
}
.is-centered-button{
  margin:0 auto;
  float:right;
  margin-top:1.4em;
}
.is-bottom-right-button{
	position: absolute;
	right:0;
	bottom:0;
}
.v-cal-dialog-card__body{
  height:100%;
  margin: 0 auto !important;
  //margin-top:1em;
  background:rgba(0,0,0,0.2);

}
.hr-sep-lib{
  height: 1px;
  background:#232323;
}
.is-media-libary{
  position:absolute;
  left:17em;
  top:5em;
  bottom:0;
  background: #121212;
  border-radius:0.5em;
  width:75% !important;
  height:85% !important;
  max-width: 25%;
  padding-top:1em;
  padding-right:2em !important;
  padding-left:2em !important;
  border: 1px solid #323232;
  box-shadow: -0.3rem 0 1rem #000;

  //box-shadow: none;
 // box-shadow: 4px 2px 3px 5px rgba(30, 30, 30, 0.1) !important;
}
.is-media-searcher{
  position:absolute;
  top:5em;
  right:17em;
  background: #121212;
  border-radius:0.5em;
  width:75% !important;
  height:85% !important;
  max-width: 45%;
  padding-top:1em;
  padding-right:2em !important;
  padding-left:2em !important;
  border: 1px solid #323232;
  box-shadow: -0.3rem 0 1rem #000;
}
.similarImages_Container{
  margin-left:3em;
  margin-top:1em;
}

.image_container{
  position: relative;
  //margin-top: 25px;
  float:left;
  &.is-small{
    width: 100px;
    height: 100px;
  }
  &.is-normal{
    width: 200px;
    height: 200px;	  
  }
  transition: all .2s ease-in-out;
  &:hover{
    display: block;
    //background: rgba(0, 0, 0, .3);
      &.zoomable{
        transform: scale(1.1);
        border-radius:0.25em;
    }
    .title {
      top: 90px;
    }
    .is-overlayed{
      opacity: 1;
    }
  }
  
}
.pagination-style
{
  margin-top:-1em;
  color:white;
  .pagination-next{
    background:white;
  }
  .pagination-previous{
    background:white;
  }
  .pagination-list{
    .pagination-link{
      background:white !important;
      opacity:.5;
      &.is-current{
        opacity: 1;
        color:#232323;
      }
    }
  }
}
</style>