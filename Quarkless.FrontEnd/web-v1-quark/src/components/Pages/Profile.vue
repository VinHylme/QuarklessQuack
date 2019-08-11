<template>
  <div class="container is-fluid" style="padding-top:0.7em; padding-right:0em;">
    <div class="columns is-variable is-4-mobile is-0-tablet is-8-desktop is-5-widescreen is-5-fullhd">
      <div class="column is-12 profile_container">
        <b-steps type="is-success" size="is-medium" v-model="activeStep" :animated="true" :has-navigation="true">
          <b-step-item label="Profile" :clickable="true" icon="account-plus">
            <section class="box container is-profile">
               <b-field grouped>
                  <b-field class="name" label="Profile Name" :type="!canEdit ?'is-success' : 'is-primary'" >
                    <b-input :value="profile.name" maxlength="30" size="is-medium" :disabled="!canEdit"></b-input>
                  </b-field>
                  <b-field class="descr" label="Profile Description" :type="!canEdit ?'is-success' : 'is-primary'" >
                    <b-input :value="profile.description" maxlength="100" size="is-medium" :disabled="!canEdit"></b-input>
                  </b-field>
                  <b-field class="lang" label="Profile Language" :type="!canEdit ?'is-success' : 'is-primary'" >
                     <b-select :disabled="!canEdit" :value="profile.language" size="is-medium">
                      <option v-for="(lang,index) in config.languages" :key="index" :value="lang">{{lang}}</option>
                    </b-select>
                  </b-field>
              </b-field>
            </section>
            <section class="section topic_area">
              <div class="box" style="background:#1f1f1f;" >
                <div style="display:inline-flex; flex-flow: row wrap; align-items: center;">
                <b-field position="is-centered" class="is-dark" label="Topic" :type="!canEdit ?'is-success' : 'is-primary'">
                  <b-autocomplete
                  size="is-medium"
                  style="width:550px; margin: 0 auto"
                  icon="magnify"
                  v-model="profile.topics.topicFriendlyName"
                  placeholder="What is your business about? e.g. Ecommerce"
                  :keep-first="false"
                  :open-on-focus="true"
                  :data="filteredDataObj"
                  field="subCategories"
                  @select="searchReleatedTopics"
                  @keyup.enter="searchReleatedTopics">
                </b-autocomplete>
                </b-field>
                <a @click="isTip1Active = !isTip1Active;" :disabled="isTip1Active"  style="margin-top:1.7em; margin-left:1em">
                  <b-tooltip label="View tip" type="is-dark">
                    <b-icon pack="fas" icon="question-circle" size="is-medium" type="is-light"></b-icon>
                  </b-tooltip>
                </a>
                </div>
                 <b-notification
                    :active.sync="isTip1Active"
                    style="width:40%; margin-left:0"
                    position="is-bottom-left"
                    size="is-small"
                    type="is-info"
                    has-icon
                    aria-close-label="Close notification">
                    You can use the above search to find the topic which most matches your business or expresses yourself
                </b-notification>
                <b-field position="is-centered" class="is-dark is-small" label="Selected topics "></b-field>
                  <b-notification
                   :active.sync="isTip1Active"
                    style="width:50%; margin-left:55.5em"
                    position="is-bottom-left"
                    size="is-small"
                    type="is-warning"
                    has-icon
                    aria-close-label="Close notification">
                    The keywords you use here are very important, you can specify a topic which most relates to your niche, each of those topics will also have many sub topics which will help our team build your profile to their best ability.
                </b-notification>
                <div class="box subtopics_container" style="background:#121212;">
                  <div v-for="(subTopic,index) in profile.topics.subTopics" :key="subTopic+'_'+index" class="field is-grouped is-grouped-multiline" style="padding:0.4em;">
                    <div class="control">
                      <div class="tags has-addons">
                        <a @click="showSubTopicReleated(subTopic,index)" style="text-decoration: none;" class="tag is-medium is-danger">
                          <b-tooltip label="Expand and show all" type="is-dark">
                          <b-icon pack="fas" icon="tag"></b-icon>
                          </b-tooltip>
                        </a>
                        <span class="tag is-medium is-dark">{{subTopic.topicName}}</span>
                        <a class="tag is-medium is-delete" @click="deleteTopic(subTopic)"></a>
                      </div>
                    </div>
                  </div> 
                  <div class="field has-addons" style="margin-top:-0.8em;">
                    <div class="control">
                      <input class="input is-tag" @keyup.enter="addTopic(undefined)" v-model="toAddTopic" type="text" placeholder="Add More">
                    </div>
                    <div class="control">
                    <button @click="addTopic(undefined)" class="button is-success">
                        <b-icon icon="plus"></b-icon>
                    </button>
                    </div>
                  </div>
                </div>               
                <b-field v-if="isExpandedSubTopics" position="is-centered" class="is-dark is-small" :label="'Showing Results For: '+selectedTopic.topicName"></b-field>
                <div class="box subtopics_container" style="background:#121212;" v-if="isExpandedSubTopics">
                  <b-notification v-if="searchingTopics" style="width:100%; background:transparent; height:150px;" :closable="false">
                    <b-loading :is-full-page="false" :active.sync="searchingTopics">
                      <b-icon
                          pack="fas"
                          icon="spinner"
                          size="is-large"
                          custom-class="fa-spin">
                      </b-icon>
                    </b-loading>
                  </b-notification>
                  <div class="closeSubTopics">
                    <a @click="isExpandedSubTopics=false">
                      <b-icon type="is-dark" pack="fas" size="is-default" icon="times-circle">
                      </b-icon>
                    </a>
                  </div>
                   <div v-for="(topic,index) in selectedTopic.relatedTopics" :key="index" class="control"  style="padding:0.4em;">
                      <div class="tags has-addons">
                        <span class="tag is-medium is-twitter">{{topic}}</span>
                        <a @click="profile.topics.subTopics[profile.topics.subTopics.findIndex((ob)=>ob.topicName === selectedTopic.topicName)].relatedTopics.splice(index,1)" class="tag is-medium is-delete"></a>
                      </div>
                    </div>
                </div>
                <div v-if="searchReleatedTopic.length>0">
                  <b-field position="is-centered" class="is-dark is-small" label="Releated topics "></b-field>
                  <div class="box subtopics_container" style="background:#121212;">
                    <div v-for="(subTopic,index) in displayedReleatedTopic" :key="subTopic+'_'+index" class="field is-grouped is-grouped-multiline" style="padding:0.4em;">
                      <div class="control">
                        <div class="tags has-addons">
                          <span class="tag is-large is-dark">{{subTopic}}</span>
                            <a @click="addTopic(subTopic)" class="tag is-large is-success">
                              <b-tooltip label="Add this to your selected list" type="is-twitter" position="is-top">
                                <b-icon icon="plus"></b-icon>
                              </b-tooltip>
                            </a>
                        </div>
                      </div>
                    </div>
                  </div>
                </div>
                <div>
                  <br>
                  <div class="buttons has-addons is-centered">
                    <span class="button is-dark is-medium" @click="prevset()">Back</span>
                    <span class="button is-dark is-medium" @click="nextset()">Next</span>
                  </div>
                </div>
              </div>
            </section>
          </b-step-item>     
          <b-step-item label="Target" :clickable="true" icon="compass">
            <div class="box" style="background:#1f1f1f;">
               <b-field label="User Target" class="is-dark">
                    <b-taginput
                        :before-adding="evaluate"
                        :on-paste-separators="[',','@','-',' ']"
                        :confirm-key-codes="[32, 13]"	
                        type="is-twitter"
                        icon="at"
                        v-model="profile.userTargetList"
                        placeholder="Add a user to watch for"
                        size="is-medium">
                    </b-taginput>
                </b-field>
            </div>
            <div class="box" style="background:#1f1f1f;">
            <b-field label="Current Locations Targeted" class="is-dark">
              <b-taginput
                  size="is-medium"
                  type="is-dark"
                  v-model="profile.locationTargetList"
                  :data="searchItems"
                  autocomplete
                  :allow-new="false"
                  field="address"
                  icon="map"
                  placeholder="Add a location"
                  @typing="performAutoCompletePlacesSearch">
              </b-taginput>
            </b-field>
            </div>
          </b-step-item>
          <b-step-item label="Theme" :clickable="true" icon="palette">
            <div class="box" style="background:#1f1f1f; color:white;">
              <b-field grouped style="margin:0 auto; width:100%;">
                  <b-field class="is-dark" style="margin:0 auto;" label="Theme Name" :type="!canEdit ?'is-success' : 'is-primary'" >
                    <b-input :value="profile.theme.name" maxlength="30" size="is-medium" :disabled="!canEdit"></b-input>
                  </b-field>            
              </b-field>
            </div>
             <b-collapse :open="true" animation="none"
                aria-id="contentIdForA11y2"
                class="card">
            <div
                style="background:#121212; border:none;"
                slot="trigger"
                slot-scope="props"
                class="card-header"
                role="button"
                aria-controls="contentIdForA11y2">
                 <p class="card-header-title" style="color:#696969;">
                    Colors
                </p>
                  <a class="card-header-icon">
                    <b-icon
                        :icon="props.open ? 'menu-down' : 'menu-up'">
                    </b-icon>
                </a>
            </div>
            <div class="card-content" style="background:#1f1f1f">
            <div class="box" style="background:#1f1f1f; color:white;">
              <div class="box" style="background:#292929;">            
                <b-field class="is-dark" label="Style your profile to suit you"></b-field>
              </div>
              <div class="box" style="background:#292929; width:40%; margin:0 auto; color:#d9d9d9;">
                <b-field class="is-dark">Colors Currently using</b-field>
                <c-color @openColorDialog="openColorDialog" v-for="(color,index) in profile.theme.colors" :key=index :color="color" :id="index"></c-color>
                <b class="hr"></b>
                <button @click="openColorDialog" class="button is-danger">
                <b-icon icon="feather"></b-icon>
                <span>Add</span>
              </button>
              </div>
              <b-field :label="'Follow the color theme('+profile.theme.percentage+'%)' " class="is-dark">
                  <input type="range" min="0" max="90" v-model="profile.theme.percentage" class="slider" id="myRange">
              </b-field>
              <b-modal :active.sync="isModalActive">
                <div class="content">
                  <ColorCard :id="currentlySelectedColor.id" :colorsAllowed="config.colorsAllowed" @deleteColor="deleteColor" @updateColor="updateColor" @addColor="addColor" :color="{r:currentlySelectedColor.color.red, g:currentlySelectedColor.color.green, b:currentlySelectedColor.color.blue}" :name="currentlySelectedColor.color.name"></ColorCard>
                </div>
              </b-modal>
            </div>
            </div>
            </b-collapse>
            <br>
            <b-collapse :open="false" aria-id="contentIdForA11y3" class="card"  animation="none">
              <div
                style="background:#121212; border:none;"
                slot="trigger"
                slot-scope="props"
                class="card-header"
                role="button"
                aria-controls="contentIdForA11y3">
                 <p class="card-header-title" style="color:#696969;">
                    Similar Images
                </p>
                  <a class="card-header-icon">
                    <b-icon
                        :icon="props.open ? 'menu-down' : 'menu-up'">
                    </b-icon>
                </a>
            </div>
           <div ref="content" class="card-content" style="background:#1f1f1f">
            <div class="box" style="background:#1f1f1f; color:white;">
              <div class="box" style="background:#292929; border:none;">            
                <b-field class="is-dark" label="Here you can tell our agent what types images you would like to have on your profile"></b-field>
              </div>
              <d-drop @readyUpload="onUpload"></d-drop>
                <b-notification style="background:transparent;" :closable="false">
                    <b-loading :is-full-page="true" :active.sync="isLoading" :can-cancel="false">
                        <b-icon
                            pack="fas"
                            icon="sync-alt"
                            size="is-large"
                            custom-class="fa-spin">
                        </b-icon>
                    </b-loading>
                </b-notification>
                <b-field class="is-dark" label="Search Results"></b-field>
                <div v-if="finishedSearching" class="searchResults" id="searchSection">
                  <div class="similarImages_Container">
                    <div v-for="(image,index) in searchMediaItems" :key="image+'_'+index" >
                        <!-- <img v-if="evaluateImage(image.url)" class="image zoomable" :src="image.url" alt=""> -->
                        <ImageItem
                        class="image zoomable"
                        v-if="image.url"
                        :source="image.url"
                        />
                    </div>
                  </div>     
              </div>
               <b class="hr anim"></b>
                  <br>
                   <!-- <d-page @pageUpdate="updatePage" :currentPage="this.currentPageAt" :perPage="25" :total="200" :isRounded="false" :rangeBefore="5" :rangeAfter="5"/> -->
                    <b-pagination
                      @change="updatePage"
                      :total="total"
                      :current.sync="currentPage"
                      :range-before="rangeBefore"
                      :range-after="rangeAfter"
                      order="is-centered"
                      size="is-medium"
                      :simple="false"
                      :rounded="isRounded"
                      :per-page="perPage"
                      aria-next-label="Next page"
                      aria-previous-label="Previous page"
                      aria-page-label="Page"
                      aria-current-label="Current page">
                    </b-pagination>                  
                   <br>
              <b class="hr anim"></b>
                <b-field class="is-dark" label="Images you have so far"></b-field>
                <div class="similarImages_Container">
                   <div  v-for="(image,index) in profile.theme.imagesLike" :key="image+'_'+index" >
                  <!-- <img class="image zoomable" v-for="(image,index) in profile.theme.imagesLike" :key=index :src="image.url" alt=""> -->
                   <ImageItem 
                        class="image zoomable"
                        :id="image+'_'+index"
                        v-if="image.url"
                        :source="image.url"
                        />
                   </div>
                </div>
            </div>
           </div>
            </b-collapse>
          </b-step-item>
          <b-step-item label="Additional" :clickable="true" icon="atom">
              {{profile.additionalConfigurations}}
          </b-step-item>
      </b-steps>
        <p class="control" style="float:right;">
            <a class="button is-success">
              Save changes
            </a>
          </p>
      </div>
    </div>   
  </div>
