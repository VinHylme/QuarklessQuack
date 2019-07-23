<template>
<!--<nav class="navbar is-info">
  <div class="navbar-brand">
    <a class="navbar-item">
     <h3>#Grow</h3>
    </a>
    <div class="navbar-burger burger" data-target="navbarExampleTransparentExample">
      <span></span>
      <span></span>
      <span></span>
    </div>
  </div>

  <div id="navbarExampleTransparentExample" class="navbar-menu">
    <div class="navbar-start">
      <a class="navbar-item" href="https://bulma.io/">
          <span class="icon">
            <i class="fa fa-rocket"></i>
          </span>
          <span>
             Manage
          </span>
      </a>
     <a class="navbar-item" href="https://bulma.io/">
        <span class="icon">
            <i class="fa fa-bookmark"></i>
          </span>
          <span>
             Profiles
          </span>
      </a>
     <a class="navbar-item" href="https://bulma.io/">
         <span class="icon">
            <i class="fa fa-heartbeat"></i>
          </span>
          <span>
             Schedule
          </span>
      </a>
      <a class="navbar-item" href="https://bulma.io/">
         <span class="icon">
            <i class="fa fa-cog"></i>
          </span>
          <span>
             Settings
          </span>
      </a>
    </div>

    <div class="navbar-end">
      <div class="navbar-item has-dropdown is-hoverable">
         <a class="navbar-link">
            <span class="icon">
               <i class="fa fa-user-circle"></i>
            </span>
          <span>
             Account
          </span>
        </a>
        <div class="navbar-dropdown">
      <a class="navbar-item">
        Logout
      </a>
       </div>
      </div>
    </div>
  </div>
</nav>-->
<aside class="menu">
  <router-link to="/">
  <a class="logo" href="#">
     <img src="../assets/Hashtag.png" width="65" height="65">
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
    <li>
      <b-tooltip label="Profiles" position="is-right">
       <router-link class="navbar-item" to="/profiles">
        <span class="icon">
            <i class="fa fa-bookmark"></i>
        </span>
       </router-link>
      </b-tooltip>
    </li>
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
        <a class="navbar-item" href="https://bulma.io/">
         <span class="icon">
            <i class="fa fa-user-circle"></i>
          </span>
        </a>
    </li>
  </ul>
</aside>
</template>

<style lang="sass" scoped>
.menu
   font-size:1.55em
   height:100%
   background-color:#fafafa

</style>
