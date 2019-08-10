<template>
<aside class="menu">
  <router-link to="/">
  <a class="logo">
     <img src="../assets/logo_grow.svg" width="90" height="90">
  </a>
  </router-link>
  <ul class="menu-list">
    <li>
      <b-tooltip type="is-dark" label="Hide Panel" position="is-right">
       <a class="navbar-item" @click="hideNav">
          <span class="icon">
            <i class="fa fa-expand"></i>
          </span>
       </a>
      </b-tooltip>
    </li>
    <li>
      <b-tooltip type="is-dark" label="Manage Accounts" position="is-right">
       <router-link  class="navbar-item" to="/manage">
          <span class="icon">
            <i class="fa fa-rocket"></i>
          </span>
       </router-link>
      </b-tooltip>
    </li>
    <!-- <li>
      <b-tooltip label="Profiles" position="is-right">
       <router-link class="navbar-item" to="/profiles">
        <span class="icon">
            <i class="fa fa-bookmark"></i>
        </span>
       </router-link>
      </b-tooltip>
    </li> -->
    <li>
     <b-tooltip type="is-dark" label="Statistics ❤️ coming soon ❤️" position="is-right">
       <a class="navbar-item is-disabled">
         <span class="icon">
            <i class="fa fa-heartbeat"></i>
          </span>
       </a>
     </b-tooltip>
    </li>
    <li>
      <b-tooltip type="is-dark" label="Settings ❤️ coming soon ❤️" position="is-right">
       <a  class="navbar-item is-disabled">
         <span class="icon">
            <i class="fa fa-cog"></i>
          </span>
       </a>
      </b-tooltip>
    </li>
    <li>
      <b-dropdown hoverable aria-role="list">
            <a class="navbar-item" slot="trigger">
            <span class="icon">
                <i class="fa fa-user-circle"></i>
              </span>
            </a>
            <b-dropdown-item position="is-top-left" @click="logout" aria-role="listitem">
              <div class="media">
                <b-icon class="media-left" pack="fas" icon="sign-out-alt"></b-icon>
                <div class="media-content">
                    <h3>Logout</h3>
                    <small>Sign me out</small>
                </div>
              </div>
            </b-dropdown-item>
        </b-dropdown>
    </li>
  </ul>
</aside>
</template>

<script>
export default {
  methods:{
    hideNav(){
      this.$emit('onHide');
    },
    logout(){
      this.$store.dispatch('logout');
      window.location.reload();
    }
  }
}
</script>


<style lang="scss" scoped>
.is-disabled{
  cursor:initial;
  opacity: 0.6;
}

.menu{
   font-size:1.55em;
   height:100%;
   background-color:#141414;
}

.navbar-item{
  color:#ddd;
  &:hover{
    background-color:#141414;
    color:#13c551;
    &.is-disabled{
      color:#696969;
      opacity:0.4;
    }
  }
  &:active{
      background-color:#141414;
      color:#13c551;
  }
  &:focus{
      background-color:#141414;
      color:#13c551;
  }
}
</style>
