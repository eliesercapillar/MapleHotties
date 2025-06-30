<template>
  <section class="w-[40rem] p-4">
    <div class="h-[784.5px] bg-white rounded-lg">
      <Input id="text" type="text" placeholder="Character Name" class="text-black"/>
      <div class="bg-red-600 w-full p-2"></div>
      <div class="flex gap-4 mb-6">
        <Button 
          @click="toggleView(true)"
          :class="[]"
          class="bg-like-gradient"
        >
          Hotties
        </button>
        <Button 
          @click="toggleView(false)"
          :class="[]"
          class="bg-nope-gradient"
        >
          Notties
        </button>
      </div>
    </div>
  </section>
</template>

<script setup lang="ts">
import { onMounted, computed, ref } from "vue";
import { useLeaderboardStore } from "@/stores/leaderboardStore";
import { Input } from '@/components/ui/input'
import Button from "@/components/ui/button/Button.vue";

const leaderboardStore = useLeaderboardStore();

onMounted(async () => { 
  await leaderboardStore.fetchTopLiked(); 
});

const likeCharacters = computed(() => leaderboardStore.likeCharacters);
const nopeCharacters = computed(() => leaderboardStore.nopeCharacters);

async function toggleView(likes: boolean) {
  leaderboardStore.showLikes = likes;
  leaderboardStore.setPage(1); // Reset to first page when switching views
  
  if (likes) await leaderboardStore.fetchTopLiked();
  else       await leaderboardStore.fetchTopNoped();
}
</script>