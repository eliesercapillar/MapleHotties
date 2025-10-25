<template>
    <footer class="w-full p-2 text-white">
        <div>
          <div class="flex items-center gap-2">
            <img src="/logos/mt_mushroom.png" class="w-10 h-10"/>
            <span class="text-2xl">MapleHotties</span>
          </div>
          <p class="text-md my-2">The hottest players in Maplestory GMS.</p>
          <Socials class="pt-2"/>
        </div>
        <div class="pt-2">
          <p class="font-thin text-xs">© 2025 maplehotties.com | All rights reserved.</p>
          <div class="pt-1">
            <p class="font-thin text-xs">Made with ❤️ by KPJS</p>
            <p class="font-thin text-xs pt-1">Any suggestions? Join the <a href="@" class="text-blue-400">community Discord server!</a></p>
          </div>
        </div>
      </footer>
</template>

<script setup lang="ts">
import Socials from "@/components/Socials.vue";
</script>