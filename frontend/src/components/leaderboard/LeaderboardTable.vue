<template>
    <section class="w-[60rem]">
      <!-- Loading state -->
      <div v-if="leaderboardStore.isLoading" class="text-center py-8">
        <div class="animate-spin rounded-full h-12 w-12 border-b-2 border-white mx-auto"></div>
        <p class="mt-4">Loading...</p>
      </div>

      <!-- Table -->
      <Table v-else class="select-none">
        <TableCaption>
          {{ leaderboardStore.showLikes ? 'Most Liked' : 'Most Noped' }} Characters - 
          Page {{ leaderboardStore.currentPage }} of {{ leaderboardStore.totalPages }}
          ({{ leaderboardStore.totalCount }} total)
        </TableCaption>
        <TableHeader>
          <TableRow>
            <TableHead class="w-[100px]">Rank</TableHead>
            <TableHead>Sprite</TableHead>
            <TableHead>IGN</TableHead>
            <TableHead>Level</TableHead>
            <TableHead>Job</TableHead>
            <TableHead>World</TableHead>
            <TableHead class="text-right">{{ leaderboardStore.showLikes ? 'Loves' : 'Nopes' }}</TableHead>
          </TableRow>
        </TableHeader>
        <TableBody>
          <!-- Likes table -->
          <template v-if="leaderboardStore.showLikes">
                <TableRow v-for="(entry, index) in likeCharacters" 
                :key="`like-${entry.character.id}-${leaderboardStore.currentPage}`">
                  <TableCell class="font-medium">
                    {{ ((leaderboardStore.currentPage - 1) * leaderboardStore.pageSize) + index + 1 }}
                  </TableCell>
                  <TableCell><img class="h-24 w-24" :src="entry.character.imageUrl" :alt="entry.character.name"></TableCell>
                  <TableCell>{{ entry.character.name }}</TableCell>
                  <TableCell>{{ entry.character.level }}</TableCell>
                  <TableCell>{{ entry.character.job }}</TableCell>
                  <TableCell>{{ entry.character.world }}</TableCell>
                  <TableCell class="text-right">{{ entry.totalLikes }}</TableCell>
                </TableRow>
          </template>

          <!-- Nopes table -->
          <template v-else>
            <HoverCard 
              v-for="(entry, index) in nopeCharacters" 
              :key="`nope-${entry.character.id}-${leaderboardStore.currentPage}`"
            >
              <HoverCardTrigger as-child>
                <TableRow>
                  <TableCell class="font-medium">
                    {{ ((leaderboardStore.currentPage - 1) * leaderboardStore.pageSize) + index + 1 }}
                  </TableCell>
                  <TableCell>{{ entry.character.name }}</TableCell>
                  <TableCell>{{ entry.character.level }}</TableCell>
                  <TableCell>{{ entry.character.job }}</TableCell>
                  <TableCell>{{ entry.character.world }}</TableCell>
                  <TableCell class="text-right">{{ entry.totalNopes }}</TableCell>
                </TableRow>
              </HoverCardTrigger>
              <HoverCardContent class="h-64 w-64 p-0 overflow-hidden bg-nope-gradient">
                <img class="h-64 w-64" :src="entry.character.imageUrl" :alt="entry.character.name">
              </HoverCardContent>
            </HoverCard>
          </template>
        </TableBody>
      </Table>
      <PaginationNav/>
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
  await leaderboardStore.fetchTopLiked(); 
});

const likeCharacters = computed(() => leaderboardStore.likeCharacters);
const nopeCharacters = computed(() => leaderboardStore.nopeCharacters);
</script>