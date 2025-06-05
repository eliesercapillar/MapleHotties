<template>
  <div id="skeleton_card" class="relative">
    <img
      id="card_background"
      src="/bgs/fairy_fountain.png"
      alt="SkeletonCard"
      class="relative h-card w-card object-cover rounded-lg select-none"
      draggable="false"
    />
    <Skeleton 
        class="absolute top-2/4 left-2/4 
               h-[250px] w-[250px]
               bg-foreground"
        style="transform: translate(-50%, -50%);"/>
    <div id="black_gradient" class="absolute bottom-0 h-[30%] w-full rounded-lg bg-black-shadow-fade"/>
    <div class="absolute bottom-[5%] ml-2">
      <div class="flex items-center mb-1">
        <Skeleton class="h-[30px] w-[150px] bg-foreground"/>
        &nbsp;
        <Skeleton class="h-[30px] w-[50px] bg-foreground"/>
      </div>
      <div class="flex items-center mb-1">
        <Skeleton class="h-[30px] w-[50px] bg-foreground"/>
      </div>
      <div class="flex items-center">
        <Skeleton class="h-[30px] w-[50px] bg-foreground"/>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { Skeleton } from '@/components/ui/skeleton'
</script>