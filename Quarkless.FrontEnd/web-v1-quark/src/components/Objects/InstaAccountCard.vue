<template>
    <div class="card">
        <header class="card-header">
            <div class="media">
                <div class="media-left">
                    <figure class="image is-96x96">
                    <img v-if="profilePicture!==null" v-bind:src="profilePicture" alt="Placeholder image">
                    <img v-else src="https://alumni.crg.eu/sites/default/files/default_images/default-picture_0_0.png" alt="default image">
                    </figure>
                </div>
                <div class="media-content">
                    <p class="title is-6">{{name}}</p>
                    <p class="subtitle is-6">@{{username}}</p>
                    <div class="stats">
                        <b-field grouped group-multiline>
                            <div class="control">
                                <b-taglist attached>
                                    <b-tag type="is-dark">Followers</b-tag>
                                    <b-tag type="is-info">{{userFollowers}}</b-tag>
                                </b-taglist>
                            </div>

                            <div class="control">
                                <b-taglist attached>
                                    <b-tag type="is-dark">Following</b-tag>
                                    <b-tag type="is-success">{{userFollowing}}</b-tag>
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
                                <i class="fas fa-globe"></i>
                            </span>
                        </div>
                    </div>
                    <div class="control">
                        <button class="button is-warning"  @click="RefreshState()">
                            <b-icon pack="fas" icon="sync-alt" >
                            </b-icon>
                            <span>Re-Login</span>
                        </button>
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
        <router-link :to="'/view/'+id" class="card-footer-item"> Manage </router-link>
           <!--<a v-if="IsAdmin" @click="RefreshState()" class="card-footer-item"> Refresh </a>-->
          <!-- <router-link v-if="IsAdmin" :to="'/view/'+id" class="card-footer-item"> Delete </router-link>-->
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
    totalPost:Number
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
          this.$store.dispatch('ChangeState', objectToSend).then(res=>{
                if(res)
                window.location.reload();
          })
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

<style lang="scss">

.card{
    margin:0.2em;
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
    background-color:#f22929;
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
