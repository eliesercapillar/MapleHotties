<template>
    <div class="min-h-screen">
      <div class="mx-auto min-h-screen max-w-3xl px-8 grid grid-rows-[auto_1fr_auto]">
        <NavBar :activeIndex="-1"/>
        <div class="p-16 flex flex-col justify-center gap-10">
          <div class="flex items-center justify-center gap-4">
            <p class="text-3xl md:text-7xl opacity-60">404</p>
            <span class="text-3xl md:text-7xl opacity-60"> | </span>
            <p class="text-3xl md:text-7xl opacity-60">¯\_(ツ)_/¯</p>
          </div>
          <div class="flex flex-col items-center gap-4">
            <p class="text-lg md:text-3xl opacity-60 text-center">Can't seem to find this page...</p>
            <a href="/" class="flex items-center gap-2 text-muted-foreground hover:text-foreground">
                <Icon icon="lucide:arrow-left" class="size-7"/>
                <p class="font-light text-lg md:text-3xl">Back to Home</p>
            </a>
          </div>
        </div>
        <Footer/>
      </div>
    </div>
  </template>

<script setup lang="ts">
</script>