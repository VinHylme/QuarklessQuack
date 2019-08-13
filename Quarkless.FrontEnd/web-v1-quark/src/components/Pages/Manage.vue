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
                                        <a @click="isAccountLinkModalOpened = true"><h3 class="title is-1">+</h3></a>
                                </div>
                        </div>
                </div>
        </div>
        <b-modal :active.sync="isAccountLinkModalOpened" has-modal-card>
                <!-- <form action="" style="width:50vw;" > -->
                <div class="modal-card" style="width: 100%; height:35vw;">
                    <header class="modal-card-head">
                        <p class="modal-card-title">Link your Instagram Account</p>
                    </header>
                    <section class="modal-card-body" v-if="!needToVerify">
                        <b-field label="Username">
                            <b-input
                                type="text"
                                v-model="linkData.username"
                                placeholder="Your Username"
                                required>
                            </b-input>
                        </b-field>

                        <b-field label="Password">
                            <b-input
                                type="password"
                                v-model="linkData.password"
                                password-reveal
                                placeholder="Your password"
                                required>
                            </b-input>
                        </b-field>

                        <!-- <b-checkbox>Remember me</b-checkbox> -->
                        <br>
                        <br>
                        <b-message title="Account Linking" type="is-danger" has-icon aria-close-label="Close message">
                        Please enter your Instagram Credentials, your password is encrypted and not seen by anyone
                        </b-message>
                    </section>
                    <section v-else class="modal-card-body" style="width:35vw;">
                        <b-field label="Verification Code">
                            <b-input
                                type="text"
                                v-model="code"
                                placeholder="Your Code"
                                required>
                            </b-input>
                        </b-field>
                        <br>
                        <b-message title="Account Linking - Verify" type="is-info" has-icon aria-close-label="Close message">
                         Please enter the verification code sent to you, sometimes Instagram detects your account as a spammer or for other security reasons, verifying your account will allow instagram to register your current location as safe
                        </b-message>
                    </section>
                    <footer class="modal-card-foot" v-if="!needToVerify">
                        <button @click="LinkAccount" :class="isLinkingAccount?'button is-light is-rounded is-large is-loading' : 'button is-light is-rounded'" style="margin:0 auto;">
                                <b-icon icon="link">

                                </b-icon>
                                <span>Link Account</span>
                        </button>                    
                    </footer>
                     <footer class="modal-card-foot" v-else>
                        <button @click="SendVerifyCode" :class="isSendingVerifyCode?'button is-light is-rounded is-large is-loading' : 'button is-light is-rounded'" style="margin:0 auto;">
                                <b-icon icon="badge">

                                </b-icon>
                                <span>Send Verification Code</span>
                        </button>                    
                    </footer>
                </div>
        <!-- </form> -->
        </b-modal>
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
                isNavOn:false,
                isAccountLinkModalOpened:false,
                isLinkingAccount:false,
                isSendingVerifyCode:false,
                linkData:{
                        username:'',
                        password:''
                },
                needToVerify:false,
                code:'',
                verifyPath:{}
        }
        },
        created(){
               
        },
        mounted(){
                this.isNavOn = this.$store.getters.MenuState === 'true';
                this.InstagramAccounts = this.$store.getters.GetInstagramAccounts;
                if(this.$store.getters.UserProfiles!==undefined)
                        this.IsProfileButtonDisabled=false;          
        },
        computed:{

        },
        methods:{
                SendVerifyCode(){
                        if(this.code){
                                this.isSendingVerifyCode = true;
                                let data = JSON.stringify({username:this.linkData.username, password:this.linkData.password, challangeLoginInfo:this.verifyPath});
                                this.$store.dispatch('SubmitCodeForChallange', {code:this.code, account:data}).then(resp=>{
                                        this.isSendingVerifyCode = false;
                                        if(resp.data===true || resp.data === 'true'){
                                                this.LinkAccount();
                                        }
                                }).catch(err=>{
                                        this.isSendingVerifyCode = false;
                                })
                        }
                },
                LinkAccount(){
                        if(this.linkData.username && this.linkData.password){
                                this.isLinkingAccount = true;
                                let data = JSON.stringify({username:this.linkData.username, password:this.linkData.password, type:0});
                                this.$store.dispatch('LinkInstagramAccount',data).then(resp=>{
                                        if(resp.data !== undefined || resp.data !==null || resp.data.instagramAccountId!==undefined){
                                                if(resp.data.verify!==undefined){
                                                        Vue.prototype.$toast.open({
                                                                message: "We need to verify you are the right account holder, please verify with the code sent to you at " + resp.data.details,
                                                                type: 'is-info',
                                                                position:'is-top',
                                                                duration:25000
                                                        });
                                                        this.needToVerify = true;
                                                        this.verifyPath  = resp.data.challangePath;

                                                }else{
                                                        Vue.prototype.$toast.open({
                                                                message: "Successfully added, will now redirect you to your profile page",
                                                                type: 'is-success',
                                                                position:'is-bottom',
                                                                duration:8000
                                                        })
                                                        this.$store.dispatch('AccountDetails', {"userId":this.$store.state.user}).then(resp=>{
                                                                console.log(resp);
                                                        }).catch(err=>{console.log(err.response)})
                                                        this.$store.dispatch('GetProfiles', this.$store.state.user).then(respo=>{
                                                                this.$router.push('/profile/'+ resp.data.profileId)
                                                        }).catch(err=>console.log(err.response));
                                                }
                                        }
                                        this.isLinkingAccount = false;
                                }).catch(err=>{                   
                                        console.log(err.response);                    
                                                Vue.prototype.$toast.open({
                                                        message: "Oops, looks like the account details don't match the instagram servers or the account has already been registered here, please try again",
                                                        type: 'is-danger',
                                                        position:'is-bottom',
                                                        duration:8000
                                                })
                                        
                                        this.isLinkingAccount = false;
                                })
                        }
                },
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
.modal-card-body{
        background:#323232;
        color:#d9d9d9;
        label{
                color:#d9d9d9;
                text-align: left;
        }
        .control-label{
                &:hover{
                        color:wheat;
                }
        }
}
.modal-card-foot{
        background:#121212;
        border:none;
}
.modal-card-head{
        background:#121212;
        border:none;
        .modal-card-title{
                color:#d9d9d9;
        }
        
}
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
