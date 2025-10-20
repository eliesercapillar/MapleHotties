import { defineStore } from 'pinia'
import { ref, computed } from 'vue'
import Backgrounds from "@/data/Backgrounds.json";
import { useHistoryStore } from './historyStore';
import { apiFetch } from '@/utils/api'
import { isLoggedIn } from '@/utils/auth';

// #region Interfaces

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
  status: 'liked' | 'noped' | 'favourited';
  seenAt: string;
}

// #endregion

// #region Helper Functions

function getRandomBG(): string {
  return Backgrounds.data[Math.floor(Math.random() * Backgrounds.data.length)]
}

// #endregion

const RETRY_SWIPES_KEY = 'retrySwipes';
const BATCH_SIZE = 10;
const FLUSH_INTERVAL = 30000; // 30 seconds

export const useSwipeStore = defineStore('swipe', () => 
{
    const historyStore = useHistoryStore();

    // #region Lifecycle Functions

    function initializeStore() {
        loadRetrySwipes();
        setupFlushTimer();
    }

    function cleanup() {
        clearFlushTimer();
    }

    // #endregion Lifecycle Functions

    // #region Cards State & Management

    /* cards
    *   The stack of cards. Should always be populated.
    *   fetchCards 
    *   removeCard      @params - id : number
    *=====================================*/
    const cards = ref([] as CharacterCard[]);
    const isLoading = ref(false);
    const curPage = ref(1);

    async function fetchCards(initialLoad = false) {
        if (isLoading.value) return;
        
        const fetchCount = initialLoad ? 30 : 15;
        isLoading.value = true;
        
        try {
            // Exclude cards currently in the stack from being retrieved
            const excludeIds = cards.value.map(c => c.character.id).join(',');
            const excludeParam = excludeIds ? `&exclude=${excludeIds}` : '';

            const url = `https://localhost:7235/api/Characters/random?quantity=${fetchCount}${excludeParam}`;
            const response = await apiFetch(url);
            if (!response.ok) throw new Error(`Failed to fetch characters: ${response.status}`);
            
            
            const data: ApiCharacter[] = await response.json()

            const existingIds = new Set(cards.value.map(c => c.character.id));
  
            const newCards: CharacterCard[] = data
                .filter(char => !existingIds.has(char.id))  // Filter duplicates just in case
                .map((char) => ({
                character: char,
                bgURL: getRandomBG()
                }))

            // TODO: remove needless filtering once Characters controller no longer returns duplicates.
            // const newCards: CharacterCard[] = data.map((char) => ({
            //     character: char,
            //     bgURL: getRandomBG()
            // }))
            
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

    /* isDragging
    *   Whether or not the current active card is being dragged.
    *   setDragging @params - b: boolean
    *=====================================*/
    const isDragging = ref(false);
    function setDragging(b : boolean) {
        isDragging.value = b;
    }

    // #endregion Cards State & Management

    // #region Normal Flow

    /* pending
    *   A list of swiped cards waiting to be flushed to backend.
    *   onSwipe            @params - event: SwipeEvent
    *   flushPending      
    *=====================================*/
    const pending = ref([] as SwipeEvent[]);
    const isFlushingPending = ref(false);

    const createSwipeEvent = (characterId: number, status: 'liked' | 'noped' | 'favourited', seenAt: string): SwipeEvent => ({
        characterId,
        status,
        seenAt
    })

    function onSwipe(character: ApiCharacter, status: 'liked' | 'noped' | 'favourited', seenAt: string) {
        removeCard(character.id);

        historyStore.appendRecentCard(character, status, seenAt)
        pending.value.push(createSwipeEvent(character.id, status, seenAt));

        if (pending.value.length >= BATCH_SIZE) flushPending();
    }

    async function flushPending() {
        if (!pending.value.length || isFlushingPending.value) return;


        if (!isLoggedIn()) {
            console.error('Not logged in - cannot save swipes');
            return;
        }

        isFlushingPending.value = true;
        const batchToFlush = [...pending.value]; // Create a copy
        pending.value = []; // Clear pending immediately

        try {
            const historySuccess = await saveToUserHistory(batchToFlush);

            // Handle failures by adding to retry queue
            if (!historySuccess) addToRetryQueue(batchToFlush);

        } 
        catch (err) {
            addToRetryQueue(batchToFlush);
            console.error("Failed to flush swipe events:", err);
        } 
        finally {
            isFlushingPending.value = false;
        }
    }

    //#endregion

    //#region Retry Mechanism

    /* retry
    *   A list of failed swipe events that need to be retried.
    *   addToRetryQueue    @params - events: SwipeEvent[]
    *   retryFailedSwipes
    *=====================================*/
    const retry = ref([] as Array<{ events: SwipeEvent[] }>);
    const isRetrying = ref(false);

    function addToRetryQueue(events: SwipeEvent[]) {
        retry.value.push({ events });
        persistRetrySwipes();
        console.log(`Added ${events.length} events to retry queue`);
    }

    async function retryFailedSwipes() {
        if (!retry.value.length || isRetrying.value) return;

        if (!isLoggedIn()) {
            console.error('Not logged in - cannot retry swipes');
            return;
        }

        isRetrying.value = true;
        const failedRetries: Array<{ events: SwipeEvent[] }> = [];

        try {
            for (const retryItem of retry.value) {
                let success = await saveToUserHistory(retryItem.events);

                if (!success) failedRetries.push(retryItem);
            }

            retry.value = failedRetries; // Update retry queue with only failed items
            persistRetrySwipes();

        } 
        catch (err) {
            console.error("Error during retry process:", err);
        } 
        finally {
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
        } 
        catch (error) {
            localStorage.removeItem(RETRY_SWIPES_KEY);
            console.error('Failed to load retry swipes:', error);
        }
    }

    function persistRetrySwipes() {
        try {
            if (retry.value.length > 0) {
                localStorage.setItem(RETRY_SWIPES_KEY, JSON.stringify(retry.value));
            } 
            else {
                localStorage.removeItem(RETRY_SWIPES_KEY);
            }
        } 
        catch (error) {
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

            console.log(`Successfully saved ${batch.length} entries to history.`)

            return response.ok;
        } catch (error) {
            console.error('Failed to save to UserHistory:', error);
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
        // Cards
        cards, isLoading,
        fetchCards, removeCard, 

        // Drag State
        isDragging, setDragging,
    
        // Swipe Events
        onSwipe, flushPending, retryFailedSwipes,

        // Monitoring
        pending, pendingCount,
        retry, retryCount,
        isFlushingStatus,

        // Lifecycle
        initializeStore, cleanup
    }
})