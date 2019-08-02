<template>
<div class="home_layout">
<div class="columns is-mobile">
  <div class="column is-1">
      <Nav></Nav>
  </div>
  <div class="column is-11" >
      <div class="main_area">
         <router-view></router-view>
      </div>
  </div>
</div>
</div>
</template>
<script>
import Nav from './Nav.vue'

export default {
  name: 'app',
  components: {
    Nav
  }
}
</script>

<style lang="sass">
.column
  background:#141414

.main_area
  width:100%
  height:100%
  background-color:#141414
  border-radius:10px
.home_layout
   padding:0.7em
   height:100%
</style>
