<template>
    <div class="bg-background flex flex-col justify-between px-2 h-[calc(100vh-var(--sidebar-nav-h))]">
      <form @submit.prevent="onSubmit">
        <h2 class="p-2 font-bold text-lg">Filter by:</h2>
        <FormField name="characterName" v-slot="{ componentField }">
          <div id="search" class="w-full p-2">
            <Input type="text" placeholder="Search for a character..." class="text-white" maxlength="12" v-bind="componentField"/>
          </div>
        </FormField>
        <FormField name="rankingType" v-slot="{ componentField }">
          <div class="w-full p-2">
            <span class="font-bold text-md">Ranking Type:</span>
            <RadioGroup class="flex justify-evenly mt-2" v-bind="componentField">
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
        </FormField>
        <FormField name="timeType" v-slot="{ componentField }">
          <div class="w-full p-2">
            <span class="font-bold text-md">Time Range:</span>
            <RadioGroup class="flex justify-evenly mt-2" v-bind="componentField">
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
        </FormField>
        <FormField name="classType" v-slot="{ componentField }">
          <div class="w-full p-2 text-black flex flex-col gap-2">
            <span class="font-bold text-md text-white">Class:</span>
            <Select v-bind="componentField">
              <SelectTrigger class="w-full">
                <SelectValue placeholder="Specify a class?" />
              </SelectTrigger>
              <SelectContent>
                <SelectGroup>
                  <SelectLabel>OTHER</SelectLabel>
                  <SelectItem value="all">All</SelectItem>
                  <SelectItem value="beginner">Beginner</SelectItem>
                  <SelectItem value="noblesse">Noblesse</SelectItem>
                  <SelectItem value="citizen">Citizen</SelectItem>
                  <SelectItem value="legend">Legend</SelectItem>
                </SelectGroup>
                <SelectGroup>
                  <SelectLabel>WARRIOR</SelectLabel>
                  <SelectItem v-for="job in Jobs.warriors" :key="job.value" :value="job.value">{{job.className}}</SelectItem>
                </SelectGroup>
                <SelectGroup>
                  <SelectLabel>MAGICIAN</SelectLabel>
                  <SelectItem v-for="job in Jobs.magicians" :key="job.value" :value="job.value">{{job.className}}</SelectItem>
                </SelectGroup>
                <SelectGroup>
                  <SelectLabel>ARCHER</SelectLabel>
                  <SelectItem v-for="job in Jobs.archers" :key="job.value" :value="job.value">{{job.className}}</SelectItem>
                </SelectGroup>
                <SelectGroup>
                  <SelectLabel>THIEF</SelectLabel>
                  <SelectItem v-for="job in Jobs.thiefs" :key="job.value" :value="job.value">{{job.className}}</SelectItem>
                </SelectGroup>
                <SelectGroup>
                  <SelectLabel>PIRATE</SelectLabel>
                  <SelectItem v-for="job in Jobs.pirates" :key="job.value" :value="job.value">{{job.className}}</SelectItem>
                </SelectGroup>
              </SelectContent>
            </Select>
          </div>
        </FormField>
        <FormField name="worldType" v-slot="{ componentField }">
          <div class="w-full p-2 text-black flex flex-col gap-2">
            <span class="font-bold text-md text-white">World:</span>
            <Select v-bind="componentField">
              <SelectTrigger class="w-full">
                <SelectValue placeholder="Specify a world?" />
              </SelectTrigger>
              <SelectContent>
                <SelectGroup>
                  <SelectLabel>OTHER</SelectLabel>
                  <SelectItem value="all">All</SelectItem>
                </SelectGroup>
                <SelectGroup>
                  <SelectLabel>INTERACTIVE</SelectLabel>
                  <SelectItem value="scania">Scania</SelectItem>
                  <SelectItem value="bera">Bera</SelectItem>
                  <SelectItem value="aurora">Aurora</SelectItem>
                  <SelectItem value="elysium">Elysium</SelectItem>
                </SelectGroup>
                <SelectGroup>
                  <SelectLabel>HEROIC</SelectLabel>
                  <SelectItem value="kronos">Kronos</SelectItem>
                  <SelectItem value="hyperion">Hyperion</SelectItem>
                </SelectGroup>
              </SelectContent>
            </Select>
          </div>
        </FormField>
        <div class="w-full p-2 flex justify-end">
          <Button type="submit" class="bg-nope-gradient font-bold text-md">Update
            <svg xmlns="http://www.w3.org/2000/svg" width="512" height="512" viewBox="0 0 512 512">
              <path fill="none" stroke="currentColor" stroke-linecap="round" stroke-miterlimit="10" stroke-width="64" d="M320 146s24.36-12-64-12a160 160 0 1 0 160 160" />
              <path fill="none" stroke="currentColor" stroke-linecap="round" stroke-linejoin="round" stroke-width="64" d="m256 58l80 80l-80 80" />
            </svg>
          </Button>
        </div>
      </form>
      <FooterSidebar/>
    </div>
</template>

<script setup lang="ts">
import { Input } from '@/components/ui/input'
import { Icon } from '@iconify/vue/dist/iconify.js';
import Button from "@/components/ui/button/Button.vue";
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
import FooterSidebar from './FooterSidebar.vue';

import { FormField } from '@/components/ui/form'
import { useForm } from 'vee-validate'
import { toTypedSchema } from '@vee-validate/zod'
import * as z from 'zod'
import { useLeaderboardStore } from "@/stores/leaderboardStore";

const leaderboardStore = useLeaderboardStore();

const formSchema = toTypedSchema(z.object({
  characterName: z.coerce.string(),
  rankingType: z.string(),
  timeType: z.string(),
  classType: z.string(),
  worldType: z.string(),
}))

const form = useForm({
  validationSchema: formSchema,
  initialValues: {
    characterName: '',
    rankingType: 'hotties',
    timeType: 'all',
    classType: 'all',
    worldType: 'all',
  },
})

const onSubmit = form.handleSubmit(
  (values) => {
    leaderboardStore.updateSearchParameters(values);
  }, 
  (errors) => {
    // TODO: flag? -> change to error msg
    console.log('Errors', errors);
  }
)

</script>