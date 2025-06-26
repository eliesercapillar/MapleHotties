<template>
  <section
    v-auto-animate
    class="w-full flex justify-center items-center gap-5"
  >
    <button
      class="h-[24px] bg-nope-gradient px-3 rounded-xl 
      text-center text-sm font-medium text-white"
      @click="showInstructions = !showInstructions"
    >
      {{ showInstructions ? "Hide" : "Show" }}
    </button>
    <ul v-if="showInstructions" class="flex justify-center items-center gap-5">
      <!-- Left Arrow -->
      <li class="flex justify-center items-center gap-1 text-nope">
        <svg
          xmlns="http://www.w3.org/2000/svg"
          focusable="false"
          aria-hidden="true"
          role="img"
          viewBox="0 0 24 24"
          width="24px"
          height="24px"
          aria-labelledby="757fbe4796a15b6c"
        >
          <g transform="translate(2 2) rotate(0 10 10)" fill="none">
            <path
              d="M5.368 11.041l1.227.985 1.785 1.433 1.226.984c.491.395.893.208.893-.413v-1.908c.743-.106 3.574-.444 4.198-.533.742-.106 1.29-.568 1.29-1.262v-.003c0-.695-.548-1.157-1.29-1.263-.624-.089-3.455-.427-4.198-.533V6.62c0-.621-.401-.807-.892-.413L8.38 7.19 6.595 8.624l-1.227.984c-.49.395-.49 1.04 0 1.433"
              fill="currentColor"
              fill-rule="nonzero"
            />
            <rect
              stroke="currentColor"
              stroke-width="2.5"
              width="20"
              height="20"
              rx="3"
            />
          </g>
        </svg>
        <span class="select-none">Nope</span>
      </li>
      <!-- Up Arrow -->
      <li class="flex justify-center items-center gap-1 text-favourite">
        <svg
          width="24px"
          height="24px"
          viewBox="0 0 24 24"
          focusable="false"
          aria-hidden="true"
          role="img"
          xmlns="http://www.w3.org/2000/svg"
        >
          <g transform="translate(2 2) rotate(90 10 10)" fill="none">
            <path
              d="M5.368 11.041l1.227.985 1.785 1.433 1.226.984c.491.395.893.208.893-.413v-1.908c.743-.106 3.574-.444 4.198-.533.742-.106 1.29-.568 1.29-1.262v-.003c0-.695-.548-1.157-1.29-1.263-.624-.089-3.455-.427-4.198-.533V6.62c0-.621-.401-.807-.892-.413L8.38 7.19 6.595 8.624l-1.227.984c-.49.395-.49 1.04 0 1.433"
              fill="currentColor"
              fill-rule="nonzero"
            />
            <rect
              stroke="currentColor"
              stroke-width="2.5"
              width="20"
              height="20"
              rx="3"
            />
          </g>
        </svg>
        <span class="select-none">Favourite</span>
      </li>
      <!-- Right Arrow -->
      <li class="flex justify-center items-center gap-1 text-like">
        <svg
          xmlns="http://www.w3.org/2000/svg"
          width="24px"
          height="24px"
          viewBox="0 0 24 24"
          focusable="false"
          aria-hidden="true"
          role="img"
          aria-labelledby="title-right"
        >
          <g transform="translate(2 2) rotate(180 10 10)" fill="none">
            <path
              d="M5.368 11.041l1.227.985 1.785 1.433 1.226.984c.491.395.893.208.893-.413v-1.908c.743-.106 3.574-.444 4.198-.533.742-.106 1.29-.568 1.29-1.262v-.003c0-.695-.548-1.157-1.29-1.263-.624-.089-3.455-.427-4.198-.533V6.62c0-.621-.401-.807-.892-.413L8.38 7.19 6.595 8.624l-1.227.984c-.49.395-.49 1.04 0 1.433"
              fill="currentColor"
              fill-rule="nonzero"
            />
            <rect
              stroke="currentColor"
              stroke-width="2.5"
              width="20"
              height="20"
              rx="3"
            />
          </g>
        </svg>
        <span class="select-none">Like</span>
      </li>
      <!-- Spacebar -->
      <!-- <li class="flex justify-center items-center gap-1">
        <svg
          width="74"
          height="24"
          viewBox="0 0 74 24"
          focusable="false"
          aria-hidden="true"
          role="img"
          xmlns="http://www.w3.org/2000/svg"
        >
          <rect
            stroke="currentColor"
            stroke-width="2.5"
            transform="rotate(-90 37 12)"
            x="27"
            y="-23"
            width="20"
            height="70"
            rx="3"
            fill="none"
          />
        </svg>
        <span class="select-none">Outfit Information</span>
      </li> -->
    </ul>
  </section>
</template>

<script setup lang="ts">
import { vAutoAnimate } from "@formkit/auto-animate/vue";
import { ref } from "vue";

const showInstructions = ref(true);
</script>
