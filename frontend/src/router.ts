import { createRouter, createWebHistory } from 'vue-router';
import Home from '@/pages/Home.vue';
import Main from '@/pages/Main.vue';
import Login from '@/pages/Login.vue';
import Register from './pages/Register.vue';
// import Contact from '@/pages/Contact.vue';
import NotFound from '@/pages/404.vue'
import { isLoggedIn } from './utils/auth';

const routes = [
  { path: '/', component: Home },
  { path: '/app', component: Main },
  { path: '/login', component: Login},
  { path: '/register', component: Register},
  // { path: '/about', component: About },
  // { path: '/leaderboard', component: Leaderboard },
  // { path: '/contact', component: Contact },
  { path: '/:pathMatch(.*)*', component: NotFound },
];

const router = createRouter({
  history: createWebHistory(),
  routes,
});

router.beforeEach((to, from, next) => {
  if ((to.path === '/login' || to.path === '/register') && isLoggedIn()) 
    next('/app')
  else if ((to.path === '/app') && !isLoggedIn())
    next('/login')
  else
    next();
})

export default router;
