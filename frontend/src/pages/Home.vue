<template>
  <main>
    <div class="bg-background">
      <div class="relative w-full h-screen bg-[url(/hero.png)] bg-cover bg-center bg-no-repeat">
        <NavBar :activeIndex="-1" />
        <!-- Gradient overlay -->
        <div class="absolute inset-0 bg-gradient-to-t from-black/70 to-transparent z-10"></div>

        <!-- Hero content -->
        <div id="hero" class="relative z-20 flex flex-col items-center justify-center text-white text-center h-[calc(100vh-4.5rem)] select-none">
          <h1 class="text-9xl font-bold font">MapleHotties</h1>
          <p class="text-xl my-4">The hottest players in Maplestory GMS.</p>
          <Button class="bg-nope-gradient rounded-3xl text-lg p-5" @click="login">Sign in</Button>
        </div>
      </div>
      <Footer/>
    </div>
  </main>
</template>

<script setup lang="ts">
import NavBar from "@/components/NavBar.vue";
import Footer from "@/components/Footer.vue";
import Socials from "@/components/Socials.vue";
import Button from "@/components/ui/button/Button.vue";
import router from "@/router";

function login() {
  router.push("/login")
}

</script>
