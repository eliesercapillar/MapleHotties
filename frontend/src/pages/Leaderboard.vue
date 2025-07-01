<template>
  <div class="relative bg-background min-h-screen flex flex-row text-white">
    <LeaderboardSidebar/>
    <main class="w-full h-screen bg-black-grey-radial text-white flex flex-col items-center justify-center">
      <h1 class="font-medium text-4xl text-center py-4">The Top {{ leaderboardStore.showLikes ? "Hotties" : "Notties" }} of Maplestory GMS</h1>
      
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

async function toggleView(likes: boolean) {
  leaderboardStore.showLikes = likes;
  leaderboardStore.setPage(1); // Reset to first page when switching views
  
  if (likes) await leaderboardStore.fetchTopLiked();
  else       await leaderboardStore.fetchTopNoped();
}

</script>