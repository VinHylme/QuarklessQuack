<template>
	<div v-if="IsHome" class="container">
		<button class="button is-success" @click="LoadComponent('login')">Login</button>
		<button class="button is-success" @click="LoadComponent('register')">Register</button>
	</div>
	<router-view v-else :key="$route.fullPath"></router-view>
</template>

<script>
import Fingerprint2 from 'fingerprintjs2';
export default {
  name:"MainPage",
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
    if (window.requestIdleCallback) {
      requestIdleCallback(function () {
          Fingerprint2.get(function (components) {
            //console.log(components) // an array of components: {key: ..., value: ...}
            var values = components.map(function (component) { return component.value })
            var murmur = Fingerprint2.x64hash128(values.join(''), 31)
            //console.log(murmur)
          })
      });
    } else {
      setTimeout(function () {
          Fingerprint2.get(function (components) {
            console.log(components) // an array of components: {key: ..., value: ...}
          })  
      }, 500)
    }
  },
}
</script>

<style lang="scss">
	.container{
		margin:0 auto;
	}
</style>