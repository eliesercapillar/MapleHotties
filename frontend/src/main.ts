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
swipeStore.initializeStore();

// Save swipe history when closing application.
window.addEventListener('beforeunload', (event) => {
    swipeStore.flushAndSave();
    
    // Optionally show a warning if there are pending swipes
    // if (swipeStore.pendingCount > 0) event.preventDefault();
});

// Save swipe history when changing pages.
router.beforeEach(async (to, from, next) => {
    try {
        await swipeStore.flushAndSave();
        next();
    } catch (error) {
        console.error('Error flushing swipes during navigation:', error);
        next();
    }
});

// Save swipe history when app going to background.
document.addEventListener('visibilitychange', () => {
    if (document.hidden && swipeStore.pendingCount > 0) {
        console.log('App going to background: flushing swipes');
        swipeStore.flushAndSave();
    }
});

// Cleanup on app unmount
window.addEventListener('unload', () => {
    swipeStore.cleanup();
});