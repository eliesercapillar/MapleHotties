<template>
  <aside class="min-w-80 max-w-96 w-full border-r-[1px] border-[#21262e]">
    <nav class="flex items-center justify-evenly py-6 h-[--sidebar-nav-h] bg-nope-gradient">
          <!-- TODO: icon of current page should be set to #ff3f29 -->
      <button class="rounded-full bg-button_primary hover:text-like hover:scale-105 p-3" @click=""><Icon icon="bxs:heart" class="scale-[1.25]"/></button>
      <button class="rounded-full bg-button_primary hover:text-nope hover:scale-105 p-3" @click=""><Icon icon="icon-park-solid:game-handle" class="scale-[1.25]"/></button>
      <button class="rounded-full bg-button_primary hover:text-favourite hover:scale-105 p-3" @click=""><Icon icon="icon-park-solid:five-star-badge" class="scale-[1.25]"/></button>
      <button class="rounded-full bg-button_primary hover:text-button_highlight hover:scale-105 p-3" @click=""><Icon icon="line-md:log-out" class="scale-[1.25]"/></button>
    </nav>
    <div class="bg-[#111418] flex flex-col items-start">
      <div class="w-full mt-2 p-2 flex items-center justify-evenly gap-4">
        <span class="font-bold text-md">Sort by:</span>
        <div class="flex items-center justify-evenly gap-2">
          <Button class="bg-like-gradient" @click="toggleView(true)">Hotties</Button>
          <Button class="bg-nope-gradient" @click="toggleView(false)">Notties</Button>
        </div>
      </div>
      <div id="search" class="w-full p-2">
        <Input type="text" placeholder="Search for a character..." class="text-black"/>
      </div>
      <div id="filters" class="w-full p-2">
        <span class="font-bold text-md">Time:</span>
        <RadioGroup default-value="all" class="flex justify-evenly mt-2">
          <div class="flex items-center gap-2">
            <RadioGroupItem id="r1" value="weekly" class="border-white text-white"/>
            <Label for="r1">Weekly</Label>
          </div>
          <div class="flex items-center space-x-2">
            <RadioGroupItem id="r2" value="monthly" class="border-white text-white"/>
            <Label for="r2">Monthly</Label>
          </div>
          <div class="flex items-center space-x-2">
            <RadioGroupItem id="r3" value="all" class="border-white text-white"/>
            <Label for="r3">All Time</Label>
          </div>
        </RadioGroup>
      </div>
    </div>
  </aside>
</template>

<script setup lang="ts">
import { useLeaderboardStore } from "@/stores/leaderboardStore";
import { Input } from '@/components/ui/input'
import { Icon } from "@iconify/vue/dist/iconify.js";
import Button from "@/components/ui/button/Button.vue";
import Checkbox from "../ui/checkbox/Checkbox.vue";
import { Label } from '@/components/ui/label'
import { RadioGroup, RadioGroupItem } from '@/components/ui/radio-group'
import router from "@/router"

const leaderboardStore = useLeaderboardStore();

async function toggleView(likes: boolean) {
  leaderboardStore.showLikes = likes;
  leaderboardStore.setPage(1); // Reset to first page when switching views
  
  if (likes) await leaderboardStore.fetchTopLiked();
  else       await leaderboardStore.fetchTopNoped();
}
</script>