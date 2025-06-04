<template>
    <section v-if="historyStore.cards.length == 0" class="h-full flex items-center justify-center">
        <div class="flex flex-col items-center justify-center gap-10">
        <!-- TODO: make new svg for this component -->
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
            <CharacterSummaryCard :card="card"/>
        </RecycleScroller>
    </section>
</template>

<script setup lang="ts">
import { onMounted, computed } from "vue";
import { useHistoryStore } from "@/stores/historyStore";
import CharacterSummaryCard from "./CharacterSummaryCard.vue";

const historyStore = useHistoryStore();

onMounted(async () => {
  historyStore.fetchHistory();
})

const cards = computed(() => 
  historyStore.cards.map((card) => ({
    // Use character ID as top level id for RecycleScroller component.
    id: card.character.id,
    ...card
  }))
)

</script>