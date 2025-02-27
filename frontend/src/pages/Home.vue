<template>
    <main>
        <Analytics />
        <div class="min-h-screen">
            <!-- <RippleCircleBG/> -->
            <div class="">
                <NavBar :activeIndex="0"/>
                <HeroSection/>
                <Technologies/>
                <Experience/>
                <FeaturedProjects/>
                <Footer/>
            </div>
        </div>
    </main>
</template>

<script setup lang="ts">
import NavBar           from '@/components/NavBar.vue';
</script>