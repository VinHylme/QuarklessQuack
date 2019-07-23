import Vue from 'vue'
import App from './App.vue'
import Buefy from 'buefy';
import 'buefy/dist/buefy.css';
import router from "./Route";
import store from "./State";

Vue.use(Buefy);
Vue.config.productionTip = false

new Vue({
  router:router,
  store,
  render: h => h(App),
}).$mount('#app')
