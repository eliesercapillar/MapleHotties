<template>
  <motion.button
    class="relative flex items-center justify-center w-16 h-16 rounded-full shadow-lg 
    transition-all duration-300 ease-in-out hover:scale-110 active:scale-75"
    :class="props.isDragging ? (isActive ? 'scale-125' : 'scale-0'): 'scale-100'"
    :style="{background: isPressed ? props.button.pressedGradient : '#21262e'}"
    @mousedown="isPressed = true"
    @mouseup="isPressed = false"
    @mouseleave="isPressed = false"
    draggable="false"
  >
  
    <!-- Default State Icon -->
    <motion.span
      class="absolute inset-0 flex items-center justify-center transition-all"
      :class="{ 'scale-0': isPressed, 'scale-100': isActive }"
    >
      <svg
        xmlns="http://www.w3.org/2000/svg"
        :width="button.svg.width"
        :height="button.svg.height"
        :viewBox="button.svg.viewBox"
        :stroke="button.svg.stroke"
        class="transition-all ease-out duration-300"
      >
        <path
          :fill="button.svg.fill"
          :fill-rule="button.svg.fillRule"
          :clip-rule="button.svg.clipRule"
          :d="button.svg.path"
          :style="{
            transform: isActive ? 'scale(1.5)' : 'scale(1)'
          }"
        />
      </svg>
    </motion.span>

    <!-- Pressed State Icon (White X) -->
    <motion.span
      class="absolute inset-0 flex items-center justify-center transition-all brightness-0 invert scale-0"
      :class="{ 'scale-100': isPressed }"
    >
      <svg
        xmlns="http://www.w3.org/2000/svg"
        :width="button.svg.width"
        :height="button.svg.height"
        :viewBox="button.svg.viewBox"
      >
        <path
          :fill="button.svg.fill"
          :fill-rule="button.svg.fillRule"
          :clip-rule="button.svg.clipRule"
          :d="button.svg.path"
        />
      </svg>
    </motion.span>
  </motion.button>
</template>

<script setup lang="ts">
import { motion, useMotionValue, useTransform, animate, useMotionValueEvent } from "motion-v"
import { ref, computed} from "vue";

const props = defineProps({
  button: {
    type: Object,
    required: true,
  },
  cardX: {
    type: Number,
    default: 0,
  },
  isDragging: {
    type: Boolean,
    default: false,
  }
});

const isActive = computed(() => {
  return (props.button.name === "Nope" && props.cardX < -10) ||
         (props.button.name === "Love" && props.cardX > 10);
});

const isPressed = ref(false);
</script>