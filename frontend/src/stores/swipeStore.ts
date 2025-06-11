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

const RETRY_SWIPES_KEY = 'retrySwipes';
const BATCH_SIZE = 10;
const FLUSH_INTERVAL = 30000; // 30 seconds

export const useSwipeStore = defineStore('swipe', () => 
{
    const historyStore = useHistoryStore();
    const favouritesStore = useFavouritesStore();

    function initializeStore() {
        loadRetrySwipes();
        setupFlushTimer();
    }

    function cleanup() {
        clearFlushTimer();
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

    //#region Normal Flow

    /* pending
    *   A list of swiped cards waiting to be flushed to backend.
    *   onSwipe            @params - event: SwipeEvent
    *   flushPending      
    *=====================================*/
    const pending = ref([] as SwipeEvent[]);
    const isFlushingPending = ref(false);

    const createSwipeEvent = (characterId: number, status: 'nope' | 'love' | 'favourite', seenAt: string): SwipeEvent => ({
        characterId,
        status,
        seenAt
    })

    function onSwipe(event: SwipeEvent) {
        const character = findCard(event.characterId)
        removeCard(event.characterId);

        // Update local stores immediately
        historyStore.appendRecentCard(character, event.status, event.seenAt)
        if (event.status == 'favourite') favouritesStore.appendRecentCard(character, event.seenAt);

        // Add to pending batch
        pending.value.push(event);

        // Flush if we've reached batch size
        if (pending.value.length >= BATCH_SIZE) {
            flushPending();
        }
    }

    async function flushPending() {
        if (!pending.value.length || isFlushingPending.value) return;

        const token = localStorage.getItem('token');
        if (!token) {
            console.error('Not logged in - cannot save swipes');
            return;
        }

        isFlushingPending.value = true;
        const batchToFlush = [...pending.value]; // Create a copy
        pending.value = []; // Clear pending immediately

        try {
            const favourites = batchToFlush.filter(swipe => swipe.status === 'favourite');
            
            // Attempt to save to UserHistory
            const historySuccess = await saveToUserHistory(batchToFlush);
            
            // Attempt to save favourites to UserFavourites (if any)
            const favouritesSuccess = favourites.length > 0 ? await saveToUserFavourites(favourites) : true;

            // Handle failures by adding to retry queue
            if (!historySuccess) {
                addToRetryQueue(batchToFlush, 'history');
            }
            
            if (!favouritesSuccess && favourites.length > 0) {
                addToRetryQueue(favourites, 'favourites');
            }

            if (historySuccess && favouritesSuccess) {
                console.log(`Successfully flushed ${batchToFlush.length} swipe events`);
            }

        } catch (err) {
            console.error("Failed to flush swipe events:", err);
            // Add entire batch to retry queue
            addToRetryQueue(batchToFlush, 'history');
        } finally {
            isFlushingPending.value = false;
        }
    }

    //#endregion

    //#region Retry Mechanism

    /* retry
    *   A list of failed swipe events that need to be retried.
    *   addToRetryQueue    @params - events: SwipeEvent[], type: 'history' | 'favourites'
    *   retryFailedSwipes
    *=====================================*/
    const retry = ref([] as Array<{ events: SwipeEvent[], type: 'history' | 'favourites' }>);
    const isRetrying = ref(false);

    function addToRetryQueue(events: SwipeEvent[], type: 'history' | 'favourites') {
        retry.value.push({ events, type });
        persistRetrySwipes();
        console.log(`Added ${events.length} events to retry queue for ${type}`);
    }

    async function retryFailedSwipes() {
        if (!retry.value.length || isRetrying.value) return;

        const token = localStorage.getItem('token');
        if (!token) {
            console.error('Not logged in - cannot retry swipes');
            return;
        }

        isRetrying.value = true;
        const failedRetries: Array<{ events: SwipeEvent[], type: 'history' | 'favourites' }> = [];

        try {
            for (const retryItem of retry.value) {
                let success = false;
                
                if (retryItem.type === 'history') {
                    success = await saveToUserHistory(retryItem.events);
                } else if (retryItem.type === 'favourites') {
                    success = await saveToUserFavourites(retryItem.events);
                }

                if (!success) {
                    failedRetries.push(retryItem);
                } else {
                    console.log(`Successfully retried ${retryItem.events.length} ${retryItem.type} events`);
                }
            }

            // Update retry queue with only failed items
            retry.value = failedRetries;
            persistRetrySwipes();

        } catch (err) {
            console.error("Error during retry process:", err);
        } finally {
            isRetrying.value = false;
        }
    }

    function loadRetrySwipes() {
        try {
            const stored = localStorage.getItem(RETRY_SWIPES_KEY);
            if (stored) {
                const parsedRetries = JSON.parse(stored);
                if (Array.isArray(parsedRetries)) {
                    retry.value = parsedRetries;
                    console.log(`Loaded ${parsedRetries.length} retry items from storage`);
                    
                    // Attempt to retry failed swipes on startup
                    if (parsedRetries.length > 0) {
                        retryFailedSwipes();
                    }
                }
            }
        } catch (error) {
            console.error('Failed to load retry swipes:', error);
            localStorage.removeItem(RETRY_SWIPES_KEY);
        }
    }

    function persistRetrySwipes() {
        try {
            if (retry.value.length > 0) {
                localStorage.setItem(RETRY_SWIPES_KEY, JSON.stringify(retry.value));
            } else {
                localStorage.removeItem(RETRY_SWIPES_KEY);
            }
        } catch (error) {
            console.error('Failed to persist retry swipes:', error);
        }
    }

    //#endregion

    //#region API Helpers

    // Helper function to save to UserHistory
    async function saveToUserHistory(batch: SwipeEvent[]): Promise<boolean> {
        try {
            const url = `https://localhost:7235/api/UserHistory/batch_save`;

            const response = await apiFetch(url, {
                method: 'POST',
                body: JSON.stringify(batch)
            });

            return response.ok;
        } catch (error) {
            console.error('Failed to save to UserHistory:', error);
            return false;
        }
    }

    // Helper function to save to UserFavourites
    async function saveToUserFavourites(favourites: SwipeEvent[]): Promise<boolean> {
        try {
            const url = `https://localhost:7235/api/UserFavourites/batch_save`;

            const response = await apiFetch(url, {
                method: 'POST',
                body: JSON.stringify(favourites)
            });

            return response.ok;
        } catch (error) {
            console.error('Failed to save to UserFavourites:', error);
            return false;
        }
    }

    //#endregion

    //#region Timer Management

    const flushTimer = ref<number | null>(null);

    function setupFlushTimer() {
        if (flushTimer.value) clearInterval(flushTimer.value);
        
        flushTimer.value = setInterval(() => {
            if (pending.value.length > 0) {
                console.log('Periodic flush triggered for pending swipes');
                flushPending();
            }
            if (retry.value.length > 0) {
                console.log('Periodic retry triggered for failed swipes');
                retryFailedSwipes();
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

    // Computed properties for monitoring
    const pendingCount = computed(() => pending.value.length);
    const retryCount = computed(() => retry.value.length);
    const isFlushingStatus = computed(() => isFlushingPending.value || isRetrying.value);

    return { 
        cards, isLoading,
        fetchCards, removeCard, 
        pending, pendingCount, createSwipeEvent, onSwipe, flushPending,
        retry, retryCount, retryFailedSwipes,
        isFlushingStatus,
        xPos, saveXPos, 
        yPos, saveYPos,
        isDragging, setDragging,
        initializeStore, cleanup
    }
})