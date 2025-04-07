<template>
  <motion.button
    class="relative flex items-center justify-center w-16 h-16 rounded-full shadow-lg overflow-hidden"
    :style="{background: isPressed ? props.button.pressedGradient : '#21262e'}"
    :animate="{
      scale: props.isDragging ? (isActive ? 1.25 : 0) : (isPressed ? 0.75 : 1)
    }"
    :transition="{ duration: 0.3, ease: 'easeInOut' }"
    @pointerdown="onPointerDown"
    @pointerup="onPointerUp"
    @pointercancel="onPointerUp"
    draggable="false"
  >
  
    <!-- Default State Icon -->
    <motion.span
      class="absolute inset-0 flex items-center justify-center transition-all"
      :class="{ 'scale-0': isPressed }"
    >
      <motion.svg
        xmlns="http://www.w3.org/2000/svg"
        :viewBox="button.svg.viewBox"
        :width="button.svg.width"
        :height="button.svg.height"
        :animate="{ scale: isDragging && isActive ? 4 : 1 }"
        :transition="{ duration: 0.3, ease: 'easeInOut' }"
      >
        <path
          :fill="button.svg.fill"
          :fill-rule="button.svg.fillRule"
          :clip-rule="button.svg.clipRule"
          :d="button.svg.path"
        />
      </motion.svg>
    </motion.span>

    <!-- Pressed State Icon (White X) -->
    <motion.span
      class="absolute inset-0 flex items-center justify-center brightness-0 invert
      transition-all scale-0"
      :class="{ 'scale-100': isPressed || (isDragging && isActive) }"
    >
      <motion.svg
        xmlns="http://www.w3.org/2000/svg"
        :viewBox="button.svg.viewBox"
        :width="button.svg.width"
        :height="button.svg.height"
      >
        <path
          :fill="button.svg.fill"
          :fill-rule="button.svg.fillRule"
          :clip-rule="button.svg.clipRule"
          :d="button.svg.path"
        />
      </motion.svg>
    </motion.span>
  </motion.button>
</template>

<script setup lang="ts">
import { motion, useMotionValue, useTransform, animate, useMotionValueEvent, time, easeInOut, easeOut } from "motion-v"
import { ref, computed, watch } from "vue";

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

const isPressed = ref(false);

const onPointerDown = (e) => {
  isPressed.value = true
  e.target.setPointerCapture(e.pointerId)
}

const onPointerUp = () => {
  isPressed.value = false
}

// const scale = useMotionValue(1);

// watch(() => props.isDragging, (isDragging) => {
//   const targetScale = isDragging ? 10 : 1;
//   if ((props.cardX > 10 && props.button.name === "Love") || (props.cardX < -10 && props.button.name === "Nope")) {
//     animate(scale, targetScale, { duration: 0.25, ease: "easeOut" });
//   }
  
//   console.log(`scale is currently ${scale.get()}. I am calling from ${props.button.name}`)
// });


const isActive = computed(() => {
  return (props.button.name === "Nope" && props.cardX < 0) ||
         (props.button.name === "Love" && props.cardX > 0);
});

</script>