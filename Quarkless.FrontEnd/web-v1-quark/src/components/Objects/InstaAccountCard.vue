<template>
    <div class="card">
        <header class="card-header">
            <div class="media">
                <div class="media-left">
                    <figure class="image is-96x96">
                    <b-tooltip label="Change Profile Picture" position="is-right" type="is-info">
                        <d-drop :isHidden="true" accept="image/*" :isMulti="true" swidth="80px" sheight="80px" @readyUpload="onUpload"/>
                        <img class="is-rounded"  v-if="profilePicture!==null" v-bind:src="profilePicture" alt="Placeholder image">
                        <img class="is-rounded"  v-else src="https://alumni.crg.eu/sites/default/files/default_images/default-picture_0_0.png" alt="default image">
                    </b-tooltip>
                    </figure>
                </div>
                <div class="media-content">
                    <p class="title is-6">{{name}}</p>
                    <p class="subtitle is-6">@{{username}}</p>
                    <div class="stats">
                        <b-field grouped group-multiline>
                            <div class="control">
                                <b-taglist attached>
                                    <b-tag size="is-default" type="is-dark">Followers</b-tag>
                                    <b-tag style="font-weight:bold;" size="is-default" type="is-info">{{userFollowers}}</b-tag>
                                </b-taglist>
                            </div>

                            <div class="control">
                                <b-taglist attached>
                                    <b-tag size="is-default" type="is-dark">Following</b-tag>
                                    <b-tag style="font-weight:bold;" size="is-default" type="is-danger">{{userFollowing}}</b-tag>
                                </b-taglist>
                            </div>
           
                        </b-field>
                    </div>
                </div>
            </div>
         </header>
        <div class="card-content">
            <div class="content">
                <div id="user_info" style="margin-left:-.7em">
                    <div class="field is-grouped">
                    <div v-if="agentState === 7 || agentState === 6" class="control">
                        <b-tooltip label="Account needs attention" type="is-dark" position="is-top">
                            <a @click="HandleVerify" class="button is-warning">
                                <b-icon pack="fas" icon="exclamation-triangle"></b-icon>
                            </a>
                        </b-tooltip>
                    </div>
                    <div v-if="agentState === 0 || agentState === 2 || (agentState === 3 || agentState === 4)" class="control">
                        <b-tooltip :label="agentState === 0 || agentState ===2 ? 'Run' : agentState === 3 || agentState === 4 ? 'Run (Recommended to wait until ready)' : 'Start Account'" type="is-dark" position="is-top">
                            <a @click="ChangeState(1)" class="button is-info is-default">
                                <b-icon pack="fas" icon="chess-queen"></b-icon>
                            </a>
                        </b-tooltip>
                    </div>
                    <div v-if="agentState === 1" class="control">
                        <b-tooltip label="Pause Account" type="is-dark" position="is-top">
                            <a @click="ChangeState(2)" class="button is-danger">
                                <b-icon pack="fas" icon="ribbon"></b-icon>
                            </a>
                        </b-tooltip>
                    </div>
                    <div v-if="agentState === 3 || agentState === 4 || agentState === 5" class="control">
                        <b-tooltip label="Resting" type="is-dark" position="is-top">
                            <a class="button is-dark is-default">
                                <b-icon pack="fas" icon="bed"></b-icon>
                            </a>
                        </b-tooltip>
                    </div>
                   <div class="control is-expanded">
                        <div class="control">
                            <a class="button is-light-dark is-fullwidth" style="font-weight:bold; font-size:1rem; max-width:150px;">{{MapToCorrectState(agentState)}}</a>
                        </div>
                    </div> 
                      <div class="control">
                        <b-tooltip label="Profile" type="is-dark" position="is-top">
                            <a @click="ViewProfile" class="button is-light-dark" :disabled="IsProfileButtonDisabled">
                                <b-icon pack="fas" icon="bookmark" >
                                </b-icon>
                            </a>
                        </b-tooltip>
                        </div>
                        <div class="control">
                        <b-tooltip label="Library" type="is-dark" position="is-top">
                            <a @click="ViewLibrary" class="button is-light-dark">
                                <b-icon pack="fas" icon="images" >
                                </b-icon>
                            </a>
                        </b-tooltip>
                        </div>
                        <div class="control">
                        <b-tooltip label="Refresh this account" type="is-dark" position="is-top">
                            <button class="button is-light-dark" @click="RefreshState()">
                                <b-icon pack="fas" icon="sync-alt" :custom-class="IsRefreshing ? 'fa-spin' : ''">
                                </b-icon>
                            </button>
                        </b-tooltip>
                        </div>
                        <div class="control">
                        <b-tooltip label="Remove this account" type="is-danger" position="is-top">
                            <a @click="DeleteAccount" class="button is-light-dark">
                                <b-icon pack="fas" icon="trash" >
                                </b-icon>
                            </a>
                        </b-tooltip>
                        </div>
                    </div>
                </div>
                <br>
                    <textarea @focus="onFocusBio" v-if="biography!==null" v-model="biography.text" rows="10" class="textarea is-biography-input" placeholder="Your biography"></textarea>
                    <p v-else>Not populated the biography yet</p>      
     
        </div>
    </div>
      <footer :class="IsAmendingAccount ? 'card-footer is-info' : 'card-footer is-green'">
            <router-link v-if="!IsAmendingAccount" :to="'/view/'+id" class="card-footer-item"> 
                <b-tooltip label="Manage this account" type="is-dark" size="is-large" position="is-right">
                    <b-icon pack="fas" icon="bolt" size="is-medium"></b-icon>
                </b-tooltip>
            </router-link>
            <a v-else class="card-footer-item" @click="saveBiography">
                <b-tooltip label="Save Changes" type="is-dark" size="is-large" position="is-right">
                   <b-icon v-if="!IsLoading" pack="fas" icon="bong" size="is-medium"></b-icon>
                   <b-icon v-else pack="fas" icon="sync-alt" custom-class="fa-spin"> </b-icon>
                </b-tooltip>
            </a>
    </footer> 
    </div>
