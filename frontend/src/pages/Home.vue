<template>
  <main>
    <div class="bg-background">
      <NavBar :activeIndex="0" />
      <!-- Hero Section -->
      <section>
        <div
          :class="`bg-[url(/hero_bg_temp.png)] bg-no-repeat bg-cover`"
          class="h-[calc(100vh-4.5rem)] flex justify-center items-center"
        >
          <div class="grid gap-2">
            <h1 class="text-[#fff500] text-center text-9xl font-bold capitalize">
              Make The Next Move
            </h1>
            <p class="text-center">It's better on the app.</p>
            <div class="grid justify-center items-center gap-2">
              <Button class="">Download now</Button>
              <Button class="">Sign In</Button>
            </div>
          </div>
        </div>
      </section>
      <footer>
        <div class="mx-32 pt-8">
          <div class="flex items-center justify-between border-b-2 border-foreground">
            <Socials />
            <div>
              <div class="flex items-center justify-center">
                <a href="#">
                  <img
                    alt="Download on the App Store"
                    loading="lazy"
                    width="120"
                    height="40"
                    decoding="async"
                    data-nimg="1"
                    style="color: transparent"
                    src="https://bumble.com/bumble-brand-assets/bumble/l-assets/en/appstore-badge.svg"
                  />
                </a>
                <a href="#">
                  <img
                    alt="Get it on Google Play"
                    loading="lazy"
                    width="151"
                    height="40"
                    decoding="async"
                    data-nimg="1"
                    style="color: transparent"
                    src="https://bumble.com/_next/image/?url=%2Fbumble-brand-assets%2Fbumble%2Fl-assets%2Fen%2Fgoogle-play-badge.png&amp;w=320&amp;q=75"
                  />
                </a>
              </div>
              <p class="text-foreground text-[10px]">
                Apple and the Apple logo are trademarks of Apple Inc.
                <br />
                Google Play is a trademark of Google LLC.
              </p>
            </div>
          </div>
          <div class="grid space-y-2 border-b-8 border-white">
            <p class="text-center">It's better on the app.</p>
            <div class="grid justify-center items-center space-y-2">
              <Button class="">Download now</Button>
              <Button class="">Sign In</Button>
            </div>
          </div>
        </div>
      </footer>
    </div>
  </main>
</template>

<script setup lang="ts">
import NavBar from "@/components/NavBar.vue";
import Socials from "@/components/Socials.vue";
import Button from "@/components/ui/button/Button.vue";
import { ref } from "vue";

const heroBGPath = ref("/hero_bg_temp.png");
</script>
