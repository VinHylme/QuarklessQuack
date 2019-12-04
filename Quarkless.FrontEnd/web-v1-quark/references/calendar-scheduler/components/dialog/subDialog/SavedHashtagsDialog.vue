<template>
  <transition name="zoom-out">
    <div class="v-cal-dialog" v-if="isActive">
      <div class="v-cal-dialog__bg" style="background:transparent !important;" @click="cancel"></div>
      <div class="section-start">
         <header class="v-cal-dialog-card__header">
            <h5 class="v-cal-dialog__title">Saved Hashtags</h5>
          </header>
          <section class="body-container">
            <div class="res_container">
                <div v-for="(hashtag,index) in hashtagLibrary.items" :key="index">
                    <TextCard @click-select="selectedHashtags" type="is-light" :isArray="true" :data="hashtag" icon="hashtag" :date="formatedDate(hashtag.dateAdded)" :messageArray="hashtag.hashtag"/>
                </div>
            </div>
          </section>
      </div>
    </div>
  </transition>
</template>

<script>
import state from '../../../../../src/State';
import route from '../../../../../src/Route';
import moment from 'moment';
import TextCard from '../../../../../src/components/Objects/TextCard';
export default {
  components:{
    TextCard
  },
  props:{
    profile:Object
  },
  data(){
    return {
      isActive:false,
      hashtagLibrary:{
        items:[]
      }
    }
  },
  beforeMount(){
    document.body.appendChild(this.$el);
  },
  mounted(){
    this.isActive = true;
    this.loadData();
  },
  methods:{
    selectedHashtags(e){
      this.$emit('click-selected', e);
      this.close();
    },
    formatedDate(e){
      return moment(e).format("YYYY-MM-DD HH:mm:ss");
    },
    loadData(){
    this.hashtagLibrary.items = state.getters.UserHashtagsLibrary(route.app.$route.params.id);
    if(this.hashtagLibrary.items === undefined || this.hashtagLibrary.items === null){
      state.dispatch('GetSavedCaption', state.getters.User).then(resp=>{
        this.hashtagLibrary.items = state.getters.UserHashtagsLibrary(route.app.$route.params.id);
      });
    };
    },
    cancel() {
        this.close()
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
.section-start{
  position:absolute;
  top:5em;
  right:3em;
  bottom:0;
  background: #141414;
  border-radius:0.5em;
  width:20% !important;
  height:85% !important;
  border: 1px solid #323232;
  box-shadow: -0.1rem 0 .5rem rgba(0, 0, 0, 0.637);

  //box-shadow: none;
 // box-shadow: 4px 2px 3px 5px rgba(30, 30, 30, 0.1) !important;
}
.body-container{
  overflow: auto;
  height:700px;
  width:100% !important;
  background:transparent; 
  color:#d9d9d9;
  margin-left:13em !important;
  margin:0 auto;
  .res_container{
    width:100% !important;
  }
}
</style>