<template>
    <AlertDialog>
        <AlertDialogTrigger as-child>
            <button class="absolute bottom-6 right-6 scale-[1.5]"><Icon icon="lucide:info"/></button>
        </AlertDialogTrigger>
        <AlertDialogContent class="bg-[#111418] border-[#21262e]">
            <AlertDialogHeader>
                <AlertDialogTitle class="text-white">Rating outfits</AlertDialogTitle>
            </AlertDialogHeader>
            <AlertDialogDescription>
                Cards displaying a character's outfit will appear one by one. 
            </AlertDialogDescription>
            <AlertDialogDescription>
                Rate the character's outfit by clicking the buttons underneath the card, swiping, or using the arrow keys.
            </AlertDialogDescription>
            <AlertDialogDescription>
                Swiping up will count as liking the character's outfit while also marking that character as a favourite— saving it for later review.
            </AlertDialogDescription>
            <AlertDialogDescription>
                Check the <a class="text-blue-400" href="/leaderboard">leaderboard</a> to see the top rated characters by the community!
            </AlertDialogDescription>
            <AlertDialogFooter>
                <AlertDialogAction class="bg-[#21262e] hover:bg-[#21262e] hover:text-[#ff3f29]">Okay!</AlertDialogAction>
            </AlertDialogFooter>
        </AlertDialogContent>
    </AlertDialog>
</template>

<script setup lang="ts">
import {
  AlertDialog,
  AlertDialogAction,
  AlertDialogCancel,
  AlertDialogContent,
  AlertDialogDescription,
  AlertDialogFooter,
  AlertDialogHeader,
  AlertDialogTitle,
  AlertDialogTrigger,
} from '@/components/ui/alert-dialog'
import { Icon } from "@iconify/vue/dist/iconify.js";
</script>