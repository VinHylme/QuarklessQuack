import Vue from 'vue'
import App from './App.vue'
import Buefy from 'buefy';
//import 'buefy/dist/buefy.css';
import router from "./Route";
import store from "./State";
import VueResource from 'vue-resource';
import Axios from 'axios'
import VueScheduler from '../references/v-calendar-scheduler/index';
import LazyLoadDirective from "./directives/LazyLoadDirective";
import EventBus from "./Plugins/EventBus"
// optionally import default styles


Vue.use(Buefy);
Vue.use(VueResource);
Vue.use(EventBus)
Vue.directive("lazyload", LazyLoadDirective);
Vue.use(VueScheduler, {
  locale: 'en',
  minDate: null,
  maxDate: null,
  timeRange: [new Date().getHours(), 23],
  availableViews: ['week','day'],
  initialDate: new Date(),
  initialView: 'week',
  labels:{
	today: 'Today',
    back: 'Back',
    next: 'Next',
    month: 'Month',
    week: 'Week',
	day: 'Day',
	all_day:''
  },
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
