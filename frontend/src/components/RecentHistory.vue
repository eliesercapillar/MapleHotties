<template>
    <section v-if="historyStore.cards.length == 0" class="flex flex-grow items-center justify-center">
        <span class="text-white">No recent history. Try rating some characters!</span>
    </section>
    <section v-else id="recent_history" class="grid grid-cols-2 gap-2 flex-grow">
        <!-- TODO: Prevent highlighting -->
        <Card v-for="card in historyStore.cards" 
         class="flex flex-col justify-center" 
         :style="{ background: getBackgroundGradient(card.status) }">
            <CardHeader class="text-center">
                <CardTitle class="text-md">{{card.character.name}}</CardTitle>
                <CardDescription>{{ card.character.level }} {{ card.character.job }}</CardDescription>
            </CardHeader>
            <img :src=card.character.imageUrl draggable="false">
        </Card>
    </section>
</template>

<script setup lang="ts">
import { onMounted } from "vue";
import { useHistoryStore } from "@/stores/historyStore";
import ButtonSVGs from "@/data/ButtonSVGs.json";
import {
  Card,
  CardContent,
  CardDescription,
  CardFooter,
  CardHeader,
  CardTitle,
} from '@/components/ui/card'

const historyStore = useHistoryStore();
onMounted(async () => {
  historyStore.fetchHistory();
})

const getBackgroundGradient = (status : string) => {
    return ButtonSVGs[status] ? ButtonSVGs[status].pressedGradient : "white";
}

</script>