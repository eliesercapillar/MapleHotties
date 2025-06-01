<template>
  <motion.button
    class="relative flex items-center justify-center w-16 h-16 rounded-full overflow-hidden"
    :style="{ background: isPressed ? props.pressedGradient : '#21262e' }"
    :animate="{ scale: swipeStore.isDragging ? (isActive ? 1.25 : 0) : (isPressed ? 0.75 : (isHovering ? 1.2 : 1)) }"
    :transition="{ duration: 0.3, ease: 'easeInOut' }"
    @pointerenter="onPointerHover"
    @pointerleave="onPointerLeave"
    @pointerdown="onPointerDown"
    @pointerup="onPointerUp"
    @pointercancel="onPointerUp"
    draggable="false"
  >
  
    <!-- Default State Icon -->
    <span class="absolute inset-0 flex items-center justify-center">
      <motion.svg
        xmlns="http://www.w3.org/2000/svg"
        :viewBox="svg.viewBox"
        :width="svg.width"
        :height="svg.height"
        :animate="{ scale: swipeStore.isDragging && isActive ? 7 : isPressed ? 0 : 1 }"
        :transition="{ duration: 0.1, ease: 'easeInOut' }"
      >
        <path
          :fill="svg.fill"
          :stroke="svg.stroke"
          :fill-rule="svg.fillRule"
          :clip-rule="svg.clipRule"
          :d="svg.path"
        />
      </motion.svg>
    </span>

    <!-- Pressed State Icon -->
    <motion.span
      class="absolute inset-0 flex items-center justify-center brightness-0 invert"
      :initial="{ scale: 0 }"
      :animate="{ scale: isPressed || (swipeStore.isDragging && isActive) ? 1 : 0 }"
    >
      <svg
        xmlns="http://www.w3.org/2000/svg"
        :viewBox="svg.viewBox"
        :width="svg.width"
        :height="svg.height"
      >
        <path
          :fill="svg.fill"
          :fill-rule="svg.fillRule"
          :clip-rule="svg.clipRule"
          :d="svg.path"
        />
      </svg>
    </motion.span>
  </motion.button>
</template>

<script setup lang="ts">
import { motion } from "motion-v"
import { ref, computed } from "vue";
import { useSwipeStore } from "@/stores/swipeStore";

const props = defineProps({
  name: {
    type: String,
    required: true
  }, 
  pressedGradient: {
    type: String,
    required: true
  },
  svg: {
    type: Object,
    required: true,
  }
});

const swipeStore = useSwipeStore();

const isHovering = ref(false);
const isPressed = ref(false);

const onPointerHover = () => { isHovering.value = true; }

const onPointerLeave = () => { isHovering.value = false; }

const onPointerDown = (e) => {
  isPressed.value = true;
  e.target.setPointerCapture(e.pointerId);
}

const onPointerUp = () => { isPressed.value = false; }

const isActive = computed(() => {
  const swipeThreshold = 20;

  return (props.name === "Nope" && swipeStore.xPos < -swipeThreshold) ||
         (props.name === "Love" && swipeStore.xPos > swipeThreshold) ||
         (props.name === "Favourite" && swipeStore.yPos < -swipeThreshold);
});

</script>