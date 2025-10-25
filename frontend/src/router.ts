import { createRouter, createWebHistory } from 'vue-router';
import Home from '@/pages/Home.vue';
import Main from '@/pages/Main.vue';
import Login from '@/pages/Login.vue';
import Register from '@/pages/Register.vue';
import RateApp from '@/pages/sub_pages/RateApp.vue';
import PlayApp from '@/pages/sub_pages/PlayApp.vue';
import Leaderboard from '@/pages/sub_pages/Leaderboard.vue';

// import Contact from '@/pages/Contact.vue';
import NotFound from '@/pages/404.vue'
import { isLoggedIn } from '@/utils/auth';
import { useNavigationStore } from '@/stores/navigationStore';
import { usePlayStore } from '@/stores/playStore';
import { useSwipeStore } from '@/stores/swipeStore';

const routes = [
  { path: '/', component: Home },
  { 
    path: '/app', 
    component: Main,
    redirect: '/app/rate',
    children: [
      { 
        path: 'rate', 
        name: 'Rate',
        component: RateApp 
      },
      { 
        path: 'play', 
        name: 'Play',
        component: PlayApp
      },
      { path: 'leaderboard', 
        name: 'leaderboard',
        component: Leaderboard},
    ]
  },
  { path: '/leaderboard', redirect: 'app/leaderboard'},
  { path: '/login', component: Login},
  { path: '/register', component: Register},
  { path: '/:pathMatch(.*)*', component: NotFound },
];

const router = createRouter({
  history: createWebHistory(),
  routes,
});

router.beforeEach((to, from, next) => {
  const navigationStore = useNavigationStore();
  const playStore = usePlayStore();

  if (from.path === '/app/play') {
    //TODO: Maybe add a "Are you sure you want to stop playing?" popup info modal?
    playStore.isCurrentlyPlaying = false;
  }

  if (to.path === '/app/rate') navigationStore.currentPage = 'Rate';
  else if (to.path === '/app/play') navigationStore.currentPage = 'Play';
  else if (to.path === '/app/leaderboard') navigationStore.currentPage = 'Leaderboard';

  if ((to.path === '/login' || to.path === '/register') && isLoggedIn()) 
    next('/app')
  else if ((to.path === '/app/rate') && !isLoggedIn())
    next('/login')
  else
    next();
})

export default router;
