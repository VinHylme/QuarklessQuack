<template>
<div class="accounts_container">
     <div v-for="(acc,index) in InstagramAccounts" v-bind:key="index">
             <InstaCard v-bind:id="acc.id" v-bind:username="acc.username" v-bind:agentState="acc.agentState" v-bind:name="acc.fullName" v-bind:profilePicture="acc.profilePicture" v-bind:biography="acc.userBiography"/>
     </div>
</div>
</template>

<script>
import InstaCard from "../Objects/InstaAccountCard";
export default {
        name:"manage",
        components:{
                 InstaCard
        },
        data(){
        return{
                InstagramAccounts:[]
        }
        },
        mounted() {
                this.$store.dispatch('AccountDetails',{"userId":"lemonkaces", "token":this.$store.state.token}).then(res=>{
                        if(res!==undefined){
                                this.InstagramAccounts = this.$store.getters.GetInstagramAccounts;
                        }
                });
         }
}
</script>

<style lang="scss">
.accounts_container{
        padding:1em;
        display: flex;
        flex-flow: row wrap;
        align-items: center;

}
body {
        width: 100%;
        text-align: center;
}
</style>
