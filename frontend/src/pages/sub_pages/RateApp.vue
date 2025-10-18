<template>
    <main class="relative w-full h-screen flex flex-col justify-center items-center overflow-hidden">
        <SkeletonCard v-if="swipeStore.isLoading || swipeStore.cards.length == 0"/>
        <div v-else class="relative h-[667px] w-[375px] rounded-lg shadow-md shadow-slate-600">
          <motion.div id="button_anim_bar"
            class="absolute h-[60%] w-[95%] z-[0] rounded-lg bg-[#111418]"
            style="bottom: -14px; left: 8px"
            :animate="{ scale: swipeStore.isDragging ? 0.6 : 1 }"
            :transition="{ duration: 0.3, ease: 'easeInOut' }"
          />
          <div class="grid justify-center items-center">
            <SwipeCard v-for="(card, index) in swipeStore.cards"
              :key="card.character.id"
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
import SkeletonCard from "@/components/rate_app/SkeletonCard.vue";
import SwipeCard from "@/components/rate_app/SwipeCard.vue";
import SwipeCardButtons from "@/components/rate_app/SwipeCardButtons.vue";
import Instructions from "@/components/rate_app/Instructions.vue";
import RateInfoModal from "@/components/rate_app/RateInfoModal.vue";

const swipeStore = useSwipeStore();

onMounted(async () => {
  swipeStore.fetchCards(true); // Initial fetch, get 30 cards instead.
})

watch(() => swipeStore.cards.length, (len) => {
  if (!swipeStore.isLoading && len <= 10) {
    swipeStore.fetchCards()
  }}
)

</script>