<template>
    <transition name="zoom-out">
        <div class="v-cal-dialog" v-if="isActive">
            <div class="v-cal-dialog__bg" @click="cancel"></div>
            <div class="v-cal-dialog-card is-big">
                <form @submit.prevent="saveEvent">
                    <header class="v-cal-dialog-card__header">
                        <h5 class="v-cal-dialog__title">{{ title }}</h5>
                        <button type="button" class="v-cal-dialog__close" @click="cancel"></button>
                    </header>
                    <section v-if="postDataBuild.typeSelected<1"  class="v-cal-dialog-card__body" id="initial" style="padding-top:5em;" >
                        <p class="subtitle thin is-5 card-title">What would you like to schedule?</p>
                            <div class="container-media-overview" >
                            <div class="container-media" style="margin-top:5em;">
                            <div class="card-post" @click="postDataBuild.typeSelected=1">
                                <h3 class="title thin">Photo</h3>
                                <div class="bar">
                                <div class="emptybar">
                                </div>
                                <div class="filledbar"></div>
                                </div>
                                <div class="circle">
                                <svg version="1.1" xmlns="http://www.w3.org/2000/svg">
                                <circle class="stroke" cx="60" cy="60" r="50"/>
                                </svg>
                                    <span class="icon icon_media">
                                        <i class="fas fa-2x fa-camera"></i>
                                    </span>
                                </div>
                            </div>
                            <div class="card-post" @click="postDataBuild.typeSelected=2">
                                <h3 class="title thin">Video</h3>
                                <div class="bar">
                                <div class="emptybar"></div>
                                <div class="filledbar"></div>
                                </div>
                                <div class="circle">
                                <svg version="1.1" xmlns="http://www.w3.org/2000/svg">
                                <circle class="stroke" cx="60" cy="60" r="50"/>
                                </svg>
                                    <span class="icon icon_media">
                                        <i class="fas fa-2x fa-video"></i>
                                    </span>
                                </div>
                            </div>
                            <div class="card-post" @click="postDataBuild.typeSelected=3">
                                <h3 class="title thin">Carousel</h3>
                                <div class="bar">
                                <div class="emptybar"></div>
                                <div class="filledbar"></div>
                                </div>
                                <div class="circle">
                                <svg version="1.1" xmlns="http://www.w3.org/2000/svg">
                                <circle class="stroke" cx="60" cy="60" r="50"/>
                                </svg>
                                    <span class="icon icon_media">
                                        <i class="fas fa-2x fa-images"></i>
                                    </span>
                                </div>
                            </div>
							<b-tooltip type="is-danger" position="is-right" label="coming soon">
								<div class="card-post is-disabled">
									<h3 class="title thin">Story</h3>
									<div class="bar">
									<div class="emptybar"></div>
									<div class="filledbar"></div>
									</div>
									<div class="circle">
									<svg version="1.1" xmlns="http://www.w3.org/2000/svg">
									<circle class="stroke" cx="60" cy="60" r="50"/>
									</svg>
										<span class="icon icon_media">
											<i class="fas fa-2x fa-circle-notch"></i>
										</span>
									</div>
								</div>
							</b-tooltip>
                            </div>
                            </div>

                    </section>
                    <section v-else class="v-cal-dialog-card__body" id="next">
                        <div class="body-container">
                            <div class="create-post-container">
                                <div class="media-section">
                                    <div class="position" v-if="!prePostData.presentingMedia">
                                        <p class="subtitle thin center">Upload Media</p>
                                        <b-tooltip label="Upload From Computer" type="is-danger">
                                            <span class="icon for-media-upload">
                                                <i class="fas fa-5x fa-cloud-upload-alt"></i>
                                                <d-drop v-if="postDataBuild.typeSelected===1" acceptFile="image/*" :isHidden="true" :isMulti="false" class="dropStyle" @readyUpload="onUpload"></d-drop> 
												<d-drop v-if="postDataBuild.typeSelected===2" acceptFile="video/*" :isHidden="true" :isMulti="false" class="dropStyle" @readyUpload="onUpload"></d-drop> 
                                                <d-drop v-if="postDataBuild.typeSelected===3" acceptFile="image/*, video/*" :isHidden="true" :isMulti="false" class="dropStyle" @readyUpload="onUpload"></d-drop> 
											</span>
                                        </b-tooltip>
                                        <b-tooltip label="Upload From My Library" type="is-danger">
                                            <span class="icon for-media-upload" @click="openLibary">
                                                <i class="fas fa-5x fa-book-reader"></i>
                                            </span>
                                        </b-tooltip>
                                        <b-tooltip label="💙 Coming soon 💙" class="is-disabled" type="is-danger">
                                        <span class="icon for-media-upload is-disabled">
                                            <i class="fab fa-5x fa-dropbox"></i>
                                        </span>
                                        </b-tooltip>
                                        <b-tooltip label="💙 Coming soon 💙" class="is-disabled" type="is-danger">
                                            <span class="icon for-media-upload is-disabled">
                                                <i class="fab fa-5x fa-google-drive"></i>
                                            </span>
                                        </b-tooltip>
                                    </div>
                                    <div v-else class="media-con">
										<img v-if="postDataBuild.typeSelected === 1" :src="postDataBuild.media[0].url" class="media-container" alt="">
										<video controls="controls" preload="metadata" v-if="postDataBuild.typeSelected === 2" :src="postDataBuild.media[0].url+'#t=0.5'" class="media-container"/>
										<carousel v-else-if="postDataBuild.typeSelected === 3" :perPage="1">
											<slide v-for="(item,index) in postDataBuild.media" v-bind:key="index">
												<img v-if="item.type === 1" class="media-container" :src="item.url" alt="">
												<video controls="controls" preload="metadata" v-else-if="item.type === 2" :src="item.url+'#t=0.5'" class="media-container"/>
											</slide>
										</carousel>
									</div>
                                </div>
                                <div class="cred-loca-section">
                                    <b-field grouped>
                                    <b-field class="loca-input">                   
                                         <b-autocomplete
                                                size="is-medium"
                                                type="is-dark"
                                                v-model="postDataBuild.location"
                                                :data="placesSearch.searchItems"
                                                autocomplete
                                                :allow-new="false"
                                                field="address"
                                                icon="map-marker"
                                                placeholder="Add a location"
                                                @select="selectedLocation"
                                                @typing="performAutoCompletePlacesSearch">
                                            </b-autocomplete>
                                    </b-field>
                                    <b-field class="loca-input">
                                        <b-clockpicker
                                            class="is-light"
                                            v-model="postDataBuild.time"
                                            rounded
                                            :placeholder="'Posting at '+ postDataBuild.time"
                                            icon="clock"
                                            type="is-info"
                                            :hour-format="format">
                                        </b-clockpicker>
                                    </b-field>
                                </b-field>
                                </div>
                                <div class="caption-section">
                                    <div class="caption-container">
                                        <b-field label="Caption" class="is-left">
                                            <b-input v-model="postDataBuild.caption" type="textarea"
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
                                        <div class="hashtag-container">
                                            <b-field label="Hashtags" class="is-left">
                                                <b-taginput type="is-twitter" size="is-small" style="height:140px; overflow:auto; background:transparent!important;"
                                                :on-paste-separators="[',','@','-',' ']"
                                                :confirm-key-codes="[32, 13]"
                                                :before-adding="evaluateTags"
                                                maxlength="25"
                                                maxtags="30"
                                                v-model="postDataBuild.hashtags">
                                                </b-taginput>
                                            </b-field>
                                            <b-switch :disabled="prePostData.isFetchingHashtags" v-model="prePostData.autoSuggest" @input="getSuggestedTags" type="is-twitter">Suggest Hashtags?</b-switch>
											<b-tooltip v-if="postDataBuild.hashtags.length>0" style="float:right; margin-top:-.4em;" type="is-dark" label="Copy to clipboard">
												<a @click="CopyToClipboard" class="button is-medium is-transparent">
													<span class="icon is-large">
														<i class="fas fa-paste">
														</i>
													</span>
												</a>
											</b-tooltip>
										</div>
                                        <div class="field has-addons saved-container" style="padding-top:1em;">
                                            <p class="control">
                                                <a @click="openSavedHashtagDialog" class="button is-text">
                                                <span class="icon is-small">
                                                    <i class="fas fa-hashtag"></i>
                                                </span>
                                                <span>Saved Hashtags</span>
                                                </a>
                                            </p>
                                            <p class="control">
                                                <a @click="openSavedCaptionDialog" class="button is-text">
                                                <span class="icon is-small">
                                                    <i class="fas fa-comment-alt"></i>
                                                </span>
                                                <span>Saved Captions</span>
                                                </a>
                                            </p>
                                        </div>
                                    </div>
                                </div>
                                <hr class="hr-sep">
                                <div class="footer-section">
                                    <div class="field is-grouped is-right">
                                         <p class="control" @click="close()">
                                            <a class="button is-dark">
                                            <span class="icon is-small">
                                                <i class="fas fa-history"></i>
                                            </span>
                                            <span>Discard Post</span>
                                            </a>
                                        </p>
                                        <p class="control">
                                            <a class="button is-info" @click="schedulePost">
                                            <span class="icon is-small">
                                                <i class="fas fa-camera"></i>
                                            </span>
                                            <span>Schedule Post</span>
                                            </a>
                                        </p>
                                    </div>
                                      <div v-if="prePostData.presentingMedia" class="field is-grouped is-left">
                                        <p class="control">
                                            <a class="button is-danger" @click="cancelUpload">
                                            <span class="icon is-small">
                                                <i class="fas fa-upload"></i>
                                            </span>
                                            <span>Retry</span>
                                            </a>
                                        </p>
                                    </div>
                                </div>
                            </div>
                        </div>       
                    </section>
                </form>
            </div>
        </div>
    </transition>
