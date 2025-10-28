<template>
    <main class="relative w-full h-screen flex flex-col items-center justify-evenly overflow-hidden">
      <div id="description">
        <h1>Higher or Lower</h1>
        <p> A game of higher or lower based on character ratings determined by the community!</p>

      </div>
      <h3>How to Play</h3>
      <div id="instructions" class="grid grid-rows-2 grid-cols-2 items-center justify-center">
        <div class="p-4 flex flex-col items-center justify-center">
          <span>Given two characters, decide whether or not the card on the right has a higher or lower All-time <span class="text-like">Like Rating</span> than the card on the left.</span>
          <div class="flex items-center justify-center select-none">
            <InfoCharacterPlayCard 
              :type="'default'"
              :name="'Xæra'"
              :level="285"
              :job="'Xenon'"
              :image-url="'https://msavatar1.nexon.net/Character/MMANDIACIHJNNOPKMBKOPNEFFFBFCPKGDBLOKKLHDCEBDCBBNEPMIOEAGBOKIGAFAOMFEEPICGIJGGMFNCIOLLEBLGPCLCJHNFAIAOBEOICGMLDFDFOLCGCHCFPIGEEAJHPOHEBINNCPAGBOJMNGCAIBFKCIKKHFIFFGCIFAHMJPECIAHFJHIJGCMLKDMPGIPGOGHLMLDDBNIMCPJFACGFPPBHNAFFDMJAANFKIKEOCOEBPMOKBLNCEJLADFCGKC.png'"
              :like-count="'5,000'"
            />
            <div class="grid items-center justify-center w-12 h-12">
              <span class="row-[1] col-[1] z-[1] text-white text-bold text-md text-center">VS</span>
            </div>
            <InfoCharacterPlayCard 
              :type="'animatedCursor'"
              :name="'ROCKOGUY'"
              :level="292"
              :job="'Adele'"
              :image-url="'https://msavatar1.nexon.net/Character/LCAPKANJACELHFCMBJHENCNGLDDHEMHEGKEGEGACAGFEBIHPJPLKHOJEFKEMDEJCCLJALHLFHIGGCJGMIOEKEFLCLEMLNDNJDJLPHBEFKOKPIGHEGCPCDFIEPPOGLMJMLEKFGDKPPJHCOGKODMHCBJAMGLDDGMLDILNEICKGBBMBGKBBHFOGAKKGEAIFDHNIBHAMJPKHHNDNLKEHPHHADKJLFNKCEIEEGBPKPEOJLFHFGMJADOFKHIBEPFOOJKEN.png'"
              :like-count="'???'"
            />
          </div>
        </div>
        <div class="p-4 flex flex-col items-center justify-center">
          <span>Lock in your answer by clicking the top or bottom half of the card on the right, or using the up or down arrow keys.</span>
          <div class="flex items-center justify-center select-none">
            <InfoCharacterPlayCard 
              :type="'static'"
              :name="'ROCKOGUY'"
              :level="292"
              :job="'Adele'"
              :image-url="'https://msavatar1.nexon.net/Character/LCAPKANJACELHFCMBJHENCNGLDDHEMHEGKEGEGACAGFEBIHPJPLKHOJEFKEMDEJCCLJALHLFHIGGCJGMIOEKEFLCLEMLNDNJDJLPHBEFKOKPIGHEGCPCDFIEPPOGLMJMLEKFGDKPPJHCOGKODMHCBJAMGLDDGMLDILNEICKGBBMBGKBBHFOGAKKGEAIFDHNIBHAMJPKHHNDNLKEHPHHADKJLFNKCEIEEGBPKPEOJLFHFGMJADOFKHIBEPFOOJKEN.png'"
              :like-count="'???'"
            />
            <Icon icon="mdi:arrow-up-bold-box"></Icon>
          </div>
        </div>
        <div>
          <span>New characters will be shown as you progress, with each being compared to the previous card.</span>
        </div>
        <div>
          <span>Try to get as many correct as you can. The game ends when you get one wrong.</span>
        </div>
      </div>
      <span class="text-slate-500 text-xs italic">
          All-Time Ratings are determined by the <RouterLink class="text-blue-400" to="/app/leaderboard">community</RouterLink> and may change between play sessions.
      </span>
    </main>
    <PlayInfoModal/>
</template>

<script setup lang="ts">
import { onMounted, onUnmounted, watch } from "vue";
import { motion, useMotionValue, animate } from 'motion-v'
import InfoCharacterPlayCard from "@/components/play_app/InfoCharacterPlayCard.vue"
import PlayInfoModal from "@/components/play_app/PlayInfoModal.vue";
import { Icon } from "@iconify/vue/dist/iconify.js";
import { usePlayStore } from "@/stores/playStore";

const CARD_SHIFT_ANIMATION_DURATION = 0.35;
const VS_ICON_SCALE_ANIMATION_DURATION = 0.20;

const playStore = usePlayStore();

const x = useMotionValue('0%');
const scale = useMotionValue(0);

onMounted(async () => { 
  window.addEventListener('keydown', handleKeyDown); 
  await playStore.initializeCards();
});

onUnmounted(() => { window.removeEventListener('keydown', handleKeyDown); });

const handleKeyDown = (event: KeyboardEvent) => 
{
  // Prevent default behavior for arrow keys
  if (['ArrowUp', 'ArrowDown'].includes(event.key)) {
    event.preventDefault();
  }

  switch (event.key) {
    case 'ArrowUp':
      playStore.makeSelection('higher');
      break;
    case 'ArrowDown':
      playStore.makeSelection('lower');
      break;
  }
};

// Play these animations when a new game starts
watch(() => playStore.isCurrentlyPlaying, async (trigger) =>
{
  if (!trigger) return;

  await animateIconScale('show');
})

// Play these animations when the user makes a selection
watch(() => playStore.userHasSelected, (trigger) => 
{
  if (!trigger) return;

  startAnimations();
})

async function startAnimations()
{
  await animateIconScale('show');
  await animateCardShift();
  // await any other animations;
  // await ...
  // await ...
  playStore.allAnimationsFinished();
}

async function animateIconScale(action : 'show' | 'hide')
{
  console.log("Animating Icon Scale")
  
  const target = action === 'show' ? 1 : 0;
  await animate(scale, target, {
    duration: VS_ICON_SCALE_ANIMATION_DURATION,
    ease: 'easeInOut',
  })
}

async function animateCardShift()
{
  console.log("Animating Card Shift")
  await animate(x, `-100%`, {
    duration: CARD_SHIFT_ANIMATION_DURATION,
    ease: 'easeInOut',
  })
  x.set('0%');
}


</script>