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

function getRandomBG(): string {
  return Backgrounds.data[Math.floor(Math.random() * Backgrounds.data.length)]
}

export const useSwipeStore = defineStore('swipe', () => 
{
  /* cards
   *   The stack of cards. Should always be populated.
   *   initializeCards @params - cardList : Card[]
   *   removeCard      @params - id : number
   *=====================================*/
  const cards = ref([] as CharacterCard[]);
  const isLoading = ref(false);
  const curPage = ref(1);

  function initializeCards(cardList : CharacterCard[]) {
    cards.value = cardList;
    currentIndex.value = 0
  }

  async function fetchCards() {
    if (isLoading.value) return;
    
    isLoading.value = true;
    try {
      const pageSize = 10;
      const url = `http://localhost:5051/api/Characters?page=${curPage.value}&pageSize=${pageSize}`;
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

  /* currentIndex
   *   The index of the current active card.
   *
   * currentCard
   *   The current active card object.
   *=====================================*/
  const currentIndex = ref(0); // I dont think this is necessary?
  const currentCard = computed(() => cards[currentIndex.value] || null); // I dont think this is necessary?


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


  return { cards, initializeCards, isLoading,
           fetchCards, removeCard, 
           currentIndex, currentCard,
           xPos, saveXPos, 
           yPos, saveYPos,
           isDragging, setDragging}
})
