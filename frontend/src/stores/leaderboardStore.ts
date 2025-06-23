import { defineStore } from 'pinia'
import { ref } from 'vue'

interface ApiCharacter {
    id: number
    name: string
    level: number
    job: string
    world: string
    imageUrl: string
}

interface LikeCharacter {
    character: ApiCharacter
    totalLikes: number
}

interface NopeCharacter {
    character: ApiCharacter
    totalNopes: number
}

interface PaginatedResponse<T> {
    data: T[]
    totalCount: number
    currentPage: number
    pageSize: number
    totalPages: number
}

export const useLeaderboardStore = defineStore('leaderboard', () => 
{
    const likeCharacters = ref([] as LikeCharacter[]);
    const nopeCharacters = ref([] as NopeCharacter[]);
    
    const showLikes = ref(true);
    const currentPage = ref(1);
    
    const totalPages = ref(0);
    const totalCount = ref(0);
    const pageSize = ref(10);
    const isLoading = ref(false);

    async function fetchTopLiked() {
        if (isLoading.value) return;

        isLoading.value = true;
        try {
            const url = `https://localhost:7235/api/CharacterStats/top_liked?page=${currentPage.value}&pageSize=5`;

            const response = await fetch(url, {
                method: 'GET'
            });

            if (!response.ok) {
                throw new Error(`Failed to fetch Top Liked page ${currentPage.value}: ${response.status}`);
            }

            const data: PaginatedResponse<LikeCharacter> = await response.json();
            
            likeCharacters.value = data.data;
            totalPages.value = data.totalPages;
            totalCount.value = data.totalCount;
            pageSize.value = data.pageSize;
        }
        catch (err) {
            console.error("Failed to load top liked characters:", err);
        }
        finally {
            isLoading.value = false;
        }
    }

    async function fetchTopNoped() {
        if (isLoading.value) return;

        isLoading.value = true;
        try {
            const url = `https://localhost:7235/api/CharacterStats/top_noped?page=${currentPage.value}&pageSize=${pageSize.value}`;

            const response = await fetch(url, {
                method: 'GET'
            });

            if (!response.ok) {
                throw new Error(`Failed to fetch Top Noped page ${currentPage.value}: ${response.status}`);
            }

            const data: PaginatedResponse<NopeCharacter> = await response.json();
            
            nopeCharacters.value = data.data;
            totalPages.value = data.totalPages;
            totalCount.value = data.totalCount;
            pageSize.value = data.pageSize;
        }
        catch (err) {
            console.error("Failed to load top noped characters:", err);
        }
        finally {
            isLoading.value = false;
        }
    }

    function setPage(page: number) {
        currentPage.value = page;
    }

    return { 
        isLoading,
        showLikes, 
        currentPage,
        totalPages,
        totalCount,
        pageSize,
        likeCharacters, 
        nopeCharacters,
        fetchTopLiked, 
        fetchTopNoped,
        setPage
    }
})
