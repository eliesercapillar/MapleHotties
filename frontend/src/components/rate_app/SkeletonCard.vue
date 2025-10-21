<template>
  <div id="skeleton_card" class="relative">
    <div class="h-card w-card rounded-lg bg-gradient-to-br from-slate-800 via-slate-700 to-slate-900" />
    <Skeleton 
        class="absolute top-2/4 left-2/4 
               h-[250px] w-[250px]
               bg-fav-gradient"
        style="transform: translate(-50%, -50%);"/>
    <div id="black_gradient" class="absolute bottom-0 h-[30%] w-full rounded-lg bg-black-shadow-fade"/>
    <div class="absolute bottom-[5%] ml-4">
      <div class="flex items-center mb-1">
        <Skeleton class="h-[40px] w-[200px] bg-fav-gradient"/>
        &nbsp;
        <Skeleton class="h-[40px] w-[50px] bg-fav-gradient"/>
      </div>
      <div class="flex items-center mb-1">
        <Skeleton class="h-[30px] w-[100px] bg-fav-gradient"/>
      </div>
      <div class="flex items-center">
        <Skeleton class="h-[30px] w-[100px] bg-fav-gradient"/>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { Skeleton } from '@/components/ui/skeleton'
</script>