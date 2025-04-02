<script setup>
import { useDraggable } from '@vueuse/core';

const props = defineProps({ card: Object, isFront: Boolean });
const emit = defineEmits(['remove']);

const { x } = useDraggable();

const handleDragEnd = () => {
  if (Math.abs(x.value) > 100) {
    emit('remove', props.card.id);
  }
};
</script>

<template>
  <img
    :src="props.card.url"
    class="h-96 w-72 origin-bottom rounded-lg bg-white object-cover 
    row-[1] col-[1]
    hover:cursor-grab active:cursor-grabbing"
    :style="{ x, transition: '0.125s transform' }"
    v-motion-drag="{ axis: 'x', constraints: { left: 0, right: 0 } }"
    @dragend="handleDragEnd"
  />
</template>
