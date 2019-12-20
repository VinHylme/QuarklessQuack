<template>
  <div class="messaging-container">
    <div class="columns is-gapless is-mobile container-column">
      <div class="column is-2">
        <div class="side-panel">
          <b-menu>
              <b-menu-list>
                <b-field label="Accounts" class="label-color-white">
                  <b-dropdown @change="CloseSession" class="custom-dropdown" aria-role="list">
                    <button class="button is-primary" type="button" slot="trigger">
                      <template>
                          <span class="icon is-large is-left">
                              <img :src="selectedAccount.account.profilePicture" class="select-pic" alt="">
                          </span>
                          <span>{{selectedAccount.account.username}}</span>
                      </template>
                      <b-icon icon="menu-down"></b-icon>
                    </button>
                    <b-dropdown-item :id="'ditem_'+index" v-for="(user,index) in UsersAccounts" :key="index" :value="user" aria-role="listitem">
                        <div class="media">
                            <span class="icon is-large is-left">
                                <img :src="user.profilePicture" class="select-pic" alt="">
                            </span>
                            <div class="media-content">
                                <h3>{{user.username}}</h3>
                                <small>{{user.fullName}}</small>
                            </div>
                        </div>
                    </b-dropdown-item>
                    </b-dropdown>
                  </b-field>
                  <!-- <div class="control is-big-sel has-icons-left">
                    <div class="select is-medium is-rounded is-fullwidth">
                      <select @change="SelectAccount">
                        <option v-for="(user,index) in UsersAccounts" :key="index" :value="user" 
                        :selected="user === selectedAccount.account.username">{{user}}</option>
                      </select>
                    </div>
                    <span class="icon is-large is-left">
                        <img :src="selectedAccount.account.profilePicture" class="select-pic" alt="">
                    </span>
                  </div> -->
                  <br> 
                  <b-menu-item icon="target" active expanded>
                      <template slot="label" slot-scope="props">
                          Targeted
                          <b-icon
                              class="is-pulled-right"
                              :icon="props.expanded ? 'menu-down' : 'menu-up'">
                          </b-icon>
                      </template>
                      <b-menu-item @click="SelectSection(MapType(1))" icon="account" label="Following"></b-menu-item>
                      <b-menu-item @click="SelectSection(MapType(2))" icon="account" label="Followers"></b-menu-item>
                      <b-menu-item @click="SelectSection(MapType(3))" icon-pack="fas" icon="map-marker-alt" label="Targeted Location"></b-menu-item>
                      <b-menu-item @click="SelectSection(MapType(4))" icon-pack="fas" icon="hashtag" label="Targeted Users"></b-menu-item>
                      <b-menu-item @click="SelectSection(MapType(5))" icon-pack="fas" icon="smile" label="Suggested"></b-menu-item>
                  </b-menu-item>
                  <b-menu-item @click="SelectSection(MapType(6))" icon="magnify" label="Search">
                  </b-menu-item>
              </b-menu-list>
              <b-menu-list>
                  <b-menu-item @click="SelectSection(MapType(7))" icon="inbox" label="Inbox"></b-menu-item>
                  <b-menu-item @click="SelectSection(MapType(8))" icon="comment" label="Recent Comments">
                  </b-menu-item>
              </b-menu-list>
          </b-menu>
          <div class="selected-content" v-if="currentlySelectedItems.length > 0">
            <br>
            <p class="subtitle">Currently Selected: ({{results.current}}/{{results.max}})</p>
            <div class="selected-holder" >
              <div class="selected-container" v-for="(sel,index) in currentlySelectedItems" :key="index">
                <img @click="RemoveFromList(sel)" :src="sel.item.object.profilePicture" alt="">
              </div>    
            </div>
            <br>
            <b-field>
              <div class="buttons has-addons is-centered">
                <span class="button is-default is-default" @click="Discard">Discard</span>
                <span class="button is-success is-default" @click="Next">Next</span>
              </div>
            </b-field>
            <br>
          </div>
        </div>
      </div>
      <div class="column is-10">
        <div class="main-panel">
          <div v-if="currentlySelected >=1 && currentlySelected <=6">
            <div class="section-container">
              <div class="section-header">
              </div>
              <div class="results-container">
                <div class="box">
                  <h3 class="subtitle is-4">{{MapSection}}</h3>
                  <div class="field" v-if="currentlySelected!==6">
                      <p class="control has-icons-left has-icons-right">
                        <input v-model="filterSearch" class="input is-rounded" type="text" placeholder="Search user...">
                        <span class="icon is-small is-left">
                          <i class="fas fa-search"></i>
                        </span>
                      </p>
                  </div>
                  <div v-else class="field has-addons dark-search">
                    <p class="control">
                      <span class="select is-info">
                        <select v-model="searchRes.type">
                          <option value="0">By Topic</option>
                          <option value="1">By Location</option>
                        </select>
                      </span>
                    </p>
                    <p class="control is-expanded">
                      <input @keypress.enter="SearchData" v-model="searchRes.query" class="input" type="text" placeholder="Search...">
                    </p>
                    <p class="control">
                      <a @click="SearchData" class="button is-dark">
                        Search
                      </a>
                    </p>
                  </div>
                  <b-notification style="background:transparent; height:150px; margin:2em;" v-if="results.loading" :closable="false">
                      <b-loading :is-full-page="false" :active.sync="results.loading" :can-cancel="false"></b-loading>
                  </b-notification>
                  <br>
                  <UserResults @selected-user="updateList" v-if="results.searchResultItems.length > 0" :datas="filteredList"/>
                  <p class="subtitle" v-else>No results found</p>
                </div>
                <!-- <div class="box" v-if="results.displayableList.length > 0">
                  <b-pagination
                    @change="updatePage"
                    :total="paginationSettings.total"
                    :current.sync="paginationSettings.currentPage"
                    :range-before="paginationSettings.rangeBefore"
                    :range-after="paginationSettings.rangeAfter"
                    order="is-centered"
                    size="is-medium"
                    :simple="false"
                    :rounded="paginationSettings.isRounded"
                    :per-page="paginationSettings.perPage"
                    aria-next-label="Next page"
                    aria-previous-label="Previous page"
                    aria-page-label="Page"
                    aria-current-label="Current page">
                  </b-pagination>   
                </div> -->
              </div>
            </div>
          </div>
          <div v-else-if="currentlySelected === 7">
            <div class="section-container is-chat">
              <div class="section-header"></div>
                <div class="chat-side-panel">
                  <div class="columns is-gapless is-mobile chat-column">
                      <div class="column is-2 side">
                        <div class="filter-search-users-chat">
                          <input type="text" class="input" v-model="chat.filterSearch" placeholder="Filter Users...">
                        </div>
						<div v-for="(thread,index) in FilteredThreads" :key="index">
							<div @click="SelectThread(index)" :class="thread.selected === false ?'user-item':'user-item is-active'">
								<div class="grouped-users" v-if="thread.item.users.length > 1">
									<img class="group-profile-con" v-for="(user,index) in thread.item.users.slice(0,2)" :key="index" :src="user.profilePicture" alt="">
								</div>
								<img v-else class="profile-con" :src="thread.item.users[0].profilePicture" alt="">
								<p v-if="!thread.item.title.includes(',')" class="subtitle">{{thread.item.title}}</p>
								<p v-else style="padding-top:0em;" class="subtitle">{{thread.item.title.split(',').slice(0,2).join(',')}} and {{thread.item.users.length}} more</p>
								<p v-if="thread.item.items[0].text" class="subtitle info">{{thread.item.items[0].text.replace(/(\r\n|\n|\r)/gm, " ").slice(0,40)}}...</p>
								<p v-else class="subtitle info">Active {{FormatDate(thread.item.lastActivity)}}</p>
							</div>
						</div>
                      </div>
                      <div class="column is-10 main">
                        <div @click="chat.emojiEnabled = false" id="chat" v-if="MessagesLength > 0" class="chat-container" ref="chatContainer">
                          <div v-for="(item,index) in Messages" :key="index">
                            <div :class="IsSameUser(item.userId) === false ? 'chat-message' : 'chat-message is-me'">
                              <div :class="IsSameUser(item.userId) === false ? 'speech-bubble' : 'speech-bubble is-me'">
                                <div class="chat-message-content">
								  <div v-if="item.itemType === 0"> <!-- text -->
                                  	<p class="subtitle is-message">{{item.text}}</p>
								  </div>
								  <div v-else-if="item.itemType === 3"> <!-- link -->
								  	<p class="subtitle is-message">{{item.linkMedia.text}}</p>
									<img v-if="item.linkMedia.linkContext.linkImageUrl" class="long-image" :src="item.linkMedia.linkContext.linkImageUrl" alt="link image">
									<a class="message-link" :href="item.linkMedia.linkContext.linkUrl">{{item.linkMedia.linkContext.linkTitle}}</a>
								  	<br>
									<p class="subtitle is-message italic">{{item.linkMedia.linkContext.linkSummary}}</p>
								  </div>
								  <div v-else-if="item.itemType === 4"> <!-- media -->
									  <div v-if="item.media.mediaType === 1">
											<img style="object-fit: cover; margin-left:2em;" width="350px" height="200px" :src="item.media.images[0].uri" alt="">
									  </div>
									  <div v-else-if="item.media.mediaType === 2">
										  <video controls="controls" preload="metadata" :src="item.media.videos[0].uri+'#t=0.5'"></video>
									  </div>			  
									  <div v-else-if="item.media.mediaType === 8"> <!--Carousel-->

									  </div>
								  </div>
								  <div v-else-if="item.itemType === 5"> <!-- reelshare -->
								  		<p class="subtitle is-message">mentioned</p>
								  		<img style="margin-left:2em;" width="150px" height="100px" :src="item.reelShareMedia.media.images[0].uri" alt="">
								  </div>
								   <div v-else-if="item.itemType === 7"> <!-- ravenMedia -->
								   		<p class="subtitle is-message" v-if="item.visualMedia.isExpired">Expired {{item.visualMedia.media.mediaType === 1 ? 'image' : 'video'}}</p>
										<div v-else>
											<div v-if="item.visualMedia.media.mediaType === 1">
												<img style="object-fit: cover; margin-left:2em;" width="350px" height="200px" :src="item.visualMedia.media.images[0].uri" alt="">
											</div>
											<div v-else-if="item.visualMedia.mediaType === 2">
											<video controls="controls" preload="metadata" :src="item.visualMedia.media.videos[0].uri+'#t=0.5'"></video>
											</div>
										</div>
									</div>
								  <div v-else-if="item.itemType === 8"> <!-- storyshare -->
								  </div>	
								  <div v-else-if="item.itemType === 9"> <!-- action -->
								  	<p class="subtitle is-message">❤️ {{item.actionLog.description}}</p>
								  </div>
                                </div>
                                <div class="chat-message-footer">
                                  <span v-if="item.timeStamp !== 0" class="icon is-info is-small">
                                    <i class="fas fa-check"></i>
                                  </span>
								  <span @click="SendChatMessage(item,true)" v-else-if="item.timeStamp === -1" class="icon is-small">
                                    <i class="fas fa-redo"></i>
                                  </span>
                                  <span v-else class="icon is-small">
                                    <i class="fas fa-check"></i>
                                  </span>
                                  <p v-if="item.timeStamp !== 0 && item.timeStamp !==undefined" class="subtitle">{{FormatTime(item.timeStamp)}}</p>
                                </div>
                              </div>
							   	<div class="where-from">
									<img :src="MapUser(item.userId).profilePicture" alt="user picture">
							   		<p class="subtitle">{{MapUser(item.userId).userName}}</p>
								</div>
                            </div>
                          </div>
                        </div>
						<div v-else class="chat-container is-empty">
							<p v-if="!chat.isLoading" class="subtitle is-empty">Start chat with a user</p>
							<b-notification style="background:transparent; height:150px; margin:2em;" :closable="false">
								<b-loading :is-full-page="false" :active.sync="chat.isLoading" :can-cancel="false"></b-loading>
							</b-notification>
						</div>
                        <div v-if="MessagesLength > 0" class="chat-footer">
                          <div class="chat-input-container">
                              <div class="field has-addons">
                                <p class="control is-expanded">
                                  <v-text-area @on-click="SendChatMessage" @on-change="(e)=>chat.input = e" :data="chat.input" :auto-row="true"></v-text-area>
                                </p>
                                <p class="control">
                                  <a @click="SendChatMessage(chat.input)" class="button is-success">
                                    <span class="icon is-small">
                                      <i class="fas fa-paper-plane"></i>
                                    </span>
                                  </a>
                                </p>                       
                              </div>
                          </div>
                        </div>
                      </div>
                  </div>
                </div>
            </div>
          </div>
		  <div class="recent-comment-container" v-else-if="currentlySelected === 8">
			  <div v-for="(item,index) in recentComments" :key="index">
			  	<MediaCard 
				  @on-submit-reply-comment="SubmitReply" 
				  @on-submit-comment="SubmitComment" 
				  @on-delete-comment="DeleteComment"
				  @on-like-comment="LikeComment" 
				  :comments="item.comments" 
				  :media="item.media" 
				  :title="item.media.user.username" 
				  :profileImage="item.media.user.profilePicture"/>
			  </div>
		  </div>
        </div>
      </div>
    </div>
  </div>
