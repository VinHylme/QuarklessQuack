<template>
<div class="home_layout">
  <div class="columns is-mobile">
    <div class="bar-nav">
      <b-tooltip type="is-dark" label="Go back to Site" position="is-right">
        <router-link to="/">
        <b-icon
          class="floater is-site"
          pack="fas"
          icon="home"
          size="is-medium"
          type="is-light">
        </b-icon>
        </router-link>
      </b-tooltip>
      <b-tooltip type="is-dark" label="Go to Dashboard" position="is-right" >
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
       <b-tooltip type="is-dark" label="Communications" position="is-right" >
        <router-link to="/communications">
        <b-icon
          class="floater is-messaging"
          pack="fa"
          icon="inbox"
          size="is-default"
          type="is-light">
        </b-icon>
        </router-link>
      </b-tooltip>
      <b-tooltip v-if="showData.userSelected" type="is-dark" label="Media Library">
        <router-link :to="'/library/'+showData.userSelected">
        <b-icon
          class="floater is-library"
          pack="fa"
          icon="images"
          size="is-default"
          type="is-light">
        </b-icon>
        </router-link>
      </b-tooltip>
      <b-tooltip v-if="showData.userSelected" type="is-dark" label="Show Activity">
       <a @click="$store.state.showingLogs = !$store.state.showingLogs">
          <b-icon
            class="floater is-show-extra"
            pack="fa"
            :icon="$store.state.showingLogs?'angle-left':'angle-right'"
            size="is-medium"
            type="is-light">
          </b-icon>
        </a>
        </b-tooltip>
        <b-tooltip type="is-dark" label="Settings ❤️ coming soon ❤️" position="is-right">
        <a class="">
          <b-icon
            class="is-setting is-disabled"
            pack="fas"
            icon="cog"
            size="is-default"
            type="is-light">
          </b-icon>
        </a>
      </b-tooltip>
        <b-tooltip type="is-dark" label="Logout">
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
    <div class="column is-12" style="margin:0 auto;" >
        <div class="main_area">
          <router-view @selectedAccount="activateUser" @unSelectAccount="deactivateUser" 
		  v-if="isLoaded" :key="$route.fullPath"></router-view>
        </div>
    </div>
  </div>
  <div class="footere">
    <p class="subtitle is-8">Copyright © 2019 Quitic; All rights have been reserved</p>
  </div>
</div>
</template>
<script>
export default {
  name: 'app',
  data(){
    return {
      timer:'',
      isProfileButtonDisabled:false,
      isLoaded:false,
      showData:{
        userSelected:null
      }
    }
  },
  mounted(){
	this.loadData();
    this.timer = setInterval(this.loadData, 12000);
  },
  methods:{
	loadData(){
		this.$store.dispatch('AccountDetails', {"userId":this.$store.state.user}).then(s=>{
			this.$store.dispatch('GetProfiles', this.$store.state.user).then(c=>{
				this.isLoaded = true;
			})
		});
	},
    deactivateUser(){
      this.showData.userSelected  = null;
    },
    activateUser(e){
      this.showData.userSelected = e;
    },
    signout(){
      this.$store.dispatch('logout');
      window.location.reload();
    }
  }
}
</script>

<style lang="scss">
@import '../Style/darkTheme.scss';
@media (min-width:850px){
  .column{
  background:$background;
  padding:0 !important;
}
.footere{
  background:$background !important;
 // opacity: .7;
  width:100%;
  padding:0;
  margin: 0 auto;
  height:200px;
  p{
    text-align: center !important;
    padding:4em;
    color:$main_font_color;
  }
}
.floater{
  z-index: 888;
  position:fixed !important;
  top:1.15em;
  left:1.6em;
  width:50px;
  height:50px;
  transition: all .2s ease-in-out;
  &:hover{
    color:$highlight_color!important;
    cursor: pointer;
    transform: scale(1.2);
    &.is-disabled{
      color:#696969;
      opacity:0.4;
    }
  }
  &.is-site{
	  font-size: .8rem;
	  left:2em;
  }
  &.is-house{
    top:4em;
    left:1.85em;
  }
  &.is-messaging{
    top:6.5em;
    left:1.85em;
  }
  &.is-library{
    top:9.3em;
    left:1.85em;
  }
  &.is-show-extra{
    top:11.6em;
    left:1.6em;
  }
}
.is-setting{
    z-index: 99999;
    position:fixed !important;
    bottom:1.15em;    
    left:1.7em;
    font-size: 17px;
    &.is-disabled{
      color:#696969;
      opacity:0.4;
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
    color:$highlight_color !important;
    cursor: pointer;
  }
}
.main_area{
  width:100% !important;
  height:100% !important;
  margin: 0 auto !important;
  background-color:$background;
  border-radius:0px;
  //padding-left:10em;
}
.home_layout{
  width: 100%;
  padding:0em;
  height:100%;
  background:$background;
}
::-webkit-scrollbar {
   display: none;
  // background: #232323; 
  // width:6px;
  //opacity: 0;
}
}

@media (max-width: 850px) {
  .bar-nav{
    position: absolute;
    bottom: 0;
    margin:0 auto;
    padding:0;
    z-index: 999;
    width:105vw;
    height:15vh !important;
    display: flex;
    flex-direction: row;
    background: rgb(31, 31, 136);
    .icon{
      width:100px;
      height:100px;
      background:black;
    }
  }
}
</style>
