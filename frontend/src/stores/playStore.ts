import { defineStore } from 'pinia'
import { ref } from 'vue'
import { apiFetch } from '@/utils/api'
import { useSwipeStore } from './swipeStore'

interface ApiCharacter {
  id: number
  name: string
  level: number
  job: string
  world: string
  imageUrl: string
}

interface BackgroundPaths {
  optimized: string;
  fallback: string;
}

interface CharacterCard 
{
  character : ApiCharacter
  bgURL: BackgroundPaths;
}

type Rating = 'Likes' | 'Nopes' | 'Favourites';

export const usePlayStore = defineStore('play', () => 
{
    const currentScore = ref(0);
    const highScore = ref(0);
    const isCurrentlyPlaying = ref(false);

    const cards = ref([] as CharacterCard[]);
    const currentRating = ref<Rating>('Likes')

    const correctSelection = ref(false);
    const shiftAnimationPending = ref(false);

    let _shiftResolve: (() => void) | null = null;

    function startShiftAnimation(): Promise<void> {
    // if an animation is already pending, return the same promise
        if (shiftAnimationPending.value && _shiftResolve) {
            return new Promise(res => {
                const prevResolve = _shiftResolve!;
                // chain: resolve once previous finished
                const chain = () => { prevResolve(); res(); };
                _shiftResolve = chain;
            });
        }

        shiftAnimationPending.value = true;

        return new Promise<void>((resolve) => { _shiftResolve = resolve; });
    }

    function completeShiftAnimation() {
        if (_shiftResolve) {
            _shiftResolve();
            _shiftResolve = null;
        }
        shiftAnimationPending.value = false;
    }

    async function initializeCards()
    {
        resetCards();
        addRandomCardFromSwipeStore();
    }

    async function startClassic() 
    {
        isCurrentlyPlaying.value = true;
        await getNewCard();
    }
    
    async function startTimed() 
    { 
        isCurrentlyPlaying.value = true;
        await getNewCard();
    }

    async function makeSelection(selection : 'higher' | 'lower')
    {
        correctSelection.value = await verify(selection);
        if (!correctSelection.value)
        {
            gameOver();
            return;
        }

        currentScore.value++;
        // Start shift animation and wait until components tell us it's finished
        await startShiftAnimation();

        // now safe to mutate the array
        // keep your existing sequencing: shift then get new card
        cards.value.shift();
        getNewRandomRating();
        await getNewCard();

        //cards.value.shift();

    }
    
    async function startNextRound() 
    {
        correctSelection.value = false;
        cards.value.shift();
        getNewRandomRating();
        await getNewCard();
    }

    async function getNewCard()
    {
        // const excludeIds = cards.value.map(c => c.character.id).join(',');
        // const excludeParam = excludeIds ? `&exclude=${excludeIds}` : '';

        // const url = `https://localhost:7235/api/Game/randomCard?`;
        // const response = await apiFetch(url);

        
        addRandomCardFromSwipeStore();
    }

    async function verify(choice : 'higher' | 'lower') : Promise<boolean>
    {
        // const url = `https://localhost:7235/api/Game/verify?previousId={cards[0].character.id}&nextId={cards[1].character.id}&choice={choice}`;
        // const response = await apiFetch(url);

        return true;
    }

    function gameOver()
    {
        isCurrentlyPlaying.value = false;
        resetCards();
        highScore.value = Math.max(highScore.value, currentScore.value);
    }

    function addRandomCardFromSwipeStore()
    {
        const swipeStore = useSwipeStore();
        var startingCard = swipeStore.cards[Math.floor(Math.random() * swipeStore.cards.length)];
        cards.value.push(startingCard);
    }

    function getNewRandomRating()
    {
        var value : Rating = 'Likes'
        switch (Math.floor(Math.random() * 3))
        {
            case 0:
                value = 'Likes'
                break;
            case 1:
                value = 'Nopes'
                break;
            case 2:
                value = 'Favourites'
                break;
            default:
                value = 'Likes'
                break;
        }

        currentRating.value = value;
    }
    
    function resetCards()
    {
        cards.value = [] as CharacterCard[];
    }


    return {
        shiftAnimationPending, startShiftAnimation, completeShiftAnimation,

        currentScore, highScore, isCurrentlyPlaying,

        cards, correctSelection,


        initializeCards,

        startClassic, startTimed,
        makeSelection, getNewCard, startNextRound,

    }

});