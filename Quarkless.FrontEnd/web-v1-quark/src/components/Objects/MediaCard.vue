<template>
	<div class="media-card-container">
		<div class="media-card-header">
			<img :src="profileImage" alt="">
			<p class="subtitle">{{title}}</p>
		</div>
		<hr>
		<div class="media-card-content">
			<img v-if="media.mediaType === 1" :src="media.mediaUrl" alt="">
			<video v-else-if="media.mediaType === 2" :src="media.mediaUrl"></video>
			<img v-if="media.mediaType === 8" :src="media.mediaUrl" alt="">
			<div class="media-info">
				<span class="icon has-text-danger">
					<i class="fas fa-heart"></i>
				</span>
				<p class="subtitle">{{media.likesCount}}</p>
			</div>
		</div>
		<div class="media-card-footer">
			<div @mouseenter="handleOver(index)" @mouseleave="handleLeave" class="comment-container" ref="commentcn"
			v-for="(comment, index) in commentsData" :key="index">
				<div class="comment-user-area">
					<img :src="comment.item.profilePicture" alt="">
					<p class="subtitle">{{comment.item.username}}</p>
				</div>
				<div class="comment-area">
					<p class="subtitle">{{comment.item.object.text}}</p>
					<div v-if="comment.isHovering"  class="field options-container has-addons">
					<p class="control">
						<a @click="replyTo(comment.item)" class="reply-comment button is-rounded is-dark is-small">
							<span class="icon">
								<i class="fas fa-reply"></i>
							</span>
							<span>Reply</span>
						</a>
					</p>
					<p class="control">
						<a @click="!comment.item.object.hasLikedComment ? LikeComment(index) : UnlikeComment(index)" 
						class="like-comment button is-rounded is-dark is-small">
							<span :class="comment.item.object.hasLikedComment? 'icon liked':'icon'">
								<i class="fas fa-heart"></i>
							</span>
							<span>Like</span>
						</a>
					</p>
					<p class="control">
						<a @click="DeleteComment(index)" class="delete-comment button is-rounded is-dark is-small">
							<span class="icon">
								<i class="fas fa-times"></i>
							</span>
							<span>Delete</span>
						</a>
					</p>
					</div>
				</div>
			</div>
		</div>
		<div class="reply-comment">
			<div class="field has-addons">
			<div v-if="replyingTo.username!==undefined" class="control">
				<a class="button is-info">
					<span class="icon">
						<i class="fa fa-reply"></i>
					</span>
				</a>
			</div>
			<div class="control is-expanded">
				<input @keyup="OnChange" @keyup.enter.exact="SubmitComment" ref="replyMessage" v-model="responseMessage" placeholder="write your comment..." type="text" class="input">
			</div>
			<div class="control">
				<a @click="emojiEnabled = !emojiEnabled" class="button is-dark">
					<span class="icon">
						<i class="fa fa-grin-alt"></i>
					</span>
				</a>
			</div>
			<div class="control">
				<a @click="SubmitComment" class="button is-success">
					<span class="icon">
						<i class="fa fa-paper-plane"></i>
					</span>
				</a>
			</div>
			</div>
		</div>
		<picker @select="selectedEmoji" v-if="emojiEnabled" set="google" :style="EmojiPosition" />
	</div>
</template>

