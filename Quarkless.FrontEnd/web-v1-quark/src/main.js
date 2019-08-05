import Vue from 'vue'
import App from './App.vue'
import Buefy from 'buefy';
import 'buefy/dist/buefy.css';
import router from "./Route";
import store from "./State";
import VueResource from 'vue-resource';
import Axios from 'axios'
import VueScheduler from '../references/v-calendar-scheduler/index';


Vue.use(Buefy);
Vue.use(VueResource);

Vue.use(VueScheduler, {
  locale: 'en',
  minDate: null,
  maxDate: null,
  timeRange: [new Date().getHours()-4, 23],
  availableViews: ['week','day'],
  initialDate: new Date(),
  initialView: 'day',
  use12: false,
  showTimeMarker: true,
  showTodayButton: true,
  eventDisplay: null
});
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
