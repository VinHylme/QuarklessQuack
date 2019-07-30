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
                </div>
            </div>
         </header>
        <div class="card-content">
            <div class="content">
            Agent State: <p class="status is-6">{{MapToCorrectState(agentState)}}</p>
            <p v-if="biography!==null">
                {{biography.text}}
            </p>
            <p v-else>Not populated the biography yet</p>
        </div>
    </div>
      <footer class="card-footer">
           <router-link :to="'/view/'+id" class="ok card-footer-item"> Manage </router-link>
           <router-link :to="'/view/'+id" class="info card-footer-item"> Refresh </router-link>
           <router-link :to="'/view/'+id" class="del card-footer-item"> Delete </router-link>
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
    agentState:Number
  },
  methods:{
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
    height:390px !important;
}
.card-header{
    padding:1em;
    height:90px;
}
.card-content{
    text-align: left;
    height:200px;
}
.card-footer{
    height:100px;
}

.ok{
    text-decoration: none;
    color: rgb(0, 145, 69);
}
.del{
    color:rgb(211, 69, 76);
}

</style>
