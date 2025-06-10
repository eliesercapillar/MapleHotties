import { defineStore } from 'pinia'
import { ref, computed } from 'vue'
import Backgrounds from "@/data/Backgrounds.json";
import { useHistoryStore } from './historyStore';
import { useFavouritesStore } from './favouritesStore';
import { apiFetch } from '@/utils/api'

//#region Interfaces

interface ApiCharacter {
  id: number
  name: string
  level: number
  job: string
  world: string
  imageUrl: string
}

interface CharacterCard 
{
  character : ApiCharacter
  bgURL: string;
}

interface SwipeEvent {
  characterId: number;
  status: 'nope' | 'love' | 'favourite';
  seenAt: string;
}

//#endregion

function getRandomBG(): string {
  return Backgrounds.data[Math.floor(Math.random() * Backgrounds.data.length)]
}

const PENDING_SWIPES_KEY = 'pendingSwipes';
const BATCH_SIZE = 10;
const FLUSH_INTERVAL = 30000; // 30 seconds

export const useSwipeStore = defineStore('swipe', () => 
{
    const historyStore = useHistoryStore();
    const favouritesStore = useFavouritesStore();

    function initializeStore() {
        loadPendingSwipes(); // Load pending swipes from localStorage on store initialization
        setupFlushTimer();
    }

    function cleanup() {
        clearFlushTimer();
        if (pending.value.length > 0) persistPendingSwipes();
    }

    //#region Cards

    /* cards
    *   The stack of cards. Should always be populated.
    *   fetchCards 
    *   removeCard      @params - id : number
    *=====================================*/
    const cards = ref([] as CharacterCard[]);
    const isLoading = ref(false);
    const curPage = ref(1);

    // TODO: change to use random characters api endpoint
    async function fetchCards() {
        if (isLoading.value) return;
        
        isLoading.value = true;
        try {
            const pageSize = 10;
            const url = `https://localhost:7235/api/Characters?page=${curPage.value}&pageSize=${pageSize}`;
            //const url = `http://localhost:5051/api/Characters?page=${curPage.value}&pageSize=${pageSize}`;
            const response = await fetch(url);
            if (!response.ok) throw new Error(`Failed to fetch characters: ${response.status}`);
            
            // Map API response into our CharacterCard shape
            const data: ApiCharacter[] = await response.json()
            const newCards: CharacterCard[] = data.map((char) => ({
                character: char,
                bgURL: getRandomBG()
            }))
            
            cards.value.unshift(...newCards);
            curPage.value++;
        }
        catch (err) {
            console.error("Failed to load more cards:", err);
        }
        finally {
            isLoading.value = false;
        }
    }

    function removeCard(id : number) {
        cards.value = cards.value.filter((card) => card.character.id !== id);
    }

    function findCard(id : number) : ApiCharacter {
        return cards.value.find((card) => card.character.id === id)!.character
    }

    /* xPos
    *   The X position of the current active card.
    *   saveXPos @params - x: number
    *=====================================*/
    const xPos = ref(0);
    function saveXPos(x : number) {
        xPos.value = x;
    }

    /* yPos
    *   The Y position of the current active card.
    *   saveYPos @params - y: number
    *=====================================*/
    const yPos = ref(0);
    function saveYPos(y : number) {
        yPos.value = y;
    }

    /* isDragging
    *   Whether or not the current active card is being dragged.
    *   setDragging @params - b: boolean
    *=====================================*/
    const isDragging = ref(false);
    function setDragging(b : boolean) {
        isDragging.value = b;
    }

    //#endregion

    //#region Persistency

    // Computed properties for debugging/monitoring
    const pendingCount = computed(() => pending.value.length);
    const isFlushingStatus = computed(() => isFlushing.value);

    /* pending
    *   A history of swiped cards, waiting to be saved into history.
    *   createSwipeEvent   @params - event: SwipeEvent
    *   onSwipe            @params - event: SwipeEvent
    *   flushAndSave      
    *=====================================*/
    const pending = ref([] as SwipeEvent[]);
    const isFlushing = ref(false);
    const flushTimer = ref<number | null>(null);

    const createSwipeEvent = (characterId: number, status: 'nope' | 'love' | 'favourite', seenAt: string): SwipeEvent => ({
        characterId,
        status,
        seenAt
    })

    function onSwipe(event: SwipeEvent) {
        const character = findCard(event.characterId)
        removeCard(event.characterId);

        historyStore.appendRecentCard(character, event.status, event.seenAt)
        if (event.status == 'favourite') favouritesStore.appendRecentCard(character, event.seenAt);

        pending.value.push(event);
        persistPendingSwipes(); // Save to localStorage immediately

        if (pending.value.length >= BATCH_SIZE) flushAndSave();
    }

    async function flushAndSave() {
        if (!pending.value.length || isFlushing.value) return;

        isFlushing.value = true;
        const batch = [...pending.value]; // Create a copy
        
        const token = localStorage.getItem('token');
        if (!token) {
            console.error('Not logged in - cannot save swipes');
            isFlushing.value = false;
            return;
        }

        try {
            // Separate favourites from all swipes
            const favourites = batch.filter(swipe => swipe.status === 'favourite');
            
            // Always save to UserHistory
            const historySuccess = await saveToUserHistory(batch, token);
            
            // Save favourites to UserFavourites if any exist
            let favouritesSuccess = true;
            if (favourites.length > 0) {
                favouritesSuccess = await saveToUserFavourites(favourites, token);
            }

            // Only clear pending swipes if both operations succeeded
            if (historySuccess && favouritesSuccess) {
                pending.value = pending.value.filter(swipe => 
                    !batch.some(batchSwipe => 
                        batchSwipe.characterId === swipe.characterId && 
                        batchSwipe.seenAt === swipe.seenAt
                    )
                );
                
                persistPendingSwipes(); // Update localStorage
                console.log(`Successfully saved ${batch.length} swipe events (${favourites.length} favourites)`);
            } else {
                console.error('Partial save failure - keeping swipes in queue for retry');
            }
            
        } catch (err) {
            console.error("Failed to save swipe events:", err);
            // Don't clear pending swipes on error - they'll be retried later
        } finally {
            isFlushing.value = false;
        }
    }

    // Helper function to save to UserHistory
    async function saveToUserHistory(batch: SwipeEvent[], token: string): Promise<boolean> {
        try {
            const url = `https://localhost:7235/api/UserHistory/batch_save`;

            const response = await fetch(url, {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                    'Authorization': `Bearer ${token}`
                },
                body: JSON.stringify(batch)
            });

            if (!response.ok) {
                throw new Error(`UserHistory save failed: ${response.status}`);
            }

            return true;
        } catch (error) {
            console.error('Failed to save to UserHistory:', error);
            return false;
        }
    }

    // Helper function to save to UserFavourites
    async function saveToUserFavourites(favourites: SwipeEvent[], token: string): Promise<boolean> {
        try {
            const url = `https://localhost:7235/api/UserFavourites/batch_save`;

            const response = await fetch(url, {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                    'Authorization': `Bearer ${token}`
                },
                body: JSON.stringify(favourites)
            });

            if (!response.ok) {
                throw new Error(`UserFavourites save failed: ${response.status}`);
            }

            return true;
        } catch (error) {
            console.error('Failed to save to UserFavourites:', error);
            return false;
        }
    }

    function loadPendingSwipes() {
        try {
            const stored = localStorage.getItem(PENDING_SWIPES_KEY);
            if (stored) {
                const parsedSwipes = JSON.parse(stored);

                if (Array.isArray(parsedSwipes)) {
                    pending.value = parsedSwipes;
                    localStorage.removeItem(PENDING_SWIPES_KEY);
                    console.log(`Loaded ${parsedSwipes.length} pending swipes from storage`);
                    
                    if (parsedSwipes.length > 0) flushAndSave();
                }
            }
        } catch (error) {
            console.error('Failed to load pending swipes:', error);
            localStorage.removeItem(PENDING_SWIPES_KEY);
        }
    }

    function persistPendingSwipes() {
        try {
            if (pending.value.length > 0) 
                localStorage.setItem(PENDING_SWIPES_KEY, JSON.stringify(pending.value));
            else 
                localStorage.removeItem(PENDING_SWIPES_KEY);
        } 
        catch (error) {
            console.error('Failed to persist pending swipes:', error);
        }
    }

    function setupFlushTimer() {
        if (flushTimer.value) clearInterval(flushTimer.value);
        
        flushTimer.value = setInterval(() => {
            if (pending.value.length > 0) {
                console.log('Periodic flush triggered.');
                flushAndSave();
            }
        }, FLUSH_INTERVAL);
    }

    function clearFlushTimer() {
        if (flushTimer.value) {
            clearInterval(flushTimer.value);
            flushTimer.value = null;
        }
    }

    //#endregion

    return { 
        cards, isLoading,
        fetchCards, removeCard, 
        pending, pendingCount, createSwipeEvent, onSwipe, flushAndSave,
        isFlushingStatus,
        xPos, saveXPos, 
        yPos, saveYPos,
        isDragging, setDragging,
        initializeStore, cleanup
    }
})
