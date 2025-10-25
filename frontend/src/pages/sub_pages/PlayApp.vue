<template>
    <main class="relative w-full h-screen flex justify-evenly items-center overflow-hidden">
      <SkeletonCard v-if="playStore.cards.length === 0"/>
      <motion.div class="w-[calc(100vw/2)] h-full flex items-center justify-center shrink-0"
        v-for="(card, index) in playStore.cards"
        :key="`${card.character.id}-${index}`"
        :style="{ x: x }"
      >
        <PlayCard :index="index"/>
      </motion.div>
      <PlayInfoModal/>
    </main>
</template>

<script setup lang="ts">
import { onMounted, onUnmounted, watch } from "vue";
import { motion, useMotionValue, animate } from 'motion-v'
import { vAutoAnimate } from "@formkit/auto-animate/vue";
import SkeletonCard from "@/components/rate_app/SkeletonCard.vue";
import PlayCard from "@/components/play_app/PlayCard.vue";
import PlayInfoModal from "@/components/play_app/PlayInfoModal.vue";
import { usePlayStore } from "@/stores/playStore";

const ANIMATION_DURATION = 0.35;

const playStore = usePlayStore();

const x = useMotionValue('0%');

onMounted(async () => { 
  window.addEventListener('keydown', handleKeyDown); 
  await playStore.initializeCards();
});

onUnmounted(() => { window.removeEventListener('keydown', handleKeyDown); });

const handleKeyDown = (event: KeyboardEvent) => {
  // Prevent default behavior for arrow keys
  if (['ArrowUp', 'ArrowDown'].includes(event.key)) {
    event.preventDefault();
  }

  switch (event.key) {
    case 'ArrowUp':
      playStore.makeSelection('higher');
      break;
    case 'ArrowDown':
      playStore.makeSelection('lower');
      break;
  }
};

watch(() => playStore.shiftAnimationPending, (trigger) => {
  if (!trigger) return

  animate(x, `-100%`, {
    duration: ANIMATION_DURATION,
    ease: 'easeInOut',
    onComplete: () => { 
      playStore.completeShiftAnimation();
      x.set('0%');
     }
  })
})

</script>