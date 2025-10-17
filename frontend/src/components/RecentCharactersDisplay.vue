<template>
    <section v-if="cards.length == 0" class="h-[calc(100vh-var(--sidebar-nav-h)-var(--sidebar-options-h))] flex items-center justify-center">
        <div class="flex flex-col items-center justify-center gap-10">
        <!-- TODO: make new svg for this component -->
            <img class="h-[256px] w-[256px]" src="/swipecard_fav_overlay_24x24.svg">
            <span class="text-white">No recent history. Try rating some characters!</span>
        </div>
    </section>
    <!-- TODO: Add loading text when fetching -->
    <section v-else id="recent_history" class="select-none h-[calc(100vh-var(--sidebar-nav-h)-var(--sidebar-options-h))]">
        <RecycleScroller
          class="h-full scroller"
          :items="cards"
          :item-size="248"
          :item-secondary-size="178"
          :grid-items="2"
          key-field="id"
          v-slot="{ item: card }"
        >
            <CharacterSummaryCard :card="card" :status="card.status"/>
        </RecycleScroller>
    </section>
</template>

<script setup lang="ts">
import { onMounted, watch, computed, ref } from "vue";
import { useHistoryStore } from "@/stores/historyStore";
import CharacterSummaryCard from "./CharacterSummaryCard.vue";

const props = defineProps({
    display: {
        type: String,
        required: true,
        validator: (value: string) => ['history', 'favourites'].includes(value)
    }
});

const historyStore = useHistoryStore();

onMounted(() => {
  historyStore.fetchHistory();
})

watch(() => props.display, async (newDisplay) => {
  if (newDisplay === 'favourites' && !historyStore.favouriteCards.length) await historyStore.fetchFavourites();
  else if (newDisplay === 'history' && !historyStore.historyCards.length) await historyStore.fetchHistory();
});

const cards = computed(() => {
  const source = props.display === 'favourites' ? historyStore.favouriteCards : historyStore.historyCards;
  
  return source.map((card) => ({
    id: card.character.id, // Use character ID as top level id for RecycleScroller component.
    ...card
  }));
})


</script>

<style lang="css">
  .scroller {
    scrollbar-color: #6e7072 #111418;
  }
</style>