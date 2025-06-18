<template>
    <div class="flex justify-center mt-6">
        <Pagination 
          :items-per-page="leaderboardStore.pageSize" 
          :total="leaderboardStore.totalCount" 
          :page="leaderboardStore.currentPage"
          @update:page="onPageChange"
        >
          <PaginationContent>
            <PaginationPrevious />

            <template v-for="(item, index) in paginationItems" :key="index">
              <PaginationItem
                v-if="item.type === 'page'"
                :value="(item as PaginationPageItem).value"
                :is-active="(item as PaginationPageItem).value === leaderboardStore.currentPage"
              >
                {{ (item as PaginationPageItem).value }}
              </PaginationItem>
              <PaginationEllipsis v-else-if="item.type === 'ellipsis'" />
            </template>

            <PaginationNext />
          </PaginationContent>
        </Pagination>
      </div>
</template>

<script setup lang="ts">
import { computed } from "vue";
import { useLeaderboardStore } from "@/stores/leaderboardStore";
import {
  Pagination,
  PaginationContent,
  PaginationEllipsis,
  PaginationItem,
  PaginationNext,
  PaginationPrevious,
} from '@/components/ui/pagination'

// Define types for pagination items
interface PaginationPageItem {
  type: 'page';
  value: number;
}

interface PaginationEllipsisItem {
  type: 'ellipsis';
}

type PaginationItemType = PaginationPageItem | PaginationEllipsisItem;

const leaderboardStore = useLeaderboardStore();

// Generate pagination items for display
const paginationItems = computed((): PaginationItemType[] => {
  const items: PaginationItemType[] = [];
  const current = leaderboardStore.currentPage;
  const total = leaderboardStore.totalPages;
  
  // Simple pagination logic - show pages around current page
  const showPages = 5; // Show 5 pages at a time
  let start = Math.max(1, current - Math.floor(showPages / 2));
  let end = Math.min(total, start + showPages - 1);
  
  // Adjust start if we're near the end
  if (end - start + 1 < showPages && start > 1) {
    start = Math.max(1, end - showPages + 1);
  }
  
  // Add ellipsis at the beginning if needed
  if (start > 1) {
    items.push({ type: 'page', value: 1 });
    if (start > 2) {
      items.push({ type: 'ellipsis' });
    }
  }
  
  // Page numbers
  for (let i = start; i <= end; i++) {
    items.push({ type: 'page', value: i });
  }
  
  // Add ellipsis at the end if needed
  if (end < total) {
    if (end < total - 1) {
      items.push({ type: 'ellipsis' });
    }
    items.push({ type: 'page', value: total });
  }
  
  return items;
});

async function onPageChange(page: number) {
  leaderboardStore.setPage(page);
  await fetchCurrentPage();
}

async function fetchCurrentPage() {
  if (leaderboardStore.showLikes) {
    await leaderboardStore.fetchTopLiked();
  } else {
    await leaderboardStore.fetchTopNoped();
  }
}

</script>