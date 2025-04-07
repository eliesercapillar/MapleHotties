<template>
  <motion.button
    class="relative flex items-center justify-center w-16 h-16 rounded-full overflow-hidden"
    :style="{ background: isPressed ? props.button.pressedGradient : '#21262e' }"
    :animate="{ scale: props.isDragging ? (isActive ? 1.25 : 0) : (isPressed ? 0.75 : (isHovering ? 1.2 : 1)) }"
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
        :viewBox="button.svg.viewBox"
        :width="button.svg.width"
        :height="button.svg.height"
        :animate="{ scale: isDragging && isActive ? 7 : isPressed ? 0 : 1 }"
        :transition="{ duration: 0.1, ease: 'easeInOut' }"
      >
        <path
          :fill="button.svg.fill"
          :fill-rule="button.svg.fillRule"
          :clip-rule="button.svg.clipRule"
          :d="button.svg.path"
        />
      </motion.svg>
    </span>

    <!-- Pressed State Icon -->
    <motion.span
      class="absolute inset-0 flex items-center justify-center brightness-0 invert"
      :initial="{ scale: 0 }"
      :animate="{ scale: isPressed || (isDragging && isActive) ? 1 : 0 }"
    >
      <svg
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
      </svg>
    </motion.span>
  </motion.button>
</template>

<script setup lang="ts">
import { motion } from "motion-v"
import { ref, computed } from "vue";

const props = defineProps({
  button: {
    type: Object,
    required: true,
  },
  cardDirection: {
    type: String,
    default: null,
  },
  isDragging: {
    type: Boolean,
    default: false,
  }
});

const isHovering = ref(false);

const onPointerHover = () => {
  isHovering.value = true;
}

const onPointerLeave = () => {
  isHovering.value = false;
}

const isPressed = ref(false);

const onPointerDown = (e) => {
  isPressed.value = true;
  e.target.setPointerCapture(e.pointerId);
}

const onPointerUp = () => {
  isPressed.value = false;
}

const isActive = computed(() => {
  return (props.button.name === "Nope" && props.cardDirection === "left") ||
         (props.button.name === "Love" && props.cardDirection === "right");
});

</script>