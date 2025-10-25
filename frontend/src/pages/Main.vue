<template>
  <div v-auto-animate class="relative bg-background min-h-screen flex flex-row text-white">
    <BaseSidebar v-if="!playStore.isCurrentlyPlaying">
      <template #content>
        <RateHistory v-if="navigationStore.currentPage === 'Rate'" />
        <PlaySettings v-else-if="navigationStore.currentPage === 'Play'" />
        <SearchForm v-else-if="navigationStore.currentPage === 'Leaderboard'" />
      </template>
    </BaseSidebar>
    <div class="relative w-full h-full bg-black-grey-radial">
      <router-view/>
    </div>
  </div>
</template>

<script setup lang="ts">
import BaseSidebar from "@/components/Sidebar/BaseSidebar.vue";
import { useNavigationStore } from "@/stores/navigationStore";
import { usePlayStore } from "@/stores/playStore";
import RateHistory from "@/components/Sidebar/RateHistorySidebar.vue";
import PlaySettings from "@/components/Sidebar/PlaySettingsSidebar.vue";
import SearchForm from "@/components/Sidebar/SearchFormSidebar.vue";
import { vAutoAnimate } from "@formkit/auto-animate/vue";

const navigationStore = useNavigationStore();
const playStore = usePlayStore();

</script>
