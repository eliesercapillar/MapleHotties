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
          <div :class="`bg-[url(${heroBGPath})]`" class="h-screen flex justify-center items-center">
            <div class="grid space-y-2">
              <h1 class="text-[#fff500] text-center text-9xl font-bold capitalize" >Make The Next Move</h1>
              <p class="text-center">It's better on the app.</p>
              <div class="grid justify-center items-center space-y-2">
                <Button class="">Download now</Button>
                <Button class="">Sign In</Button>
              </div>
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
import Button from "@/components/ui/button/Button.vue";
import { ref } from "vue";

const heroBGPath = ref("/hero_bg.png");
</script>
