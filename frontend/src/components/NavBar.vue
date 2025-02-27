<template>
    <header class="z-50 py-6 border-b-2 w-full">
        <div>
            <nav class="w-full flex justify-between items-center">
                <!-- LHS -->
                <div class="flex items-center gap-4">
                    <RouterLink to="/">
                        <!-- SVG Logo -->
                    </RouterLink>
                    <ul class="flex gap-4 sm:gap-8">
                        <li v-for="(page, index) in Pages.data" 
                            :key="page.name" 
                            :class="['font-supreme font-normal', 
                            index === activeIndex ? 'text-foreground cursor-pointer' : 'text-muted-foreground hover:opacity-50']">
                                <RouterLink v-if="index !== activeIndex" :to="page.href">{{ page.name }}</RouterLink>
                                <span v-else>{{ page.name }}</span>
                        </li>
                    </ul>
                </div>
                <!-- RHS -->
                <div class="items-center">
                    <RouterLink to="/login" class="text-muted-foreground hover:opacity-50">Login</RouterLink>
                </div>
            </nav>
        </div>
        <nav class="flex items-center px-6 py-4 bg-white shadow-md">
            <!-- Logo Placeholder -->
            <div class="w-10 h-10 bg-gray-300 rounded-full"></div>
            
            <!-- Navigation Links -->
            <div class="flex gap-6 ml-6">
            <RouterLink v-for="page in Pages.data" :key="page.href" :to="page.href" class="text-gray-700 hover:text-blue-500">
                {{ page.name }}
            </RouterLink>
            </div>
            
            <!-- Spacer to push Login to the right -->
            <div class="flex-grow w-full"></div>
            
            <!-- Login Link -->
            <RouterLink to="/login" class="text-gray-700 hover:text-blue-500">Login</RouterLink>
        </nav>
        <!-- <div class="flex items-center justify-between">
            <div>
                <nav class="flex items-center justify-between">
                    <ul class="flex gap-4 sm:gap-8">
                        <li v-for="(page, index) in Pages.data" 
                        :key="page.name" 
                        :class="['font-supreme font-normal', 
                        index === activeIndex ? 'text-foreground cursor-pointer' : 'text-muted-foreground hover:opacity-50']">
                            <a v-if="index !== activeIndex" :href="page.href">{{ page.name }}</a>
                            <span v-else>{{ page.name }}</span>
                        </li>
                    </ul>
                    <div>
                        <ModeToggle/>
                    </div>
                </nav>
            </div>
            <div>
            </div>
        </div> -->
    </header>

</template>

<script setup lang="ts">
import { ref } from 'vue';
import Pages from '@/data/Pages.json';
import { RouterLink } from 'vue-router';

defineProps<{
    activeIndex?: number; // Optional prop (defaults to -1)
}>();
</script>