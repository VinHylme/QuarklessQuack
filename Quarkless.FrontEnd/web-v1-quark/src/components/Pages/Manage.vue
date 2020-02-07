<template>
<div class="ccontainer">
        <div :style="!isNavOn?'margin-left:7em;':''">
                <div class="accounts_container" >
                        <div v-for="(acc,index) in InstagramAccounts" :key="index">
                                <InstaCard @onChangeBiography="onChangeBiography" @onChangeProfilePicture="onChangeProfilePic" 
                                @ChangeState="StateChanged" @RefreshState="NotifyRefresh" @ViewLibrary="GetLibrary" 
                                @ViewProfile="GetProfile" @DeleteAccount="DeleteInstagramAccount" @HandleVerify="onHandleVerify"
                                :id="acc.id" :username="acc.username" :agentState="acc.agentState" 
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
                <div class="modal-card is-custom" style="width: 100%; height:45vw; padding:0;">
                    <header class="modal-card-head">
                        <p class="modal-card-title">Link your Instagram Account</p>
                    </header>
                    <section class="modal-card-body">
                        <b-field label="Username" custom-class="has-text-white">
                            <b-input
                                type="text"
                                v-model="linkData.username"
                                placeholder="Your Username"
                                required>
                            </b-input>
                        </b-field>

                        <b-field label="Password" custom-class="has-text-white">
                            <b-input
                                type="password"
                                v-model="linkData.password"
                                password-reveal
                                placeholder="Your password"
                                required>
                            </b-input>
                        </b-field>
                        <br>
                        <b-field>
                                <p class="control">
                                        <b-radio-button v-model="linkData.useMyLocation"
                                                native-value="true"
                                                type="is-success">
                                                <b-icon icon="check"></b-icon>
                                                <span>Use My Location</span>
                                        </b-radio-button>
                                </p>
                                <p class="control">
                                        <b-radio-button v-model="linkData.useMyLocation"
                                                native-value="false"
                                                type="is-danger">
                                                <b-icon icon="close"></b-icon>
                                                <span>Manually enter location</span>
                                        </b-radio-button>
                                </p>
                        </b-field>
                        
                        <b-field v-if="linkData.useMyLocation === 'false'">
                                <b-autocomplete
                                        size="is-medium"
                                        type="is-dark"
                                        v-model="linkData.location.address"
                                        :data="searchItems"
                                        autocomplete
                                        :allow-new="false"
                                        field="address"
                                        icon="map-marker"
                                        placeholder="Add a location"
                                        @typing="performAutoCompletePlacesSearch">
                                </b-autocomplete>
                        </b-field>
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
        <div v-if="needToVerify">
               <InstaVerify @Finished="needToVerify = false" :details="verifyDetails"/>
        </div>
</div>
</template>

