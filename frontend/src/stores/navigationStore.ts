import { defineStore } from 'pinia'
import { ref } from 'vue'
import router from "@/router"
type Page = 'Rate' | 'Play' | 'Leaderboard';

export const useNavigationStore = defineStore('navigation', () => 
{
    const currentPage = ref<Page>('Rate');


    function navigateToRate() 
    {
        currentPage.value = 'Rate';
        router.push('/app/rate');
    }
    
    function navigateToPlay() 
    {
        currentPage.value = 'Play';
        router.push('/app/play'); 
    }

    function navigateToLeaderboard() 
    {
        currentPage.value = 'Leaderboard';
        router.push('/app/leaderboard');
    }

    function logout() {
        localStorage.removeItem('token');
        router.push('/')
    }

    return {
        currentPage, 
        navigateToRate, navigateToPlay, navigateToLeaderboard, logout
    }

});