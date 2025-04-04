<template>
  <button
    class="relative flex items-center justify-center w-16 h-16 rounded-full shadow-lg transition-all hover:scale-110 active:scale-75 focus:outline-none"
    :class="button.bgColor"
    @mousedown="isPressed = true"
    @mouseup="isPressed = false"
    @mouseleave="isPressed = false"
    draggable="false"
  >
    <!-- Default State Icon -->
    <span
      class="absolute inset-0 flex items-center justify-center transition-all"
      :class="{ 'scale-0': isPressed }"
    >
      <svg
        xmlns="http://www.w3.org/2000/svg"
        :width="button.svg.width"
        :height="button.svg.height"
        :viewBox="button.svg.viewBox"
        class="scale-[0.4]"
      >
        <path
          :fill="button.svg.fill"
          :fill-rule="button.svg.fillRule"
          :d="button.svg.path"
          :clip-rule="button.svg.clipRule"
        />
      </svg>
    </span>

    <!-- Pressed State Icon -->
    <span
      class="absolute inset-0 flex items-center justify-center transition-all brightness-0 invert scale-0"
      :class="{ 'scale-100': isPressed }"
    >
      <svg
        xmlns="http://www.w3.org/2000/svg"
        :width="button.svg.width"
        :height="button.svg.height"
        :viewBox="button.svg.viewBox"
        class="scale-[0.4]"
      >
        <path
          :fill="button.svg.fill"
          :fill-rule="button.svg.fillRule"
          :d="button.svg.path"
          :clip-rule="button.svg.clipRule"
        />
      </svg>
    </span>
  </button>
</template>

<script setup lang="ts">
import { ref } from "vue";

defineProps({
  button: {
    type: Object,
    required: true,
  },
});

const isPressed = ref(false);
</script>
