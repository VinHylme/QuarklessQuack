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
import Axios from 'axios';
export default {
  name: 'app',
  components: {
    Layout,
    MainPage
  },
 created: function () {
   this.MockResponseTest();
   this.timer = setInterval(this.MockResponseTest, 6000)
  },
  data(){
    return {
      Authorized:this.$store.getters.IsLoggedIn,
      timer:''
    }
  },
  methods:{
    MockResponseTest(){
     Axios.interceptors.response.use(undefined, function (err) {
          return new Promise(function (resolve, reject) {
            if (err.status === 401 && err.config && !err.config.__isRetryRequest) {
            // if you ever get an unauthorized, logout the user
              this.$store.dispatch('logout')
            // you can also redirect to /login if needed !
            }
            throw err;
          });
      });
    }
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
</style>