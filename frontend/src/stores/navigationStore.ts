import { defineStore } from 'pinia'
import { ref } from 'vue'
import router from "@/router"
type Page = 'Rate' | 'Play' | 'Leaderboard';

export const useNavigationStore = defineStore('navigation', () => 
{
    const currentPage = ref<Page>(
        router.currentRoute.value.path.includes('/app/play') ? 'Play' : 
        router.currentRoute.value.path.includes('/app/leaderboard') ? 'Leaderboard' : 
        'Rate'
    );

    function navigateToRate() { router.push('/app/rate');}
    
    function navigateToPlay() { router.push('/app/play'); }

    function navigateToLeaderboard() { router.push('/app/leaderboard') };

    function logout() {
        localStorage.removeItem('token');
        router.push('/')
    }

    return {
        currentPage, 
        navigateToRate, navigateToPlay, navigateToLeaderboard, logout
    }

});