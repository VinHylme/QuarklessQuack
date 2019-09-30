<template>
<div class="contain">
  <a class="logoImage">
    <img src="../assets/logo_grow.svg" width="275" height="275">
  </a>
  <p class="title is-light">Quitic</p>
  <div class="modal-card">
        <section v-if="!verificationNeeded" class="modal-card-body">
            <h3 class="title has-text-centered">Login</h3>
            <div class="box long">
              <b-field>
                  <b-input v-model="username" type="text" placeholder="username"></b-input>
              </b-field>
              <b-field>
                  <b-input v-model="password" type="password"  @keyup.enter.native="doLogin" placeholder="Password" minlength="6" password-reveal></b-input>
              </b-field>
              <button type="submit" class="button is-large is-success is-fullwidth"  @click="doLogin" :disabled="isActive">
                  Login
              </button>
            </div>
        </section>
        <section v-else class="modal-card-body">
          <h3 class="title has-text-centered">Confirm Account</h3>
          <div class="box long">
            <b-field>
              <b-input v-model="confirmationCode" placeholder="your confirmation code"></b-input>
            </b-field>
            <div class="buttons has-addons is-centered">
              <a @click="verificationNeeded=false" class="button">Back to login</a>
              <a class="button">Confirm Account</a>
              <a @click="resendConfirmation" class="button">Resend Confirmation Code</a>
            </div>
          </div>
        </section>
  </div>
    <div class="footer">
      <p class="subtitle is-8">Copyright © 2019 Quitic; All rights have been reserved</p>
    </div>
</div>
</template>

<script>
import Vue from 'vue';
import Fingerprint2 from 'fingerprintjs2';
export default {
  name:"MainLoginPage",
  data(){
    return {
      confirmationCode:'',
      username:'',
      password:'',
      isSucesss:false,
      isActive :false,
      verificationNeeded:false
    }
  },
  mounted(){
    if (window.requestIdleCallback) {
      requestIdleCallback(function () {
          Fingerprint2.get(function (components) {
            //console.log(components) // an array of components: {key: ..., value: ...}
            var values = components.map(function (component) { return component.value })
            var murmur = Fingerprint2.x64hash128(values.join(''), 31)
            //console.log(murmur)
          })
      });
    } else {
      setTimeout(function () {
          Fingerprint2.get(function (components) {
            console.log(components) // an array of components: {key: ..., value: ...}
          })  
      }, 500)
    }
  },
  methods:{
    resendConfirmation(){
      this.$store.dispatch('resendConfirmation',this.username).then(resp=>{
         Vue.prototype.$toast.open({
            message: 'Your confirmation code has been resend to ' + resp.data,
            type: 'is-info',
            position:'is-top',
            duration:6000,
            queue:false
          });
      }).catch(err=>{
         Vue.prototype.$toast.open({
            message: err.response.data.message,
            type: 'is-danger',
            position:'is-bottom',
            duration:6000,
            queue:false
          });
      })
    },
    doLogin(){
      this.isActive = true;
      this.$store.dispatch('login',{Username:this.username, Password:this.password}).then(res=>{
        this.isSucesss = true;
        this.alert_text = "Welcome back " + this.username;
        Vue.prototype.$toast.open({
            message: 'Welcome back ' + this.username,
            type: 'is-success',
            position:'is-top',
            duration:2000
          })
        this.isActive = false;
        window.location.reload();
      }).catch(err=>{
        this.isActive = false;
        if(err.response.status === 401){
          this.verificationNeeded = true;
          Vue.prototype.$toast.open({
            message: 'Please confirm your account by entering the confirmation code.',
            type: 'is-info',
            position:'is-top',
            duration:6000
          });
        } 
        else{
         Vue.prototype.$toast.open({
            message: 'Failed to login, please check your account details',
            type: 'is-danger',
            position:'is-bottom',
            duration:6000
          })
        }
      })
    }
  }
}
</script>

<style lang="scss">
@import '../Style/darkTheme.scss';
::-webkit-scrollbar {
    display: none;
}
body,html{
  margin: 0 auto;
  background: $backround_back;
  scrollbar-width: none;
}
.contain{
  //background-image: url("../assets/9294.jpg");
  background-size: cover;
  background-repeat: no-repeat;
  background-color:$backround_back;
  text-align: center;
  width:100%;
  height:100vh;
  margin: 0 auto;
  .logoImage{
    margin: 0 auto;
    width:100%;
    img{
      margin: 0 auto ;
    }
  }
  input,.input,b-input{
    background:$backround_back !important;
    border:none !important;
    color:$main_font_color !important;
    &:focus{
      color:$main_font_color !important;
      box-shadow: 0;
      border:none !important;
    }
    &:hover{
      background:$input_hover !important;
    }
  }
  ::placeholder{
    color:$input_placeholder !important;
  }
  label{
    color:$main_font_color;
  }
}

.long{
  width:100%;
}
.title{
  &.is-light{
    color:white;
    font-size: 4vh;
    text-shadow: 2px 2px rgba(163, 163, 163, 0.089);
  }
}
.footer{
  background:transparent !important;
  opacity: .7;
  width:100%;
  padding:0;
  margin: 0 auto;
  margin-top:2em;
  height:200px;
  p{
    text-align: center !important;
    padding:4em;
    color:$main_font_color;
    text-shadow: 2px 2px rgba(163, 163, 163, 0.096);

  }
}
.modal-card{
  background-color:#303030;
  padding:2em;
  //border-radius: 0.8em;
  opacity: 0.95;
  margin: 0 auto !important;
  //height:500px;
  box-shadow: 1px 5px 5px 5px rgba(0, 0, 0, 0.06);
  .modal-card-body{
    border:0 !important;
    width: 100%;
    padding:0;
    margin: 0 auto;
    .box{
      padding:1em;
      background-color:transparent;
      border: none !important;
      box-shadow: none;
    }
    background-color: transparent;
    .title{
      font-size:3vh;
      color:#d9d9d9;
    }
    h3{
      color:$title;
    }
  }
}

</style>
