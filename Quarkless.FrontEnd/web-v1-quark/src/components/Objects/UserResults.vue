<template>
  <div class="result-container">
    <div class="result-holder zoomable" v-for="(data, index) in datas" :key="index">
        <div @click="SelectUser(index)" class="card-res" :style="data.selected ? 'border:2px solid #008542' : ''">
          <div class="card-res-header">
            <div class="card-res-header-headline">
               <div v-if="data.item.lookup !== null && data.item.lookup.length > 0">
                 <div v-for="(lookup,index) in data.item.lookup" :key="'item_'+index">
                  <b-tooltip v-if="lookup.lookupStatus === 2" :label="'Recently Sent ' + MapActionType(lookup.actionType).tooltip" type="is-success">
                    <b-icon :icon="MapActionType(lookup.actionType).icon" pack="fas" type="is-success" size="is-default"/>
                  </b-tooltip>
                  <b-tooltip v-if="lookup.lookupStatus === 1" label="Pending" type="is-info">
                    <b-icon :icon="MapActionType(lookup.actionType).icon" pack="fas" type="is-info" size="is-default"/>
                  </b-tooltip>
                </div>
              </div>
            </div>
            <div class="card-res-header-figure">
              <img :src="data.item.object.profilePicture" width="95px" height="95px" class="image_circle">
             <!-- <ImageItem v-if="data.item.object.profilePicture" :source="data.item.object.profilePicture" width="100px" height="100px" :isRounded="true"/> -->
            </div>
            <div class="card-res-header-disc">
              <p class="subtitle">@{{data.item.object.username}}</p>
            </div>
          </div>
          <div class="card-res-content">
          </div>
        </div>
      </div>
  </div>
</template>

<script>
import ImageItem from './ImageItem';
export default {
  props:{
    datas:Array
  },
  computed:{
  },
  mounted(){
  },
  components:{
    ImageItem
  },
  methods:{
    SelectUser(position){
      this.$emit('selected-user', position)
    },
    MapActionType(actionType){
      if(actionType === undefined || actionType === null){
        return;
      }
      switch(actionType){
        case 20: //text
          return { icon: 'inbox', tooltip: 'Text' }
        case 21: //link
          return { icon: 'link', tooltip: 'Link' }
        case 22: //photo
          return { icon: 'portrait', tooltip: 'Photo' }
        case 23: //video
          return { icon: 'film', tooltip: 'Video' } 
        case 24: //audio
          return { icon: 'microphone', tooltip: 'Audio' }
        case 25: //profile
          return { icon: 'history', tooltip: 'Profile Link' }
      }
    }
  }
}
</script>

<style lang="scss">
@-webkit-keyframes glow {
  from {
    text-shadow: 0 0 0px rgb(32, 115, 224), 0 0 0px rgb(21, 101, 206), 0 0 10px rgb(27, 111, 219), 0 0 0px rgb(27, 111, 219), 0 0 00px rgb(27, 111, 219), 0 0 0px rgb(27, 111, 219), 0 0 22px rgb(27, 111, 219);
  }
  
  to {
    text-shadow: 0 0 5px rgb(27, 111, 219), 0 0 0px rgb(27, 111, 219), 0 0 20px rgb(27, 111, 219), 0 0 0px rgb(27, 111, 219), 0 0 00px rgb(27, 111, 219), 0 0 0px rgb(27, 111, 219), 0 0 0px rgb(27, 111, 219);
  }
}
.card-res{
  width:250px;
  height: 150px;
  background:#000;
  padding:.5em;
  margin:.5em;
  border-radius: .8em;
  box-shadow: -0.01rem 0 .7rem rgba(0, 0, 0, 0.19);
  //border: 2px solid rgb(67, 141, 226);
  .card-res-header{
    .card-res-header-headline{
      float:left;
      .icon{
        -webkit-animation: glow 1s ease-in-out infinite alternate;
        -moz-animation: glow 1s ease-in-out infinite alternate;
        animation: glow 1s ease-in-out infinite alternate;
        
        font-size: 20px;
        margin-top:.15em;
        //color:#e6e6e6 !important;
        &:hover{
          //color:rgb(0, 152, 223) !important;
        }
      }
    }
    .card-res-header-figure{
      .image_circle{
        border-radius: 5em;
        display: block;
        margin-left: auto;
        margin-right: auto;     
      }
    }
    .card-res-header-disc{
      .subtitle{
        font-size: 19px;
      }
    }
  }
  .card-res-content{

  }
}
.result-container{
  width:100%;
  display: flex;
  margin:0 auto;
  flex-flow: row wrap;
  align-items: center;
  background:transparent;
  margin-left:0.5em;
}
.result-holder{
  transition: all 0.5s ease;
  &:hover{
    transition: all .2s ease-in-out;
    cursor: pointer;
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
</style>