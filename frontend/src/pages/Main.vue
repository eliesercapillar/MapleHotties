<template>
  <div class="bg-background min-h-screen flex flex-row">
    <aside class="min-w-80 max-w-96 w-full border-r-[1px] border-[#21262e]">
      <nav class="bg-red-300 flex items-center justify-evenly py-6 h-[--navbar-h]">
        <Button @click="logout">Logout</Button>
        <Button>Leaderboard</Button>
        <Button>CogWheel</Button>
      </nav>
      <div class="bg-blue-300 flex flex-col items-start h-[calc(100vh-var(--navbar-h))]">
        <div class="mx-6 py-2 flex items-center justify-center gap-4">
          <Button variant="ghost"
               :disabled="showingHistory"
               @click="showingHistory = true">Recent History</Button>
          <Button variant="ghost" 
          :disabled="!showingHistory"
          @click="showingHistory = false">Favourites</Button>
        </div>
        <div class="bg-black-grey-radial h-full w-full">
          <div v-if="showingHistory" id="recent_history" class="mx-2 py-4 flex flex-col h-full">
            <div class="grid grid-cols-2 gap-2 flex-grow">
              <Card class="flex flex-col justify-center">
                <CardHeader class="text-center">
                  <CardTitle class="text-md">ROCKOGUY</CardTitle>
                  <CardDescription>290 Adele</CardDescription>
                </CardHeader>
                <img src="/rockoguy.png">
              </Card>
              <Card class="flex flex-col justify-center">
                <CardHeader class="text-center">
                  <CardTitle class="text-md">ROCKOGUY</CardTitle>
                  <CardDescription>290 Adele</CardDescription>
                </CardHeader>
                <img src="/rockoguy.png">
              </Card>
              <Card class="flex flex-col justify-center">
                <CardHeader class="text-center">
                  <CardTitle class="text-md">ROCKOGUY</CardTitle>
                  <CardDescription>290 Adele</CardDescription>
                </CardHeader>
                <img src="/rockoguy.png">
              </Card>
              <Card class="flex flex-col justify-center">
                <CardHeader class="text-center">
                  <CardTitle class="text-md">ROCKOGUY</CardTitle>
                  <CardDescription>290 Adele</CardDescription>
                </CardHeader>
                <img src="/rockoguy.png">
              </Card>
            </div>
            <div class="mx-2 mt-4 flex-shrink-0 flex-grow-0">
              <Button class="w-full">See More</Button>
            </div>
          </div>
          <div v-if="!showingHistory" id="recent_history" class="mx-2 py-4 flex flex-col h-full">
            <div class="grid grid-cols-2 gap-2 flex-grow">
              <Card class="flex flex-col justify-center">
                <CardHeader class="text-center">
                  <CardTitle class="text-md">ROCKOGUY</CardTitle>
                  <CardDescription>290 Adele</CardDescription>
                </CardHeader>
                <img src="/rockoguy.png">
              </Card>
            </div>
            <div class="mx-2 mt-4 flex-shrink-0 flex-grow-0">
              <Button class="w-full">See More</Button>
            </div>
          </div>
        </div>
      </div>
    </aside>
    <div class="w-full h-screen bg-black-grey-radial">
      <main class="relative w-full h-screen flex flex-col justify-center items-center overflow-hidden">
        <SkeletonCard v-if="swipeStore.cards.length == 0"/>
        <div v-else class="relative h-[667px] w-[375px] rounded-lg shadow-md shadow-slate-600">
          <motion.div id="button_anim_bar"
            class="absolute h-[60%] w-[95%] z-[0] rounded-lg bg-[#111418]"
            style="bottom: -14px; left: 8px"
            :animate="{ scale: swipeStore.isDragging ? 0.6 : 1 }"
            :transition="{ duration: 0.3, ease: 'easeInOut' }"
          />
          <div class="grid justify-center items-center">
            <SwipeCard v-for="(card, index) in swipeStore.cards"
              :key="card.id"
              :index="index"
            />
          </div>
          <div id="buttons" class="absolute z-20 isolate w-[375px] bottom-[-2rem]">
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
import { onMounted, watch, ref } from "vue";
import { motion } from "motion-v";
import Button from "@/components/ui/button/Button.vue";
import SkeletonCard from "@/components/SkeletonCard.vue";
import SwipeCard from "@/components/SwipeCard.vue";
import TinderButton from "@/components/TinderButton.vue";
import Instructions from "@/components/Instructions.vue";
import ButtonSVGs from "@/data/ButtonSVGs.json";
import { useSwipeStore } from "@/stores/swipeStore";
import { useHistoryStore } from "@/stores/historyStore";
import router from "@/router"
import {
  Card,
  CardContent,
  CardDescription,
  CardFooter,
  CardHeader,
  CardTitle,
} from '@/components/ui/card'

const showingHistory = ref(true);

const swipeStore = useSwipeStore();
const historyStore = useHistoryStore();

onMounted(async () => {
  swipeStore.fetchCards();
  historyStore.fetchHistory();
})

watch(() => swipeStore.cards.length, (len) => {
  if (!swipeStore.isLoading && len <= 5) {
    swipeStore.fetchCards()
  }}
)

function logout() {
  localStorage.removeItem('token');
  router.push('/')
}

</script>