<script>
import Vue from 'vue';
import InstaCard from "../Objects/InstaAccountCard";
import debounce from 'lodash/debounce'
import InstaVerify from '../Objects/VerifyInstagram';
export default {
        name:"manage",
        components:{
                InstaCard,
                InstaVerify
        },
        data(){
        return{
                searchItems:[],
                IsProfileButtonDisabled:true,
                InstagramAccounts:[],
                alert_text:'',
                isNavOn:false,
                isAccountLinkModalOpened:false,
                isLinkingAccount:false,
                linkData:{
                        username:'',
                        password:'',
                        useMyLocation:'true',
                        location:{
                                address:'',
                                coordinates:{
                                        latitude:0,
                                        longitude:0
                                }
                        }
                },
                verifyDetails:{
                        instagramAccountId:'',
                        challengeDetail:{}
                },
                needToVerify:false,
        }
        },
        created(){
               this.$emit('unSelectAccount');
        },
        mounted(){
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
                performAutoCompletePlacesSearch: debounce(function (query){
                        if(query && query!==''){
                                this.$store.dispatch('GooglePlacesAutoCompleteSearch', {query: query, radius:1500}).then(({ data })=>{
                                this.searchItems = []
                                JSON.parse(data).predictions.forEach((item) =>
                                this.searchItems.push(
                                        {
                                                city:item.structured_formatting.main_text, 
                                                address:item.description
                                        })); // this.searchItems.push(item))
                                })
                        }
                },500),
                clickOutside(){
                        this.$bus.$emit('clickedOutside')
                },
                onHandleVerify(id){
                        this.verifyDetails.instagramAccountId = id
                        this.verifyDetails.challengeDetail = this.InstagramAccounts.find(s=>s.id === id).challengeInfo
                        this.needToVerify = true;
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
                LinkAccount(){
                        if(this.linkData.username && this.linkData.password){
                                this.isLinkingAccount = true;
                                if(this.linkData.useMyLocation === 'true'){
                                        this.$store.getters.UserInformation.then(res=>{
                                                const locationDetails = res.userInformation.geoLocationDetails
                                                this.linkData.location.address = locationDetails.city + "," + locationDetails.country;
                                                this.linkData.location.coordinates.latitude = locationDetails.location.latitude
                                                this.linkData.location.coordinates.longitude = locationDetails.location.longitude
                                        }).catch(err=>{
                                                this.isLinkingAccount = false;
                                                Vue.prototype.$toast.open({
                                                        message: "Please enter your location manually as we are having trouble detecting your location",
                                                        type: 'is-info',
                                                        position:'is-top',
                                                        duration:4000
                                                })
                                                this.linkData.useMyLocation = 'false'
                                        })
                                }
                                if(!this.linkData.location.address)
                                        return;
                                let data = 
                                        {
                                                username:this.linkData.username, 
                                                password:this.linkData.password, 
                                                type:0,
                                                location: this.linkData.location,
                                                enableAutoLocate: this.linkData.useMyLocation === 'true'
                                        };
                                
                                
                                this.$store.dispatch('LinkInstagramAccount',data).then(resp=>{
                                        const instaId = resp.data.results.instagramAccountId
                                        if(resp.data !== undefined || resp.data !==null || instaId!==undefined){
                                                Vue.prototype.$toast.open({
                                                        message: "Successfully added, will now redirect you to your profile page",
                                                        type: 'is-success',
                                                        position:'is-bottom',
                                                        duration:8000
                                                })

                                                this.$store.dispatch('AccountDetails', {"userId":this.$store.state.user}).then(_=>{

                                                }).catch(err=>{})

                                                this.$store.dispatch('GetProfiles', this.$store.state.user).then(resp=>{
                                                        const index = resp.data.findIndex(ob=>ob.instagramAccountId === instaId)
                                                        if(index > -1){
                                                                const profileId = resp.data[index]._id
                                                                this.$router.push('/profile/'+ profileId)
                                                        }
                                                }).catch(err=>console.log(err.response));
                                
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
                DeleteInstagramAccount(id){
                        if(id){
                                this.$store.dispatch('DeleteInstagramAccount', id).then(resp=>{
                                        this.InstagramAccounts = this.$store.getters.GetInstagramAccounts;
                                }).catch(err => console.log(err))
                        }
                },
                GetLibrary(id){
                        this.$router.push('/library/'+ id)
                },
                StateChanged(data){
                        this.$store.dispatch('ChangeState', data).then(res=>{
                                if(res){
                                        Vue.prototype.$toast.open({
                                                message: 'Updated!',
                                                type: 'is-success',
                                                position: 'is-bottom',
                                        })
                                window.location.reload();
                                }
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
                                window.location.reload();
                        }
                }
        }
}
</script>

<style lang="scss">
@import '../../Style/darkTheme.scss';
.modal-card{
        &.is-custom{
                 background-color:$modal_background;
                 .modal-card-body{
                        padding-top:1em;
                        padding-left:2em;
                        padding-right:2em;
                        background:$modal_body;
                        color:$main_font_color;
                        label{
                                //color:$main_font_color;
                                text-align: left;
                        }
                        .input{
                                color:$main_font_color !important;
                                &::placeholder{
                                        color:$main_font_color;
                                }
                                border:none;
                        }
                        .control-label{
                                &:hover{
                                        color:$wheat;
                                }
                        }
                        .dropdown-menu{
                                background:$backround_back;
                                .dropdown-item{
                                        color:$main_font_color;
                                        &:hover{
                                                background:#292929;
                                        }
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
