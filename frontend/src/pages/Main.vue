<template>
    <div class="bg-background min-h-screen flex flex-row">
        <aside class="min-w-80 max-w-96 w-full">
            <nav class="bg-red-300 flex items-center justify-evenly py-6 h-[--navbar-h]">
                <Button>1</Button>
                <Button>2</Button>
                <Button>3</Button>
            </nav>
            <div class="bg-blue-300 flex flex-col items-start h-[calc(100vh-var(--navbar-h))]">
                <div class="py-2 flex items-center justify-center">
                    <a href="#" class="ml-6">Matches</a>
                    <a href="#" class="ml-6">Messages</a>
                </div>
                <div class="bg-green-400 h-full w-full">
                    
                </div>
            </div>
        </aside>
        <div class="w-full h-screen">
            <main class="w-full h-screen flex justify-center items-center">
                <div class="grid justify-center items-center w-full">
                    <SwipeCard v-for="card in cardData" :key="card.id" 
                    :card="card" :isFront="card.id === cardData[cardData.length - 1].id" 
                    @remove="removeCard" />
                </div>
            </main>
        
        </div>
    </div>
</template>

<script setup lang="ts">
import { ref } from "vue";
import Button from "@/components/ui/button/Button.vue";
import SwipeCard from '@/components/SwipeCard.vue';

const cardData = ref([
  { id: 1, url: 'https://images.unsplash.com/photo-1542291026-7eec264c27ff?q=80&w=2370&auto=format&fit=crop' },
  { id: 2, url: 'https://images.unsplash.com/photo-1512374382149-233c42b6a83b?q=80&w=2235&auto=format&fit=crop' },
  { id: 3, url: 'https://images.unsplash.com/photo-1539185441755-769473a23570?q=80&w=2342&auto=format&fit=crop' },
  { id: 4, url: 'https://images.unsplash.com/photo-1549298916-b41d501d3772?q=80&w=2224&auto=format&fit=crop' },
]);

const removeCard = (id) => {
  cardData.value = cardData.value.filter((card) => card.id !== id);
};
</script>
