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

interface FavouriteCharacter {
    character: ApiCharacter
    seenAt: string
}

export const useFavouritesStore = defineStore('favourites', () => 
{
    const cards = ref([] as FavouriteCharacter[]);
    const maxCards = 6;
    const isLoading = ref(false);

    async function fetchFavourites() {
        if (isLoading.value) return;
        
        const token = localStorage.getItem('token');
        if (!token) throw new Error('Not logged in.');

        isLoading.value = true;
        try {
            //const url = `https://localhost:7235/api/UserHistory/recent?quantity=${maxCards}`
            const url = `https://localhost:7235/api/UserFavourites/`

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

    function appendRecentCard(character : ApiCharacter, seenAt: string) {
        const card: FavouriteCharacter = {
            character,
            seenAt
        }

        // cards.value = [card, ...cards.value.slice(0, maxCards - 1)];
        cards.value.unshift(card);
        console.log(cards.value);
    }

    return { isLoading, cards,
             fetchFavourites, appendRecentCard
    }
})
