<template>
    <Dialog>
        <DialogTrigger as-child>
            <button class="absolute bottom-6 right-6 scale-[1.5]"><Icon icon="lucide:info"/></button>
        </DialogTrigger>
        <DialogContent class="bg-bg_primary border-border">
            <DialogHeader>
                <DialogTitle class="text-white">Rating outfits</DialogTitle>
            </DialogHeader>
            <DialogDescription class="text-white">
                Cards displaying a character's outfit will appear one by one. 
            </DialogDescription>
            <DialogDescription class="text-white">
                Rate the character's outfit by clicking the buttons underneath the card, swiping, or using the arrow keys.
            </DialogDescription>
            <DialogDescription class="text-white">
                Swiping up will count as liking the character's outfit while also marking that character as a favourite— saving it for later review.
            </DialogDescription>
            <DialogDescription class="text-white">
                Check the <a class="text-blue-400" href="/leaderboard">leaderboard</a> to see the top rated characters by the community!
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