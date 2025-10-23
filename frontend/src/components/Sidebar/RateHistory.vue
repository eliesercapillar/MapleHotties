<template>
  <div class="bg-background flex flex-col items-start">
    <div class="h-[--sidebar-options-h] ml-6 py-2 flex items-center justify-center gap-4 font-bold text-md">
      <div id="history_button">
        <button class="px-2 mb-[2px]" @click="display = 'history'">History</button>
        <hr 
          aria-hidden="true" 
          class="h-[3px] m-0 border-0 transition-all duration-300 ease-in-out bg-button_highlight"
          :class="display == 'history' ? 'translate-x-0 opacity-100' : 'translate-x-full opacity-0'"
        >
      </div>
      <div id="favourite_button">
        <button class="px-2 mb-[2px]" @click="display = 'favourites'">Favourites</button>
        <hr 
          aria-hidden="true" 
          class="h-[3px] m-0 border-0 transition-all duration-300 ease-in-out bg-button_highlight"
          :class="display == 'favourites' ? 'translate-x-0 opacity-100' : '-translate-x-full opacity-0'"
        >
      </div>
    </div>
    <div class="w-full">
      <div v-auto-animate class="ml-2">
        <RecentCharactersDisplay :display='display'/>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref } from "vue";
import RecentCharactersDisplay from "@/components/rate_app/RecentCharactersDisplay.vue";
import { vAutoAnimate } from "@formkit/auto-animate/vue";

const display = ref('history');
</script>