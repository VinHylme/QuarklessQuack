import Vue from "vue";
import Router from "vue-router";
import store from "./State";
Vue.use(Router);

function LazyLoad(view){
    return() => import('@/components/Pages/'+view+'.vue')
}

const router = new Router({
    mode:"history",
    base: process.env.BASE_URL,
    routes:[
        {
        name:"home",
        path:"/",
        component:LazyLoad('Home'),
        meta:{
            requiresAuth: true
        }
		},
		{
			name:"login",
			path:"/login",
			component:LazyLoad("Login")
		},
		{
			name:"register",
			path:"/register",
			component:LazyLoad("Register")
		},
		{
			name:"checkout",
			path:"/checkout/",
			component:LazyLoad("Checkout"),
			meta:{
				requiresAuth:true
			}
		},
		{
			name:"notice",
			path:"/notice/",
			component:LazyLoad("Notice"),
			meta:{
				requiresAuth:true
			}
		},
       {
           name:"view",
           path:"/view/:id",
           component:LazyLoad('ViewAccount'),
           meta:{
               requiresAuth: true
           }
       },
        {
        name:'Communications',
        path:'/communications',
        component:LazyLoad('Messaging'),
        meta:{
            requiresAuth:true
        }
        },
       {
           name:"library",
           path:"/library/:id",
           component:LazyLoad('Library'),
           meta:{
               requiresAuth:true
           }
       },
       {
            name:"manage",
            path:"/manage",
            component:LazyLoad('Manage'),
            meta:{
                requiresAuth:true
            }
       },
       {
           name:"stats",
           path:"/stats",
           component:LazyLoad('Stats'),
           meta:{
               requiresAuth: true
           }
       },
       {
           name:"profile",
           path:"/profile/:id",
           component:LazyLoad('Profile'),
           meta:{
               requiresAuth: true
           }
       },
       {
           name:"settings",
           path:"settings",
           component:LazyLoad('Settings'),
           meta:{
               requiresAuth: true
           }
       },
       {
           path:"/profile/",
           redirect:{
               name:"profile"
           }, 
           meta:{
             requiresAuth: true
            }
       },
       {
           name:"Error404",
           path:"*",
           component:LazyLoad('NotFound')
       }
]
});


router.beforeEach((to, from, next) => {
    if(to.matched.some(record => record.meta.requiresAuth)) {
      if (store.getters.IsLoggedIn) {
        next()
        return
      }
      if(!store.getters.IsLoggedIn){
        next('/login')
        return;
      }
      
      if(router.currentRoute.fullPath !== '/login'){
        next()
        return;
      }
    } else {
      next() 
    }
});

export default router;