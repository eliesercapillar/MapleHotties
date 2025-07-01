<template>
  <aside class="min-w-80 max-w-96 w-full border-r-[1px] border-[#21262e]">
    <nav class="flex items-center justify-evenly py-6 h-[--sidebar-nav-h] bg-nope-gradient">
          <!-- TODO: icon of current page should be set to #ff3f29 -->
      <button class="rounded-full bg-button_primary hover:text-like hover:scale-105 p-3" @click="rate"><Icon icon="bxs:heart" class="scale-[1.25]"/></button>
      <button class="rounded-full bg-button_primary hover:text-nope hover:scale-105 p-3" @click="play"><Icon icon="icon-park-solid:game-handle" class="scale-[1.25]"/></button>
      <button class="rounded-full bg-button_primary hover:text-favourite hover:scale-105 p-3" @click="leaderboard"><Icon icon="icon-park-solid:five-star-badge" class="scale-[1.25]"/></button>
      <button class="rounded-full bg-button_primary hover:text-button_highlight hover:scale-105 p-3" @click="logout"><Icon icon="line-md:log-out" class="scale-[1.25]"/></button>
    </nav>
    <div class="bg-[#111418] flex flex-col items-start">
      <div class="h-[--sidebar-options-h] ml-6 py-2 flex items-center justify-center gap-4 font-bold text-md">
        <div id="history_button">
          <button class="px-2 mb-[2px]" @click="showingHistory = true">History</button>
          <hr 
            aria-hidden="true" 
            class="h-[3px] m-0 border-0 transition-all duration-300 ease-in-out bg-button_highlight"
            :class="showingHistory ? 'translate-x-0 opacity-100' : 'translate-x-full opacity-0'"
          >
        </div>
        <div id="favourite_button">
          <button class="px-2 mb-[2px]" @click="showingHistory = false">Favourites</button>
          <hr 
            aria-hidden="true" 
            class="h-[3px] m-0 border-0 transition-all duration-300 ease-in-out bg-button_highlight"
            :class="!showingHistory ? 'translate-x-0 opacity-100' : '-translate-x-full opacity-0'"
          >
        </div>
      </div>
      <div class="w-full">
        <div v-auto-animate class="ml-2">
          <RecentHistory v-if="showingHistory"/>
          <RecentFavourites v-else/>
        </div>
      </div>
    </div>
  </aside>
</template>

<script setup lang="ts">
import { ref } from "vue";
import router from "@/router"
import { Icon } from "@iconify/vue/dist/iconify.js";
import RecentHistory from "@/components/RecentHistory.vue";
import RecentFavourites from "@/components/RecentFavourites.vue";
import { vAutoAnimate } from "@formkit/auto-animate/vue";

const emit = defineEmits(['clickRate','clickLeaderboard',])

const showingHistory = ref(true);



function rate() {
  emit("clickRate");
}

function play() {

}

function logout() {
  localStorage.removeItem('token');
  router.push('/')
}

function leaderboard() {
  emit("clickLeaderboard");
}

</script>