</template>

<script>
import UserResults from '../Objects/UserResults';
import VTextArea from '../Objects/VTextArea';
import MediaCard from '../Objects/MediaCard';
import Dialog from '../Dialogs';
import moment from 'moment';
import { Base64ToBuffer, ValidateUrl, UUIDV4 } from '../../helpers'
import vue from 'vue';
export default {
  components:{
    UserResults,
    VTextArea,
	MediaCard,
  },
  data(){
    return{
      paginationSettings:{
        total:0,
        perPage:28,
        currentPage:0,
        rangeBefore:4,
        rangeAfter:5,
        isRounded:false,
      },
      currentlySelected:0,
      accounts:[],
      selectedAccount:{
        account:{},
        profile:{}
      },
      results:{
        loading:false,
        searchResultItems:[],
        displayableList:[],
        selectedList:[],
        max: 20,
        current:0
      },
      searchRes:{
        query:'',
        type:0
      },
      filterSearch:'',
      selected:[],
      justEntered:true,
      previousSelected:0,
      chat:{
		filterSearch:'',
		isLoading:false,
		inbox:[],
		input:'',
	  },
	  recentComments:[]
    }
  },
  beforeMount(){
    this.accounts = this.$store.getters.GetInstagramAccounts;
  },
  mounted(){
    this.SelectAccount(0);
    this.justEntered = false;
  },
  computed:{
    SelectedItemsList(){
      return this.results.selectedList;
    },
    currentlySelectedItems(){
      this.results.current = 0;
      //this.results.selectedList = []

      this.results.searchResultItems.filter(user => {
        if(user.selected === true){
          if(this.results.selectedList.findIndex(res=>res.item.object.username === user.item.object.username) === -1)
            this.results.selectedList.push(user)
        }
      });

      this.SelectedItemsList.forEach(res=>{
        if(res.selected === true){
          this.results.current++;
        }
      });

      return this.results.selectedList;
	},
	FilteredThreads(){
		return this.chat.inbox.filter(thread => {
			let user = thread.item.users.filter(user => {
				return user.userName.toLowerCase().includes(this.chat.filterSearch.toLowerCase())
			});
			if(user.length>0){
				return thread;
			}
		})
	},
    filteredList() {
      return this.results.searchResultItems.filter(user => {
        let cindex = this.SelectedItemsList.findIndex(r => r.item.object.username === user.item.object.username)
        if(cindex > -1)
         user.selected = this.SelectedItemsList[cindex].selected;

        return user.item.object.username.toLowerCase().includes(this.filterSearch.toLowerCase())
      });
    },
    MapSection(){
      switch(this.currentlySelected){
        case 1: return 'Following'
        case 2: return 'Followers'
        case 3: return 'Targeted Locations'
        case 4: return 'Targeted Users'
        case 5: return 'Suggested Users'
        case 7: return 'Search Location'
        case 8: return 'Search Topic'
      }
    },
    UsersAccounts:{
      get() {
        return this.accounts.map(res=>{
          return res
        });
      }
	},
	SelectedThread(){
		return this.chat.inbox[this.chat.inbox.findIndex(res=>res.selected === true)];
	},
	Messages:{
		get(){
			if(this.MessagesLength > 0){
				return this.SelectedThread.item.items;
			}
		}
	},
	MessagesLength(){
		if(this.SelectedThread === undefined) return 0;
		return this.SelectedThread.item.items.length;
	}
  },
  updated(){
	  let chatContext = this.$el.querySelector('#chat');
	  if(chatContext){
		chatContext.scrollTop = chatContext.clientHeight+3000;
	  }
  },
  methods:{
	SubmitReply(e){
		if(e===undefined){
			vue.prototype.$toast.open({
				message: 'Error has occured, please try again',
				type: 'is-danger'
			});
			return;
		}
		if(!e.message.split(' ')[1]){
			vue.prototype.$toast.open({
				message: 'Please write your message before submitting',
				type: 'is-danger'
			});
			return;
		}
		this.$store.dispatch('ReplyComment', {
			instagramAccountId: this.selectedAccount.account.id,
			media:e.media.mediaId,
			comment:e.replyData.object.pk,
			message:{text: e.message}
		}).then(res=>{
			vue.prototype.$toast.open({
				message: 'Sent Comment, please wait a minute before it updates here',
				type: 'is-info'
			});
		}).catch(err=>{
			vue.prototype.$toast.open({
				message: 'Failed to like comment ' +  err.response.data.message,
				type: 'is-danger'
        	});
		})
	},
	SubmitComment(e){
		if(e===undefined){
			vue.prototype.$toast.open({
				message: 'Error has occured, please try again',
				type: 'is-danger'
			});
			return;
		}
		if(!e.message){
			vue.prototype.$toast.open({
				message: 'Please write your message before submitting',
				type: 'is-danger'
			});
			return;
		}
		this.$store.dispatch('CreateComment', {
			instagramAccountId: this.selectedAccount.account.id,
			media:e.media.mediaId,
			message:{text: e.message}
		}).then(res=>{
			vue.prototype.$toast.open({
				message: 'Sent Comment, please wait a minute before it updates here',
				type: 'is-info'
			});
		}).catch(err=>{
			vue.prototype.$toast.open({
				message: 'Failed to like comment ' +  err.response.data.message,
				type: 'is-danger'
        	});
		})
	},
	DeleteComment(e){
		if(e===undefined){
			vue.prototype.$toast.open({
				message: 'Please make sure you have selected a comment to like',
				type: 'is-danger'
			});
			return;
		}
		this.$store.dispatch('DeleteComment', {
			instagramAccountId: this.selectedAccount.account.id,
			media:e.commentData.mediaId,
			comment:e.commentData.object.pk 
		}).then(resp=>{
			vue.prototype.$toast.open({
				message: 'Deleted Comment',
				type: 'is-info'
			});
		}).catch(err=>{
			vue.prototype.$toast.open({
				message: 'Failed to like comment ' +  err.response.data.message,
				type: 'is-danger'
        	});
		})
	},
	LikeComment(e){
		if(e===undefined)
		{
			vue.prototype.$toast.open({
				message: 'Please make sure you have selected a comment to like',
				type: 'is-danger'
			});
			return;
		}
		this.$store.dispatch('LikeComment',{ instagramAccountId: this.selectedAccount.account.id, comment:e.commentData.object.pk }).then(resp=>{
			vue.prototype.$toast.open({
				message: 'Liked Comment',
				type: 'is-info'
			});
		}).catch(err=>{
 			vue.prototype.$toast.open({
				message: 'Failed to like comment ' +  err.response.data.message,
				type: 'is-danger'
        	});
		})
	},
	MapUser(userId){
		let users = this.SelectedThread.item.users;
		let user = users.filter(x=>x.pk === userId)[0];
		if(user !== undefined){
			return {
				profilePicture: user.profilePicture,
				userName: user.userName,
				userId: user.pk,
				fullName: user.fullName
			}
		}
		else {
			let selfuser = this.selectedAccount.account;
			return {
				profilePicture: selfuser.profilePicture,
				userName: selfuser.username,
				userId: selfuser.userId,
				fullName: selfuser.fullName
			}
		}
	},
	ThreadAt(threadId){
		let threadIndex = this.ThreadIndex(threadId);
		if(threadIndex > -1)
			return this.chat.threads[threadIndex];
		else
			return undefined;
	},
	ThreadIndex(threadId){
		return this.chat.threads.findIndex(res => res.threadId === threadId);
	},
	GetThreadAsync(threadId){
		this.chat.isLoading = true;
		let localThread = this.ThreadAt(threadId);

		if(localThread !== undefined){
			this.chat.isLoading = false;
			return localThread;
		}
		else{
			this.$store.dispatch('GetThread', {instagramAccountId: this.selectedAccount.account.id, threadId: threadId, limit:1 })
			.then(res=>{
				this.chat.threads.push(res.data);
				let thread_ = this.ThreadAt(threadId);
				this.chat.isLoading = false;
				return thread_ === undefined ? [] : thread_;
			}).catch(err=>{
				console.log('errored' + err);
				this.chat.isLoading = false;
				return [];
			})
		}
	},
	IsSameUser(targetId){
		return this.selectedAccount.account.userId === targetId
	},
	SelectThread(index){
		this.chat.input = '';
		this.$bus.$emit('on-clean');
		this.chat.inbox.forEach(x=>{x.selected = false})
		this.chat.inbox[index].selected = true;
		//this.GetThreadAsync(this.SelectedThread.item.threadId);
		//document.getElementsByClassName('chat-container').scrollIntoView({ behavior: 'smooth', block: 'end' });
	},
    SendChatMessage(e, retry = false){
      if(e && retry!==true){
		  let request = {
			  recipients: this.SelectedThread.item.users.map(res=>{return res.pk}),
			  textMessage: e,
			  threads:null
		  }
		  let tempKey = UUIDV4();
		  let responsePre = {
 			text:e,
			userId:this.selectedAccount.account.userId,
			timeStamp: 0,
			itemId: tempKey,
			itemType: 0,
			media:null,
			mediaShare:null,
			clientContext: null,
			storyShare:null,
			ravenMedia:null,
			visualMedia:null,
			ravenViewModel:null,
			ravenSeenUserIds:null,
			ravenReplayChainCount:null,
			ravenSeenCount:null,
			ravenExpiringMediaActionSummary:null,
			actionLog:null,
			profileMedia:null,
			previewMedias:null,
			linkMedia:null,
			locationMedia:null,
			felixShareMedia:null,
			reelShareMedia:null,
			voiceMedia:null,
			animatedMedia:null,
			hashtagMedia:null,
			liveViewerInvite:null,
			videoCallEvent:null
		  }

		  this.SelectedThread.item.items.push(responsePre);

		  this.$store.dispatch('DMMessage', { type: 'text', id: this.selectedAccount.account.id, message:request }).then(resp=>{
			  const response = resp.data;

			  responsePre.timeStamp = response.timestamp;
			  responsePre.itemId = response.itemId;
			  responsePre.clientContext = response.clientContext;

			  let index = this.SelectedThread.item.items.findIndex(res=>res.itemId === tempKey);
			  this.SelectedThread.item.items[index] = responsePre;
			  
		  }).catch(err=>{
			  responsePre.timeStamp = -1;
			  responsePre.clientContext = 'error';
			  let index = this.SelectedThread.item.items.findIndex(res=>res.itemId === tempKey);
			  this.SelectedThread.item.items[index] = responsePre;
		  })
	  }
	  else if(retry === true){
		let request = {
			  recipients: this.SelectedThread.item.users.map(res=>{return res.pk}),
			  textMessage: e.text,
			  threads:null
		}
		let responsePre = item;
		this.$store.dispatch('DMMessage', {type: 'text', id: this.selectedAccount.account.id, message:request}).then(resp=>{
			  const response = resp.data;
			  responsePre.timeStamp = response.timestamp;
			  responsePre.itemId = response.itemId;
			  responsePre.clientContext = response.clientContext;
			  let index = this.SelectedThread.item.items.findIndex(res=>res.itemId === item.itemId);
			  this.SelectedThread.item.items[index] = responsePre;

		  }).catch(err=>{
			  responsePre.timeStamp = 0;
			  let index = this.SelectedThread.item.items.findIndex(res=>res.itemId === item.itemId);
			  this.SelectedThread.item.items[index] = responsePre;
		  })
	  }
	},
	FormatDate(date){
		return moment(date).fromNow()
	},
    FormatTime(date){
		if(/^(\d{16})$/.test(date) === true){
			return moment(parseInt(date)/1000).format('LT');
		}
		else {
	  		return moment(date).format('LT')
		}
    },
    Next(){
      Dialog.View({selectedItems: this.SelectedItemsList, selectedAccount:this.selectedAccount})
      .$on('send-post', (e) => {
        this.SendPost(e); 
      })
      .$on('send-message', (e)=> {
        this.ScheduleMessage(e);
      })
    },
    ScheduleMessage(e){
      var submitData = []
      var actionType = '';
      if(e === undefined)
        return;
      if(!e.entity.messages && e.entity.message.length<=0)
      {
        vue.prototype.$toast.open({
          message: 'Message body cannot be empty',
          type: 'is-danger'
        });
      }
      if(e.entity.link !== ''){ //if yes then send link message
        actionType = 'link';
        if(!ValidateUrl(e.entity.link)){
          vue.prototype.$toast.open({
            message: 'The link '+ e.entity.link +' you provided is not in a correct format',
            type: 'is-danger'
          });
        }
        submitData.push({
          recipients: this.results.selectedList.map(res=>res.item.object.userId.toString()),
          threads:null,
          textMessage: e.entity.message,
          link: e.entity.link
        })
      }
      else{
        actionType = 'text';
        submitData.push({ //send normal text message
          recipients: this.results.selectedList.map(res=>res.item.object.userId.toString()),
          threads:null,
          textMessage: e.entity.message
        })
      }
      
      this.$store.dispatch('CreateMessage', {type: actionType, id: this.selectedAccount.account.id, messages:submitData}).then(resp=>{
        vue.prototype.$toast.open({
          message: 'Scheduled Messages',
          type: 'is-success'
        });
        this.results.selectedList = [];
        this.results.current = [];
        this.SelectSection(this.MapType(this.currentlySelected));
      }).catch(err=>{
        vue.prototype.$toast.open({
          message: 'Could not send these messages' + err,
          type: 'is-danger'
        });
      })
    },
    SendPost(e){
      var type = '';
      var submitData = []
      if(e === undefined)
        return;
      if(e.mediaType < 0)
        return;
      switch(e.mediaType){
        case 1:
        case 8:
          type = 'photo'
          submitData.push({
            recipients: this.results.selectedList.map(res=>res.item.object.userId.toString()),
             image: {
              uri: (e.mediaUrl === undefined || e.mediaUrl === null) ? e.mediaBytes.toString() : e.mediaUrl,
              imageBytes: null,
              width:0,
              height:0
            }
          });
          break;
        case 2:
          type = 'video'
          submitData.push({
            recipients: this.results.selectedList.map(res=>res.item.object.userId.toString()),
             video: {
               video:{
                uri: (e.mediaUrl === undefined || e.mediaUrl === null) ? e.mediaBytes.toString() : e.mediaUrl,
                videoBytes: null,
                width:0,
                height:0
               }
            }
          });
          break;
      }
      this.$store.dispatch('CreateMessage', {type:type, id: this.selectedAccount.account.id, messages:submitData}).then(resp=>{
        vue.prototype.$toast.open({
          message: 'Scheduled Messages',
          type: 'is-success'
        });
        this.results.selectedList = [];
        this.results.current = [];
        this.SelectSection(this.MapType(this.currentlySelected));
      }).catch(err=>{
        vue.prototype.$toast.open({
          message: 'Could not send these messages' + err,
          type: 'is-danger'
        });
      })
    },
    Discard(){
      this.results.searchResultItems.forEach(item=>{item.selected = false});
      this.results.selectedList = [];
      this.results.current = [];
    },
    Close(e){
      this.results.selectedList = [];
      this.results.current = [];
      this.SelectAccount(e);
    },
    Revert(){
      document.getElementById('ditem_'+this.previousSelected).classList.add('is-active');
      document.getElementById('ditem_'+this.currentlySelected).classList.remove('is-active')
    },
    CloseSession(data){
      if(!this.justEntered && this.SelectedItemsList.length>0){
       vue.prototype.$dialog.confirm({
          title: 'Leave without saving?',
          cancelText: 'Cancel',
          confirmText: 'Leave',
          type: 'is-success',
          onConfirm: () => this.Close(data),
          onCancel: () => this.Revert()
        });
      } else {
        this.SelectAccount(data);
      }
    },
    updateList(e){
      let selectedUser = this.results.searchResultItems[e];
      if(selectedUser === undefined)
        return;
      if(selectedUser.selected === false){
        if(this.results.current >= this.results.max)
          return;
        let index = this.results.selectedList.findIndex(res => res.item.object.username === selectedUser.item.object.username);
        if(index===-1){
          selectedUser.selected = true;
        }
      }
      else{
        selectedUser.selected = false;
        this.results.selectedList.splice(this.results.selectedList.findIndex(r=>r.item.object.username === selectedUser.item.object.username),1);
      }
    },
    RemoveFromList(e){
      e.selected = false;
      this.results.selectedList.splice(this.results.selectedList.findIndex(r=>r.item.object.username === e.item.object.username),1);
    },
    MapType(e){
      if(e===undefined) return;
      switch(e){
        case 1: return {index: 1, name:'GetUserFollowingList'}
        case 2: return {index: 2, name:'GetUserFollowerList'}
        case 3: return {index: 3, name:'GetUserTargetLocation'}
        case 4: return {index: 4, name:'GetUsersTargetList'}
        case 5: return {index: 5, name:'GetUserFollowingSuggestionList'}
        case 6: return {index: 6, name:undefined}
		case 7: return {index: 7, name: undefined}
		case 8: return {index: 8, name: undefined}
      }
    },
    SelectSection(s){
      if(s !== undefined){
        this.previousSelected = this.currentlySelected;
        this.currentlySelected = s.index;
        if(s.index === 6){
          this.results.searchResultItems = []
		}
		else if(s.index === 7){
			this.results.searchResultItems = []
			this.GetUserInbox();
		}
		else if(s.index === 8){
			this.results.searchResultItems = []
			this.GetRecentComments();
		}
        else{
          if(s.name!==undefined && s.name !== null)
            this.LoadData(s.name)
        }
      }
	},
	GetRecentComments(){
		this.$store.dispatch('GetRecentComments', {
			instagramAccountId: this.selectedAccount.account.id,
			topic: this.selectedAccount.profile.profileTopic
		}).then(res=>{
			this.recentComments = res.data;
		}).catch(err=>{
			console.log(err);
		})
	},
	GetUserInbox(){
		this.$store.dispatch('GetUserInbox', {
			instagramAccountId: this.selectedAccount.account.id,
			topic: this.selectedAccount.profile.profileTopic
		}).then(res=>{
			console.log(res)
			this.chat.inbox = res.data.threads.map(res=> { return {item:res, selected:false}})
		}).catch(err=>{
			console.log(err)
		})
	},
    updatePage(e){
      this.paginationSettings.currentPage = e;
      this.results.displayableList = [];
      var millisecondsToWait = 250;
      let _this = this;
      setTimeout(function() {
        _this.NextLoad();
      }, millisecondsToWait);
    },
    NextLoad(){
      let currentSet = (this.paginationSettings.currentPage-1) * this.paginationSettings.perPage;
      this.results.displayableList  = this.results.searchResultItems.slice(currentSet, currentSet + this.paginationSettings.perPage)
    },
    SearchData(){
      if(!this.searchRes.query)
        return;
      switch(this.searchRes.type){
        case 0:
        case '0':
          this.results.searchResultItems = [];
          this.results.loading = true;
          this.$store.dispatch('SearchByTopic', {query:this.searchRes.query, instagramAccountId:this.selectedAccount.account.id, limit:2 }).then(r=>{
             this.results.searchResultItems = r.data.map(r=>{
                return {item: r, selected: false}
              });
              this.results.loading = false;
          }).catch(err=>{
            console.log(err);
            this.results.loading = false;
          });
          break;
        case 1:
        case '1':
          this.results.searchResultItems = [];
          this.results.loading = true;
          this.$store.dispatch('SearchByLocation', {query:this.searchRes.query, instagramAccountId:this.selectedAccount.account.id, limit:2 }).then(r=>{
            this.results.searchResultItems = r.data.map(r=>{
              return {item: r, selected: false}
            });
            this.results.loading = false;
          }).catch(err=>{
            console.log(err);
            this.results.loading = false;
          });
          break;
      }
    },
    LoadData(name){
      //this.paginationSettings.currentPage = 1;
      this.results.loading = true;
      this.results.searchResultItems = []
      this.$store.dispatch(name,
      { 
        instagramAccountId: this.selectedAccount.account.id,
        topic: this.selectedAccount.profile.profileTopic
      }).then((result) => {
        this.results.searchResultItems = result.data.map(res=>{
          return {item: res, selected: false}
        });
        //this.paginationSettings.total = result.data.length;
        //this.updatePage(this.paginationSettings.currentPage);
        this.results.loading = false;
      }).catch((err) => {
        this.results.loading = false;
      });
    },
    SelectAccount(index){
      if(index.id!==undefined)
        index = this.accounts.findIndex(res=>res.id === index.id)
      if(index > this.accounts.length || index < 0)
        this.selectedAccount.account = this.accounts[0];    
      else
        this.selectedAccount.account = this.accounts[index]
    
      this.selectedAccount.profile = this.$store.getters.UserProfile(this.selectedAccount.account.id)
      this.SelectSection(this.MapType(this.currentlySelected));
    }
  }
}
</script>

