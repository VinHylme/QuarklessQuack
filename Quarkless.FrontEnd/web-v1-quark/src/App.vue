<template>
  <div id="app">
    <div v-if="Authorized">
      <Layout/>
    </div>
    <div v-else>
      <MainPage/>
    </div>
  </div>
</template>

<script>
import Layout from "./components/Layout.vue";
import MainPage from "./components/MainPage";
import axios from 'axios';
export default {
  name: 'app',
  components: {
    Layout,
    MainPage
  },
  created: function () {

  },
  mounted(){

  },
  data(){
    return {
      Authorized:this.$store.getters.IsLoggedIn,
      timer:''
    }
  },
  methods:{
  
  }
}
</script>

<style lang="scss">
//$card-background-color:black;
@import "~bulma/sass/utilities/_all";
//$primary: #3ea6ff;
//$primary-invert: findColorInvert($primary);
$sucess:#13b94d;
$twitter: rgb(35, 73, 117);
$twitter-invert: findColorInvert($twitter);
$colors: (
    "white": ($white, $black),
    "black": ($black, $white),
    "light": ($light, $light-invert),
    "dark": ($dark, $dark-invert),
    //"primary": ($primary, $primary-invert),
    "info": ($info, $info-invert),
    "success": ($success, $success-invert),
    "warning": ($warning, $warning-invert),
    "danger": ($danger, $danger-invert),
    "twitter": ($twitter, $twitter-invert)
);
//$link: $primary;
//$link-invert: $primary-invert;
//$link-focus-border: $primary;
@import "~bulma";
@import "~buefy/src/scss/buefy";
@import 'Style/darkTheme.scss';

html,
body{
  background: $background;
  height:100%;
  overflow-x: hidden !important;
}

/* Enter and leave animations can use different */
/* durations and timing functions.              */
.slide-fade-enter-active {
  transition: all .3s ease;
}
.slide-fade-leave-active {
  transition: all .3s cubic-bezier(1.0, 0.5, 0.8, 1.0);
}
.slide-fade-enter, .slide-fade-leave-to
/* .slide-fade-leave-active below version 2.1.8 */ {
  transform: translateX(10px);
  opacity: 0;
}
</style>