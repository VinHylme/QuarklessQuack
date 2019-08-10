<template>
    <div class="card">
        <header class="card-header">
            <div class="media">
                <div class="media-left">
                    <figure class="image is-96x96">
                    <img class="is-rounded"  v-if="profilePicture!==null" v-bind:src="profilePicture" alt="Placeholder image">
                    <img class="is-rounded"  v-else src="https://alumni.crg.eu/sites/default/files/default_images/default-picture_0_0.png" alt="default image"> 
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
                                    <b-tag size="is-default" type="is-info">{{userFollowers}}</b-tag>
                                </b-taglist>
                            </div>

                            <div class="control">
                                <b-taglist attached>
                                    <b-tag size="is-default" type="is-dark">Following</b-tag>
                                    <b-tag size="is-default" type="is-danger">{{userFollowing}}</b-tag>
                                </b-taglist>
                            </div>
           
                        </b-field>
                    </div>
                </div>
            </div>
         </header>
        <div class="card-content">
            <div class="content">
                <div id="user_info">
                    <div class="field is-grouped">
                    <div class="control has-icons-left">
                        <div class="control has-icons-left">
                            <div class="select">
                                <select is-primary @change="OnSelectChange($event)" :disabled="!IsAdmin">
                                    <option selected>{{MapToCorrectState(agentState)}}</option>
                                    <option v-for="(option,index) in GetAgentOptionList(agentState)" :value="option.index" v-bind:key="index">{{option.name}}</option>
                                </select>
                            </div>
                            <span class="icon is-left">
                                <i class="fas fa-bug"></i>
                            </span>
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
                        <b-tooltip label="Refresh this account" type="is-dark" position="is-bottom">
                            <button class="button is-light-dark" @click="RefreshState()">
                                <b-icon pack="fas" icon="sync-alt" >
                                </b-icon>
                            </button>
                        </b-tooltip>
                        </div>
                        <div class="control">
                        <b-tooltip label="Delete this account" type="is-dark" position="is-top">
                            <a @click="ViewProfile" class="button is-light-dark">
                                <b-icon pack="fas" icon="trash" >
                                </b-icon>
                            </a>
                        </b-tooltip>
                        </div>
                    </div>
                </div>
                <br>
                <p v-if="biography!==null">
                    {{biography.text}}
                </p>
                <p v-else>Not populated the biography yet</p>           
        </div>
    </div>
      <footer class="card-footer">
            <router-link :to="'/view/'+id" class="card-footer-item"> 
                <b-tooltip label="Manage this account" type="is-dark" size="is-large" position="is-right">
                    <b-icon pack="fas" icon="bolt" size="is-medium"></b-icon>
                </b-tooltip>
            </router-link>
    </footer> 
    </div>
</template>

<script>
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
  data(){
      return {
          IsAdmin:false,
          AgentOptions:[
              {name:"Not Started", index:0},
              {name:"Running", index:1},
              {name:"Stopped", index:2},
              {name:"Sleeping", index:3},
              {name:"Deep Sleeping",index:4},
              {name:"Blocked by instagram",index:5},
              {name:"Challange required",index:6},
              {name:"Awaiting from user",index:7}
          ]
      }
  },
  mounted(){
      this.IsAdmin = this.$store.getters.UserRole == 'Admin';
  },
  methods:{
      ViewProfile(){
          this.$emit("ViewProfile", this.id);
      },
      RefreshState(){
          this.$store.dispatch('RefreshState',this.id).then(res=>{
              if(res.data == 'true' || res.data == true){
                  //SUCCESS
                    this.$emit("RefreshState", true);
              }
              else{
                  this.$emit("RefreshState",false)
              }
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
      MapToCorrectState(state){
          switch(state){
              case 0:
                  return "Not Started";
              case 1:
                  return "Running";
              case 2:
                  return "Stopped";
              case 3:
                  return "Sleeping";
              case 4:
                  return "Deep Sleeping";
              case 5:
                  return "Blocked by instagram";
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
    }
}
.card-content{
    padding-top:2em;
    text-align: left;
    height:200px;
    width: 100%;
}
.card-footer{
    height:100px;
    border: none;
    font-weight: bold;
    background-color:#13b94d;
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
