<template>
  <div id="skeleton_card" class="">
    <img
      id="card_background"
      src="/bgs/fairy_fountain.png"
      alt="SkeletonCard"
      class="h-[667px] w-[375px] object-cover rounded-lg select-none"
      draggable="false"
    />
    <Skeleton 
        class="absolute top-2/4 left-2/4 
               h-[250px] w-[250px]
               bg-foreground"
        style="transform: translate(-50%, -50%);"/>
    <div 
        class="absolute bottom-[0%] h-[30%] w-full rounded-lg"
        style="background-image: linear-gradient(to top, rgb(0, 0, 0) 40%, rgba(255, 255, 255, 0) 100%);" />
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