<template>
<div class="contain">
  <a class="logoImage">
    <img src="../assets/logo_grow.svg" width="275" height="275">
  </a>
  
  <div class="modal-card">
        <section class="modal-card-body">
            <h3 class="title has-text-centered">HashtagGrow</h3>
            <div class="box long">
              <b-field>
                  <b-input v-model="username" type="text" placeholder="username"></b-input>
              </b-field>
              <b-field>
                  <b-input v-model="password" type="password"  @keyup.enter.native="doLogin" placeholder="Password" minlength="6" password-reveal></b-input>
              </b-field>
              <button type="submit" class="button is-large is-dark is-fullwidth"  @click="doLogin" :disabled="isActive">
                  Login
              </button>
            </div>
        </section>
    </div>
    <br>
    <b-notification v-if="showNotification"
      v-bind:type="isSucesss?'is-success':'is-danger'"
      aria-close-label="Close notification"
      role="alert">
      {{alert_text}}
    </b-notification>
    <div class="footer">
      <p class="subtitle is-8">Copyright © 2019 HashtagGrow; All rights have been reserved</p>
    </div>
</div>
</template>

<script>
export default {
  name:"MainLoginPage",
  data(){
    return {
      username:'',
      password:'',
      alert_text:'',
      showNotification:false,
      isSucesss:false,
      isActive :false
    }
  },
  methods:{
    doLogin(){
      this.isActive = true;
      this.$store.dispatch('login',{Username:this.username, Password:this.password}).then(res=>{
        this.isSucesss = true;
        this.showNotification = true;
        this.alert_text = "Welcome back " + this.username;
        this.isActive = false;
        window.location.reload();
      }).catch(err=>{
        this.showNotification = true;
        this.alert_text =  "Failed to login, please try again.";
        this.isActive = false;
      })
    }
  }
}
</script>

<style lang="scss">
body,html{
  background-color:#121212!important;
  margin: 0 auto;
}
.contain{
  background:#121212;
  text-align: center;
  width:100%;
  height:100%;
  margin: 0 auto;
  .logoImage{
    margin: 0 auto;
    width:100%;
    img{
      margin: 0 auto ;
    }
  }
  input{
    background:#121212 !important;
    border:none !important;
    color:#d9d9d9 !important;
    &:focus{
      color:#d9d9d9 !important;
      box-shadow: 0;
      border:none !important;
    }
    &:hover{
      background:#212121 !important;
    }
  }
  ::placeholder{
    color:#d8d8d8 !important;
  }
  label{
    color:#d9d9d9;
  }
}

.long{
  width:100%;
}

.footer{
  background:#121212 !important;
  opacity: .7;
  width:100%;
  padding:0;
  margin: 0 auto;
  margin-top:2em;
  height:200px;
  p{
    text-align: center !important;
    padding:4em;
    color:#d9d9d9;
  }
}
.modal-card{
  background-color:#242424;
  padding:2em;
  border-radius: 0.8em;
  opacity: 0.95;
  margin: 0 auto !important;
  box-shadow: 1px 5px 5px 5px rgba(0, 0, 0, 0.06);
  .modal-card-body{
    width: 100%;
    padding:0;
    margin: 0 auto;
    .box{
      padding:1em;
      background-color:#242424;
      border: none !important;

    }
    background-color: #242424;
    .title{
      font-size:3vh;
      color:white;
    }
    h3{
      color:white;
    }
  }
}

</style>