</template>

<script>
    import Event from '../../model/Event';
	import moment from 'moment';
	import { Carousel, Slide } from 'vue-carousel';
    import EventDialogInput from './EventDialogInput';
    import DropZone from '../../../../src/components/Objects/DropZone';
    import { Emoji } from 'emoji-mart-vue';
    import debounce from 'lodash/debounce';
    import state from '../../../../src/State';
    import route from '../../../../src/Route';
	import Libary from './subDialog/index';
	import {GetMediaType} from '../../../../src/helpers';
	import Vue from 'vue';
	
    export default {
        components: { 
            EventDialogInput,
            'd-drop': DropZone,
			Emoji,
			Carousel,
   			Slide,
        },
        props: {
            title: String,
            inputClass: String,
            overrideInputClass: Boolean,
            fields: Array,
            createButtonLabel: String,
        },
        data() {
            return {
                profile:{},
                isActive: false,
                event: {},
                prePostData:{
                    presentingMedia:false,
                    selectedFile:{},
                    autoSuggest:false,
                    isFetchingHashtags:false,
                },
                postDataBuild:{
                    time:Date,
                    typeSelected:-1,
                    caption:'',
                    hashtags:[],
                    location:'',
                    media:[],
                    locationActual:{}
                },
                placesSearch:{
                    isFetching:false,
                    searchItems:[]
                },
                
            }
        },
        beforeMount() {
            let plainEvent = {};
            this.fields.map( field => {
                if ( !field.fields )
                    plainEvent[field.name] = field.value;
                else {
                    const fields = field.fields;
                    fields.map( field => {
                        if ( field.type === 'time' ) {
                            plainEvent[field.name] = field.value ? moment(field.value, 'YYYY-MM-DD HH:mm:ss') : null
                        } else
                            plainEvent[field.name] = field.value;
                    })
                }
            });

			this.event = new Event(plainEvent);
			this.postDataBuild.time = new Date(moment(this.event.startTime).format("YYYY-MM-DD HH:mm:ss"));
            this.profile = state.getters.UserProfile(route.app.$route.params.id);
            document.body.appendChild(this.$el);
        },
        mounted() {
            this.isActive = true;
        },
        computed:{
            format() {
                return this.isAmPm ? '12' : '24'
            }
        },
        methods: {
			CopyToClipboard(){
				let hashtags_ = this.postDataBuild.hashtags.join(' ');
				var ele = document.createElement('textarea');
				ele.value = hashtags_;
				ele.setAttribute('readonly', '');
				ele.style = {position: 'absolute', left: '-9999px'};
				document.body.appendChild(ele);
				ele.select();
				document.execCommand("copy");
				document.body.removeChild(ele);
			},
            openSavedCaptionDialog(){
                Libary.showSavedCaption().$on('click-selected', (e) => {
                    this.postDataBuild.caption = e.caption;
                });
            },
            openSavedHashtagDialog(){
                Libary.showSavedHashtags().$on('click-selected', (e) => {
                    this.postDataBuild.hashtags = e.hashtag;
                });
            },
            schedulePost(){
				if(!this.postDataBuild.caption){
					Vue.prototype.$toast.open(
					{
						message: 'Looks like you forgot to include a caption, please provide one as this will help boost your viewing 😋',
						type: 'is-warning'
					});
					return;
				}
				if(this.postDataBuild.hashtags.length <=0){
					Vue.prototype.$toast.open(
					{
						message: "It's important to provide hashtags, this is by far the best way to get exposure 😋",
						type: 'is-warning'
					});
					return;
				}
                let medias = []
                this.postDataBuild.media.forEach(async item=>{
                    medias.push({
						urlToSend: null,
                        media64BaseData : item.url,
						mediaType: item.type
                    })
				});
                let data = {
                    executeTime:this.postDataBuild.time,
                    optionSelected:this.postDataBuild.typeSelected,
                    caption:this.postDataBuild.caption,
                    hashtags:this.postDataBuild.hashtags,
                    rawMediaDatas: medias,
                    location:{address:this.postDataBuild.locationActual.address, city: this.postDataBuild.locationActual.city}
				}
                let event = { id: route.app.$route.params.id, event: data }
                this.$emit('CreatePost', event);
                this.close();
            },
            openLibary(){
                Libary.show({
					profile : this.profile,
					type: this.postDataBuild.typeSelected
                }).$on('media-omitted', (media) => {
					this.postDataBuild.media = [];
					this.postDataBuild.media = media;
                    this.prePostData.presentingMedia = true;
                    this.prePostData.selectedFile = media[0];
                });
            },
            getSuggestedTags(e){
                if(e===true){
                    this.prePostData.isFetchingHashtags = true;
                    const request = {
                        profileTopic: this.profile.profileTopic,
                        mediaTopic: null,
                        pickAmount: 25,
                        mediaUrls: this.postDataBuild.media.map(res=>res.url)
                    }
                    state.dispatch('BuildTags', request).then(resp=>{
                        this.postDataBuild.hashtags = [];
                        resp.data.forEach((item)=>this.postDataBuild.hashtags.push(item));
                        this.prePostData.autoSuggest = false;
                        this.prePostData.isFetchingHashtags = false;
                    }).catch(err=>{
                        this.prePostData.autoSuggest = false;
                        this.prePostData.isFetchingHashtags = false;
                    })
                }
            },
            cancelUpload(){
                this.postDataBuild.media = [];
                this.prePostData.presentingMedia = false;
                this.prePostData.selectedFile = {};
            },
            selectedLocation(e){
                this.postDataBuild.locationActual = e;
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
            performAutoCompletePlacesSearch: debounce(function (query){
                if(query && query!==''){
                    this.placesSearch.isFetching = true
                    state.dispatch('GooglePlacesAutoCompleteSearch', {query: query, radius:1500}).then(({ data })=>{
                    this.placesSearch.searchItems = []
                    JSON.parse(data).predictions.forEach((item) => this.placesSearch.searchItems.push({city:item.structured_formatting.main_text, address:item.description})); // this.searchItems.push(item))
                    this.placesSearch.isFetching = false;
                    })
                }
                },500),
            emojiFallback(emoji){
                this.postDataBuild.caption+=emoji.native;
            },
            onUpload(media){
               if(media.requestData.length<=0){
                   return false;
               }
               else if(media.requestData.length == 1 || media.requestData){       
                   if(this.postDataBuild.typeSelected==1){
                       this.postDataBuild.media = [];
                   }     
                    this.readFile(media.requestData).then(resp=>{
                        this.postDataBuild.media.push({url:resp});
                        this.prePostData.presentingMedia = true;
                        this.prePostData.selectedFile = media;
                        return true;
                    })             
                   return false;
               }
               else if(media.requestData.length > 1){
                   return true;
               }

            this.$emit('media-upload', { data: media.formData, mediaType: this.postDataBuild.typeSelected });
            // this.close();
            },
           readFile(file) {
                return new Promise((resolve, reject) => {
                    let fr = new FileReader();
                    fr.onload = x=> resolve(fr.result);
                    fr.readAsDataURL(file) // or readAsText(file) to get raw content
                })
            },
            saveEvent() {
                this.$emit('event-created', this.event);
                this.close();
            },
            cancel() {
               Vue.prototype.$dialog.confirm({
                    title: 'Leave without saving?',
                    cancelText: 'Cancel',
                    confirmText: 'Leave',
                    type: 'is-success',
                    onConfirm: () => this.close()
                })
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

<style lang="scss">
@import url('https://fonts.googleapis.com/css?family=Roboto:300&display=swap');
.hr-sep{
    position: absolute;
    left:-3.3em;
    right:-3em;
    bottom:-6em;
    height: 1px;
    background:#232323;
}
.menu{
    margin:0 auto;
    margin-top:1em;
    margin-left:17.3em;
}
.icon{
    &.for-media-upload{
        opacity: 0.9;
        padding:5em;
        margin-left:3em;
        //margin-top:1em;
        color:wheat;
        input{
            &:hover{
                    cursor: pointer;
            }
        }
        &:hover{   
            opacity:0.5;
            cursor: pointer;
            &.is-disabled{
                cursor:default;
                opacity:.9;
                color:#d9d9d9;
                .b-tooltip{
                    display: none;
                }
            }
        }
        //text-shadow: 1px 1px 1px 1px #fff 1px 1px 1px 1px #fff;
    }
}
.subtitle{
    &.thin{
        &.center{
            font-family: 'Roboto', sans-serif !important;
            font-style: 'light' !important;
            text-align: center;
            position:absolute;
            right:25em;
            color:#d9d9d9;
            top:9.5em;
            font-size: 22px;
            opacity: 0.8;
        }
    }
}

.body-container{
    position: absolute;
    height:450px;
    width: 800px;
    top: 100px;
    left: calc(50% - 400px);
    display: flex;
    .dropStyle{
        margin-top:4em;
        margin-left:15em;
        &:hover{}
    }
}

.card-title{
    text-align: center;
    font-family: 'Roboto', sans-serif !important;
    font-style: 'light' !important;
    color:white;
}
.media-section{
    width:480px;
    height:410px;
    .media-container{
        height: 410px;
        width: 480px;
        object-fit: fill;
	}
   // border: 1px dashed #f1f1f1;
    background:#23232338;
    border-radius:0.7em;
    box-shadow: -0.3rem 0 1rem #000;
    margin-top:.5em;
    margin-left:-3.3em;
    .position{
        text-align: left !important;
        margin-left:0em;
        padding-top:3em;
    }
}

.footer-section{
    position:absolute;
    bottom:-8.5em;
    right:-2.5em;
    //left:0;
    //background:wheat;
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
.caption-section{
    position:absolute;
    top:.5em;
    right:-3em;
    width:400px;
    height:495px;
    background:#23232338;
    border-radius:0.7em;
    box-shadow: -0.1rem 0 1rem #000;
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
		.button{
			&.is-transparent{
				border:none;
				background:transparent;
				color:wheat;
			}
		}
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
textarea{
    background:#323232 !important;
}
.VueCarousel-pagination{
	margin-top:-2.75em;
}
.cred-loca-section{
    margin-top:1em;
    margin-left:-3.3em;
    width:480px;
    height:100px;
    border-radius: 0.7em;
    .loca-input{
        .autocomplete{
            .icon{
                margin-top:-.2em;
            }
            width:240px !important;
            margin-top:0em;
            input{
                height:36px !important;
                font-size: 17px;

             }
        }
        .dropdown-menu{
            opacity: .9;
            border-radius: 1em;
            border:none;
            margin-top:.5em;
            background:#323232 !important;
            color:white !important;
            .dropdown-content{
                .dropdown-item{
                    color:#d9d9d9 !important;
                    &:hover{
                        background:#232323;
                        opacity: .8;
                    }
                    &.is-hovered{
                        background:#232323;
                        opacity: .8;
                    }
                }
            }
        }
        margin-top:.8em;
        .button{
            border-radius: 1em;
        }
        .input{
            border-radius: 1em;
            background:#333333 !important;
            &:hover{
                background:#505050 !important;
            }
            //border: 1px dashed white !important;
        }
    }
}
.saved-container{
    .button{
        color:wheat !important;
        &:hover{
            background:transparent !important;
            color:whitesmoke !important;
        }   
    }
}
.container-media {
  position: absolute;
  height: 300px;
  width: 600px;
  top: 180px;
  left: calc(50% - 300px);
  display: flex;
}
.icon{
    &.icon_media{
        color:wheat;
        position:absolute;
        top:3em;
        left:3em;
    }
}
//17141d
.card-post {

  display: flex;
  height: 280px;
  width: 200px;
  background-color: #232323;
  border-radius: 10px;
  box-shadow: -0.4rem 0 1.5rem #000;
/*   margin-left: -50px; */
  transition: 0.4s ease-out;
  position: relative;
  left: 0px;
}
.b-clockpicker{
    .card{
        background:#333;
        border-radius: .5em !important;
        .b-clockpicker-btn{
            color:white;
        }
    }
}
.dropdown-content{
    background:transparent;
}

.card-post:not(:first-child) {
    margin-left: -50px;
}
.input{
    &.for-media-upload{
        background:#434343 !important;
    }
}
.card-post:hover {
  transform: translateY(-20px);
  transition: 0.4s ease-out;
  cursor: pointer;
  &.is-disabled{
	  cursor: default;
  }
}

.card-post:hover ~ .card-post {
  position: relative;
  left: 50px;
  transition: 0.4s ease-out;
}

.title {
    &.thin{
        color: white;
        font-weight: 300;
        position: absolute;
        left: 20px;
        top: 15px;
    }
}

.bar {
  position: absolute;
  top: 100px;
  left: 20px;
  height: 5px;
  width: 150px;
}

.emptybar {
  background-color: #2e3033;
  width: 100%;
  height: 100%;
}

.filledbar {
  position: absolute;
  top: 0px;
  z-index: 3;
  width: 0px;
  height: 100%;
  background: rgb(0,154,217);
  background: linear-gradient(90deg, rgb(144, 68, 243) 0%, rgb(0, 184, 217) 65%, rgba(255,186,0,1) 100%);
  transition: 0.4s ease-out;
}

.card-post:hover .filledbar {
  width: 120px;
  transition: 0.4s ease-out;
}

.circle {
  position: absolute;
  top: 150px;
  left: calc(50% - 60px);
}

.stroke {
  stroke: white;
  stroke-dasharray: 360;
  stroke-dashoffset: 360;
    font-family: "Font Awesome 5 Free";
    content: "\f095";
  transition: 0.6s ease-out;
}

svg {
  fill: #232323;
  stroke-width: 2px;
}

.card-post:hover .stroke {
  stroke-dashoffset: 100;
  transition: 0.6s ease-out;
}

.is-big{
  background: #141414;
  border-radius:0.7em;
  width:50% !important;
  height:75% !important;
  max-width: 55%;
  padding-top:1em;
  padding-right:2em !important;
  padding-left:2em !important;
  box-shadow: 10px 2px 3px 14px rgba(30, 30, 30, 0.1) !important;
  border: 1px solid #323232;

}
.zoom-out-enter-active,
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
.dialog{
    &.modal{
        z-index: 1000;
        margin:0;
        padding:0;

        .modal-background{
            background:rgba(143, 204, 245, 0.4);
        }
        .modal-card-head{
            background:transparent;
            .modal-card-title{
                color:white !important;
            }
        }
        .modal-card-body{
            background:transparent;
            padding:0;
        }
        .modal-card-foot{
            background:transparent;
        }
        .modal-card{
            background: transparent;
            box-shadow: none !important;
        }
        .media-content{
            background:transparent !important;
            border:none;
            p{
                background:transparent !important;
                color:white;
            }
        }
    }
}
</style>