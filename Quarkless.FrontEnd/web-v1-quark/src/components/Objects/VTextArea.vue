<template id="v-text-area">
  <div class="textarea_wrap">
	<div class="field has-addons">
		<p class="control is-expanded">
			<textarea @keydown.enter.exact.prevent @keyup.enter.exact="sendData(true)" @keydown.enter.shift.exact="newline" @change="sendData(false)" placeholder="Write a message..." class="text_input textarea" v-model="Text" ref="textarea"></textarea>
		</p>
		<div v-if="autoRow" class="text_input textarea shadow" ref="shadow">{{ Text }}!</div>	
		<p class="control">
			<a @click="emojiEnabled = !emojiEnabled" class="button is-darker">
			<span class="icon is-default">
				<i class="fa fa-grin-alt"></i>
			</span>
			</a>
		</p>
	</div>
	 <picker @select="selectedEmoji" v-if="emojiEnabled" set="google" :style="{ position: 'absolute', bottom: '5em', right: '0em' }" />
  </div>
</template>

<script>
import {Picker} from 'emoji-mart-vue'
export default {
  template: '#v-text-area',
  components:{
	picker : Picker
  },
  props:{
    data: String,
    autoRow: {
      type: Boolean,
      default: false
    },
    default: String
  },
  data(){
    return{
	  input:'',
	  emojiEnabled:false,
    }
  },
  computed:{
    Text:{
      get(){
        return this.input;
      },
      set(value){
        this.input = value;
      }
    }
  },
  mounted(){
    this.updateHeight();
	this.input = this.data;
	let _this = this;
	this.$bus.$on('on-clean', ()=>{
		_this.onClean();
	})
  },
  updated(){
    this.updateHeight();
  },
  methods: {
	onClean(){
		this.input = ''
	},
	selectedEmoji(emoji){
		this.Text+=emoji.native;
	},
    newLine(){
      this.Text = '\n';
    },
    sendData(direct){
      if(direct)
      {
        this.$emit('on-click', this.Text);
      }
      else{
        this.$emit('on-change', this.Text);
      }
    },
    updateHeight(){
      if(this.Text.length > 100){
        this.$refs.textarea.style.height = this.Text.split(' ').length*1 +'px' //this.$refs.shadow.clientHeight + 5 + 'px'; 
      }
      else{
        this.$refs.textarea.style.height = 45 +'px' //this.$refs.shadow.clientHeight + 5 + 'px'; 
      }
    }
  }
}
</script>

<style lang="scss" scoped>
.textarea_wrap {
  position: relative;
}
.textarea {
  padding-right:2.5em;
  line-height: 1.5;
  min-height: 31px;
  width: 95% !important;
  font-family: inherit;
  font-size: 14px; 
}

.shadow {
  position: absolute;
  left: -9999px;
  pointer-events: none;
  white-space: pre-wrap;
  word-wrap: break-word;
  resize: none;
}
</style>