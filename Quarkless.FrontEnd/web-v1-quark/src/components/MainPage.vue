<template>
<div class="contain">
  <a class="logo">
    <img src="../assets/logo_grow.svg" width="275" height="275">
  </a>
  <div class="modal-card">
        <section class="modal-card-body">
            <h3 class="title has-text-centered">HashtagGrow - VIP Login</h3>
            <div class="box">
                    <b-field label=""  type="is-light">
                        <b-input v-model="username" type="text" placeholder="username">
                        </b-input>
                    </b-field>

                    <b-field label="">
                        <b-input v-model="password" type="password" placeholder="Password" minlength="6" password-reveal>
                        </b-input>
                    </b-field>
                    <button class="button is-large is-fullwidth" @click="doLogin()" :disabled="isActive">
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

<style lang="scss" scoped>
body,html{
  background-color:#212121!important;
  overflow: hidden !important;
}
.footer{
  background:#141414;
  opacity: .7;
  width:100%;
  padding:0;
  margin: 0 auto;
  margin-top:2em;
  height:200px;
  p{
    padding:4em;
    color:#d9d9d9;
  }
}
.modal-card{
  background-color:#242424;
  padding:4em;
  border-radius: 1em;
  opacity: 0.95;
  box-shadow: 1px 5px 5px 5px rgba(0, 0, 0, 0.06);
  .modal-card-body{
    .box{
      padding:1em;
      background-color:#242424;
      border: none !important;

    }
    background-color: #242424;
    .title{
      color:white;
    }
    h3{
      color:white;
    }
  }
}
</style>
