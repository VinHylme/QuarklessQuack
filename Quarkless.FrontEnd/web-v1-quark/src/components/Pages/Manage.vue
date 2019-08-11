<template>
<div class="container-fluid">
        <b-notification v-if="refreshShowNotification"
                v-bind:type="isSuccess?'is-success':'is-danger'"
                aria-close-label="Close notification"
                role="alert">
                {{alert_text}}
        </b-notification>
        <div :style="!isNavOn?'margin-left:5.5em;':''">
                <div class="accounts_container">
                        <div v-for="(acc,index) in InstagramAccounts" :key="index">
                                <InstaCard @ChangeState="StateChanged" @RefreshState="NotifyRefresh" @ViewProfile="GetProfile" :id="acc.id" :username="acc.username" :agentState="acc.agentState" 
                                :name="acc.fullName" :profilePicture="acc.profilePicture" :biography="acc.userBiography"
                                :userFollowers="acc.followersCount" :userFollowing="acc.followingCount" :totalPost="acc.totalPostsCount" :IsProfileButtonDisabled="IsProfileButtonDisabled"/>
                        </div>
                        <div class="card is-hover">
                                <div class="card-content">
                                        <a href="#"><h3 class="title is-1">+</h3></a>
                                </div>
                        </div>
                </div>
        </div>
</div>
</template>

<script>
import Vue from 'vue';
import InstaCard from "../Objects/InstaAccountCard";
export default {
        name:"manage",
        components:{
                InstaCard
        },
        data(){
        return{
                IsProfileButtonDisabled:true,
                InstagramAccounts:[],
                isSuccess:false,
                refreshShowNotification:false,
                alert_text:'',
                isNavOn:false
        }
        },
        created(){
               
        },
        beforeMount(){
                this.isNavOn = this.$store.getters.MenuState === 'true';
                this.InstagramAccounts = this.$store.getters.GetInstagramAccounts;
                if(this.$store.getters.UserProfiles!==undefined)
                        this.IsProfileButtonDisabled=false;          
        },
        computed:{

        },
        methods:{
                GetProfile(id){
                        if(!this.IsProfileButtonDisabled){
                                var profile = this.$store.getters.UserProfiles[this.$store.getters.UserProfiles.findIndex((_)=>_.instagramAccountId == id)];
                                this.$router.push('/profile/'+ profile._id)
                        }
                },
                StateChanged(data){
                        this.$store.dispatch('ChangeState', data).then(res=>{
                                if(res)
                                        Vue.prototype.$toast.open(
                                        {
                                                message: 'Updated!',
                                                type: 'is-success',
                                                position: 'is-bottom',
                                        }   
                                );   
                        })
                },
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
        width: 100%;
        padding-top:0.5em;
        padding-left:3.5em;
        display: flex;
        flex-flow: row wrap;
        align-items: center;

}
.card {
        &.is-hover{
                margin-left:0.4em;
                margin-top:1em;
                width:400px !important;
                height:395px !important;
                background-color: #292929 !important;
                color:white !important;
                .card-content{
                        h3{
                                color:wheat;
                                padding:0em;
                                font-size:250px;
                                text-align: center;
                                &:hover{
                                        color:#292929;
                                }
                        }
                        
                }
                &:hover{
                        background:wheat !important;
                }
        }
       
}
body,html {
        overflow-y: hidden;
        width: 100%;
        text-align: center;
}
</style>
