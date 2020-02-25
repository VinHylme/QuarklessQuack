<template>
	<div v-if="IsHome" class="container home">
		<button class="button is-success" @click="LoadComponent('login')">Login</button>
		<!-- <button class="button is-success" @click="LoadComponent('register')">Register</button> -->
	</div>
  <div v-else class="container">
    <router-view :key="$route.fullPath"></router-view>
  </div>
</template>

<script>
import {GetUserLocation} from '../localHelpers';
import vue from 'vue';
export default {
  name:"MainPage",
  data(){
    return {
    }
  },
  methods:{
    LoadComponent(componentName){
      this.$router.push({name:componentName});
    }
  },
  computed:{
    IsHome(){
      return this.$route.path === '/'
    }
  },
  mounted(){
    GetUserLocation().then(resp=>{
    }).catch(err=>{
      vue.prototype.$toast.open({
				message: 'Enabling your location allows us to provide you with safer and more accurate results',
        type: 'is-white',
        duration:10000
			});
    })
  }
}
</script>

<style lang="scss">
.container{
  &.home{
    margin:0 auto;
    margin-top:10vw;
    padding:5em;
    width:50vw;
    height:30vw;
    border-radius:.5em;
    background: #000;
    .button{
      width:150px;
      height:80px;
      border-radius: 0;
    }
  }
}
</style>