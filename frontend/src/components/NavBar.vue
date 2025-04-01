<template>
    <header class="z-50 py-6 bg-foreground sticky top-0">
      <div class="mx-5">
        <div class="flex justify-between items-center">
          <!-- Logo -->
          <div>
            <RouterLink to="/">
              <Icon icon="lucide:aperture" class="size-5 text-white" />
            </RouterLink>
          </div>
          <!-- Mobile Hamburger Button -->
          <div v-if="!isDesktopView">
            <Button @click="isMenuOpen = !isMenuOpen">
              <Icon :icon="isMenuOpen ? 'lucide:x' : 'lucide:align-justify'"/>
            </Button>
          </div>
          <nav v-if="isDesktopView" class="">
            <ul class="flex items-center gap-4 sm:gap-8">
              <li
                v-for="(page, index) in Pages.data"
                :key="page.name"
                :class="['text-muted-foreground hover:opacity-50']"
              >
                <RouterLink v-if="index !== activeIndex" :to="page.href">{{
                  page.name
                }}</RouterLink>
                <span v-else>{{ page.name }}</span>
              </li>
              <!-- TODO: Add V-IF to check if logged in. If logged in, show profile page link instead -->
              <li>
                <RouterLink to="/login" class="text-muted-foreground hover:opacity-50"
                  >Login</RouterLink
                >
              </li>
            </ul>
          </nav>
        </div>
      </div>
      <nav v-if="!isDesktopView && isMenuOpen" class="m-5 font-bold text-4xl flex flex-col justify-between h-screen">
        <ul class="flex flex-col gap-4">
          <li v-for="(page, index) in Pages.data" :key="page.name"
          class="text-muted-foreground hover:opacity-50"
          >
            <RouterLink v-if="index !== activeIndex" :to="page.href">{{ page.name }}</RouterLink>
            <span v-else>{{ page.name }}</span>
          </li>
          <!-- TODO: Add V-IF to check if logged in. If logged in, show profile page link instead -->
          <li class="text-muted-foreground hover:opacity-50">
            <RouterLink to="/login">Login</RouterLink>
          </li>
        </ul>
        <Socials/>
      </nav>
    </header>
</template>

<script setup lang="ts">
import Socials from "@/components/Socials.vue";
import { Icon } from "@iconify/vue";
import { Button } from "@/components/ui/button";
import { ref, onMounted, onUnmounted } from "vue";
import { RouterLink } from "vue-router";
import Pages from "@/data/Pages.json";

const isMenuOpen = ref(false);
const isDesktopView = ref(false);

defineProps<{
  activeIndex?: number; // Optional prop (defaults to -1)
}>();

function getMobileIcon() : string {
  return isMenuOpen ? "lucide:x" : "lucide:align-justify";
}

const handleMediaChange = (event: MediaQueryListEvent) => {
  console.log(`Handling media change: ${event.media}`)
  isDesktopView.value = event.matches; // `matches` is true for desktop, false for mobile
  console.log("isMobileview is now: " + isDesktopView.value);

  if (isDesktopView.value) {
    isMenuOpen.value = false; // Close menu when switching to desktop
  }
};

onMounted(() => {
  const mediaQuery = window.matchMedia("(min-width: 768px)");
  
  isDesktopView.value = mediaQuery.matches; // Set inital state

  mediaQuery.addEventListener("change", handleMediaChange);
});

onUnmounted(() => {
  const mediaQuery = window.matchMedia("(min-width: 768px)");
  mediaQuery.removeEventListener("change", handleMediaChange);
});


</script>
