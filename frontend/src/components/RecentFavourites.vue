<template>
    <section id="recent_favourites" class="grid grid-cols-2 gap-2 flex-grow">
        <Card class="flex flex-col justify-center">
            <CardHeader class="text-center">
                <CardTitle class="text-md">ROCKOGUY</CardTitle>
                <CardDescription>290 Adele</CardDescription>
            </CardHeader>
            <img src="/rockoguy.png">
        </Card>
    </section>
</template>

<script setup lang="ts">
import {
  Card,
  CardContent,
  CardDescription,
  CardFooter,
  CardHeader,
  CardTitle,
} from '@/components/ui/card'
</script>