<template>
    <main class="relative w-full h-screen flex flex-col justify-center items-center overflow-hidden">
        <SkeletonCard v-if="isInitializing || swipeStore.cards.length == 0"/>
        <div v-else class="relative h-[667px] w-[375px] rounded-lg shadow-md shadow-slate-600">
          <motion.div id="button_anim_bar"
            class="absolute h-[60%] w-[95%] z-[0] rounded-lg bg-[#111418]"
            style="bottom: -14px; left: 8px"
            :animate="{ scale: swipeStore.isDragging ? 0.6 : 1 }"
            :transition="{ duration: 0.3, ease: 'easeInOut' }"
          />
          <div class="grid justify-center items-center">
            <SwipeCard v-for="(card, index) in swipeStore.cards"
              :key="`${card.character.id}-${index}`"
              :index="index"
            />
          </div>
          <SwipeCardButtons/>
        </div>
        <Instructions class="absolute bottom-6" />
        <RateInfoModal/>
      </main>
</template>

<script setup lang="ts">
import { onMounted, watch, ref } from "vue";
import { motion } from "motion-v";
import { useSwipeStore } from "@/stores/swipeStore";
import Backgrounds from "@/data/Backgrounds.json";
import SkeletonCard from "@/components/rate_app/SkeletonCard.vue";
import SwipeCard from "@/components/rate_app/SwipeCard.vue";
import SwipeCardButtons from "@/components/rate_app/SwipeCardButtons.vue";
import Instructions from "@/components/rate_app/Instructions.vue";
import RateInfoModal from "@/components/rate_app/RateInfoModal.vue";

const swipeStore = useSwipeStore();
const isInitializing = ref(true);


onMounted(async () => {
  await Promise.all([
    preloadAllBackgrounds(),
    swipeStore.fetchCards(true) // Initial fetch, start with 30 cards instead.
  ]);
  
  isInitializing.value = false;
})

watch(() => swipeStore.cards.length, (len) => {
  if (!swipeStore.isLoading && len <= 10) {
    swipeStore.fetchCards()
  }}
)

function preloadImage(url: string): Promise<void> {
  return new Promise((resolve, reject) => {
    const img = new Image();
    img.onload = () => resolve();
    img.onerror = () => reject(new Error(`Failed to load image: ${url}`));
    img.src = url;
  });
}

async function preloadAllBackgrounds(): Promise<void> {
  const imagePromises = Backgrounds.data.flatMap(bg => {
    return [
      preloadImage(bg.optimized),
      preloadImage(bg.fallback)
    ];
  });
  
  await Promise.allSettled(imagePromises);
}

</script>