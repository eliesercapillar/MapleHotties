<template>
    <div
        class="relative flex flex-col justify-center rounded-lg border-[1px] aspect-[3/4] h-[240px] w-[170px] mx-1 mb-2"
        :style="{
            background: getBackgroundGradient(props.status),
            border: getBorderColour(props.status)
        }"
    >
        <img :src="card.character.imageUrl" draggable="false">
        <img 
            id="status_overlay" 
            class="scale-[1.5] absolute right-2 top-2" 
            :src="getOverlay(props.status)" 
            draggable="false"
        />
        <div id="black_gradient" class="absolute bottom-0 h-[30%] w-full rounded-lg bg-black-shadow-fade"/>
        <div id="character_info" class="absolute bottom-[5%] ml-2 text-white overflow-hidden">
        <span class="font-extrabold text-sm">{{ card.character.name }}</span>
        <div class="flex items-center text-xs">
            <span class="font-normal">{{ card.character.level }}</span>
            &nbsp;
            <span class="font-normal">{{ card.character.job }}</span>
        </div>
        </div>
    </div>
</template>

<script setup lang="ts">
import ButtonSVGs from "@/data/ButtonSVGs.json";

const props = defineProps({
    card: {
        type: Object,
        required: true
    },
    status: {
        type: String,
        required: true
    },
});

const getBackgroundGradient = (status : string) => {
    return ButtonSVGs[status] ? ButtonSVGs[status].pressedGradient : "white";
}

const getBorderColour = (status : string) => {
    return ButtonSVGs[status] ? ButtonSVGs[status].borderColour : "white";
}

const getOverlay = (status: string) => {
    return ButtonSVGs[status] ? ButtonSVGs[status].overlayPath : undefined;
}
</script>