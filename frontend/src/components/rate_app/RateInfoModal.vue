<template>
    <Dialog>
        <DialogTrigger as-child>
            <button class="absolute bottom-6 right-6 scale-[1.5]"><Icon icon="lucide:info"/></button>
        </DialogTrigger>
        <DialogContent class="bg-background border-border">
            <DialogHeader>
                <DialogTitle class="text-white">Rating Outfits</DialogTitle>
            </DialogHeader>
            <DialogDescription class="text-white">
                Cards displaying a character's outfit will appear one by one. 
            </DialogDescription>
            <DialogDescription class="text-white">
                Rate the character's outfit by clicking the buttons underneath the card, swiping, or using the arrow keys.
                Swiping up marks that character as a favourite.
            </DialogDescription>
            <DialogDescription class="text-white">
                Favourited characters count for twice as many likes.
                You only have 5 favourites a day, so only use them wisely!
            </DialogDescription>
            <DialogDescription class="text-white">
                Check the <a class="text-blue-400" href="/leaderboard">leaderboard</a> to see the top rated characters by the community!
            </DialogDescription>
            <DialogDescription class="text-slate-500 text-xs">
                Leaderboard rankings reset weekly and monthly. 
                <br/>
                Character sprite data is refreshed weekly.
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