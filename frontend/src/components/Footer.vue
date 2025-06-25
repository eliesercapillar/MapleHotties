<template>
  <footer class="bg-black-grey-radial h-60 text-white">
    <div class="mx-96 py-8">
      <div class="flex items-center justify-between border-b-2 border-zinc-500 pb-2">
        <div>
          <div class="flex items-center gap-2">
            <img src="/logos/mt_mushroom.png" class="w-10 h-10"/>
            <span class="text-2xl">MapleHotties</span>
          </div>
          <p class="text-md my-2">The hottest players in Maplestory GMS.</p>
          <Socials class="pt-2"/>
        </div>
        <div class="flex flex-col items-start justify-center gap-2">
          <RouterLink to="/faq">FAQ</RouterLink>
          <RouterLink to="/cookies">Cookies</RouterLink>
          <RouterLink to="/privacy">Privacy Policy</RouterLink>
        </div>
      </div>
      <div class="pt-2 flex justify-between">
        <p class="font-thin text-xs">© 2025 maplehotties.com | All rights reserved.</p>
        <div class="text-right">
          <p class="font-thin text-xs">Made with ❤️ by KPJS</p>
          <p class="font-thin text-xs pt-1">Any suggestions? Join the <a href="@" class="text-blue-400">community discord!</a></p>
        </div>
      </div>
      <!-- <div class="flex items-center justify-between border-b-2 border-foreground">
        <Socials />
      </div> -->
    </div>
  </footer>
</template>

<script setup lang="ts">
import Socials from "@/components/Socials.vue";
import { RouterLink } from "vue-router";
</script>