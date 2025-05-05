<template>
  <div class="bg-background min-h-screen flex flex-row">
    <aside class="min-w-80 max-w-96 w-full border-r-[1px] border-[#21262e]">
      <nav class="bg-red-300 flex items-center justify-evenly py-6 h-[--navbar-h]">
        <Button>1</Button>
        <Button>Leaderboard</Button>
        <Button>CogWheel</Button>
      </nav>
      <div class="bg-blue-300 flex flex-col items-start h-[calc(100vh-var(--navbar-h))]">
        <div class="py-2 flex items-center justify-center">
          <a href="#" class="ml-6">History</a>
          <a href="#" class="ml-6">Favourites</a>
        </div>
        <div class="bg-black-grey-radial h-full w-full"></div>
      </div>
    </aside>
    <div class="w-full h-screen bg-black-grey-radial">
      <main class="relative w-full h-screen flex flex-col justify-center items-center overflow-hidden">
        <div class="relative h-[667px] w-[375px] rounded-lg shadow-md shadow-slate-600">
          <div class="grid justify-center items-center">
            <p v-if="isLoading" class="text-white">Loading...</p>
            <SwipeCard
              v-else
              v-for="card in swipeStore.cards"
              :key="card.id"
              :card="card"
              :isActive="card.id === swipeStore.cards[swipeStore.cards.length - 1].id"
            />
          </div>
          <motion.div
            class="absolute h-[60%] w-[95%] z-[0] rounded-lg bg-[#111418]"
            style="bottom: -14px; left: 8px"
            :animate="{ scale: swipeStore.isDragging ? 0.6 : 1 }"
            :transition="{ duration: 0.3, ease: 'easeInOut' }"
          />
          <div class="absolute z-20 isolate w-[375px] bottom-[-2rem]">
            <div class="flex justify-around items-center">
              <TinderButton
                v-for="(button, index) in ButtonSVGs.data"
                :key="index"
                :button="button"
              />
            </div>
          </div>
        </div>
        <Instructions class="absolute bottom-6" />
      </main>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted } from "vue";
import { motion } from "motion-v";
import Button from "@/components/ui/button/Button.vue";
import SwipeCard from "@/components/SwipeCard.vue";
import TinderButton from "@/components/TinderButton.vue";
import Instructions from "@/components/Instructions.vue";
import ButtonSVGs from "@/data/ButtonSVGs.json";
import { useSwipeStore } from "@/stores/swipeStore";
import Backgrounds from "@/data/Backgrounds.json";

const swipeStore = useSwipeStore();

const isLoading = ref(false)

const page = ref(1);
const pageSize = 10
onMounted(async () => {
  try {
    const response = await fetch(`http://localhost:5051/api/Characters?page=${page.value}&pageSize=${pageSize}`)
    if (!response.ok) throw new Error("Failed to fetch characters")
    const data = await response.json()

    const cards = data.map((character: any, index: number) => ({
      id: character.id,
      bgURL: Backgrounds.data[index % Backgrounds.data.length],
      spriteURL: character.imageUrl
    }))

    swipeStore.initializeCards(cards)
  } 
  catch (error) {
    console.error(error)
  } 
  finally {
    isLoading.value = false
  }
})

</script>
