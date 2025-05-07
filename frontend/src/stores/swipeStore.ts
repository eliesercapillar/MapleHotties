import { defineStore } from 'pinia'
import { ref, computed } from 'vue'

interface CharacterCard 
{
  id: number;
  bgURL: string;
  spriteURL: string;
  info: CharacterInfo;
}

interface CharacterInfo {
  ranking: number;
  name: string;
  level: number;
  job: string;
  world: string;
}

export const useSwipeStore = defineStore('swipe', () => 
{
  /* cards
   *   The stack of cards. Should always be populated.
   *   initializeCards @params - cardList : Card[]
   *   removeCard      @params - id : number
   *=====================================*/
  const cards = ref([] as CharacterCard[]);
  function initializeCards(cardList : CharacterCard[]) {
    cards.value = cardList;
    currentIndex.value = 0
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


  return { cards, initializeCards, removeCard,
           currentIndex, currentCard,
           xPos, saveXPos, 
           yPos, saveYPos,
           isDragging, setDragging}
})
