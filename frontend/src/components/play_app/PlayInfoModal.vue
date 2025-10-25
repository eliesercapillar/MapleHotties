<template>
    <Dialog>
        <DialogTrigger as-child>
            <button class="absolute bottom-6 right-6 scale-[1.5]"><Icon icon="lucide:info"/></button>
        </DialogTrigger>
        <DialogContent class="bg-background border-border">
            <DialogHeader>
                <DialogTitle class="text-white">How To Play</DialogTitle>
            </DialogHeader>
            <DialogDescription class="text-white">
                Given two characters, you must decide whether or not the card on the right has a higher or lower All-time Rating than the card on the left.
            </DialogDescription>
            <DialogDescription class="text-white">
                Lock in your answer by clicking the top or bottom half of the card on the right, or using the up or down arrow keys.
            </DialogDescription>
            <DialogDescription class="text-white">
                New characters will be shown as you progress, with each being compared to the previous card.
            </DialogDescription>
            <DialogDescription class="text-white">
                Try to get as many correct as you can. The game ends when you get one wrong.
            </DialogDescription>
            <DialogDescription class="text-slate-500 text-xs italic">
                All-Time Ratings are determined by the <RouterLink class="text-blue-400" to="/app/leaderboard">community</RouterLink> and may change between play sessions.
            </DialogDescription>
        </DialogContent>
    </Dialog>
</template>

<script setup lang="ts">
import {
  Dialog,
  DialogContent,
  DialogDescription,
  DialogHeader,
  DialogTitle,
  DialogTrigger,
} from '@/components/ui/dialog'
import { Icon } from "@iconify/vue/dist/iconify.js";
</script>