import Vue from 'vue'
import App from './App.vue'
import Buefy from 'buefy';
import 'buefy/dist/buefy.css';
import router from "./Route";
import store from "./State";
import VueResource from 'vue-resource';
import Axios from 'axios'


Vue.use(Buefy);
Vue.use(VueResource);

//Vue.prototype.$http = Axios;

const token = localStorage.getItem('token')
if (token) {
  Axios.defaults.headers.common['Authorization'] = token
}

Vue.config.productionTip = false

new Vue({
  router:router,
  store,
  render: h => h(App),
}).$mount('#app')
