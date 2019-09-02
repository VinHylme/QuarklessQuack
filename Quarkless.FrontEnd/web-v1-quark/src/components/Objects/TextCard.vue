<template>
  <div @mouseover="onHover" @mouseout="outHover" :class="isHovering ? 'text-card-container is-hovering' : 'text-card-container'">
      <div class="card-text" :style="'width:'+width">
        <div class="card-text-header">
          <div class="card-text-header-avatar">
            <b-icon class="avatar-icon" :type="type" pack="fas" :icon="icon"></b-icon>
            <p class="subtitle">{{date}}</p>
          </div>
        </div>
        <div class="card-text-content" @click="onSelected">
          <textarea @mouseover="onHover" @mouseout="outHover" :disabled="!allowEdit" v-if="!isArray" style="padding:.2em;" :class="isHovering?'subtitle light is-hovering':'subtitle light'" :rows="rows" v-model="message"></textarea>
          <b-taginput :allow-new="!allowEdit" v-else type="is-twitter" size="is-medium" class="hash-tag-input"
            :on-paste-separators="[',','@','-',' ']"
            :confirm-key-codes="[32, 13]"
            :before-adding="evaluateTags"
            icon="hashtag"
            icon-pack="fas"
            maxlength="100"
            maxtags="30"
            :has-counter="false"
            v-model="messageArray">
          </b-taginput>
          <b-field v-if="link" label="Link">
            <input class="input" type="text" v-model="link" placeholder="e.g. www.mycoolwebsite.com">
          </b-field>
        </div>
        <div class="card-text-footer" v-if="allowDelete || allowEdit">
          <a v-if="allowDelete" class="footer-item" @click="onClickDelete">
            <b-icon icon="trash" pack="fas" type="is-danger"></b-icon>
          </a>
          <a v-if="allowEdit" class="footer-item" @click="onClickEdit">
            <b-icon icon="edit" pack="fas" type="is-light"></b-icon>
          </a>
        </div>
      </div>
  </div>
</template>

<script>
export default {
  props:{
    width:String,
    rows:Number,
    type:String,
    isArray:Boolean,
    data:Object,
    icon:String,
    date:String,
    message:String,
    link:String,
    messageArray:Array,
    allowDelete:Boolean,
    allowEdit:Boolean
  },
  methods:{
    onSelected(){
      this.$emit('click-select', this.data);
    },
    onHover(){
      if(!this.allowEdit){
        this.isHovering = true;
      }
    },
    outHover(){
      if(!this.allowEdit){
        this.isHovering = false;
      }
    },
    onClickDelete(){
      this.$emit('click-delete', this.data);
    },
    onClickEdit(){
      this.$emit('click-edit', {originalData: this.data, message: this.isArray? this.messageArray : this.message, link:this.link});
    },
    evaluateTags(hashtag){
      if(hashtag.length<2) return false;
        if(hashtag.includes("#")){
          if(!this.messageArray.includes(hashtag))
            return true;
          else
            return false;
        }
        else{
          hashtag = '#' + hashtag;
          if(!this.messageArray.includes(hashtag)){
            this.messageArray.push(hashtag);
            return false;
          }
          else
            return false;
        }
    }
  },
  data(){
    return{
      isHovering:false
    }
  }
}
</script>

<style lang="scss" >
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
.taginput-container{
    //height: 250px;
    color:#fefefe !important;
    border:none !important;
    background:#232323 !important;
}
input{
  color:#fefefe !important;
  background:#232323 !important;
}
.is-hovering{
  cursor: pointer;
}
.card-text{
  //height:150px;
  margin:1em;
  border-radius: .7em;
  box-shadow: -0.1rem 0 .3rem rgba(0,0,0,.2);
  background:#232323;
  transition: background 0.9s ease;
  &:hover{
   transition: all .2s ease-in-out;
    background:#333333;
    //cursor: pointer;
    .card-text-header{
      background: #232323;
    }
  }
  .card-text-header{
    border-radius: .7em .7em 0 0;
    height:50px!important;
    background:#333333;
    .card-text-header-avatar{
      .avatar-icon{
        float:left !important;
        padding:1.5em;
      }
      .subtitle{
        color:#d9d9d9;
        float:right;
        padding-right:1em;
        padding-top:.5em;
      }
    }
  }
  .card-text-content{
    label{
      text-align: left;
      margin-left:.1em;
      color:#d9d9d9;
    }
    .input{
      border:none;
      border-radius: 0;
      &::placeholder{
        color:#d0d0d0;
      }
    }
    padding:0.4em; 
    .subtitle{
      &.light{
        color:#d9d9d9;
        font-size: 20px;
        resize:none;
        width:100%;
        height:100%;
        border:none;
        background:transparent !important;
      }
    }
  }
  .card-text-footer{
    height:55px;
    .footer-item{
      opacity: .9;
      float:right;
     // padding:1em;
      margin-top:1.5em;
      margin-right:1em;
      &:hover{
        opacity: .6;
      }
    }
    //height:50px;
  }
}
</style>