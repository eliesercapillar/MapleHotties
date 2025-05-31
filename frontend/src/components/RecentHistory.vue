<template>
    <section id="recent_history" class="grid grid-cols-2 gap-2 flex-grow">
        <Card v-for="card in historyStore.cards" class="flex flex-col justify-center">
            <CardHeader class="text-center">
                <CardTitle class="text-md">{{card.character.name}}</CardTitle>
                <CardDescription>{{ card.character.level }} {{ card.character.job }}</CardDescription>
            </CardHeader>
            <img :src=card.character.imageUrl>
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

</script>