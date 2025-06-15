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
    totalCount: number
}

interface NopeCharacter {
    character: ApiCharacter
    totalCount: number
}

interface FullCharacter {
    character: ApiCharacter
    totalLikeCount: number
    totalNopeCount: number
    totalFavouriteCount: number
}

export const useLeaderboardStore = defineStore('leaderboard', () => 
{
    const likeCharacters = ref([] as LikeCharacter[]);
    const nopeCharacters = ref([] as NopeCharacter[]);
    const currentPage = ref(1);
    const maxCards = 6;
    const isLoading = ref(false);

    async function fetchTopLiked() {
        if (isLoading.value) return;

        isLoading.value = true;
        try {
            const pageSize = 10;
            const url = `https://localhost:7235/api/CharacterStats/top_liked?page=${currentPage.value}&pageSize=${pageSize}`

            const response = await fetch(url, {
                method: 'GET'
            })

            if (!response.ok) throw new Error(`Failed to fetch Top Liked page ${currentPage.value}: ${response.status}`);

            likeCharacters.value = await response.json();
        }
        catch (err) {
            console.error("Failed to load more cards:", err);
        }
        finally {
            isLoading.value = false;
        }
    }

    async function fetchTopNoped() {
        if (isLoading.value) return;

        isLoading.value = true;
        try {
            const pageSize = 10;
            const url = `https://localhost:7235/api/CharacterStats/top_noped?page=${currentPage}&pageSize=${pageSize}`

            const response = await fetch(url, {
                method: 'GET'
            })

            if (!response.ok) throw new Error(`Failed to fetch Top Liked page ${currentPage}: ${response.status}`);

            nopeCharacters.value = await response.json();
        }
        catch (err) {
            console.error("Failed to load more cards:", err);
        }
        finally {
            isLoading.value = false;
        }
    }

    const nextPage = currentPage.value += 1;

    return { isLoading, currentPage,
        likeCharacters, nopeCharacters,
        fetchTopLiked, fetchTopNoped,
    }
})
