<template>
  <motion.div
    class="relative overflow-hidden
           row-[1] col-[1] z-10 
           hover:cursor-grab active:cursor-grabbing"
    :class="{ 'z-[11]': isActive }"
    :style="{ x, y, rotate }"
    :drag="isActive ? true : false"
    dragSnapToOrigin
    @dragStart="handleDragStart"
    @dragEnd="handleDragEnd"
  >
    <picture>
      <source :srcset="card.bgURL.replace('/bgs/', '/bgs/optimized/').replace('.png', '.webp')" type="image/webp">
      <img
        id="card_background"
        :src="card.bgURL"
        alt=""
        class="h-[667px] w-[375px] object-cover rounded-lg select-none"
        draggable="false"
      />
    </picture>
    <!-- TODO: Add remove width & height once upscaled pngs are implemented -->
    <img 
      id="character_sprite"
      :src="card.character.imageUrl"
      alt="Player Character"
      class="absolute top-[50%] left-[50%] select-none"
      width="480" height="480"
      style="transform: translate(-50%, -50%); "
      draggable="false"
    />
    <div class="absolute bottom-[0%] h-[30%] w-full rounded-lg bg-black-shadow-fade"/>
    <div id="character_info" class="absolute bottom-[5%] ml-2 select-none text-white">
      <div id="character_name_and_level" class="flex items-center text-3xl">
        <span class="font-extrabold">{{ card.character.name }}</span>
        &nbsp;
        <span class="font-normal">{{ card.character.level }}</span>
      </div>
      <div id="character_world" class="flex items-center text-md">
        <Icon icon="lucide:globe"></Icon>
        &nbsp;
        <span class="font-normal">{{ card.character.world }}</span>
      </div>
      <div id="character_job" class="flex items-center text-md">
        <Icon icon="lucide:swords"></Icon>
        &nbsp;
        <span class="font-normal">{{ card.character.job }}</span>
      </div>
    </div>
    <div id="overlays">
      <motion.div id="fav_overlay" class="absolute bottom-[2%] left-[50%]" :style="{ opacity: favOpacity, marginLeft: '-128px' }">
        <img
          class="h-[256px] w-[256px]"
          src="/swipecard_fav_overlay_24x24.svg"
          draggable="false"
        />
      </motion.div>
      <motion.div id="like_overlay" class="absolute top-[2%] left-[2%] -rotate-12" :style="{ opacity: likeOpacity }">
        <img
          class="h-[256px] w-[256px]"
          src="/swipecard_like_overlay_24x24.svg"
          draggable="false"
        />
      </motion.div>
      <motion.div id="nope_overlay" class="absolute top-[2%] right-[2%] rotate-12" :style="{ opacity: nopeOpacity }">
        <img
          class="h-[216px] w-[216px]"
          src="/swipecard_nope_overlay_24x24.svg"
          draggable="false"
        />
      </motion.div>
    </div>
  </motion.div>
</template>

<script setup lang="ts">
import { computed, onMounted, onUnmounted } from "vue";
import { Icon } from "@iconify/vue/dist/iconify.js";
import {
  motion,
  useMotionValue,
  useTransform,
  animate,
  useMotionValueEvent,
} from "motion-v";
import { useSwipeStore } from '@/stores/swipeStore'

const props = defineProps<{
  index: number;
}>();

const swipeStore = useSwipeStore();
const cards = computed(() => swipeStore.cards)
const card = computed(() => cards.value[props.index])
const isActive = computed(() => props.index === cards.value.length - 1)

const x = useMotionValue(0);
const y = useMotionValue(0);
const rotate = useTransform(x, [-300, 300], [-30, 30]);

onMounted(() => {
  window.addEventListener('keydown', handleKeyDown);
});

onUnmounted(() => {
  window.removeEventListener('keydown', handleKeyDown);
});

