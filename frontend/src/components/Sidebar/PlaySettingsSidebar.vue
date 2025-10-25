<template>
  <div class="bg-background h-[calc(100vh-var(--sidebar-nav-h))] w-full flex flex-col justify-between px-2">
    <div class="p-2 flex flex-col">
      <h2 class="font-bold text-lg">Play Higher or Lower</h2>
      <div class="text-md my-2 flex flex-col justify-between gap-2"> 
        <span>A game of higher or lower based on character ratings determined by the community!</span>
        <span>For instructions, click the information button on the bottom right.</span>
      </div>
      <div class="flex justify-evenly my-2">
        <Button @click="playStore.startClassic" class="bg-nope-gradient font-bold text-md">Classic<Icon icon="ion:play"/></Button>
        <Button @click="playStore.startTimed" class="bg-nope-gradient font-bold text-md">Timed<Icon icon="ion:play"/></Button>
      </div>
      <div class="mt-4">
        <p class="font-bold text-md">High Score: <span class="font-normal">{{ playStore.highScore }}</span></p>
      </div>
    </div>
    <FooterSidebar/>
  </div>
</template>

<script setup lang="ts">
import Button from "@/components/ui/button/Button.vue";
import { Icon } from '@iconify/vue/dist/iconify.js';
import FooterSidebar from '@/components/Sidebar/FooterSidebar.vue';
import { usePlayStore } from "@/stores/playStore";

const playStore = usePlayStore();


</script>