<template>
    <Button variant="ghost" size="icon" @click="toggleTheme">
        <Icon icon="radix-icons:moon" class="h-[1.2rem] w-[1.2rem] rotate-0 transition-transform dark:-rotate-90 dark:scale-0" />
        <Icon icon="radix-icons:sun" class="absolute h-[1.2rem] w-[1.2rem] scale-0 transition-transform dark:rotate-0 dark:scale-100" />
    </Button>
</template>

<script setup lang="ts">
import { Button } from '@/components/ui/button'
import { Icon } from '@iconify/vue'
import { useColorMode } from '@vueuse/core'

const mode = useColorMode()

function toggleTheme() : void {
    console.log(`Current mode is ${mode.value}. Changing.`);
    mode.value = mode.value === 'dark' ? 'light' : 'dark'
}
</script>