</template>
<script>
import debounce from 'lodash/debounce'
import ColorCard from '../Objects/ColorCard';
import Color from '../Objects/Colors';
import DropZone from '../Objects/DropZone';
import Pagination from '../Objects/Pagination';
import ImageItem from '../Objects/ImageItem';

export default {
  components:{
    ImageItem,
    ColorCard,
    'c-color':Color,
    'd-drop':DropZone,
    'd-page':Pagination
  },
  data(){
    return{
      perPage:30,
      total:200,
      isRounded:false,
      rangeBefore:5,
      rangeAfter:5,
      isLoading:false,
      profile:{},
      searchQuery:'',
      activeStep: 0,
      canEdit :true,
      config:{},
      objectResp:{},
      searchItems:[],
      isFetching: false,
      selected:null,
      isModalActive:false,
      currentlySelectedColor:{color:{red:0,green:0,blue:0, name:undefined}, id:0},
      searchMediaItems:[],
      urls:[],
      currentPage: 1,
      finishedSearching:false,
      searchReleatedTopic:[],
      displayedReleatedTopic:[],
      currentPage:1,
      toAddTopic:'',
      isTip1Active:false,
      isExpandedSubTopics:false,
      selectedTopic:[],
      searchingTopics:false
    }
  },
  created(){
    this.profile = this.$store.getters.UserProfiles[this.$store.getters.UserProfiles.findIndex(_=>_._id == this.$route.params.id)];
    this.profile.topics.topicFriendlyName = this.profile.topics.topicFriendlyName.replace(/^\w/, c => c.toUpperCase());
  },
  mounted(){
    this.$store.dispatch('GetProfileConfig').then(con => this.config = this.$store.getters.GetProfileConfig);
    this.searchReleatedTopics(this.profile.topics.topicFriendlyName);
  },
  computed:{
    filteredDataObj() {
      var total = []
      if(this.config.topics!==undefined){
        this.config.topics.forEach( (item) =>
        {
          //total.push(item.categoryName);
          item.subCategories.forEach((subItems)=>total.push(subItems))
        });
        return total.filter((option) => {
          return option.toString()
          .toLowerCase()
          .indexOf(this.profile.topics.topicFriendlyName.toLowerCase()) >= 0
        })
      }
    },
    filterReleatedTopics(){
      // let subtopics = this.profile.topics.subTopics;
      //  return this.searchReleatedTopic.filter((val,index)=> {
      //    return subtopics.indexOf((ob) => ob.topicName.toLowerCase() == val.toLowerCase()) == -1
      //  });
      // //return this.searchReleatedTopic.filter(val=>!this.profile.topics.subTopics.includes(val))
      var index;
      for(var x = 0 ; x < this.profile.topics.subTopics.length; x++){
        index = this.searchReleatedTopic.indexOf(this.profile.topics.subTopics[x].topicName);
        if(index > -1){
          this.searchReleatedTopic.splice(index, 1);
        }
      }
      return this.searchReleatedTopic;
    }
  },
  methods:{
    showSubTopicReleated(item, index){
      this.isExpandedSubTopics = true;
      if(this.profile.topics.subTopics[index].relatedTopics.length<=0){
        this.searchingTopics = true;
        this.$store.dispatch('ReleatedTopics', {instaId: this.profile.instagramAccountId, topic:item.topicName}).then(resp=>{
          this.profile.topics.subTopics[index].relatedTopics = resp.data.relatedTopics;
          this.searchingTopics = false;
        })
      }
      this.selectedTopic = this.profile.topics.subTopics[index];
      //this.searchingTopics = false;
    },
    deleteTopic(e){
      var index = this.profile.topics.subTopics.findIndex((ob=>ob===e));
      this.profile.topics.subTopics.splice(index,1);
    },
    addTopic(e){
      if(e!==undefined){
        if(e){
          if(this.profile.topics.subTopics.findIndex((ob)=>ob.topicName === e) < 0)
            this.profile.topics.subTopics.push({topicName:e, relatedTopics:[] })
        }
      }
      else if(this.toAddTopic){
        if(this.profile.topics.subTopics.findIndex((ob)=>ob.topicName === this.toAddTopic) < 0){
          this.profile.topics.subTopics.push({topicName:this.toAddTopic, relatedTopics:[] })
          this.toAddTopic = ''
        }
      }
    },
    nextset(){
      if(this.searchReleatedTopic.length!==0 || this.searchReleatedTopic!==undefined){
        const maxpagerelated = this.searchReleatedTopic.length / this.perPage;
        if(maxpagerelated > this.currentPage){
            this.currentPage++;
            this.displayedReleatedTopic = this.filterReleatedTopics.slice((this.currentPage-1)*this.perPage,this.currentPage*this.perPage);
        }
      }
    },
    prevset(){
      if(this.currentPage > 1){
        this.currentPage--;
        this.displayedReleatedTopic = this.filterReleatedTopics.slice((this.currentPage-1)*this.perPage,this.currentPage*this.perPage);
      }
    },
    searchReleatedTopics(e){
      if(e!==null){
        this.$store.dispatch('ReleatedTopics', {instaId: this.profile.instagramAccountId, topic:e}).then(resp=>{
          this.searchReleatedTopic = []
          this.displayedReleatedTopic = []
          resp.data.relatedTopics.forEach((item)=>this.searchReleatedTopic.push(item))
          this.displayedReleatedTopic = this.filterReleatedTopics.slice((this.currentPage-1)*this.perPage,this.currentPage*this.perPage);
        })
      }
      //this.$store.dispatch('ReleatedTopics',)
    },
    scrollBehavior: function (to) {
      if (to.hash) {
        return {
          selector: to.hash
        }
      }
    },
    evaluateImage(emage){
      if(new RegExp('(.*?)(.jpg|.png|.svg)').test(emage))
        return true;
      else 
        return false;
    },
    updatePage(e){
      this.currentPage = e;
      this.searchMediaItems = []
      this.scrollBehavior("#searchSection")
      this.searchImage();
    },
    onUpload(e){
      this.currentPage = 1;
      const data = {instaId: this.profile.instagramAccountId, profileId: this.profile._id, formData:e};
      this.$store.dispatch('UploadFileForProfile',data).then(resp=>{
        this.urls = resp.data.urls;
        this.searchImage(this.urls);
      })
    },
    searchImage(data){
      this.isLoading = true;
      this.$store.dispatch('SimilarSearch',{urls:this.urls, limit:this.perPage, offset:this.currentPage}).then(
        res=> {
          this.searchMediaItems = []
          res.data.medias.forEach((item=> this.searchMediaItems.push({url:item.mediaUrl[0]})));
          this.isLoading = false;
          this.finishedSearching = true;
        }).catch(err=>{this.isLoading = false})
    },
    deleteColor(id){
      this.profile.theme.colors.splice(id,1);
      this.isModalActive = false;
    },
    updateColor(id, data){
      if(data!=undefined){
        this.profile.theme.colors[id] = data;
      }
      this.isModalActive = false;
    },
    addColor(data){
      if(data!==undefined){
        this.profile.theme.colors.push(data);
      }
      this.isModalActive = false;
    },
    evaluate(val){
      return true;
    },
    openColorDialog(id, data){
      this.currentlySelectedColor={color:{red:0,green:0,blue:0, name:undefined}, id:0}
      if(data!==undefined)
        this.currentlySelectedColor = {id:id, color:data};
      this.isModalActive = true;
    },
    performAutoCompletePlacesSearch: debounce(function (query){
      if(query && query!==''){
        this.isFetching = true
        this.$store.dispatch('GooglePlacesAutoCompleteSearch', {query: query, radius:1500}).then(({ data })=>{
          this.searchItems = []
          JSON.parse(data).predictions.forEach((item) => this.searchItems.push({city:item.structured_formatting.main_text, address:item.description})); // this.searchItems.push(item))
          this.isFetching = false;
        })
      }
    },500),
    performGoogleSearch: debounce(function(query){
      if(query && query!==''){
        this.$store.dispatch('GooglePlacesSearch',query).then(({data})=>{
          JSON.parse(data).results.forEach((item) => this.searchItems.push(item))
        })
      }
    },500),
    deleteSubTopic(index){
      if(this.canEdit)
        this.profile.topics.subTopics.splice(index,1);
    }
  }
}
</script>

