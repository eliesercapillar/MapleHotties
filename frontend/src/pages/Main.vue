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
            <SwipeCard
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
                :isDragging="swipeStore.isDragging"
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
import { onMounted } from "vue";
import { motion } from "motion-v";
import Button from "@/components/ui/button/Button.vue";
import SwipeCard from "@/components/SwipeCard.vue";
import TinderButton from "@/components/TinderButton.vue";
import Instructions from "@/components/Instructions.vue";
import ButtonSVGs from "@/data/ButtonSVGs.json";
import { useSwipeStore } from "@/stores/swipeStore";

const swipeStore = useSwipeStore();

onMounted(() => {
  swipeStore.initializeCards([
    {
    id: 1,
    url:
      "/bgs/kerning_city.png",
    },
    {
      id: 2,
      url:
        "/bgs/leafre.png",
    },
    {
      id: 3,
      url:
        "/bgs/maple_island.png",
    },
    {
      id: 4,
      url:
        "/bgs/florina_beach.png",
    },
    {
      id: 5,
      url:
        "/bgs/partem.png",
    },
    {
      id: 6,
      url:
        "/bgs/ellinia_forest.png",
    },
    {
      id: 7,
      url:
        "/bgs/fairy_fountain.png",
    },
    {
      id: 8,
      url:
        "/bgs/elluel.png",
    },
  ]);
})

</script>
