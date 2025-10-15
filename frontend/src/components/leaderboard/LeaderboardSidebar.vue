<template>
  <aside class="min-w-80 max-w-96 w-full border-r-[1px] border-[#21262e]">
    <nav class="flex items-center justify-evenly py-6 h-[--sidebar-nav-h] bg-nope-gradient">
          <!-- TODO: icon of current page should be set to #ff3f29 -->
      <button class="rounded-full bg-button_primary hover:text-like hover:scale-105 p-3" @click="rate"><Icon icon="bxs:heart" class="scale-[1.25]"/></button>
      <button class="rounded-full bg-button_primary hover:text-nope hover:scale-105 p-3" @click=""><Icon icon="icon-park-solid:game-handle" class="scale-[1.25]"/></button>
      <button class="rounded-full bg-like-gradient p-3"><Icon icon="icon-park-solid:five-star-badge" class="scale-[1.25]"/></button>
      <button class="rounded-full bg-button_primary hover:text-button_highlight hover:scale-105 p-3" @click=""><Icon icon="line-md:log-out" class="scale-[1.25]"/></button>
    </nav>
    <SearchForm/>
  </aside>
</template>

<script setup lang="ts">
import { ref } from "vue";
import { Icon } from "@iconify/vue/dist/iconify.js";
import SearchForm from "./SearchForm.vue";

const emit = defineEmits(['clickRate','clickLeaderboard',])

function rate() {
  emit("clickRate");
}

</script>