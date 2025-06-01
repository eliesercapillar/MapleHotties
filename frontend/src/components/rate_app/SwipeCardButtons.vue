<template>
    <div id="buttons" class="absolute z-20 isolate w-[375px] bottom-[-2rem]">
        <div class="flex justify-around items-center">
            <TinderButton
            name="Nope"
            :pressedGradient="ButtonSVGs.nope.pressedGradient"
            :svg="ButtonSVGs.nope.svg"
            />
            <TinderButton
            name="Favourite"
            :pressedGradient="ButtonSVGs.favourite.pressedGradient"
            :svg="ButtonSVGs.favourite.svg"
            />
            <TinderButton
            name="Love"
            :pressedGradient="ButtonSVGs.love.pressedGradient"
            :svg="ButtonSVGs.love.svg"
            />
        </div>
    </div>
</template>

<script setup lang="ts">
import ButtonSVGs from "@/data/ButtonSVGs.json";
import TinderButton from "@/components/rate_app/TinderButton.vue";
</script>