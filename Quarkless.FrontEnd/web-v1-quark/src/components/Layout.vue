<template>
<div class="home_layout">
  <div class="columns is-mobile">
    <div v-if="isNavOn" class="column is-1">
        <Nav @onHide="changeState"></Nav>
    </div>
    <div v-else>
      <b-tooltip type="is-dark" label="Show Panel" position="is-right">
        <a @click="changeState">
        <b-icon
          class="floater"
          pack="fas"
          icon="compress"
          size="is-medium"
          type="is-light">
        </b-icon>
        </a>
      </b-tooltip>
      <b-tooltip type="is-dark" label="Go to Dashboard" position="is-right">
        <router-link to="/manage">
        <b-icon
          class="floater is-house"
          pack="fas"
          icon="rocket"
          size="is-default"
          type="is-light">
        </b-icon>
        </router-link>
      </b-tooltip>
        <b-tooltip type="is-dark" label="Logout" style="position:absolute; bottom:3em;right:2.8em;">
        <a @click="signout">
        <b-icon
          class="is-user"
          pack="fas"
          icon="sign-out-alt"
          size="is-medium"
          type="is-light">
        </b-icon>
        </a>
        </b-tooltip>
    </div>
    <div :class="isNavOn ? 'column is-11' : 'column is-12'" :style="isNavOn ? '' : 'margin:0 auto'" >
        <div class="main_area">
          <router-view v-if="isLoaded" :key="$route.fullPath"></router-view>
        </div>
    </div>
  </div>
  <div class="footere">
    <p class="subtitle is-8">Copyright © 2019 HashtagGrow; All rights have been reserved</p>
  </div>
</div>
</template>
<script>
import Nav from './Nav.vue'

export default {
  name: 'app',
  components: {
    Nav
  },
  data(){
    return {
      isProfileButtonDisabled:false,
      isNavOn:true,
      isLoaded:false
    }
  },
  mounted(){
    this.$store.dispatch('AccountDetails', {"userId":this.$store.state.user}).then(s=>{
          this.$store.dispatch('GetProfiles', this.$store.state.user).then(c=>{
            this.isLoaded = true;
          })
    });
    this.isNavOn = this.$store.getters.MenuState === 'true'
  },
  methods:{
    signout(){
      this.$store.dispatch('logout');
      window.location.reload();
    },
    changeState(){
      this.$store.dispatch('HideunHideMenu', this.isNavOn = !this.isNavOn)
      console.log(this.isNavOn)
    }
  }
}
</script>

<style lang="scss">
.column{
  background:#141414;
  padding:0 !important;
}
.footere{
  background:#121212 !important;
  opacity: .7;
  width:100%;
  padding:0;
  margin: 0 auto;
  height:200px;
  p{
    text-align: center !important;
    padding:4em;
    color:#d9d9d9;
  }
}
.floater{
  z-index: 99999;
  position:fixed !important;
  top:1.15em;
  left:1.6em;
  width:50px;
  height:50px;
  transition: all .2s ease-in-out;
  &:hover{
    color:#13b94d !important;
    cursor: pointer;
    transform: scale(1.2);
  }
  &.is-house{
    top:4.5em;
    left:1.85em;
  }
}

.is-user{
  z-index: 99999;
  position:fixed !important;
  bottom:1.15em;
  right:1.6em;
  width:50px;
  height:50px;
  transition: all .2s ease-in-out;
    &:hover{
    transform: scale(1.2);
    color:#13b94d !important;
    cursor: pointer;
  }
}

.main_area{
  width:100% !important;
  height:100% !important;
  margin: 0 auto !important;
  background-color:#141414;
  border-radius:0px;
  //padding-left:10em;
}
.home_layout{
  width: 100%;
  padding:0em;
  height:100%;
}
::-webkit-scrollbar {
    display: none;
}
</style>
