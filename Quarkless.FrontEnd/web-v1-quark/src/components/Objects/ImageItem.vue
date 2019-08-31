<template>
  <figure @click="click" v-lazyload class="image-figure__wrapper">
    <ImageSpinner
      class="image-figure__spinner"
    />
    <img
      :class="['image-figure__item']"
      :data-url="source"
      :style="[size]"
      alt="random image"
    >
  </figure>
</template>

<script>
import ImageSpinner from "./ImageSpinner";

export default {
  name: "ImageItem",
  components: {
    ImageSpinner
  },
  props: {
    source: {
      type: String,
      required: true
    },
    width:String,
    height:String,
    isRounded:Boolean
  },
  data(){
    return {
      size:{
        width:this.width,
        height:this.height,
        objectfit: 'cover',
        borderRadius:this.isRounded ? '.4em' : '0'
      }
    }
  },
  created(){

  },
  mounted(){
  },
  methods:{
    click(e){
      this.$emit('click',e);
    }
  }
};
</script>

<style scoped lang="scss">
.image-figure {
  &__wrapper {
    display: flex;
    justify-content: center;
    align-items: center;
    border-radius: 4px;

    &.loaded {
      .image-figure {
        &__item {
          visibility: visible;
          opacity: 1;
          border: 0;
        }

        &__spinner {
          display: none;
          width: 100%;
        }
      }
    }
  }

  &__item {
    border-radius: 4px;
    transition: all 0.4s ease-in-out;
    opacity: 0;
    visibility: hidden;
    padding:0.4em;
    border-radius: 0.5em;
    box-shadow: 5px 4px 3px 4px rgba(0,0,0,0.05);
    transition: all .2s ease-in-out;
  }
}
</style>
