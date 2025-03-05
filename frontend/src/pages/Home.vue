<template>
  <main>
    <Analytics />
    <div>
      <!-- <RippleCircleBG/> -->
      <div>
        <NavBar :activeIndex="0"/>
        <!-- <NavBarAlt /> -->
        <!-- Hero Section -->
        <section>
          <div class="relative">
            <img src="/hero_bg.png" class="block w-full">
            <div class="w-full absolute top-0 left-0 text-center">
              <h1 class="relative text-[#ff0000] text-center text-9xl font-bold" >ARE YOU HENEHOE ENOUGH?</h1>
            </div>
          </div>
        </section>
        <HeroSection />
        <Technologies />
        <Experience />
        <FeaturedProjects />
        <Footer />
      </div>
    </div>
  </main>
</template>

<script setup lang="ts">
import NavBar from "@/components/NavBar.vue";
</script>
