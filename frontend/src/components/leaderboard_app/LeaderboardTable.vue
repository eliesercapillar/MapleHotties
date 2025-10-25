<template>
    <section class="w-[60rem]">
      <!-- Loading state -->
      <div v-if="leaderboardStore.isLoading" class="h-[784.5px] text-center flex flex-col justify-center">
        <div class="animate-spin rounded-full h-12 w-12 border-b-2 border-white mx-auto"></div>
        <p class="mt-4">Loading...</p>
      </div>

      <!-- Table -->
      <div v-else class="h-[784.5px]  rounded-lg">
        <Table class="select-none table-fixed p-4">
          <TableCaption>
            {{ leaderboardStore.getTimeType() }} {{ leaderboardStore.getRankingType() }} - 
            Page {{ leaderboardStore.currentPage }} of {{ leaderboardStore.totalPages }}
            ({{ leaderboardStore.totalCount }} total)
          </TableCaption>
          <TableHeader class="">
            <TableRow>
              <TableHead class="w-[80px] text-center">Rank</TableHead>
              <TableHead class="w-[100px] text-center">Sprite</TableHead>
              <TableHead class="w-[150px] text-center">IGN</TableHead>
              <TableHead class="w-[80px] text-center">Level</TableHead>
              <TableHead class="w-[100px] text-center">Job</TableHead>
              <TableHead class="w-[100px] text-center">World</TableHead>
              <TableHead class="w-[100px] text-center">{{ getHeadFromRankingType }}</TableHead>
            </TableRow>
          </TableHeader>
          <TableBody>
            <TableRow v-for="(entry, index) in characters" :key="`like-${entry.character.id}-${leaderboardStore.currentPage}`"
            class="font-medium text-center text-white">
              <TableCell class="font-medium">
                {{ ((leaderboardStore.currentPage - 1) * leaderboardStore.pageSize) + index + 1 }}
              </TableCell>
              <TableCell>
                <img :src="entry.character.imageUrl" :alt="entry.character.name">
              </TableCell>
              <TableCell>{{ entry.character.name }}</TableCell>
              <TableCell>{{ entry.character.level }}</TableCell>
              <TableCell>{{ entry.character.job }}</TableCell>
              <TableCell>{{ entry.character.world }}</TableCell>
              <TableCell>{{ entry.count }}</TableCell>
            </TableRow>
          </TableBody>
        </Table>
        <PaginationNav/>
      </div>
    </section>
</template>
<script setup lang="ts">
import { onMounted, computed, ref } from "vue";
import { useLeaderboardStore } from "@/stores/leaderboardStore";
import {
  Table,
  TableBody,
  TableCaption,
  TableCell,
  TableHead,
  TableHeader,
  TableRow,
} from '@/components/ui/table'
import {
  HoverCard,
  HoverCardContent,
  HoverCardTrigger,
} from '@/components/ui/hover-card'
import PaginationNav from "./PaginationNav.vue";

const leaderboardStore = useLeaderboardStore();

onMounted(async () => { 
  await leaderboardStore.fetchSearch(); 
});

const characters = computed(() => leaderboardStore.characters);

const getHeadFromRankingType = computed(() => {
  const labels = 
  { 
      hotties: "Likes", 
      notties: "Nopes", 
      favourites: "Favourites" 
  }

  return labels[leaderboardStore.searchSettings.rankingType] || "Likes"
})

</script>