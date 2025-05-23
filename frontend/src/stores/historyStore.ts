import { defineStore } from 'pinia'
import { ref, computed } from 'vue'

export const useHistoryStore = defineStore('history', () => 
{
    const cards = ref([] as Object[]);
    const isLoading = ref(false);

    async function fetchHistory() {
        if (isLoading.value) return;
        
        isLoading.value = true;
        try {
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

    return { isLoading,
            fetchHistory,
    }
})
