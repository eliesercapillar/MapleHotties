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

interface HistoryCharacter {
    character: ApiCharacter
    status: string
    seenAt: string
}

export const useHistoryStore = defineStore('history', () => 
{
    const historyCards = ref([] as HistoryCharacter[]);
    const favouriteCards = ref([] as HistoryCharacter[]);
    const isLoading = ref(false);

    async function fetchHistory() {
        if (isLoading.value) return;
        
        const token = localStorage.getItem('token');
        if (!token) throw new Error('Not logged in.');

        isLoading.value = true;
        try {
            const url = `https://localhost:7235/api/UserHistory/`

            const response = await fetch(url, {
                method: 'GET',
                headers: {
                    'Authorization': `Bearer ${token}`
                }
            })

            if (!response.ok) throw new Error(`Failed to fetch history: ${response.status}`);

            historyCards.value = await response.json();
        }
        catch (err) {
            console.error("Failed to load more cards:", err);
        }
        finally {
            isLoading.value = false;
        }
    }

    async function fetchFavourites() {
        if (isLoading.value) return;
        
        const token = localStorage.getItem('token');
        if (!token) throw new Error('Not logged in.');

        isLoading.value = true;
        try {
            const url = `https://localhost:7235/api/UserHistory/favourites`

            const response = await fetch(url, {
                method: 'GET',
                headers: {
                    'Authorization': `Bearer ${token}`
                }
            })

            if (!response.ok) throw new Error(`Failed to fetch history: ${response.status}`);

            favouriteCards.value = await response.json();
        }
        catch (err) {
            console.error("Failed to load more cards:", err);
        }
        finally {
            isLoading.value = false;
        }
    }

    function appendRecentCard(character : ApiCharacter, status : string, seenAt: string) {
        historyCards.value.unshift({
            character,
            status,
            seenAt
        });

        if (status == "favourited")
        {
            favouriteCards.value.unshift({
                character,
                status,
                seenAt
            });
        }
    }

    return { 
        isLoading, historyCards, favouriteCards,
        fetchHistory, fetchFavourites, 
        appendRecentCard
    }
})
