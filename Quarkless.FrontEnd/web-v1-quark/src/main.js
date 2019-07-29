import Vue from 'vue'
import App from './App.vue'
import Buefy from 'buefy';
import 'buefy/dist/buefy.css';
import router from "./Route";
import store from "./State";

Vue.use(Buefy);
Vue.config.productionTip = false

router.beforeEach((to, from, next) => {
  if (to.matched.some(record => record.meta.requiresAuth)) {
    // this route requires auth, check if logged in
    // if not, redirect to login page.
    if (!store.getters.isLoggedIn) {
      next({ name: 'Login' })
    } else {
      next() // go to wherever I'm going
    }
  } else {
    next() // does not require auth, make sure to always call next()!
  }
})

new Vue({
  router:router,
  store,
  render: h => h(App),
}).$mount('#app')
