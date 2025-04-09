<template>
  <motion.div
    class="relative h-[667px] w-[375px] row-[1] col-[1] z-10 hover:cursor-grab active:cursor-grabbing"
    :class="{ 'z-[11]': isFront }"
    :style="{ x, y, rotate }"
    :drag="isFront ? true : false"
    dragSnapToOrigin
    @dragStart="handleDragStart"
    @dragEnd="handleDragEnd"
  >
    <img
      :src="props.card.url"
      alt="SwipeCard"
      class="h-[667px] w-[375px] object-cover rounded-lg select-none"
      draggable="false"
    />
    <img 
      src="/rockoguy_up5.png"
      alt="Player Character"
      class="absolute top-[50%] left-[50%] scale-[1]"
      style="transform: translate(-50%, -50%); "
      draggable="false"/>
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
  </motion.div>
</template>

<script setup lang="ts">
import { ref } from "vue";
import {
  motion,
  useMotionValue,
  useTransform,
  animate,
  useMotionValueEvent,
} from "motion-v";

const props = defineProps<{
  card: {
    id: number;
    url: string;
  };
  isFront: Boolean;
}>();

const emit = defineEmits(["remove", "swiping", "dragStarted", "dragEnded"]);

const x = useMotionValue(0);
const y = useMotionValue(0);
const rotate = useTransform(x, [-300, 300], [-30, 30]);
const likeOpacity = useTransform(x, [0, 300], [0, 1]);
const nopeOpacity = useTransform(x, [-300, 0], [1, 0]);
const favOpacity = useTransform([x, y], (values: number[]) => {
  const currentX = values[0];
  const currentY = values[1];

  const isCentered = Math.abs(currentX) < 150;
  const isSwipingUp = currentY < 0;

  if (isCentered && isSwipingUp) {
    return Math.min(1, Math.abs(currentY) / 300);
  }

  return 0;
});

useMotionValueEvent(x, "change", (latest) => {
  emit("swiping", latest);
});

useMotionValueEvent(y, "change", (latest) => {
  emit("swiping", latest);
});


const handleDragStart = () => {
  emit("dragStarted");
};

const handleDragEnd = () => {
  emit("dragEnded");

  const currentX = x.get();
  const currentY = y.get();
  const isCentered = Math.abs(currentX) < 150;
  const xThresholdHit = currentX < -300;
  const yThresholdHit = currentY < -300;

  if (isCentered && yThresholdHit) {
    const targetY = -window.innerHeight; // Move outside viewport
    
    animate(y, targetY, {
      duration: 0.15,
      onComplete: () => emit("remove", props.card.id),
    });
  } 
  else if (xThresholdHit) {
    const targetX = currentX > 0 ? window.innerWidth : -window.innerWidth; // Move outside viewport

    animate(x, targetX, {
      duration: 0.15,
      onComplete: () => emit("remove", props.card.id),
    });
  }
};
</script>
