<template>
  <div class="profiles_container">
    <ProfileCard v-for="(profile,index) in profiles" :key="index" :config="profile"/>
  </div>
</template>

<script>
import ProfileCard from '../Objects/ProfileCard'
export default {
  name:"profiles",
  components:{
    ProfileCard
  },
  data(){
    return{
      profiles:[]
    }
  },
  created(){    
  },
  beforeMount(){
    // this.$store.dispatch('AccountDetails', {"userId":this.$store.state.user});
    // this.$store.dispatch('GetProfiles', this.$store.state.user);
    this.profiles = this.$store.getters.UserProfiles;
  }
}
</script>

<style lang="scss" scoped>
.profiles_container{
  padding:1em;
  display: flex;
  flex-flow: row wrap;
  align-items: center;
}

</style>
