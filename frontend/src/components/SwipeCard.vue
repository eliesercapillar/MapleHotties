<template>
  <motion.div
    class="relative h-[667px] w-[375px] row-[1] col-[1] z-10 hover:cursor-grab active:cursor-grabbing"
    :class="{ 'z-[11]': isActive }"
    :style="{ x, y, rotate }"
    :drag="isActive ? true : false"
    dragSnapToOrigin
    @dragStart="handleDragStart"
    @dragEnd="handleDragEnd"
  >
    <img
      id="card_background"
      :src="card.bgURL"
      alt="SwipeCard"
      class="h-[667px] w-[375px] object-cover rounded-lg select-none"
      draggable="false"
    />
    <img 
      id="character_sprite"
      src="/rockoguy_up5.png"
      alt="Player Character"
      class="absolute top-[50%] left-[50%] scale-[1]"
      style="transform: translate(-50%, -50%); "
      draggable="false"
    />
    <!-- <img 
      id="character_sprite"
      :src="card.spriteURL"
      alt="Player Character"
      class="absolute top-[50%] left-[50%] scale-[1]"
      style="transform: translate(-50%, -50%); "
      draggable="false"
    /> -->
    <div id="character_info" class="absolute top-[75%]">
      <div id="character_name_and_level" class="ml-2 text-2xl">
        <span class="font-bold">{{ card.info.name }}</span>
        &nbsp;
        <span class="font-normal">{{ card.info.level }}</span>
      </div>
      
    </div>
    <div id="overlays">
      <!-- Fav Overlay -->
      <!-- TODO: Add y checking logic -->
      <motion.div 
      class="absolute bottom-[2%]" 
      :style="{ opacity: favOpacity }"
      >
        <img
          class="h-[256px] w-[256px]"
          src="/swipecard_like_overlay_24x24.svg"
          draggable="false"
        />
      </motion.div>
      <!-- Like Overlay -->
      <motion.div
        class="absolute top-[2%] left-[2%] -rotate-12"
        :style="{ opacity: likeOpacity }"
      >
        <img
          class="h-[256px] w-[256px]"
          src="/swipecard_like_overlay_24x24.svg"
          draggable="false"
        />
      </motion.div>
      <!-- Nope Overlay -->
      <motion.div
        class="absolute top-[2%] right-[2%] rotate-12"
        :style="{ opacity: nopeOpacity }"
      >
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
import {
  motion,
  useMotionValue,
  useTransform,
  animate,
  useMotionValueEvent,
} from "motion-v";
import { useSwipeStore } from '@/stores/swipeStore'

const props = defineProps<{
  card: {
    id: number;
    bgURL: string;
    spriteURL: string;
    info: 
    {
      ranking: number,
      name: string,
      level: number,
      job: string,
      world: string,
    };
  };
  isActive: Boolean;
}>();

const swipeStore = useSwipeStore()

const x = useMotionValue(0);
const y = useMotionValue(0);
const rotate = useTransform(x, [-300, 300], [-30, 30]);

function mapRange(value: number, inMin: number, inMax: number, outMin = 0, outMax = 1) {
  const ratio = (value - inMin) / (inMax - inMin);
  return Math.min(outMax, Math.max(outMin, ratio * (outMax - outMin) + outMin));
}

const likeOpacity = useTransform([x, y], (values: number[]) => {
  const currentX = values[0];
  const currentY = values[1];
  const xThreshold = 150;
  const yThreshold = 0;

  const isCentered = Math.abs(currentX) < xThreshold;
  const isSwipingUp = currentY < yThreshold;

  return isSwipingUp && isCentered ? 0 : mapRange(currentX, 0, 300);
});

const nopeOpacity = useTransform([x, y], (values: number[]) => {
  const currentX = values[0];
  const currentY = values[1];
  const xThreshold = 150;
  const yThreshold = 0;

  const isCentered = Math.abs(currentX) < xThreshold;
  const isSwipingUp = currentY < yThreshold;

  return isSwipingUp && isCentered ? 0 : mapRange(currentX, 0, -300);
});

const favOpacity = useTransform([x, y], (values: number[]) => {
  const currentX = values[0];
  const currentY = values[1];
  const xThreshold = 150;
  const yThreshold = 0;

  const isCentered = Math.abs(currentX) < xThreshold;
  const isSwipingUp = currentY < yThreshold;

  const targetOpacity = isCentered && isSwipingUp ? mapRange(currentY, 0, -300) : 0;

  // return isCentered && isSwipingUp ? mapRange(currentY, 0, -300) : 0;
  return targetOpacity;
});


useMotionValueEvent(x, "change", (latest) => { swipeStore.saveXPos(latest); });

useMotionValueEvent(y, "change", (latest) => { swipeStore.saveYPos(latest); });


const handleDragStart = () => { swipeStore.setDragging(true); };

const handleDragEnd = () => {
  swipeStore.setDragging(false);

  const currentX = x.get();
  const currentY = y.get();
  const isCentered = Math.abs(currentX) < 150;
  const xThresholdHit = Math.abs(currentX) > 300;
  const yThresholdHit = currentY < -300;

  if (isCentered && yThresholdHit) {
    const targetY = -window.innerHeight; // Move outside viewport
    
    animate(y, targetY, {
      duration: 0.15,
      onComplete: () => swipeStore.removeCard(props.card.id),
    });
  } 
  else if (xThresholdHit) {
    const targetX = currentX > 0 ? window.innerWidth : -window.innerWidth; // Move outside viewport

    animate(x, targetX, {
      duration: 0.15,
      onComplete: () => swipeStore.removeCard(props.card.id),
    });
  }
};
</script>