<style lang="scss">
.loading-overlay .loading-background{
  background:transparent;
}
.closeSubTopics{
  position:relative;
}
.pagination-previous, .pagination-next, .pagination-link{
  color:#121212 !important;
  background:#d9d9d9;
}
.dropdown-content{
  max-height: 430px!important;
  background: #242424;
  color:#d5d5d5;
  &:hover{
    background:#323232;
  }
}
.dropdown-item{
  color:#d5d5d5 !important;
  &:hover{
    background:#444 !important;
    color:white !important;
  }
}
.similarImages_Container{
  width:100%;
  display: flex;
  margin:0 auto;
  flex-flow: row wrap;
  align-items: center;
  background:transparent;
  margin-left:0.5em;
}
.image{
  transition: all .2s ease-in-out;
  &:hover{
    opacity: 0.4;
    cursor: pointer;
      &.zoomable{
        transform: scale(1.1);
        border-radius:0.25em;
      }
  }

}
.searchResults{
  margin-left:6em;
}
.map_container #map{
  border-radius: 0.5em;
  min-height: 400px !important;
}
.vc-chrome-body{
  background:#212121 !important;
}
.profile_container{
  width:100%;
  height: 100%;
  color:white;
  background:#232323;
  border-radius: 0.0em;
  padding:2em;
}
.modal-background{
  background:rgba(150, 150, 150, 0.5)
}
.is-small{
  border-radius: 100em !important;
  margin-left:1em;
  margin-top:1em;
}
.step-navigation{
  a{
    background:#efefef;
    color:#242424;
  }
}
.subtopics_container{
  display: flex;
  margin: 0 auto;
  flex-flow: row wrap;
  align-items: center;
  background:#4f4f4f;
}
.is-dark{
  label{
    color:#d9d9d9 ;
    font-size:20px;
  }
  &.is-small{
    label{
      color:#d9d9d9 ;
      font-size:16px;
    }
  }
}
.is-button{
  width:200px;
  height:100px;
  font-size:50px;
  background:#121212;
  color:#696969;
  margin:0 auto;
  //border-radius: 0;
  &:hover{
    cursor: pointer;
    background:#2e2e2e;
    color:#121212;
  }
}
.topic_area{
  padding:0;
  margin: 0 auto;
  width:100%;
  label{
    color:white;
  }
  select{
    text-align: center !important;
    width:100% !important;
  }
  .full-rounded{
    width:100%;
    text-align: center !important;
  }
}
.is-profile{
  border-radius: 0;
  background:#1f1f1f;
  padding:3em;
  .name{
    width:20%;
  }
  .descr{
    width:65%;
  }
  .lang{
    width:15%;
  }
  label{
    color:#d9d9d9;
  }
}
.step_size{
  border:none;
}
.step-marker{
  background:#d9d9d9;
  color:#1f1f1f !important;
}
.step-title{
  color:#d9d9d9;
}
.slidecontainer {
  width: 100%;
}