</template>

<script>
import DropZone from '../Objects/DropZone';

export default {
name:"InstaAccountCard",
props: {
    id: String,
    name:String,
    username: String,
    biography: Object,
    profilePicture: String,
    agentState:Number,
    userFollowers:Number,
    userFollowing:Number,
    totalPost:Number,
    IsProfileButtonDisabled:Boolean
  },
  components:{
      'd-drop':DropZone
  },
  data(){
      return {
        IsRefreshing:false,
        IsLoading:false,
        IsAdmin:false,
        IsAmendingAccount:false,
        AgentOptions:[
            {name:"Not Started", index:0},
            {name:"Running", index:1},
            {name:"Stop", index:2},
            {name:"Resting", index:3},
            {name:"Sleeping",index:4},
            {name:"Blocked by instagram",index:5},
            {name:"Challange required",index:6},
            {name:"Awaiting from user",index:7}
        ]
      }
  },
  mounted(){
      this.IsAdmin = this.$store.getters.UserRole == 'Admin';
      let _self = this;
      this.$bus.$on('doneUpdatingBiography',()=>{
          _self.onFinishedUpdate();
      });
      this.$bus.$on('clickedOutside',()=>{
          _self.onFinishedUpdate();
      });
      this.$bus.$on('cancel-other-focused',()=>{
          _self.onFinishedUpdate();
      });
      this.$bus.$on('focus-main', (id)=>{
          if(id == this.id){
              this.IsAmendingAccount = true;
          }
      })
  },
  methods:{
      onFocusBio(){
          this.$bus.$emit('onFocusBio', this.id);
      },
      onFinishedUpdate(){
        this.IsAmendingAccount = false;
        this.IsLoading = false;
      },          
      saveBiography(){
          this.IsLoading = true;
          this.$emit("onChangeBiography", {biography: this.biography, id:this.id});
      },
      onUpload(e){
          this.$emit("onChangeProfilePicture", {image:e, id:this.id});
      },
      ViewProfile(){
          this.$emit("ViewProfile", this.id);
      },
      DeleteAccount(){
          this.$emit("DeleteAccount", this.id);
      },
      HandleVerify(){
          this.$emit("HandleVerify", this.id);
      },
      ViewLibrary(){
        this.$emit("ViewLibrary", this.id);
      },
      RefreshState(){
        this.IsRefreshing = true;
        this.$store.dispatch('RefreshState',this.id).then(res=>{
            if(res.data == 'true' || res.data == true){
                //SUCCESS
                this.$emit("RefreshState", true);
            }
            else{
                this.$emit("RefreshState",false)
            }
        this.IsRefreshing = false;
        }).catch(err=>{
        this.IsRefreshing = false;
        })
      },
       GetAgentOptionList(currentSelected){
        var index = this.AgentOptions.findIndex(item=>item.index == currentSelected);
        if(index>-1){
            return this.AgentOptions.splice(index,1);
        }
        return this.AgentOptions;
      },
      OnSelectChange(event){
          var objectToSend = {"instaId": this.id, "state": event.target.value};
          this.$emit("ChangeState", objectToSend);
      },
      ChangeState(state){
          const objectToSend = {"instaId":this.id, "state": state}
          this.$emit("ChangeState", objectToSend)
      },
      MapToCorrectState(state){
          switch(state){
              case 0:
                  return "Not Started";
              case 1:
                  return "Running";
              case 2:
                  return "Paused";
              case 3:
                  return "Resting";
              case 4:
                  return "Sleeping";
              case 5:
                  return "Limit Reached";
              case 6:
                  return "Challange required";
              case 7:
                  return "Awaiting from user"
          }
      }
  }
}
</script>

