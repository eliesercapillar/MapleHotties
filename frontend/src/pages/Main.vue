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
          <div>
            <button class="">Recent History</button>
            <hr class="bg-[#ff4458]">
          </div>
          <Button variant="ghost"
               :disabled="showingHistory"
               @click="showingHistory = true">Recent History</Button>
          <Button variant="ghost" 
          :disabled="!showingHistory"
          @click="showingHistory = false">Favourites</Button>
        </div>
        <div class="bg-black-grey-radial h-full w-full">
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
      </main>
    </div>
  </div>
</template>

<script setup lang="ts">
import { onMounted, watch, ref } from "vue";
import { motion } from "motion-v";
import Button from "@/components/ui/button/Button.vue";
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

function logout() {
  localStorage.removeItem('token');
  router.push('/')
}

</script>
