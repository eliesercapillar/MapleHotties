<template>
    <motion.img
      :src="props.card.url"
      alt="SwipeCard"
      class="h-96 w-72 origin-bottom rounded-lg bg-white object-cover
      row-[1] col-[1] hover:cursor-grab, active:cursor-grabbing"
      :class="{'z-[1]': isFront}"
      draggable="false"
      :drag="isFront ? true : false"
      dragSnapToOrigin
      :style="{x, rotate}"
      @dragEnd="handleDragEnd"
      :animate="{scale: isFront ? 1 : 0.95}"
    />
</template>

<script setup lang="ts">
import { motion, useMotionValue, useTransform } from "motion-v"

const props = defineProps<{
  card: {
    id: number
    url: string,
  }, 
  isFront: Boolean
}>();

const emit = defineEmits(['remove']);

const x = useMotionValue(0);
const opacity = useTransform(x, [-300, 0, 300], [0, 1, 0])
const rotate = useTransform(x, [-300, 300], [-30, 30])


const handleDragEnd = () => {
  if (Math.abs(x.get()) > 300) {
    emit('remove', props.card.id);
  }
};
</script>