<style lang="scss" scoped>

.is-biography-input{
    overflow-y: scroll !important;
    resize: none;
    width:100%;
    height:99px !important;
    color:#d9d9d9;
    font-size: 14px;
    border:none;
    padding:.15em;
    margin:0;
    background:#292929 !important;
    &:hover{
        background:#323232 !important;
    }
}
.card{
    margin-left:0.4em;
    margin-top:1em;
    width:400px !important;
    height:395px !important;
    background-color: #292929 !important;
    color:white !important;
}
.select{
    width:190px;
}
select{
    background: #414141 !important;
    border:none !important;
    color:white!important;
    option {
        color: #f3f3f3 !important;
        background: #414141;
    }
}
.media-content{
    overflow: hidden;
    padding-top:0.3em;    
}

.is-light-dark{
    background: #414141;
    color:white;
    border:none;
    &:focus{
        border:none;
    }
    &:hover{
        background:#212121;
        color:wheat;
    }
}
.card-header{
    background-color: #414141;
    padding-top:0.4em;
    padding-left:0.5em;
    height:95px;
    .stats{
        padding:0;
        margin-top:-0.8em;
    }
    .title{
        color:white !important;
    }
    .subtitle{
        color: antiquewhite !important;
    }
}
img{
    &:hover{
        cursor: pointer;
        opacity: 1 !important;
    }
}
.card-content{
    margin:0;

    padding-top:2em;
    text-align: left;
    height:200px;
    width: 100%;

}
.card-footer{
    height:100px;
    width:100%;
    border: none;
    font-weight: bold;
    &.is-green{
        background-color:#13b94d;
    }
    &.is-info{
        background-color:#1792da;
    }
    .card-footer-item{
        font-size: 20px;
        border: none;
        color:white;
        &:hover{
            color:bisque;
        } 
    }
}
#user-info{
    display: block;
    width:100%;
}

</style>
