<template>
<div class="container-fluid">
        <b-notification v-if="refreshShowNotification"
                v-bind:type="isSuccess?'is-success':'is-danger'"
                aria-close-label="Close notification"
                role="alert">
                {{alert_text}}
        </b-notification>
        <div class="accounts_container">
                <div v-for="(acc,index) in InstagramAccounts" v-bind:key="index">
                        <InstaCard @RefreshState="NotifyRefresh" v-bind:id="acc.id" v-bind:username="acc.username" v-bind:agentState="acc.agentState" 
                        v-bind:name="acc.fullName" v-bind:profilePicture="acc.profilePicture" v-bind:biography="acc.userBiography"
                        v-bind:userFollowers="acc.followersCount" v-bind:userFollowing="acc.followingCount" v-bind:totalPost="acc.totalPostsCount"/>
                </div>
                <div class="card is-dark">
                        <div class="card-content">
                                <a href="#"><h3 class="title is-1">+</h3></a>
                        </div>
                </div>
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
                InstagramAccounts:[],
                isSuccess:false,
                refreshShowNotification:false,
                alert_text:''
        }
        },
        mounted() {
                this.$store.dispatch('AccountDetails', {"userId":this.$store.state.user}).then(res=>{
                        if(res!==undefined){
                                this.InstagramAccounts = this.$store.getters.GetInstagramAccounts;
                        }
                });
        },
        methods:{
                NotifyRefresh(isSuccess){
                        if(isSuccess){
                                this.isSuccess = true;
                                this.refreshShowNotification = true;
                                this.alert_text = "Account state has been refreshed";
                        }
                        else{
                                this.isSuccess = false;
                                this.refreshShowNotification = true;
                                this.alert_text = "Could not log into the account";
                        }
                }
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
.card {
        &.is-dark{
                margin:0.2em;
                width:400px !important;
                height:395px !important;
                background-color: #292929 !important;
                color:white !important;
        }
        .card-content{
        h3{
                color:#fefefe;
                padding:0em;
                font-size:250px;
                text-align: center;
        }
   }
}
body {
        width: 100%;
        text-align: center;
}
</style>
