<template>
  <figure v-lazyload class="image__wrapper">
    <ImageSpinner
      class="image__spinner"
    />
    <img
      class="image__item"
      :data-url="source"
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
    }
  }
};
</script>

<style scoped lang="scss">
.image {
  &__wrapper {
    display: flex;
    justify-content: center;
    align-items: center;
    border-radius: 4px;

    &.loaded {
      .image {
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
    width: 100%;
    border-radius: 4px;
    transition: all 0.4s ease-in-out;
    opacity: 0;
    visibility: hidden;
    padding:1em;
    border-radius: 0.5em;
    width:250px;
    height:250px;
    box-shadow: 5px 4px 3px 4px rgba(0,0,0,0.05);
    object-fit: cover;
    transition: all .2s ease-in-out;
  }
}
</style>
