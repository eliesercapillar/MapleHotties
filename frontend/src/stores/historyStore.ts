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
    const cards = ref([] as HistoryCharacter[]);
    const maxCards = 6;
    const isLoading = ref(false);

    async function fetchHistory() {
        if (isLoading.value) return;
        
        const token = localStorage.getItem('token');
        if (!token) throw new Error('Not logged in.');

        isLoading.value = true;
        try {
            //const url = `https://localhost:7235/api/UserHistory/recent?quantity=${maxCards}`
            const url = `https://localhost:7235/api/UserHistory/`

            const response = await fetch(url, {
                method: 'GET',
                headers: {
                'Authorization': `Bearer ${token}`
                }
            })

            if (!response.ok) throw new Error(`Failed to fetch history: ${response.status}`);

            cards.value = await response.json();
        }
        catch (err) {
            console.error("Failed to load more cards:", err);
        }
        finally {
            isLoading.value = false;
        }
    }

    function appendRecentCard(character : ApiCharacter, status : string, seenAt: string) {
        const card: HistoryCharacter = {
            character,
            status,
            seenAt
        }

        // cards.value = [card, ...cards.value.slice(0, maxCards - 1)];
        cards.value.unshift(card);
    }

    return { isLoading, cards,
            fetchHistory, appendRecentCard
    }
})
