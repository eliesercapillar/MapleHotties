<template>
  <div class="relative overflow-hidden h-card w-card">
    <picture>
    <source :srcset="card.bgURL.optimized" type="image/webp">
    <img
        id="card_background"
        :src="card.bgURL.fallback"
        alt=""
        class="h-full w-full object-cover rounded-lg select-none"
        draggable="false"
    />
    </picture>
    <!-- TODO: Add remove width & height once upscaled pngs are implemented -->
    <img 
    id="character_sprite"
    :src="card.character.imageUrl"
    alt=""
    class="absolute top-[50%] left-[50%] select-none"
    width="480" height="480"
    style="transform: translate(-50%, -50%); "
    draggable="false"
    />
    <div class="absolute bottom-[0%] h-[30%] w-full rounded-lg bg-black-shadow-fade"/>
    <div id="character_info" class="absolute bottom-[5%] ml-2 select-none text-white">
    <div id="character_name_and_level" class="flex items-center text-3xl">
        <span class="font-extrabold">{{ card.character.name }}</span>
        &nbsp;
        <span class="font-normal">{{ card.character.level }}</span>
    </div>
    <div id="character_world" class="flex items-center text-md">
        <Icon icon="lucide:globe"></Icon>
        &nbsp;
        <span class="font-normal">{{ card.character.world }}</span>
    </div>
    <div id="character_job" class="flex items-center text-md">
        <Icon icon="lucide:swords"></Icon>
        &nbsp;
        <span class="font-normal">{{ card.character.job }}</span>
    </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { computed } from 'vue'
import { Icon } from "@iconify/vue/dist/iconify.js";
import { usePlayStore } from "@/stores/playStore";
import { vAutoAnimate } from "@formkit/auto-animate/vue";

const props = defineProps({
  index: {
    type: Number,
    required: true
  }
});


const playStore = usePlayStore();
const cards = computed(() => playStore.cards);
const card = computed(() => cards.value[props.index])

</script>