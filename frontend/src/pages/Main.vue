<template>
  <div class="relative bg-background min-h-screen flex flex-row text-white">
    <AppSidebar/>
    <div class="w-full h-full bg-black-grey-radial">
      <RateApp/>
    </div>
  </div>
</template>

<script setup lang="ts">
import RateApp from "./sub_pages/RateApp.vue";
import AppSidebar from '@/components/AppSidebar.vue'

</script>
