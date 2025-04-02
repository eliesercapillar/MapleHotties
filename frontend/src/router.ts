import { createRouter, createWebHistory } from 'vue-router';
import Home from '@/pages/Home.vue';
import Main from '@/pages/Main.vue';
// import Projects from '@/pages/Projects.vue';
// import Contact from '@/pages/Contact.vue';
import NotFound from '@/pages/404.vue'

const routes = [
  // { path: '/', component: Home },
  { path: '/', component: Main },
//   { path: '/projects', component: Projects },
//   { path: '/contact', component: Contact },
  { path: '/:pathMatch(.*)*', component: NotFound },
];

const router = createRouter({
  history: createWebHistory(),
  routes,
});

export default router;
