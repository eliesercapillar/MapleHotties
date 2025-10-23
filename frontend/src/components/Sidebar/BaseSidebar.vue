<template>
  <aside class="min-w-80 max-w-96 w-full border-r-[1px] border-border">
    <nav class="flex items-center justify-evenly py-6 h-[--sidebar-nav-h] bg-nope-gradient">
      <button 
        class="rounded-full bg-background p-3" 
        :class="navigationStore.currentPage === 'Rate' ? 'bg-like-gradient' : 'hover:text-like hover:scale-105'"
        @click="navigationStore.navigateToRate()"
        :disabled="navigationStore.currentPage === 'Rate'"
      >
        <Icon icon="bxs:heart" class="scale-[1.25]"/>
      </button>
      <button 
        class="rounded-full bg-background p-3" 
        :class="navigationStore.currentPage === 'Play' ? 'bg-purp-gradient': 'hover:text-purp hover:scale-105'"
        @click="navigationStore.navigateToPlay()"
        :disabled="navigationStore.currentPage === 'Play'"
      >
        <Icon icon="icon-park-solid:game-handle" class="scale-[1.25]"/>
      </button>
      <button 
        class="rounded-full bg-background p-3" 
        :class="navigationStore.currentPage === 'Leaderboard' ? 'bg-fav-gradient': 'hover:text-favourite hover:scale-105'"
        @click="navigationStore.navigateToLeaderboard()"
        :disabled="navigationStore.currentPage === 'Leaderboard'"
      >
        <Icon icon="icon-park-solid:five-star-badge" class="scale-[1.25]"/>
      </button>
      <button 
        class="rounded-full bg-background p-3 hover:text-button_highlight hover:scale-105" 
        @click="navigationStore.logout()">
        <Icon icon="line-md:log-out" class="scale-[1.25]"/>
      </button>
    </nav>
    <slot name="content"></slot>
  </aside>
</template>

<script setup lang="ts">
import { Icon } from "@iconify/vue/dist/iconify.js";
import { useNavigationStore } from "@/stores/navigationStore";

const navigationStore = useNavigationStore();
</script>