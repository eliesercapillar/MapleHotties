import './assets/index.css'
import App from './App.vue'
import router from './router';
import { createApp } from 'vue'
import { createPinia } from 'pinia';
import { useSwipeStore } from '@/stores/swipeStore';
import { RecycleScroller } from 'vue-virtual-scroller';
import 'vue-virtual-scroller/dist/vue-virtual-scroller.css';

const pinia = createPinia();
const app = createApp(App);

app.use(pinia);
app.use(router);
app.component('RecycleScroller', RecycleScroller);
app.mount('#app');

const swipeStore = useSwipeStore();

// Save swipe history when closing application or changing pages.
window.addEventListener('beforeunload', () => swipeStore.flushAndSave());
router.beforeEach((to, from, next) => {
    swipeStore.flushAndSave().finally(() => next());
});