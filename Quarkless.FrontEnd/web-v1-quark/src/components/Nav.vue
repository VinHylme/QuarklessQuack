<template>
<aside class="menu">
  <router-link to="/">
  <a class="logo" href="#">
     <img src="../assets/logo_grow.svg" width="90" height="90">
  </a>
  </router-link>
  <ul class="menu-list">
    <li>
      <b-tooltip label="Manage Accounts" position="is-right">
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
     <b-tooltip label="Statistics" position="is-right">
       <router-link  class="navbar-item" to="/stats">
         <span class="icon">
            <i class="fa fa-heartbeat"></i>
          </span>
       </router-link>
     </b-tooltip>
    </li>
    <li>
      <b-tooltip label="Settings" position="is-right">
       <router-link  class="navbar-item" to="/settings">
         <span class="icon">
            <i class="fa fa-cog"></i>
          </span>
       </router-link>
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
    logout(){
      this.$store.dispatch('logout');
      window.location.reload();
    }
  }
}
</script>


<style lang="sass" scoped>
.menu
   font-size:1.55em
   height:100%
   background-color:#141414
.navbar-item
  color:#ddd
  &:hover
    background-color:#141414
    color:#3ea6ff
  &:active
      background-color:#141414
      color:#3ea6ff
  &:focus
      background-color:#141414
      color:#3ea6ff
</style>
