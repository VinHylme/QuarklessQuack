<template>
<div class="container">
  <div class="modal-card">
        <section class="modal-card-body">
            <h3 class="title has-text-centered">VIP LOGIN</h3>
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
body{
  background-color:#242424;
}

.modal-card{
  background-color:#242424;
  padding:4em;
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