.slider {
  -webkit-appearance: none;
  width: 500px;
  height: 15px;
  border-radius: 5px;  
  background: #d9d9d9;
  outline: none;
  opacity: 0.7;
  -webkit-transition: .2s;
  transition: opacity .2s;
}

.slider:hover {
  opacity: 1;
}

.slider::-webkit-slider-thumb {
  -webkit-appearance: none;
  appearance: none;
  width: 25px;
  height: 25px;
  border-radius: 50%; 
  background: rgb(75, 117, 255);
  cursor: pointer;
}

.slider::-moz-range-thumb {
  width: 25px;
  height: 25px;
  border-radius: 50%;
  background: #4CAF50;
  cursor: pointer;
}
input{
  background:#121212 !important;
  border:none !important;
  color:#d9d9d9 !important;
  &:focus{
    color:#d9d9d9 !important;
    box-shadow: 0;
    border:none !important;
  }
  &:hover{
    background:#212121 !important;
  }
}
::placeholder{
  color:#d8d8d8 !important;
}
.input{
  background:#121212 !important;
  border:none !important;
  color:#d9d9d9 !important;
  &:focus{
    color:#d9d9d9 !important;
    box-shadow: 0;
    border:none !important;
  }
   &:hover{
    background:#212121 !important;
    &.is-tag{
      background:#333 !important;
    }
  }
  &.is-tag
  {
    background:#212121 !important;
  }
}
select{
  background:#121212 !important;
  border:none !important;
  color:#d9d9d9 !important;
  &:focus{
    color:#d9d9d9 !important;
    box-shadow: 0 !important;
    border:none !important;
  }
  &:hover{
    background:#212121 !important;
  }
  option{
    background:#212121 !important;
    color:#d9d9d9 !important;
  }
}
.control{
  background:transparent;
  padding:0;
  margin:0;
  border:none !important;
}
.taginput{
  padding:0;
  margin:0;
  background:transparent;
  border:none !important;
  color:white;
   &:hover{
    background:#212121 !important;
  }
}
.input, .taginput .taginput-container.is-focusable, .textarea{
  background:#121212;
  border:none;
  color:#d9d9d9;
   &:hover{
    background:#212121 !important;
  }
}
$bg: #292929;
$barsize: 25px;

