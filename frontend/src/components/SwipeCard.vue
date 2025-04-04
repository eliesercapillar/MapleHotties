<template>
    <motion.img
      :src="props.card.url"
      alt="SwipeCard"
      class="h-[667px] w-[375px] rounded-lg object-cover row-[1] col-[1] hover:cursor-grab, active:cursor-grabbing"
      :class="{'z-[1]': isFront}"
      draggable="false"
      :drag="isFront ? true : false" dragSnapToOrigin
      :style="{x, rotate}" :animate="{scale: isFront ? 1 : 0.95}"
      @dragEnd="handleDragEnd"
    />
</template>

<script setup lang="ts">
import { motion, useMotionValue, useTransform, animate } from "motion-v"

const props = defineProps<{
  card: {
    id: number
    url: string,
  }, 
  isFront: Boolean
}>();

const emit = defineEmits(['remove']);

const x = useMotionValue(0);
// const opacity = useTransform(x, [-300, 0, 300], [0, 1, 0])
const rotate = useTransform(x, [-300, 300], [-30, 30])


const handleDragEnd = () => {
  const currentX = x.get();

  if (Math.abs(currentX) > 300) {
    const targetX = currentX > 0 ? window.innerWidth : -window.innerWidth; // Move beyond viewport

    animate(x, targetX, { duration: 0.15, onComplete: () => emit("remove", props.card.id) });
  }
};
</script>