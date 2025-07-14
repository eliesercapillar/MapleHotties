<template>
  <aside class="min-w-80 max-w-96 w-full border-r-[1px] border-[#21262e]">
    <nav class="flex items-center justify-evenly py-6 h-[--sidebar-nav-h] bg-nope-gradient">
          <!-- TODO: icon of current page should be set to #ff3f29 -->
      <button class="rounded-full bg-button_primary hover:text-like hover:scale-105 p-3" @click=""><Icon icon="bxs:heart" class="scale-[1.25]"/></button>
      <button class="rounded-full bg-button_primary hover:text-nope hover:scale-105 p-3" @click=""><Icon icon="icon-park-solid:game-handle" class="scale-[1.25]"/></button>
      <button class="rounded-full bg-button_primary hover:text-favourite hover:scale-105 p-3" @click=""><Icon icon="icon-park-solid:five-star-badge" class="scale-[1.25]"/></button>
      <button class="rounded-full bg-button_primary hover:text-button_highlight hover:scale-105 p-3" @click=""><Icon icon="line-md:log-out" class="scale-[1.25]"/></button>
    </nav>
    <div class="bg-[#111418] flex flex-col justify-between px-2 h-[calc(100vh-var(--sidebar-nav-h))]">
      <div>
        <div id="search" class="w-full p-2">
          <Input type="text" placeholder="Search for a character..." class="text-black"/>
        </div>
        <div class="w-full p-2">
          <span class="font-bold text-md">Search for:</span>
          <RadioGroup default-value="hotties" class="flex justify-evenly mt-2">
            <div class="flex items-center gap-2">
              <RadioGroupItem id="hotties" value="hotties" class="border-white text-white"/>
              <Label for="hotties">Hotties</Label>
            </div>
            <div class="flex items-center space-x-2">
              <RadioGroupItem id="notties" value="notties" class="border-white text-white"/>
              <Label for="notties">Notties</Label>
            </div>
            <div class="flex items-center space-x-2">
              <RadioGroupItem id="favourites" value="favourites" class="border-white text-white"/>
              <Label for="favourites">Favourites</Label>
            </div>
          </RadioGroup>
        </div>
        <div class="w-full p-2">
          <span class="font-bold text-md">Time:</span>
          <RadioGroup default-value="all" class="flex justify-evenly mt-2">
            <div class="flex items-center gap-2">
              <RadioGroupItem id="weekly" value="weekly" class="border-white text-white"/>
              <Label for="weekly">Weekly</Label>
            </div>
            <div class="flex items-center space-x-2">
              <RadioGroupItem id="monthly" value="monthly" class="border-white text-white"/>
              <Label for="monthly">Monthly</Label>
            </div>
            <div class="flex items-center space-x-2">
              <RadioGroupItem id="all" value="all" class="border-white text-white"/>
              <Label for="all">All Time</Label>
            </div>
          </RadioGroup>
        </div>
        <div class="w-full p-2 text-black">
          <span class="font-bold text-md text-white">Class:</span>
          <Select class="mt-2" default-value="all">
            <SelectTrigger class="w-full">
              <SelectValue placeholder="Specify a class?" />
            </SelectTrigger>
            <SelectContent>
              <SelectGroup>
                <SelectLabel>OTHER</SelectLabel>
                <SelectItem value="all">
                  All
                </SelectItem>
                <SelectItem value="beginner">
                  Beginner
                </SelectItem>
              </SelectGroup>
              <SelectGroup>
                <SelectLabel>WARRIOR</SelectLabel>
                <SelectItem v-for="job in Jobs.warriors" :value="job.value">
                  {{job.className}}
                </SelectItem>
              </SelectGroup>
              <SelectGroup>
                <SelectLabel>MAGICIAN</SelectLabel>
                <SelectItem v-for="job in Jobs.magicians" :value="job.value">
                  {{job.className}}
                </SelectItem>
              </SelectGroup>
              <SelectGroup>
                <SelectLabel>ARCHER</SelectLabel>
                <SelectItem v-for="job in Jobs.archers" :value="job.value">
                  {{job.className}}
                </SelectItem>
              </SelectGroup>
              <SelectGroup>
                <SelectLabel>THIEF</SelectLabel>
                <SelectItem v-for="job in Jobs.thiefs" :value="job.value">
                  {{job.className}}
                </SelectItem>
              </SelectGroup>
              <SelectGroup>
                <SelectLabel>PIRATE</SelectLabel>
                <SelectItem v-for="job in Jobs.pirates" :value="job.value">
                  {{job.className}}
                </SelectItem>
              </SelectGroup>
            </SelectContent>
          </Select>
        </div>
        <div class="w-full p-2 text-black">
          <span class="font-bold text-md text-white">World:</span>
          <Select class="mt-2" default-value="all">
            <SelectTrigger class="w-full">
              <SelectValue placeholder="Specify a world?" />
            </SelectTrigger>
            <SelectContent>
              <SelectGroup>
                <SelectLabel>OTHER</SelectLabel>
                <SelectItem value="all">
                  All
                </SelectItem>
              </SelectGroup>
              <SelectGroup>
                <SelectLabel>INTERACTIVE</SelectLabel>
                <SelectItem value="scania">
                  Scania
                </SelectItem>
                <SelectItem value="bera">
                  Bera
                </SelectItem>
                <SelectItem value="aurora">
                  Aurora
                </SelectItem>
                <SelectItem value="elysium">
                  Elysium
                </SelectItem>
              </SelectGroup>
              <SelectGroup>
                <SelectLabel>HEROIC</SelectLabel>
                <SelectItem value="kronos">
                  Kronos
                </SelectItem>
                <SelectItem value="hyperion">
                  Hyperion
                </SelectItem>
              </SelectGroup>
            </SelectContent>
          </Select>
        </div>
        <div class="w-full p-2 flex justify-end">
          <Button class="bg-nope-gradient font-bold">Search</Button>
        </div>
      </div>
      <footer class="w-full p-2 text-white">
        <div>
          <div class="flex items-center gap-2">
            <img src="/logos/mt_mushroom.png" class="w-10 h-10"/>
            <span class="text-2xl">MapleHotties</span>
          </div>
          <p class="text-md my-2">The hottest players in Maplestory GMS.</p>
          <Socials class="pt-2"/>
        </div>
        <div class="pt-2">
          <p class="font-thin text-xs">© 2025 maplehotties.com | All rights reserved.</p>
          <div class="pt-1">
            <p class="font-thin text-xs">Made with ❤️ by KPJS</p>
            <p class="font-thin text-xs pt-1">Any suggestions? Join the <a href="@" class="text-blue-400">community Discord server!</a></p>
          </div>
        </div>
      </footer>
    </div>
  </aside>
</template>

<script setup lang="ts">
import { ref } from "vue";
import { useLeaderboardStore } from "@/stores/leaderboardStore";
import { Input } from '@/components/ui/input'
import { Icon } from "@iconify/vue/dist/iconify.js";
import Button from "@/components/ui/button/Button.vue";
import Checkbox from "../ui/checkbox/Checkbox.vue";
import { Label } from '@/components/ui/label'
import { RadioGroup, RadioGroupItem } from '@/components/ui/radio-group'
import {
  Select,
  SelectContent,
  SelectGroup,
  SelectItem,
  SelectLabel,
  SelectTrigger,
  SelectValue,
} from '@/components/ui/select'
import Jobs from "@/data/Jobs.json";
import router from "@/router"
import Socials from "@/components/Socials.vue";
import { RouterLink } from "vue-router";

const showStats = ref(false);

const leaderboardStore = useLeaderboardStore();

async function toggleView(likes: boolean) {
  leaderboardStore.showLikes = likes;
  leaderboardStore.setPage(1); // Reset to first page when switching views
  
  if (likes) await leaderboardStore.fetchTopLiked();
  else       await leaderboardStore.fetchTopNoped();
}
</script>