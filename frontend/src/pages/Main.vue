<template>
  <div class="relative bg-background min-h-screen flex flex-row text-white">
    <AppSidebar
      @click-rate="clickRateApp"
      @click-leaderboard="clickLeaderboard"
    />
    <div class="w-full h-full bg-black-grey-radial">
      <RateApp v-if="showRateApp"/>
      <!-- <Leaderboard v-if="showLeaderboard"/> -->
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref } from "vue";
import router from "@/router";
import AppSidebar from '@/components/rate_app/AppSidebar.vue'
import RateApp from "./sub_pages/RateApp.vue";
// import Leaderboard from "./Leaderboard.vue";

const showRateApp = ref(true);
const showLeaderboard = ref(false);
const showGame = ref(false);
const showDetails = ref(false);


function clickRateApp() {
  showRateApp.value = true;
  showLeaderboard.value = false;
  showGame.value = false;
  showDetails.value = false;
}

function clickLeaderboard() {
  router.push("/leaderboard");
}


</script>
