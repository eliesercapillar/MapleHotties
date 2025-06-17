<template>
  <main class="w-full h-screen bg-black-grey-radial text-white flex flex-col items-center justify-center">
    <h1 class="font-medium text-4xl text-center py-4">The Hottest Characters of the week!</h1>
    <div class="w-[60rem]">
      <Table class="select-none">
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
          <TableRow v-for="(entry, id) in testList" :key="id">
            <TableCell class="font-medium">{{ entry.character.id }}</TableCell>
            <TableCell>
              <HoverCard>
                <HoverCardTrigger class="text-blue-400">{{ entry.character.name }}</HoverCardTrigger>
                <HoverCardContent class="h-64 w-64 p-0">
                  <img class="h-64 w-64" :src="entry.character.imageUrl">
                </HoverCardContent>
              </HoverCard>
            </TableCell>
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
import {
  HoverCard,
  HoverCardContent,
  HoverCardTrigger,
} from '@/components/ui/hover-card'

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

const testList = [
  {
    totalCount: '100',
    character: {
      id: 1,
      name: "CHARACTER 1",
      level: 300,
      job: "Dual Blade",
      world: "Kronos",
      imageUrl: "https://i.mapleranks.com/u/HJPKNMCLGHPGJFODCAGJAKPKCDLCJBCAACIJKMBDGDCBGGDPLIMAJLEGNJKKMNLGDABBGLGNNKLJCIJHCGMKEPHAIKNAJKFHPJPDHPCDGBGLPGCCNCBJFEHGKHHCJOLJPDNKFNAKCPAFDICIKJMMGJCKIJPCLLNHEAILIKPGNALMAOIHEBFDPHHOAOGCBKMBMPFBPKMOFHNFAJLDNAKPOPCOJNLEGOLKHJDFMLDBJDBFFIKFIMJNFPJAMAIFOFLC.png",
    }
  },
  {
    totalCount: '100',
    character: {
      id: 2,
      name: "CHARACTER 2",
      level: 300,
      job: "Dual Blade",
      world: "Kronos",
      imageUrl: "https://i.mapleranks.com/u/HJPKNMCLGHPGJFODCAGJAKPKCDLCJBCAACIJKMBDGDCBGGDPLIMAJLEGNJKKMNLGDABBGLGNNKLJCIJHCGMKEPHAIKNAJKFHPJPDHPCDGBGLPGCCNCBJFEHGKHHCJOLJPDNKFNAKCPAFDICIKJMMGJCKIJPCLLNHEAILIKPGNALMAOIHEBFDPHHOAOGCBKMBMPFBPKMOFHNFAJLDNAKPOPCOJNLEGOLKHJDFMLDBJDBFFIKFIMJNFPJAMAIFOFLC.png",
    }
  },
  {
    totalCount: '100',
    character: {
      id: 3,
      name: "CHARACTER 3",
      level: 300,
      job: "Dual Blade",
      world: "Kronos",
      imageUrl: "https://i.mapleranks.com/u/HJPKNMCLGHPGJFODCAGJAKPKCDLCJBCAACIJKMBDGDCBGGDPLIMAJLEGNJKKMNLGDABBGLGNNKLJCIJHCGMKEPHAIKNAJKFHPJPDHPCDGBGLPGCCNCBJFEHGKHHCJOLJPDNKFNAKCPAFDICIKJMMGJCKIJPCLLNHEAILIKPGNALMAOIHEBFDPHHOAOGCBKMBMPFBPKMOFHNFAJLDNAKPOPCOJNLEGOLKHJDFMLDBJDBFFIKFIMJNFPJAMAIFOFLC.png",
    }
  },
  {
    totalCount: '100',
    character: {
      id: 4,
      name: "CHARACTER 4",
      level: 300,
      job: "Dual Blade",
      world: "Kronos",
      imageUrl: "https://i.mapleranks.com/u/HJPKNMCLGHPGJFODCAGJAKPKCDLCJBCAACIJKMBDGDCBGGDPLIMAJLEGNJKKMNLGDABBGLGNNKLJCIJHCGMKEPHAIKNAJKFHPJPDHPCDGBGLPGCCNCBJFEHGKHHCJOLJPDNKFNAKCPAFDICIKJMMGJCKIJPCLLNHEAILIKPGNALMAOIHEBFDPHHOAOGCBKMBMPFBPKMOFHNFAJLDNAKPOPCOJNLEGOLKHJDFMLDBJDBFFIKFIMJNFPJAMAIFOFLC.png",
    }
  },
  {
    totalCount: '100',
    character: {
      id: 5,
      name: "CHARACTER 5",
      level: 300,
      job: "Dual Blade",
      world: "Kronos",
      imageUrl: "https://i.mapleranks.com/u/HJPKNMCLGHPGJFODCAGJAKPKCDLCJBCAACIJKMBDGDCBGGDPLIMAJLEGNJKKMNLGDABBGLGNNKLJCIJHCGMKEPHAIKNAJKFHPJPDHPCDGBGLPGCCNCBJFEHGKHHCJOLJPDNKFNAKCPAFDICIKJMMGJCKIJPCLLNHEAILIKPGNALMAOIHEBFDPHHOAOGCBKMBMPFBPKMOFHNFAJLDNAKPOPCOJNLEGOLKHJDFMLDBJDBFFIKFIMJNFPJAMAIFOFLC.png",
    }
  },
  {
    totalCount: '100',
    character: {
      id: 6,
      name: "CHARACTER 6",
      level: 300,
      job: "Dual Blade",
      world: "Kronos",
      imageUrl: "https://i.mapleranks.com/u/HJPKNMCLGHPGJFODCAGJAKPKCDLCJBCAACIJKMBDGDCBGGDPLIMAJLEGNJKKMNLGDABBGLGNNKLJCIJHCGMKEPHAIKNAJKFHPJPDHPCDGBGLPGCCNCBJFEHGKHHCJOLJPDNKFNAKCPAFDICIKJMMGJCKIJPCLLNHEAILIKPGNALMAOIHEBFDPHHOAOGCBKMBMPFBPKMOFHNFAJLDNAKPOPCOJNLEGOLKHJDFMLDBJDBFFIKFIMJNFPJAMAIFOFLC.png",
    }
  },
  {
    totalCount: '100',
    character: {
      id: 7,
      name: "CHARACTER 7",
      level: 300,
      job: "Dual Blade",
      world: "Kronos",
      imageUrl: "https://i.mapleranks.com/u/HJPKNMCLGHPGJFODCAGJAKPKCDLCJBCAACIJKMBDGDCBGGDPLIMAJLEGNJKKMNLGDABBGLGNNKLJCIJHCGMKEPHAIKNAJKFHPJPDHPCDGBGLPGCCNCBJFEHGKHHCJOLJPDNKFNAKCPAFDICIKJMMGJCKIJPCLLNHEAILIKPGNALMAOIHEBFDPHHOAOGCBKMBMPFBPKMOFHNFAJLDNAKPOPCOJNLEGOLKHJDFMLDBJDBFFIKFIMJNFPJAMAIFOFLC.png",
    }
  },
  {
    totalCount: '100',
    character: {
      id: 8,
      name: "CHARACTER 8",
      level: 300,
      job: "Dual Blade",
      world: "Kronos",
      imageUrl: "https://i.mapleranks.com/u/HJPKNMCLGHPGJFODCAGJAKPKCDLCJBCAACIJKMBDGDCBGGDPLIMAJLEGNJKKMNLGDABBGLGNNKLJCIJHCGMKEPHAIKNAJKFHPJPDHPCDGBGLPGCCNCBJFEHGKHHCJOLJPDNKFNAKCPAFDICIKJMMGJCKIJPCLLNHEAILIKPGNALMAOIHEBFDPHHOAOGCBKMBMPFBPKMOFHNFAJLDNAKPOPCOJNLEGOLKHJDFMLDBJDBFFIKFIMJNFPJAMAIFOFLC.png",
    }
  },
  {
    totalCount: '100',
    character: {
      id: 9,
      name: "CHARACTER 9",
      level: 300,
      job: "Dual Blade",
      world: "Kronos",
      imageUrl: "https://i.mapleranks.com/u/HJPKNMCLGHPGJFODCAGJAKPKCDLCJBCAACIJKMBDGDCBGGDPLIMAJLEGNJKKMNLGDABBGLGNNKLJCIJHCGMKEPHAIKNAJKFHPJPDHPCDGBGLPGCCNCBJFEHGKHHCJOLJPDNKFNAKCPAFDICIKJMMGJCKIJPCLLNHEAILIKPGNALMAOIHEBFDPHHOAOGCBKMBMPFBPKMOFHNFAJLDNAKPOPCOJNLEGOLKHJDFMLDBJDBFFIKFIMJNFPJAMAIFOFLC.png",
    }
  },
  {
    totalCount: '100',
    character: {
      id: 10,
      name: "CHARACTER 10",
      level: 300,
      job: "Dual Blade",
      world: "Kronos",
      imageUrl: "https://i.mapleranks.com/u/HJPKNMCLGHPGJFODCAGJAKPKCDLCJBCAACIJKMBDGDCBGGDPLIMAJLEGNJKKMNLGDABBGLGNNKLJCIJHCGMKEPHAIKNAJKFHPJPDHPCDGBGLPGCCNCBJFEHGKHHCJOLJPDNKFNAKCPAFDICIKJMMGJCKIJPCLLNHEAILIKPGNALMAOIHEBFDPHHOAOGCBKMBMPFBPKMOFHNFAJLDNAKPOPCOJNLEGOLKHJDFMLDBJDBFFIKFIMJNFPJAMAIFOFLC.png",
    }
  },
]
</script>