const handleKeyDown = (event: KeyboardEvent) => {
  if (!isActive.value) return;
  
  // Prevent default behavior for arrow keys
  if (['ArrowLeft', 'ArrowRight', 'ArrowUp'].includes(event.key)) {
    event.preventDefault();
  }

  let targetPos = 0;
  let status;

  switch (event.key) {
    case 'ArrowLeft':
      targetPos = -window.innerWidth;
      status = 'noped';
      break;
    case 'ArrowRight':
      targetPos = window.innerWidth;
      status = 'liked';
      break;
    case 'ArrowUp':
      targetPos = -window.innerHeight;
      status = 'favourited';
      break;
    default:
      return;
  }

  const swipeEvent = swipeStore.createSwipeEvent(card.value.character.id, status, new Date().toISOString());

  if (status === 'favourited') {
    animate(y, targetPos, {
      duration: 0.15,
      onComplete: () => swipeStore.onSwipe(swipeEvent),
    });
  } else {
    animate(x, targetPos, {
      duration: 0.15,
      onComplete: () => swipeStore.onSwipe(swipeEvent),
    });
  }
};

function mapRange(value: number, inMin: number, inMax: number, outMin = 0, outMax = 1) {
  const ratio = (value - inMin) / (inMax - inMin);
  return Math.min(outMax, Math.max(outMin, ratio * (outMax - outMin) + outMin));
}

const likeOpacity = useTransform([x, y], (values: number[]) => {
  const currentX = values[0];
  const currentY = values[1];
  const xThreshold = 150;
  const xMax = 300;

  const isCentered = Math.abs(currentX) < xThreshold;
  const isUpStronger = Math.abs(currentY) > Math.abs(currentX);
  const isSwipingUp = currentY < 0;

  // Only hide x-axis overlays if we're centered AND the upward movement is stronger than sideways
  return isSwipingUp && isCentered && isUpStronger ? 0 : mapRange(currentX, 0, xMax);
});

const nopeOpacity = useTransform([x, y], (values: number[]) => {
  const currentX = values[0];
  const currentY = values[1];
  const xThreshold = 150;
  const xMax = 300;

  const isCentered = Math.abs(currentX) < xThreshold;
  const isUpStronger = Math.abs(currentY) > Math.abs(currentX);
  const isSwipingUp = currentY < 0;

  // Only hide x-axis overlays if we're centered AND the upward movement is stronger than sideways
  return isSwipingUp && isCentered && isUpStronger ? 0 : mapRange(currentX, 0, -xMax);
});

const favOpacity = useTransform([x, y], (values: number[]) => {
  const currentX = values[0];
  const currentY = values[1];
  const xThreshold = 150;
  const yMax = 250; // Typically, there is less vertical real estate so i'll lower this slightly.

  const isCentered = Math.abs(currentX) < xThreshold;
  const isUpStronger = Math.abs(currentY) > Math.abs(currentX);
  const isSwipingUp = currentY < 0;

  // Show fav overlay when centered, swiping up, AND upward movement is stronger than sideways
  return isCentered && isSwipingUp && isUpStronger ? mapRange(currentY, 0, -yMax) : 0;
});


useMotionValueEvent(x, "change", (latest) => { swipeStore.saveXPos(latest); });

useMotionValueEvent(y, "change", (latest) => { swipeStore.saveYPos(latest); });


const handleDragStart = () => { 
  x.stop();
  y.stop();
  swipeStore.setDragging(true);
};

const handleDragEnd = () => {
  swipeStore.setDragging(false);

  const currentX = x.get();
  const currentY = y.get();
  const isCentered = Math.abs(currentX) < 150;
  const xThresholdHit = Math.abs(currentX) > 300;
  const yThresholdHit = currentY < -300;

  if (isCentered && yThresholdHit) {
    const targetY = -window.innerHeight; // Move outside viewport
    
    const swipeEvent = swipeStore.createSwipeEvent(card.value.character.id, "favourited", new Date().toISOString())

    animate(y, targetY, {
      duration: 0.15,
      onComplete: () => swipeStore.onSwipe(swipeEvent),
    });
  } 
  else if (xThresholdHit) {
    const targetX = currentX > 0 ? window.innerWidth : -window.innerWidth; // Move outside viewport

    const status = currentX > 0 ? "liked" : "noped";

    const swipeEvent = swipeStore.createSwipeEvent(card.value.character.id, status, new Date().toISOString())

    animate(x, targetX, {
      duration: 0.15,
      onComplete: () => swipeStore.onSwipe(swipeEvent),
    });
  }
};
</script>