<script>
import {Picker} from 'emoji-mart-vue'
export default {
	props:{
		title:String,
		profileImage:String,
		media:Object,
		comments:Array
	},
	components:{
		picker : Picker
	},
	data(){
		return{
			emojiEnabled:false,
			commentsData:[],
			responseMessage:'',
			replyingTo:{

			}
		}
	},
	mounted(){
		console.log(this.media)
		if(this.comments!==undefined && this.comments!==null)
			this.commentsData = this.comments.map(res=> { return { item:res, isHovering:false } });
	},
	computed:{
		EmojiPosition(){
			let div = this.$refs.commentcn;
			return {
				position:'absolute',
				bottom:'12em',
				left: div[0].offsetLeft
			}
		}
	},
	methods:{
		OnChange(e){
			if(this.responseMessage === '' && e.key === 'Backspace'){
				this.replyingTo = {}
			}
		},
		SubmitComment(){
			if(this.replyingTo.username!==undefined){
				this.$emit('on-submit-reply-comment',{message:this.responseMessage, replyData: this.replyingTo, media:this.media})
			}
			else{
				this.$emit('on-submit-comment',{message:this.responseMessage, media:this.media})
			}
			this.responseMessage = '';
			this.replyingTo = {};
		},
		LikeComment(index){
			this.$emit('on-like-comment',{commentData:this.commentsData[index].item})
		},
		UnlikeComment(index){
			this.$emit('on-unlike-comment',{commentData:this.commentsData[index].item})
		},
		DeleteComment(index){
			this.$emit('on-delete-comment',{commentData:this.commentsData[index].item})
		},
		selectedEmoji(emoji){
			this.responseMessage += emoji.native;
		},
		handleOver(index){
			this.commentsData.forEach(r=>r.isHovering = false);
			this.commentsData[index].isHovering = true;
		},
		handleLeave(){
			this.commentsData.forEach(r=>r.isHovering = false);
		},
		replyTo(e){
			if(this.responseMessage.includes(e.username))
				return;
			this.replyingTo = e;
			this.responseMessage = '@' + e.username + ' '
			this.$refs.replyMessage.focus();
		}
	}
}
</script>

<style lang="scss" scoped>
::-webkit-scrollbar{
	//width:20px;
	width:5px;
	background:#1414149a;
	display: block;
}
::-webkit-scrollbar-track {
  background: #1414149a; 
}
/* Handle */
::-webkit-scrollbar-thumb {
  background: #323232; 
}
.media-card-container {
	background:#232323;
	width:450px;
	height:800px;
	padding:1em;
	margin:1em;
	box-shadow: -0.01rem 0 .7rem rgba(0, 0, 0, 0.19);
	hr{
		margin-top:0.3em;
		background:#121212;
		width:100%;
		height:1px;
	}
	.media-card-header{
		margin-top:-.4em;
		display: flex;
		.subtitle{
			color:#d9d9d9;
			margin-left:.4em;
			margin-top:.4em;
			text-align: left;
		}
		img {
			width:50px;
			height:50px;
			border-radius: 5em;
		}
	}
	.media-card-content{
		img{
			width:450px;
			height:450px;
			object-fit: fill;
		}
		video{
			width:450px;
			height:450px;
		}
		.media-info{
			display: flex;
			.icon{
				margin-top:.3em;
			}
			.subtitle{
				color:#d9d9d9;
				margin-top:.1em;
			}
		}
	}
	.reply-comment{
		margin-top:1em;
		input,.input{
			border:none;
			background:#121212 !important;
			color:#d9d9d9 !important;
			&:focus{
				box-shadow: none;
			}
			
		}
		input::placeholder{
    		color:#d9d9d98a !important;
  		}
	}
	.media-card-footer{
		height: 155px;
		overflow-y: auto;
		.comment-container{
			margin-top:1em;
			&:hover{
				cursor: pointer;
				background:#12121213;
				border-radius: 2em;
			}
		}
		.comment-area{
			.options-container{
				margin-left:8em;
			//display: flex;
				.button{
					margin-top:-1em;
					&.like-comment{
						.icon{
							&.liked{color:rgb(231, 52, 52);}
						}
					}
					&:hover{
						&.delete-comment{
							color:rgba(241, 214, 125, 0.87);
						}
						&.like-comment{
							color:rgb(231, 52, 52);
						}
						&.reply-comment{
							color:#93b6f8;
						}
					}
				}
			}
			margin-left:3.4em;
			margin-top:-1.9em;
			.subtitle{
				font-size: 16px;
				color:#d9d9d9;
				text-align: left;
			}
		}
		.comment-user-area{
			display: flex;
			margin:.5em;
			.subtitle{
				color:#d9d9d9;
				margin-left:.4em;
				//margin-top:.7em;
				text-align: left;
				font-size: 14px;
			}
			img {
				width:40px;
				height:40px;
				border-radius: 5em;
			}
		}
	}
}
</style>