<style lang="scss">
.dialog{
    .modal-card-body{
      background:transparent;
      padding:0;
  }
}
.recent-comment-container{
	display: flex;  
	flex-direction: row; /* let the content flow to the next row */
	flex-wrap: wrap;
	//justify-content:flex-end;
	align-items: flex-end;
}
.loading-background{
	background:transparent !important;
}
.label-color-white{
  text-align: center;
  .label{color:#d9d9d9;}
}
.custom-dropdown{
  width:100%;
  .select-pic{
      border-radius:2em 2em 0em 2em;
      padding:0em;
      margin-left:-1em;
  }
  .button{
    text-align: left;
    width:250px;
    height:40px;
    border-radius:2em;
    background:#232323;
    color:#d9d9d9;
    border:none;
    margin-left:1em;
  }
  .dropdown {
    //width:100% !important;
    //background:#121212 !important; 
  }
  .dropdown-menu{
    border-radius: 1em!important;
    padding:0;
    background:#232323 !important;
    width:100%;
    margin-left:.5em;
    margin-top:.5em;
  }
  .dropdown-item{
    &:active{
      &.is-active{
        background:transparent !important;
      }
    }
    &.is-active{
      background:#121212 !important;
    }
    &:hover{
      border-radius: 0 !important;
      background:#121212 !important;
    }
  }
}
.selected-content{
  position:absolute;
  width:300px;
  text-align: center;
  background:#101010;
  border-radius: .5em;
}
.selected-holder{
  display: flex;
  margin:0 auto;
  flex-flow: row wrap;
  align-items: center;
  background:transparent;
  margin-left:3em;
  .selected-container{
    padding:.1em;
    img{
      border-radius: .3em;
      width:50px;
      height:50px;
      &:hover{
        cursor: pointer;
        transform: scale(1.1);
        border-radius:0.25em;
      }
    }
  }
}
.section-container{
	&.is-chat{
		position: absolute;
		width:90%;
		top:1em;
		bottom:0;
	}
  .speech-bubble {
    position: relative;
    background:#4CAF50;
    &.is-me{
      background: #232323;
    }
    border-radius: .5em;
    padding:1.1em;
    font-size: 1.1rem;
  }
  .speech-bubble.is-me:after{
    content: '';
    position: absolute;
    left:auto !important;
    right: 0em;
    top: 50%;
    width: 0;
    height: 0;
    border: 0.531em solid transparent;
    border-left-color: #232323;
    border-right: 0;
    border-bottom: 0;
    margin-top: -0.266em;
    margin-right: -0.531em;
  }
  .speech-bubble:after {
    content: '';
    position: absolute;
    left: 0;
    top: 50%;
    width: 0;
    height: 0;
    border: 1em solid transparent;
    border-right-color: #4CAF50;
    border-left: 0;
    border-bottom: 0;
    margin-top: -0.266em;
    margin-left: -0.532em;
  }
  .chat-side-panel{
    margin-left:1em;
    margin-right:1em;
    border: 1px solid #121212;
    box-shadow: -0.01rem 0 .7rem rgba(0, 0, 0, 0.19);
    .chat-column{
      max-height:86vh;
      .filter-search-users-chat{
		position: relative;
        margin:2em;
        .input{
          border:none;
          height:60px;
          background:#111111 !important;
          &::placeholder{
            color:#d9d9d988;
          }
        }
      }
      .user-item{
        padding-left:1.6em;
        //margin-bottom:.5em;
       // margin-top:.5em;
        background:#191919;
        width:100%;
        height:100px;
      	transition: background .5s ease;
        &.is-active{
			&:hover{
				background:#4CAF50;
			}
          background:#4CAF50;
        }
        &:hover{
		 transition: all .4s ease-in-out;
          background:#4CAF50;
          cursor: pointer;
        }
		.grouped-users{
			width: 60px;
			height:60px;
			border-radius: 50em;
			border: 2px solid wheat;
			float:left;
			background:rgb(252, 245, 233);
			>.group-profile-con:nth-child(1){
				border-radius: 1em;
				width: 30px;
				float:left;
				object-fit: cover;
				margin-left:.8em;
				margin-top:0em;
			}
			>.group-profile-con:nth-child(2){
				border-radius: 1em;
				width: 30px;
				float:left;
				object-fit: cover;
				margin-left:.8em;
				margin-top:0em;
			}
		}
        .profile-con{
          float:left;
          width:60px;
          height:60px;
          border-radius: 50em;
          margin-top:.8em;
          margin-left:.4em;
          border: 2px solid wheat;
          padding:0.1em;
          object-fit: cover;
        }
        .subtitle{
		  padding-top:1em;
		  margin:0;
          color:#d9d9d9;
          text-align: left;
          margin-left:5em;
          font-size: 16px;
		  &.info{
			  color:#d9d9d98e;
		  }
        }
      }
      .side{
        overflow: auto;
		scrollbar-width: none;
        background:#191919;
      }
      .main{
		display: flex;  
		flex-direction: row; /* let the content flow to the next row */
		flex-wrap: wrap;
		//justify-content:flex-end;
		align-items: flex-end;
        .chat-header{
          width:100%;
          height:15%;
          background:#161616;
        }
        .chat-footer{
          position: absolute;
          width:69.35%;
		  //margin-left:-20em;
          background:#161616;
          .send-message{
            float:right;
            margin:0;
            margin-top:-5.2em;
            border-radius: 0;
            .icon{
              font-size:40px;
            }
          }
          .chat-input-container{
            .textarea, textarea{
			margin-top:.6em;
              resize: none;
              border:none;
              color:#d9d9d9;
              background:#161616 !important;
              &:focus{
                border:none !important;
                box-shadow: none;           
              }
            }
            .button{
             // position: absolute;
              bottom: 0;
              right:0;
			  height:52.5px;
			  width:50px;
              border-radius: 0 !important;
              &.is-darker{
                background:#161616;
                border:none;
                color:#d9d9d970 !important;
              }
            }
            .input, input {
              border:none;
              color:#d9d9d9;
              background:#161616 !important;
              &:focus{
                border:none !important;
                box-shadow: none;           
              }
            }
            input::placeholder,textarea::placeholder{
                color:#d9d9d9a2;
            }
          }
        }
        .chat-container{
			overflow: auto;
			scrollbar-width: none;
			width:85%;
			height:80vh;
			background:#111;
			padding-bottom:2.5em;
			margin-bottom:3.5em;

			&.is-empty{
				padding-top:20em;
				height: 86vh;
			}
			.subtitle{
				&.is-empty{
					text-align: center;
					color:#d9d9d9;
				}
			}
          .chat-message{
            margin:1em;
            width:40%;
			min-width: 200px;
            &.is-me{
              margin-left:40em;
            }
			.where-from{
				display: flex;
				margin-top:.6em;
				img{
					width:30px;
					height:30px;
					border-radius: 2em;
				}
				.subtitle{
					color:#d9d9d9;
					font-size: 14px;
					margin-left:.5em;
					margin-top:.5em;
				}
			}
            .chat-message-header{
              //margin-top:2em;
              .subtitle
              {
                color:white;
              }
              //margin-left:31em;
            }
            .chat-message-content{
				.subtitle{
					&.is-message{
						text-align: left;
						color:white;
						font-size:16px;
						&.italic{
							font-style: italic;
							font-size:14px;
						}
					}
				}
				.message-link{
					color:#ebebeb;
					font-size:15px;
					font-weight: bold;
					&:hover{
						color:#fff;
					}
				}
				.long-image{
					width:100%;
				}
            }
            .chat-message-footer{
              float:right;
              width:90px;
              span{
                color:#d9d9d9 !important;
				&.is-info{
					color:#1f6ce7 !important;
				}
                float:right;
                font-size: 12px;
              }
              .subtitle{
                color:#d9d9d9;
                font-size:14px;
              }
            }
          }
        }
      }
    }
  }
  .section-header{
    padding:1em;
    margin-right:1em;
  }
  .results-container{
    .dark-search{
      margin: 0 auto !important;
      select{
        border:none;
        background:#121212;
        color:#d9d9d9;
      }
      option{
        color:#d7d7d7;
      }
    }
    .field{
      width:50%;
      margin:0 auto;
      margin-left: 25%;
    }
    .input{
      border:none;

      background:#111111 !important;
      &::placeholder{
        color:#d9d9d988;
      }
    }
    width:91%;
    margin:0 auto;
    padding:1em;
    .subtitle{
      text-align: center;
      color:#d9d9d9;
    }
  }
  .box{
    background:#232323;
    &.is-expanded{
      width:100% !important;
    }
  }
}
.messaging-container{
  margin:0 auto;
  width:100%;
  padding-left:5em;
  padding-top:.5em;
}
.container-column{
  .main-panel{
    //background:#222222;
    //height:100vh;
  }
  .side-panel{   
    //border-radius: 1em;
    .menu{
      text-align: left;
      .menu-label{
        color:#d9d9d9;
      }
      .menu-list{
        .is-small{
          margin: 0 auto;
        }
        color:#d0d0d0;
        a{
          padding:0.5em 0.75em;
          color:#d9d9d9;
          &:hover{
            background:#232323;
            //border-radius: 2em;
          }
          &.is-active{
           // border-radius:2em;
            //background:#13b94d !important;
          }
        }
        ul{
          li{
            span{
              color:#d0d0d0;
            }
          }
        }
      }
      padding:0;
      margin:0 auto;
      padding:1em;
      color:#d9d9d9;
    }
    .subtitle{
      color:#d9d9d9;
    }
    //background: #393939;
    //height: 100vh;
  }
}

.control{
  .select-pic{
    border-radius:2em 2em 0em 2em;
    margin-left:-.16em;
    padding:0em
  }
  &.is-big-sel{
    width: 100%;
    select{
      //padding:1.35em !important;
      padding-left:3em !important;
      background:#232323 !important;
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
      &:active{
      }
    }
  }
}
.pagination-previous, .pagination-next, .pagination-link{
  color:#121212 !important;
  background:#d9d9d9;
}
</style>