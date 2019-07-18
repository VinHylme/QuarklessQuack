<template>
  <div id="app">
    <Nav></Nav>
    <Home msg="Welcome to Your Vue.js App"/>
  </div>
</template>

<script>
import Home from './components/Home.vue'
import Nav from './components/Nav.vue'

export default {
  name: 'app',
  components: {
    Nav,
    Home
  }
}
</script>

<style>
#app {
  font-family: 'Avenir', Helvetica, Arial, sans-serif;
  -webkit-font-smoothing: antialiased;
  -moz-osx-font-smoothing: grayscale;
  text-align: center;
  color: #2c3e50;
}
</style>
