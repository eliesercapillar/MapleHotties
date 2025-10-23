<template>
  <div class="relative bg-background min-h-screen flex flex-row text-white">
    <BaseSidebar>
      <template #content>
        <RateHistory v-if="navigationStore.currentPage === 'Rate'" />
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
import RateHistory from "@/components/Sidebar/RateHistory.vue";
import SearchForm from "@/components/Sidebar/SearchForm.vue";

const navigationStore = useNavigationStore();
// import Leaderboard from "./Leaderboard.vue";


</script>
