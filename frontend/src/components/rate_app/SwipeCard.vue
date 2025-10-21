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
      <source :srcset="card.bgURL.optimized" type="image/webp">
      <img
        id="card_background"
        :src="card.bgURL.fallback"
        alt=""
        class="h-[667px] w-[375px] object-cover rounded-lg select-none"
        draggable="false"
      />
    </picture>
    <!-- TODO: Add remove width & height once upscaled pngs are implemented -->
    <img 
      id="character_sprite"
      :src="card.character.imageUrl"
      alt=""
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
import { motion, useMotionValue, useTransform, animate} from "motion-v";
import { useSwipeStore } from '@/stores/swipeStore'

const props = defineProps({
  index: {
    type: Number,
    required: true
  }
});

// #region Constants
const SWIPE_THRESHOLD_X = 300;
const SWIPE_THRESHOLD_Y = 300;
const IN_CENTER_THRESHOLD = 150;
const OVERLAY_MAX_X = 300;
const OVERLAY_MAX_Y = 250; // Typically less vertical real estate, so this is lowered slightly.
const ROTATION_RANGE = 30;
const ANIMATION_DURATION = 0.15;

// #endregion Constants

const swipeStore = useSwipeStore();
const cards = computed(() => swipeStore.cards)
const card = computed(() => cards.value[props.index])
const isActive = computed(() => props.index === cards.value.length - 1)

const x = useMotionValue(0);
const y = useMotionValue(0);
const rotate = useTransform(x, [-OVERLAY_MAX_X, OVERLAY_MAX_X], [-ROTATION_RANGE, ROTATION_RANGE]);

// #region Life Cycle Hooks
onMounted(() => { window.addEventListener('keydown', handleKeyDown); });

onUnmounted(() => { window.removeEventListener('keydown', handleKeyDown); });

// #endregion Life Cycle Hooks

// #region Event Handlers
const handleKeyDown = (event: KeyboardEvent) => {
  if (!isActive.value) return;
  
  // Prevent default behavior for arrow keys
  if (['ArrowLeft', 'ArrowRight', 'ArrowUp'].includes(event.key)) {
    event.preventDefault();
  }

  switch (event.key) {
    case 'ArrowLeft':
      animateSwipe(-window.innerWidth, 0, 'noped');
      break;
    case 'ArrowRight':
      animateSwipe(window.innerWidth, 0, 'liked');
      break;
    case 'ArrowUp':
      animateSwipe(0, -window.innerHeight, 'favourited');
      break;
  }
};

const handleDragStart = () => { 
  x.stop();
  y.stop();
  swipeStore.setDragging(true);
};

const handleDragEnd = () => {
  swipeStore.setDragging(false);

  const currentX = x.get();
  const currentY = y.get();
  const isCentered = Math.abs(currentX) < IN_CENTER_THRESHOLD;
  const xThresholdHit = Math.abs(currentX) > SWIPE_THRESHOLD_X;
  const yThresholdHit = currentY < -SWIPE_THRESHOLD_Y;
    
  if (isCentered && yThresholdHit) {
    animateSwipe(0, -window.innerHeight, 'favourited');
  } 
  else if (xThresholdHit) {
    const status = currentX > 0 ? 'liked' : 'noped';
    const targetX = currentX > 0 ? window.innerWidth : -window.innerWidth;
    animateSwipe(targetX, 0, status);
  }
};

// #endregion Event Handlers

// #region Opacities
const likeOpacity = useTransform([x, y], (values: number[]) => {
  const [currentX, currentY] = values;
  const { isCentered, isUpStronger, isSwipingUp } = calculateSwipeContext(currentX, currentY);

  // Only hide x-axis overlays if we're centered AND the upward movement is stronger than sideways
  return isSwipingUp && isCentered && isUpStronger ? 0 : mapRange(currentX, 0, OVERLAY_MAX_X);
});

const nopeOpacity = useTransform([x, y], (values: number[]) => {
  const [currentX, currentY] = values;
  const { isCentered, isUpStronger, isSwipingUp } = calculateSwipeContext(currentX, currentY);

  // Only hide x-axis overlays if we're centered AND the upward movement is stronger than sideways
  return isSwipingUp && isCentered && isUpStronger ? 0 : mapRange(currentX, 0, -OVERLAY_MAX_X);
});

const favOpacity = useTransform([x, y], (values: number[]) => {
  const [currentX, currentY] = values;
  const { isCentered, isUpStronger, isSwipingUp } = calculateSwipeContext(currentX, currentY);

  // Show fav overlay when centered, swiping up, AND upward movement is stronger than sideways
  return isCentered && isSwipingUp && isUpStronger ? mapRange(currentY, 0, -OVERLAY_MAX_Y) : 0;
});

// #endregion Opacities

// #region Helper Functions
function animateSwipe(targetX: number, targetY: number, status: 'liked' | 'noped' | 'favourited') {
  const axis = status === 'favourited' ? y : x;
  const targetPos = status === 'favourited' ? targetY : targetX;

  animate(axis, targetPos, {
    duration: ANIMATION_DURATION,
    onComplete: () => swipeStore.onSwipe(card.value.character, status, new Date().toISOString()),
  });
}

function mapRange(value: number, inMin: number, inMax: number, outMin = 0, outMax = 1) {
  const ratio = (value - inMin) / (inMax - inMin);
  return Math.min(outMax, Math.max(outMin, ratio * (outMax - outMin) + outMin));
}

function calculateSwipeContext(currentX: number, currentY: number) {
  return {
    isCentered: Math.abs(currentX) < IN_CENTER_THRESHOLD,
    isUpStronger: Math.abs(currentY) > Math.abs(currentX),
    isSwipingUp: currentY < 0,
  };
}

// #endregion Helper Functions
</script>

