import { defineStore } from 'pinia'
import { ref, computed } from 'vue'
import Backgrounds from "@/data/Backgrounds.json";

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
  id: number;
  bgURL: string;
  spriteURL: string;
  info: CharacterInfo;
}

interface CharacterInfo {
  name: string;
  level: number;
  job: string;
  world: string;
}

interface SwipeEvent {
  characterId: number;
  status: 'nope' | 'like' | 'favourite';
  seenAt: string;
}

function getRandomBG(): string {
  return Backgrounds.data[Math.floor(Math.random() * Backgrounds.data.length)]
}

export const useSwipeStore = defineStore('swipe', () => 
{
  /* cards
   *   The stack of cards. Should always be populated.
   *   fetchCards 
   *   removeCard      @params - id : number
   *=====================================*/
  const cards = ref([] as CharacterCard[]);
  const isLoading = ref(false);
  const curPage = ref(1);


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
        id: char.id,
        bgURL: getRandomBG(),
        spriteURL: char.imageUrl,
        info: {
          name: char.name,
          level: char.level,
          job: char.job,
          world: char.world,
        }
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
    cards.value = cards.value.filter((card) => card.id !== id);
  }

  /* pending
   *   A history of swiped cards, waiting to be saved into history.
   *   createSwipeEvent   @params - event: SwipeEvent
   *   onSwipe            @params - event: SwipeEvent
   *   flushAndSave      
   *=====================================*/
  const pending = ref([] as SwipeEvent[]);

  const createSwipeEvent = (characterId: number, status: 'nope' | 'like' | 'favourite', seenAt: string): SwipeEvent => ({
    characterId,
    status,
    seenAt
  })

  function onSwipe(event: SwipeEvent) {
    removeCard(event.characterId);
    pending.value.push(event);
    if (pending.value.length >= 10) flushAndSave();
  }

  async function flushAndSave() {
    if (!pending.value.length) return;

    const batch = pending.value;
    pending.value = [] as SwipeEvent[];

    try {
      const url = `https://localhost:7235/api/UserHistory/batch_save`;
      //const url = `http://localhost:5051/api/UserHistory/batch`;

      const response = await fetch(url, {
        method: 'POST',
        headers: {'Content-Type': 'application/json'},
        body: JSON.stringify(batch)
      })
    }
    catch (err) {
      console.error("Failed to load more cards:", err);
    }
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


  return { cards, isLoading,
           fetchCards, removeCard, 
           pending, createSwipeEvent, onSwipe, flushAndSave,
           xPos, saveXPos, 
           yPos, saveYPos,
           isDragging, setDragging}
})
