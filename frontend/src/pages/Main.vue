<template>
  <div class="relative bg-background min-h-screen flex flex-row text-white">
    <aside class="min-w-80 max-w-96 w-full border-r-[1px] border-[#21262e]">
      <nav class="flex items-center justify-evenly py-6 h-[--navbar-h]"
           style="background: linear-gradient(135deg, #f92999, #ff3f29)">
        <button class="rounded-full bg-[#21262e] p-3" @click="rate"><Icon icon="bxs:heart" class="scale-[1.25]"/></button>
        <button class="rounded-full bg-[#21262e] p-3" @click="play"><Icon icon="icon-park-solid:game-handle" class="scale-[1.25]"/></button>
        <button class="rounded-full bg-[#21262e] p-3" @click="leaderboard"><Icon icon="icon-park-solid:five-star-badge" class="scale-[1.25]"/></button>
        <button class="rounded-full bg-[#21262e] p-3" @click="logout"><Icon icon="line-md:log-out" class="scale-[1.25]"/></button>
      </nav>
      <div class="bg-[#111418] h-[calc(100vh-var(--navbar-h))] flex flex-col items-start">
        <div class="mx-6 py-2 flex items-center justify-center gap-4 font-bold text-md">
          <div class="">
            <button class="px-2">History</button>
            <hr class="bg-[#ff4458] h-[3px] border-0 mt-1">
          </div>
          <div class="">
            <button class="px-2">Favourites</button>
            <!-- <hr class="bg-[#ff4458] h-[3px] border-0 mt-1"> -->
          </div>
        </div>
        <div class="h-full w-full">
          <div class="mx-2 py-6 h-full">
            <RecentHistory v-if="showingHistory"/>
            <RecentFavourites v-else/>
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
              :key="card.character.id"
              :index="index"
            />
          </div>
          <SwipeCardButtons/>
        </div>
        <Instructions class="absolute bottom-6" />
        <button class="absolute bottom-6 right-6 scale-[1.5]" @click="info"><Icon icon="lucide:info"/></button>
      </main>
    </div>
  </div>
</template>

<script setup lang="ts">
import { onMounted, watch, ref } from "vue";
import { motion } from "motion-v";
import { Icon } from "@iconify/vue/dist/iconify.js";
import RecentHistory from "@/components/RecentHistory.vue";
import RecentFavourites from "@/components/RecentFavourites.vue";
import SkeletonCard from "@/components/SkeletonCard.vue";
import SwipeCard from "@/components/SwipeCard.vue";
import Instructions from "@/components/Instructions.vue";
import { useSwipeStore } from "@/stores/swipeStore";
import router from "@/router"
import SwipeCardButtons from "@/components/tinder/SwipeCardButtons.vue";

const showingHistory = ref(true);

const swipeStore = useSwipeStore();

onMounted(async () => {
  swipeStore.fetchCards();
})

watch(() => swipeStore.cards.length, (len) => {
  if (!swipeStore.isLoading && len <= 5) {
    swipeStore.fetchCards()
  }}
)

function rate() {

}

function play() {

}

function info() {

}

function logout() {
  localStorage.removeItem('token');
  router.push('/')
}

function leaderboard() {
  router.push('/leaderboard')
}

</script>
