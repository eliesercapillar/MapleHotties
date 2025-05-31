import { defineStore } from 'pinia'
import { ref } from 'vue'

interface HistoryCharacter {
    character: {
        id: number
        name: string
        level: number
        job: string
        world: string
        imageUrl: string
    }
    status: string
    seenAt: string
}

export const useHistoryStore = defineStore('history', () => 
{
    const cards = ref([] as HistoryCharacter[]);
    const isLoading = ref(false);

    async function fetchHistory() {
        if (isLoading.value) return;
        
        const token = localStorage.getItem('token');
        if (!token) throw new Error('Not logged in.');

        isLoading.value = true;
        try {
            const quantity = 4;
            const url = `https://localhost:7235/api/UserHistory/recent?quantity=${quantity}`

            const response = await fetch(url, {
                method: 'GET',
                headers: {
                'Authorization': `Bearer ${token}`
                }
            })

            if (!response.ok) throw new Error(`Failed to fetch history: ${response.status}`);

            console.log('Response status:', response.status);
            console.log('Response headers:', response.headers);

            cards.value = await response.json();
            console.log(cards.value);
        //const pageSize = 10;
        //const url = `https://localhost:7235/api/Characters?page=${curPage.value}&pageSize=${pageSize}`;
        //const url = `http://localhost:5051/api/Characters?page=${curPage.value}&pageSize=${pageSize}`;
        //const response = await fetch(url);
        //if (!response.ok) throw new Error(`Failed to fetch history: ${response.status}`);
        }
        catch (err) {
        console.error("Failed to load more cards:", err);
        }
        finally {
        isLoading.value = false;
        }
    }

    return { isLoading, cards,
            fetchHistory,
    }
})
