<template>
  <div class="relative bg-background min-h-screen flex flex-row text-white">
    <LeaderboardSidebar/>
    <main class="w-full h-screen bg-black-grey-radial text-white flex flex-col items-center justify-center">
      <h1 v-if="isCurrentTimeRange('all')" class="font-medium text-4xl text-center py-4">The All Time {{ leaderboardStore.getRankingType() }} of Maplestory GMS</h1>
      <h1 v-else-if="isCurrentTimeRange('monthly')" class="font-medium text-4xl text-center py-4">The {{ getCurrentMonth() }} {{ leaderboardStore.getRankingType() }} of Maplestory GMS</h1>
      <h1 v-else-if="isCurrentTimeRange('weekly')" class="font-medium text-4xl text-center py-4">The Weekly {{ leaderboardStore.getRankingType() }} of Maplestory GMS</h1>
      <div class="flex gap-4 ">
        <LeaderboardTable/>
      </div>

    </main>
  </div>
</template>
  
<script setup lang="ts">
import { useLeaderboardStore } from "@/stores/leaderboardStore";
import Button from "@/components/ui/button/Button.vue";
import LeaderboardTable from "@/components/leaderboard/LeaderboardTable.vue";
import LeaderboardSidebar from "@/components/leaderboard/LeaderboardSidebar.vue";

const leaderboardStore = useLeaderboardStore();

function isCurrentTimeRange(type : string) : boolean
{
  return leaderboardStore.searchSettings.timeType == type;
} 

function getCurrentMonth() : string
{
  return "October"
}


</script>