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
    Axios.interceptors.response.use(undefined, function (err) {
      return new Promise(function () {
        if (err.status === 401 && err.config && !err.config.__isRetryRequest) {
          this.$store.dispatch('logout')
        }
        throw err;
      });
    });
  },
  data(){
    return {
      Authorized:this.$store.getters.IsLoggedIn
    }
  }
}
</script>

<style lang="scss">
@import "~bulma/sass/utilities/_all";
$primary: #065b9c;
$primary-invert: findColorInvert($primary);
$twitter: rgb(16, 51, 90);
$twitter-invert: findColorInvert($twitter);
$colors: (
    "white": ($white, $black),
    "black": ($black, $white),
    "light": ($light, $light-invert),
    "dark": ($dark, $dark-invert),
    "primary": ($primary, $primary-invert),
    "info": ($info, $info-invert),
    "success": ($success, $success-invert),
    "warning": ($warning, $warning-invert),
    "danger": ($danger, $danger-invert),
    "twitter": ($twitter, $twitter-invert)
);
$link: $primary;
$link-invert: $primary-invert;
$link-focus-border: $primary;
@import "~bulma";
@import "~buefy/src/scss/buefy";
html,
body{
  overflow:hidden;
  background: #fafafa;
  height:100%;
}
</style>