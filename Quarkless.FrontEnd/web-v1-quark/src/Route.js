import Vue from "vue";
import Router from "vue-router";
import Home from "./components/Pages/Home.vue";
import Manage from "./components/Pages/Manage.vue";
import Profiles from "./components/Pages/Profiles.vue";
import Profile from "./components/Pages/Profile.vue";
import Stats from "./components/Pages/Stats.vue";
import Settings from "./components/Pages/Settings.vue";
import CreateAccount from "./components/Pages/CreateAccount.vue";
import CreateProfile from "./components/Pages/CreateProfile.vue";
import NotFound from "./components/Pages/HandlerPages/NotFound.vue";
Vue.use(Router);

export default new Router({
    mode:"history",
    base: process.env.BASE_URL,
    routes:[
        {
        name:"home",
        path:"/",
        component:Home
       },
       {
           name:"view",
           path:"/view/:id",
           component:Manage
       },
       {
           name:"stats",
           path:"stats/",
           component:Stats
       },
       {
       name:"profiles",
       path:"/profiles",
       component:Profiles
       },
       {
           name:"profile",
           path:"/profile/:id",
           component:Profile
       },
       {
           name:"settings",
           path:"/settings",
           component:Settings
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
           component:CreateAccount
       },
       {
           name:"createProfile",
           path:"/createProfile",
           component:CreateProfile
       },
       {
           name:"Error404",
           path:"*",
           component:NotFound
       }
]
})