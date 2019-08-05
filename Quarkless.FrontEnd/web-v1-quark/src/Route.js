import Vue from "vue";
import Router from "vue-router";
import Home from './components/Pages/Home.vue';
import Manage from "./components/Pages/Manage.vue";
import Profiles from "./components/Pages/Profiles.vue";
import Profile from "./components/Pages/Profile.vue";
import Stats from "./components/Pages/Stats.vue";
import Settings from './components/Pages/Settings.vue';
import CreateAccount from "./components/Pages/CreateAccount.vue";
import CreateProfile from "./components/Pages/CreateProfile.vue";
import ViewAccount from "./components/Pages/ViewAccount.vue";
import NotFound from "./components/Pages/HandlerPages/NotFound.vue";

Vue.use(Router);
var router = new Router({});
router.beforeEach((to, from, next) => {
    if(to.matched.some(record => record.meta.requiresAuth)) {
      if (this.$store.getters.isLoggedIn) {
        next()
        return
      }
      next('/login') 
    } else {
      next() 
    }
  })

export default new Router({
    mode:"history",
    base: process.env.BASE_URL,
    routes:[
        {
        name:"home",
        path:"/",
        component:Home,
        meta:{
            requiresAuth: true
        }
       },
       {
           name:"view",
           path:"/view/:id",
           component:ViewAccount,
           meta:{
               requiresAuth: true
           }
       },
       {
            name:"manage",
            path:"/manage/",
            component:Manage,
            meta:{
                requiresAuth:true
            }
       },
       {
           name:"stats",
           path:"/stats",
           component:Stats,
           meta:{
               requiresAuth: true
           }
       },
       {
       name:"profiles",
       path:"/profiles",
       component:Profiles,
       meta:{
           requiresAuth: true
       }
       },
       {
           name:"profile",
           path:"/profile/:id",
           component:Profile,
           meta:{
               requiresAuth: true
           }
       },
       {
           name:"settings",
           path:"/settings",
           component:Settings,
           meta:{
               requiresAuth: true
           }
       },
       {
           path:"/profile/",
           redirect:{
               name:"profiles"
           }
       },
       {
           name:"linkAccount",
           path:"/linkAccount",
           component:CreateAccount,
           meta:{
               requiresAuth: true
           }
       },
       {
           name:"createProfile",
           path:"/createProfile",
           component:CreateProfile,
           meta:{
               requiresAuth: true
           }
       },
       {
           name:"Error404",
           path:"*",
           component:NotFound
       }
]
})