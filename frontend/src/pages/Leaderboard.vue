<template>
  <main class="w-full h-screen bg-black-grey-radial text-white flex flex-col items-center justify-center">
    <h1 class="font-medium text-4xl text-center py-4">The Hottest Characters of the week!</h1>
    
    <div class="flex gap-4 mb-6">
      <Button 
        @click="toggleView(true)"
        :class="[]"
      >
        Most Liked
      </button>
      <Button 
        @click="toggleView(false)"
        :class="[]"
      >
        Most Noped
      </button>
    </div>

    <LeaderboardTable/>
  </main>
</template>
  
<script setup lang="ts">
import { useLeaderboardStore } from "@/stores/leaderboardStore";
import Button from "@/components/ui/button/Button.vue";
import LeaderboardTable from "@/components/leaderboard/LeaderboardTable.vue";

const leaderboardStore = useLeaderboardStore();

async function toggleView(likes: boolean) {
  leaderboardStore.showLikes = likes;
  leaderboardStore.setPage(1); // Reset to first page when switching views
  
  if (likes) await leaderboardStore.fetchTopLiked();
  else       await leaderboardStore.fetchTopNoped();
}

</script>