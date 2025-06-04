<template>
  <aside class="min-w-80 max-w-96 w-full border-r-[1px] border-[#21262e]">
      <nav class="flex items-center justify-evenly py-6 h-[--sidebar-nav-h]"
           style="background: linear-gradient(135deg, #f92999, #ff3f29)">
           <!-- TODO: icon of current page should be set to #ff3f29 -->
        <button class="rounded-full bg-[#21262e] hover:text-[#ff3f29] p-3" @click="rate"><Icon icon="bxs:heart" class="scale-[1.25]"/></button>
        <button class="rounded-full bg-[#21262e] hover:text-[#ff3f29] p-3" @click="play"><Icon icon="icon-park-solid:game-handle" class="scale-[1.25]"/></button>
        <button class="rounded-full bg-[#21262e] hover:text-[#ff3f29] p-3" @click="leaderboard"><Icon icon="icon-park-solid:five-star-badge" class="scale-[1.25]"/></button>
        <button class="rounded-full bg-[#21262e] hover:text-[#ff3f29] p-3" @click="logout"><Icon icon="line-md:log-out" class="scale-[1.25]"/></button>
      </nav>
      <div class="bg-[#111418] h-[calc(100vh-var(--sidebar-nav-h))] flex flex-col items-start">
        <div class="h-[--sidebar-options-h] mx-6 py-2 flex items-center justify-center gap-4 font-bold text-md">
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
          <div class="ml-2 h-full">
            <RecentHistory v-if="showingHistory"/>
            <RecentFavourites v-else/>
          </div>
        </div>
      </div>
    </aside>
</template>

<script setup lang="ts">
import { onMounted, watch, ref } from "vue";
import router from "@/router"
import { Icon } from "@iconify/vue/dist/iconify.js";
import RecentHistory from "@/components/RecentHistory.vue";
import RecentFavourites from "@/components/RecentFavourites.vue";

const showingHistory = ref(true);

function rate() {

}

function play() {

}

function logout() {
  localStorage.removeItem('token');
  router.push('/')
}

function leaderboard() {
  router.push('/leaderboard')
}

</script>