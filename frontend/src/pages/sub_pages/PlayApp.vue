<template>
    <main class="relative w-full h-screen flex flex-col items-center justify-evenly overflow-hidden">
      <div id="description">
        <h1>Higher or Lower</h1>
        <p> A game of higher or lower based on character ratings determined by the community!</p>

      </div>
      <h3>How to Play</h3>
      <div id="instructions" class="grid grid-rows-2 grid-cols-2 items-center justify-center">
        <div >
          <span>Given two characters, decide whether or not the card on the right has a higher or lower All-time <span class="text-like">Like Rating</span> than the card on the left.</span>
          <div class="flex items-center justify-center select-none">
            <InfoCharacterPlayCard 
              :static="true"
              :name="'Xæra'"
              :level="285"
              :job="'Xenon'"
              :image-url="'/rockoguy.png'"
            />
            <InfoCharacterPlayCard 
              :static="false"
              :name="'DWlGHT'"
              :level="290"
              :job="'Dual Blade'"
              :image-url="'/rockoguy.png'"
            />
            <div class="flex flex-col">
              <span>More!</span>
              <span>Less!</span>
            </div>
          </div>
        </div>
        <div>
          <span>Lock in your answer by clicking the top or bottom half of the card on the right, or using the up or down arrow keys.</span>
        </div>
        <div>
          <span>New characters will be shown as you progress, with each being compared to the previous card.</span>
        </div>
        <div>
          <span>Try to get as many correct as you can. The game ends when you get one wrong.</span>
        </div>
      </div>
      <span class="text-slate-500 text-xs italic">
          All-Time Ratings are determined by the <RouterLink class="text-blue-400" to="/app/leaderboard">community</RouterLink> and may change between play sessions.
      </span>
      <!-- <SkeletonCard v-if="playStore.cards.length === 0"/>
      <motion.div class="w-[calc(100vw/2)] h-full flex items-center justify-center shrink-0"
        v-for="(card, index) in playStore.cards"
        :key="`${card.character.id}-${index}`"
        :style="{ x: x }"
      >
        <PlayCard :index="index"/>
      </motion.div>
      <motion.div v-if="playStore.isCurrentlyPlaying" class="absolute select-none grid items-center justify-center top-1/2 -translate-y-1/2 lg:w-32 lg:h-32"
        :style="{ scale: scale }"
      >
        <Icon class="row-[1] col-[1] z-0 w-full h-full" icon="carbon:circle-solid"/>
        <span class="row-[1] col-[1] z-[1] text-black text-5xl text-center">VS</span>
      </motion.div> -->
    </main>
    <PlayInfoModal/>
</template>

<script setup lang="ts">
import { onMounted, onUnmounted, watch } from "vue";
import { motion, useMotionValue, animate } from 'motion-v'
import InfoCharacterPlayCard from "@/components/play_app/InfoCharacterPlayCard.vue"
import CharacterSummaryCard from "@/components/CharacterSummaryCard.vue";
import SkeletonCard from "@/components/rate_app/SkeletonCard.vue";
import PlayCard from "@/components/play_app/PlayCard.vue";
import PlayInfoModal from "@/components/play_app/PlayInfoModal.vue";
import { Icon } from "@iconify/vue/dist/iconify.js";
import { usePlayStore } from "@/stores/playStore";

const CARD_SHIFT_ANIMATION_DURATION = 0.35;
const VS_ICON_SCALE_ANIMATION_DURATION = 0.20;

const playStore = usePlayStore();

const x = useMotionValue('0%');
const scale = useMotionValue(0);

onMounted(async () => { 
  window.addEventListener('keydown', handleKeyDown); 
  await playStore.initializeCards();
});

onUnmounted(() => { window.removeEventListener('keydown', handleKeyDown); });

const handleKeyDown = (event: KeyboardEvent) => 
{
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

// Play these animations when a new game starts
watch(() => playStore.isCurrentlyPlaying, async (trigger) =>
{
  if (!trigger) return;

  await animateIconScale('show');
})

// Play these animations when the user makes a selection
watch(() => playStore.userHasSelected, (trigger) => 
{
  if (!trigger) return;

  startAnimations();
})

async function startAnimations()
{
  await animateIconScale('show');
  await animateCardShift();
  // await any other animations;
  // await ...
  // await ...
  playStore.allAnimationsFinished();
}

async function animateIconScale(action : 'show' | 'hide')
{
  console.log("Animating Icon Scale")
  
  const target = action === 'show' ? 1 : 0;
  await animate(scale, target, {
    duration: VS_ICON_SCALE_ANIMATION_DURATION,
    ease: 'easeInOut',
  })
}

async function animateCardShift()
{
  console.log("Animating Card Shift")
  await animate(x, `-100%`, {
    duration: CARD_SHIFT_ANIMATION_DURATION,
    ease: 'easeInOut',
  })
  x.set('0%');
}


</script>