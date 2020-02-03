<template>
<div class="ccontainer">
        <div :style="!isNavOn?'margin-left:7em;':''">
                <div class="accounts_container" >
                        <div v-for="(acc,index) in InstagramAccounts" :key="index">
                                <InstaCard @onChangeBiography="onChangeBiography" @onChangeProfilePicture="onChangeProfilePic" @OnConfirmUser="ConfirmUser" @ChangeState="StateChanged" @RefreshState="NotifyRefresh" @ViewLibrary="GetLibrary" @ViewProfile="GetProfile" :id="acc.id" :username="acc.username" :agentState="acc.agentState" 
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
                <div class="modal-card is-custom" style="width: 100%; height:35vw; padding:0;">
                    <header class="modal-card-head">
                        <p class="modal-card-title">Link your Instagram Account</p>
                    </header>
                    <section class="modal-card-body" style="padding-top:1em; padding-left:2em; padding-right:2em;">
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
                        <br>
                        <b-message title="Account Linking" type="is-danger" has-icon aria-close-label="Close message">
                        Please enter your Instagram Credentials, your password is encrypted and not seen by anyone
                        </b-message>
                        <b-message title="For best experience" type="is-info" has-icon aria-close-label="Close message">
                        Please ensure your account is at least 2 weeks old, Instagram spam detection system targets new accounts more than older ones.
                        </b-message>
                    </section>
                    <footer class="modal-card-foot">
                        <button @click="LinkAccount" :class="isLinkingAccount?'button is-light is-rounded is-large is-loading' : 'button is-light is-rounded'" style="margin:0 auto;">
                                <b-icon icon="link">

                                </b-icon>
                                <span>Link Account</span>
                        </button>                    
                    </footer>
                </div>
        </b-modal>
        <b-modal :active.sync="needToVerify">
                <div class="modal-card is-custom" style="width: 90%; height:25vw; padding:0; z-index:99999;">
                        <header class="modal-card-head">
                                <p class="modal-card-title">Verify your Instagram Account</p>
                         </header>
                    <section class="modal-card-body" style="width:35vw;">
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
                <footer class="modal-card-foot">
                   <button @click="SendVerifyCode" :class="isSendingVerifyCode?'button is-light is-rounded is-large is-loading' : 'button is-light is-rounded'" style="margin:0 auto;">
                                <b-icon icon="badge">
                                </b-icon>
                                <span>Send Verification Code</span>
                        </button>                    
                    </footer>
                </div>
        </b-modal>
</div>
</template>

<script>
import Vue from 'vue';
import InstaCard from "../Objects/InstaAccountCard";
import {GetUserDetails} from '../../localHelpers'
export default {
        name:"manage",
        components:{
                InstaCard
        },
        data(){
        return{
                IsProfileButtonDisabled:true,
                InstagramAccounts:[],
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
               this.$emit('unSelectAccount');
        },
        mounted(){
		// this.$store.getters.UserInformation.then(res=>{
		// 	this.$store.dispatch('AddUserDetails', res).then(r=>{
		// 		console.log(r)
		// 	})
		// })

		this.isNavOn = this.$store.getters.MenuState === 'true';
		this.InstagramAccounts = this.$store.getters.GetInstagramAccounts;
		if(this.$store.getters.UserProfiles!==undefined)
			this.IsProfileButtonDisabled=false;       
			
			this.$bus.$on('onFocusBio', (id)=>{
				this.$bus.$emit('cancel-other-focused');
				this.$bus.$emit('focus-main', id);
			})
        },
        computed:{

        },
        methods:{
                clickOutside(){
                        this.$bus.$emit('clickedOutside')
                },
                onChangeBiography(data){
                        this.$store.dispatch('ChangeBiography', {instagramAccountId: data.id, biography: data.biography}).then(resp=>{
                                  Vue.prototype.$toast.open({
                                        message: "Successfully Changed Profile Biography",
                                        type: 'is-success',
                                        position:'is-top',
                                        duration:4000
                                });
                                this.$bus.$emit("doneUpdatingBiography")
                        }).catch(err=>{
                                Vue.prototype.$toast.open({
                                        message: "Could not Update your biography at this time",
                                        type: 'is-danger',
                                        position:'is-top',
                                        duration:4000
                                })
                                 this.$bus.$emit("doneUpdatingBiography")
                        })
                },
                onChangeProfilePic(data){
                        this.$store.dispatch('ChangeProfilePicture', {instagramAccountId: data.id, image: data.image}).then(resp=>{
                                 Vue.prototype.$toast.open({
                                        message: "Successfully Changed Profile Picture",
                                        type: 'is-success',
                                        position:'is-top',
                                        duration:4000
                                });
                        }).catch(err=>{
                                Vue.prototype.$toast.open({
                                        message: "Could not Change profile picture at this time",
                                        type: 'is-danger',
                                        position:'is-top',
                                        duration:4000
                                })
                        })
                },
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
                                                        }).catch(err=>{console.log(err.response)})
                                                        this.$store.dispatch('GetProfiles', this.$store.state.user).then(respo=>{
                                                                this.$router.push('/profile/'+ resp.data.profileId)
                                                        }).catch(err=>console.log(err.response));
                                                }
                                        }
                                        this.isLinkingAccount = false;
                                }).catch(err=>{                   
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
                GetLibrary(id){
                        this.$router.push('/library/'+ id)
                },
                ConfirmUser(id){
                       let targetAccount = this.InstagramAccounts[this.InstagramAccounts.findIndex((op)=>op.id === id)]
                       if(targetAccount.challengeInfo){
                               Vue.prototype.$toast.open({
                                        message: "We need to verify you are the right account holder, please verify with the code sent to you at " + targetAccount.challengeInfo.details,
                                        type: 'is-info',
                                        position:'is-top',
                                        duration:25000
                                });
                                this.needToVerify = true;
                                this.verifyPath  = targetAccount.challengeInfo.challangePath
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
                                Vue.prototype.$toast.open({
                                        message: 'Account state has been refreshed',
                                        type: 'is-success'
                                })
                        }
                        else{
                                Vue.prototype.$toast.open({
                                        message: 'Could not log into the account',
                                        type: 'is-danger'
                                })
                        }
                }
        }
}
</script>

<style lang="scss">
@import '../../Style/darkTheme.scss';
.modal-card{
  background-color:$modal_background;

}
.modal-card .modal-card-body{
     background-color:$modal_background;
     
}
.modal-card-body{
        background:$modal_body;
        color:$main_font_color;
        label{
                color:$main_font_color;
                text-align: left;
        }
        .input{
                color:$main_font_color !important;
                &::placeholder{
                        color:$main_font_color;
                }
        }
        .control-label{
                &:hover{
                        color:$wheat;
                }
        }
}
.modal-card-foot{
        background:$backround_back;
        border:none;
}
.modal-card-head{
        background:$backround_back;
        border:none;
        .modal-card-title{
                color:$main_font_color;
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
body {
       // overflow-y:hidden;
        width: 100%;
        text-align: center;
}

</style>
