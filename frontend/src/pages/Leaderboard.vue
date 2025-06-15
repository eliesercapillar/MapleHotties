<template>
  <main class="w-full h-screen bg-black-grey-radial text-white flex flex-col items-center justify-center">
    <h1 class="font-medium text-4xl text-center py-4">The Hottest Characters of the week!</h1>
    <div class="w-[60rem]">
      <Table>
        <TableCaption>A list of your recent invoices.</TableCaption>
        <TableHeader>
          <TableRow>
            <TableHead class="w-[100px]">Rank</TableHead>
            <TableHead>IGN</TableHead>
            <TableHead>Level</TableHead>
            <TableHead>Job</TableHead>
            <TableHead>World</TableHead>
            <TableHead class="text-right">Loves</TableHead>
          </TableRow>
        </TableHeader>
        <TableBody>
          <TableRow v-for="(entry, id) in leaderboardStore.likeCharacters" :key="id">
            <TableCell class="font-medium">{{ entry.character.id }}</TableCell>
            <TableCell>{{ entry.character.name }}</TableCell>
            <TableCell>{{ entry.character.level }}</TableCell>
            <TableCell>{{ entry.character.job }}</TableCell>
            <TableCell>{{ entry.character.world }}</TableCell>
            <TableCell class="text-right">{{ entry.totalCount }}</TableCell>
          </TableRow>
        </TableBody>
      </Table>
    </div>
  </main>
</template>
  
<script setup lang="ts">
import { onMounted, watch, ref } from "vue";
import { useLeaderboardStore } from "@/stores/leaderboardStore";
import AppSidebar from '@/components/AppSidebar.vue'
import {
  Table,
  TableBody,
  TableCaption,
  TableCell,
  TableHead,
  TableHeader,
  TableRow,
} from '@/components/ui/table'

const leaderboardStore = useLeaderboardStore();
const showLikes = ref(true);

onMounted(async () => { leaderboardStore.fetchTopLiked(); })

function fetchPage() {
  showLikes ? leaderboardStore.fetchTopLiked() : leaderboardStore.fetchTopNoped()
}

function showNextPage() {
  leaderboardStore.currentPage++;
  fetchPage();
}

function showLastPage() {
  if (leaderboardStore.currentPage > 0) leaderboardStore.currentPage--;
  fetchPage();
}

const invoices = [
  {
    invoice: 'INV001',
    paymentStatus: 'Paid',
    totalAmount: '$250.00',
    paymentMethod: 'Credit Card',
  },
  {
    invoice: 'INV002',
    paymentStatus: 'Pending',
    totalAmount: '$150.00',
    paymentMethod: 'PayPal',
  },
  {
    invoice: 'INV003',
    paymentStatus: 'Unpaid',
    totalAmount: '$350.00',
    paymentMethod: 'Bank Transfer',
  },
  {
    invoice: 'INV004',
    paymentStatus: 'Paid',
    totalAmount: '$450.00',
    paymentMethod: 'Credit Card',
  },
  {
    invoice: 'INV005',
    paymentStatus: 'Paid',
    totalAmount: '$550.00',
    paymentMethod: 'PayPal',
  },
  {
    invoice: 'INV006',
    paymentStatus: 'Pending',
    totalAmount: '$200.00',
    paymentMethod: 'Bank Transfer',
  },
  {
    invoice: 'INV007',
    paymentStatus: 'Unpaid',
    totalAmount: '$300.00',
    paymentMethod: 'Credit Card',
  },
]
</script>