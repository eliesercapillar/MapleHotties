<template>
    <section class="flex gap-6">
        <a v-for="t in Socials.data" :key="t.name" :href="t.link" target="_blank" 
        class="text-muted-foreground hover:text-foreground">
                <Icon :icon="t.icon" class="size-6"></Icon>
        </a>
    </section>
</template> 

<script setup lang="ts">
import Socials from "@/data/Socials.json";
import { Icon } from '@iconify/vue';
</script>