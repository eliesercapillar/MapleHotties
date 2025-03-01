<template>
  <main>
    <Analytics />
    <div>
      <!-- <RippleCircleBG/> -->
      <div>
        <NavBar :activeIndex="0" />
        <!-- <NavBarAlt /> -->
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
import NavBarAlt from "@/components/NavBarAlt.vue";
</script>
