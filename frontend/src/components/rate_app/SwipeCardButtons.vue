<template>
    <div id="buttons" class="absolute z-20 isolate w-[375px] bottom-[-2rem]">
        <div class="flex justify-around items-center">
            <TinderButton
            name="Nope"
            :pressedGradient="ButtonSVGs.noped.pressedGradient"
            :svg="ButtonSVGs.noped.svg"
            />
            <TinderButton
            name="Favourite"
            :pressedGradient="ButtonSVGs.favourited.pressedGradient"
            :svg="ButtonSVGs.favourited.svg"
            />
            <TinderButton
            name="Love"
            :pressedGradient="ButtonSVGs.liked.pressedGradient"
            :svg="ButtonSVGs.liked.svg"
            />
        </div>
    </div>
</template>

<script setup lang="ts">
import ButtonSVGs from "@/data/ButtonSVGs.json";
import TinderButton from "@/components/rate_app/TinderButton.vue";
</script>