.hr {   
    width: 100%;
    height: 1px;
    display: block;
    position: relative;
    margin-bottom: 0em;
    padding: 0.4em 0;
    &:after,
    &:before {
        content: "";
        position: absolute;
        width: 100%;
        height: 1px;
        bottom: 50%;
        left: 0;
    }
    &:before {
        background: linear-gradient( 90deg, $bg 0%, $bg 50%, transparent 50%, transparent 100% );
        background-size: $barsize;
        background-position: center;
        z-index: 1;
    }
    &:after {
        transition: opacity 0.3s ease, animation 0.3s ease;
        background: linear-gradient(
            to right, 
            #62efab 5%, 
            #F2EA7D 15%, 
            #F2EA7D 25%, 
            #FF8797 35%, 
            #FF8797 45%, 
            #e1a4f4 55%, 
            #e1a4f4 65%, 
            #82fff4 75%, 
            #82fff4 85%, 
            #62efab 95%);
        background-size: 200%;
        background-position: 0%;
        animation: bar 15s linear infinite;
    }
    @keyframes bar {
        0% { background-position: 0%; }
        100% { background-position: 200%; }
    } 
}
.hr.anim {
    &:before {
        background: linear-gradient( 
            90deg, 
            $bg 0%, $bg 5%, 
            transparent 5%, transparent 10%, 
            $bg 10%, $bg 15%, 
            transparent 15%, transparent 20%, 
            $bg 20%, $bg 25%,
            transparent 25%, transparent 30%,
            $bg 30%, $bg 35%, 
            transparent 35%, transparent 40%, 
            $bg 40%, $bg 45%, 
            transparent 45%, transparent 50%, 
            $bg 50%, $bg 55%,
            transparent 55%, transparent 60%,
            $bg 60%, $bg 65%,
            transparent 65%, transparent 70%, 
            $bg 70%, $bg 75%, 
            transparent 75%, transparent 80%, 
            $bg 80%, $bg 85%,
            transparent 85%, transparent 90%,
            $bg 90%, $bg 95%, 
            transparent 95%, transparent 100% );

        background-size: $barsize * 10;
        background-position: center;
        z-index: 1;
        
        animation: bar 200s linear infinite;
        
    }
    
    &:hover {
        &:before {
            animation-duration: 20s;
        }
        &:after {
            animation-duration: 2s;
        }
    }
}
</style>
