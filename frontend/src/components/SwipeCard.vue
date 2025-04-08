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
    <!-- Fav Overlay -->
    <!-- TODO: Add y checking logic -->
    <motion.div class="absolute bottom-[2%]" :style="{ opacity: favOpacity }">
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
import { computed } from "vue";
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
const favOpacity = useTransform(y, [0, -300], [0, 1]);
const likeOpacity = useTransform(x, [0, 300], [0, 1]);
const nopeOpacity = useTransform(x, [-300, 0], [1, 0]);

useMotionValueEvent(x, "change", (latest) => {
  emit("swiping", latest);
});

const handleDragStart = () => {
  emit("dragStarted");
};

const handleDragEnd = () => {
  emit("dragEnded");

  const currentX = x.get();

  if (Math.abs(currentX) > 300) {
    const targetX = currentX > 0 ? window.innerWidth : -window.innerWidth; // Move beyond viewport

    animate(x, targetX, {
      duration: 0.15,
      onComplete: () => emit("remove", props.card.id),
    });
  }
};
</script>
