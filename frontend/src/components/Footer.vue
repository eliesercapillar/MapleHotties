<template>
  <footer class="bg-black-grey-radial h-64 text-white">
    <div class="mx-32 py-8">
      <div class="flex items-center justify-between border-b-2 border-zinc-500 pb-2">
        <div>
          <div class="flex items-center gap-2">
            <img src="/logos/mt_mushroom.png" class="w-10 h-10"/>
            <span class="text-2xl">MapleHotties</span>
          </div>
          <p class="text-md my-2">The hottest players in Maplestory GMS.</p>
          <Socials class="pt-6"/>
        </div>
        <div class="flex flex-col items-start justify-center gap-2">
          <RouterLink to="/faq">FAQ</RouterLink>
          <RouterLink to="/privacy">Privacy Policy</RouterLink>
        </div>
        <div>
          <RouterLink to="/">FAQ</RouterLink>
        </div>
        <div>
          <RouterLink to="/">FAQ</RouterLink>
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