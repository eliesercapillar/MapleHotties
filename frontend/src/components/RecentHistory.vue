<template>
    <section v-if="historyStore.cards.length == 0" class="h-full flex items-center justify-center">
        <div class="flex flex-col items-center justify-center gap-10">
            <img class="h-[256px] w-[256px]" src="/swipecard_fav_overlay_24x24.svg">
            <span class="text-white">No recent history. Try rating some characters!</span>
        </div>
    </section>
    <!-- TODO: Add loading text when fetching -->
    <section v-else id="recent_history" class="select-none h-[calc(100vh-var(--sidebar-nav-h)-var(--sidebar-options-h))]">
        <RecycleScroller
        class="h-full"
        :items="cards"
        :item-size="248"
        :item-secondary-size="178"
        :grid-items="2"
        key-field="id"
        v-slot="{ item: card }"
        >
            <div
                class="relative flex flex-col justify-center rounded-lg border-[1px] aspect-[3/4] h-[240px] w-[170px] mx-1 mb-2"
                :style="{
                    background: getBackgroundGradient(card.status),
                    border: getBorderColour(card.status)
                }"
            >
                <img :src="card.character.imageUrl" draggable="false">
                <img 
                    id="status_overlay" 
                    class="scale-[1.5] absolute right-2 top-2" 
                    :src="getOverlay(card.status)" 
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
        </RecycleScroller>
    </section>
</template>

<script setup lang="ts">
import { onMounted, computed } from "vue";
import { useHistoryStore } from "@/stores/historyStore";
import ButtonSVGs from "@/data/ButtonSVGs.json";

const historyStore = useHistoryStore();

onMounted(async () => {
  historyStore.fetchHistory();
})

const cards = computed(() => historyStore.cards)

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