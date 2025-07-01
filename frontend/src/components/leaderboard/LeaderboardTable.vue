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
            {{ leaderboardStore.showLikes ? 'Most Liked' : 'Most Noped' }} Characters - 
            Page {{ leaderboardStore.currentPage }} of {{ leaderboardStore.totalPages }}
            ({{ leaderboardStore.totalCount }} total)
          </TableCaption>
          <TableHeader class="bg-prim">
            <TableRow>
              <TableHead class="w-[80px] text-center">Rank</TableHead>
              <TableHead class="w-[100px] text-center">Sprite</TableHead>
              <TableHead class="w-[150px] text-center">IGN</TableHead>
              <TableHead class="w-[80px] text-center">Level</TableHead>
              <TableHead class="w-[100px] text-center">Job</TableHead>
              <TableHead class="w-[100px] text-center">World</TableHead>
              <TableHead class="w-[100px] text-center">{{ leaderboardStore.showLikes ? 'Loves' : 'Nopes' }}</TableHead>
            </TableRow>
          </TableHeader>
          <TableBody>
            <!-- Likes table -->
            <template v-if="leaderboardStore.showLikes">
              <TableRow v-for="(entry, index) in likeCharacters" :key="`like-${entry.character.id}-${leaderboardStore.currentPage}`"
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
                <TableCell>{{ entry.totalLikes }}</TableCell>
              </TableRow>
            </template>

            <!-- Nopes table -->
            <template v-else>
              <TableRow v-for="(entry, index) in nopeCharacters" :key="`nope-${entry.character.id}-${leaderboardStore.currentPage}`"
              class="font-medium text-center text-white">
                  <TableCell class="font-medium">
                  {{ ((leaderboardStore.currentPage - 1) * leaderboardStore.pageSize) + index + 1 }}
                  </TableCell>
                  <TableCell><img class="h-24 w-24" :src="entry.character.imageUrl" :alt="entry.character.name"></TableCell>
                  <TableCell>{{ entry.character.name }}</TableCell>
                  <TableCell class="">{{ entry.totalNopes }}</TableCell>
              </TableRow>
            </template>
          </TableBody>

          <!-- <TableBody>
          <!-- Likes table
          <template v-if="leaderboardStore.showLikes">
            <HoverCard 
              v-for="(entry, index) in likeCharacters" 
              :key="`like-${entry.character.id}-${leaderboardStore.currentPage}`"
            >
              <HoverCardTrigger as-child>
                <TableRow class="font-medium text-center text-black">
                <TableCell class="font-medium">
                  {{ ((leaderboardStore.currentPage - 1) * leaderboardStore.pageSize) + index + 1 }}
                </TableCell>
                <TableCell class="h-24">
                  <img :src="entry.character.imageUrl" :alt="entry.character.name">
                </TableCell>
                <TableCell>{{ entry.character.name }}</TableCell>
                <TableCell class="">{{ entry.totalLikes }}</TableCell>
              </TableRow>
              </HoverCardTrigger>
              <HoverCardContent class="h-64 w-64 p-0 overflow-hidden bg-like-gradient">
                <span class="">{{ entry.character.level }}</span>
                <span class="">{{ entry.character.job }}</span>
                <span class="">{{ entry.character.world }}</span>
              </HoverCardContent>
            </HoverCard>
          </template>



          <!-- Nopes table
          <template v-else>
            <HoverCard 
              v-for="(entry, index) in nopeCharacters" 
              :key="`nope-${entry.character.id}-${leaderboardStore.currentPage}`"
            >
              <HoverCardTrigger as-child>
                <TableRow class="font-medium text-center">
                  <TableCell class="font-medium">
                  {{ ((leaderboardStore.currentPage - 1) * leaderboardStore.pageSize) + index + 1 }}
                  </TableCell>
                  <TableCell><img class="h-24 w-24" :src="entry.character.imageUrl" :alt="entry.character.name"></TableCell>
                  <TableCell>{{ entry.character.name }}</TableCell>
                  <TableCell>{{ entry.character.level }}</TableCell>
                  <TableCell>{{ entry.character.job }}</TableCell>
                  <TableCell>{{ entry.character.world }}</TableCell>
                  <TableCell class="">{{ entry.totalNopes }}</TableCell>
              </TableRow>
              </HoverCardTrigger>
              <HoverCardContent class="h-64 w-64 p-0 overflow-hidden bg-nope-gradient">
                <img class="h-64 w-64" :src="entry.character.imageUrl" :alt="entry.character.name">
              </HoverCardContent>
            </HoverCard>
          </template>
        </TableBody> -->
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
  await leaderboardStore.fetchTopLiked(); 
});

const likeCharacters = computed(() => leaderboardStore.likeCharacters);
const nopeCharacters = computed(() => leaderboardStore.nopeCharacters);
</script>