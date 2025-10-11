import { defineStore } from 'pinia'
import { ref } from 'vue'

interface SearchSettings {
    characterName: string
    rankingType: string
    timeType: string
    classType: string
    worldType: string
}

interface Character {
    id: number
    name: string
    level: number
    job: string
    world: string
    imageUrl: string
}

interface LeaderboardCharacterDTO {
    character: Character
    count: number
}

interface PaginatedResponse<T> {
    data: T[]
    totalCount: number
    currentPage: number
    pageSize: number
    totalPages: number
}

const defaultSearchSettings: SearchSettings = {
    characterName: "",
    rankingType: "hotties",
    timeType: "all",
    classType: "all",
    worldType: "all",
}

export const useLeaderboardStore = defineStore('leaderboard', () => 
{
    const characters = ref([] as LeaderboardCharacterDTO[])
    const searchSettings = ref<SearchSettings>({...defaultSearchSettings});

    const currentPage = ref(1);
    const totalPages = ref(0);
    const totalCount = ref(0);
    const pageSize = ref(5);

    const isLoading = ref(false);

    function updateSearchParameters(params: Partial<SearchSettings>)
    {
        Object.assign(searchSettings.value, {
            ...defaultSearchSettings,
            ...params
        })

        // TODO: Make function async? That would mean SearchForm.vue's onSubmit would need to be async too?
        fetchSearch();
    }

    async function fetchSearch() {
        if (isLoading.value) return;

        isLoading.value = true;
        try {
            const searchParams = new URLSearchParams({
                page: currentPage.value.toString(),
                pageSize: pageSize.value.toString(),
                ...searchSettings.value
            });
            
            const url = `https://localhost:7235/api/CharacterStats/search?${searchParams}`;

            const response = await fetch(url, {
                method: 'GET'
            });

            if (!response.ok) {
                throw new Error(`Failed to search: ${response.status}`);
            }

            const data: PaginatedResponse<LeaderboardCharacterDTO> = await response.json();
            
            characters.value = data.data;
            totalPages.value = data.totalPages;
            totalCount.value = data.totalCount;
            pageSize.value = data.pageSize;
        }
        catch (err) {
            console.error("Failed to load searched characters:", err);
        }
        finally {
            isLoading.value = false;
        }
    }

    function setPage(page: number) {
        currentPage.value = page;
    }

    function getRankingType() : string
    {
        const labels = 
        { 
            hotties: "Hotties", 
            notties: "Notties", 
            favourites: "Favourites" 
        }
        return labels[searchSettings.value.rankingType] || "Hotties"
    }
    
    return { 
        isLoading,
        currentPage, totalPages, totalCount, pageSize,
        characters, searchSettings,
        updateSearchParameters,
        fetchSearch,
        setPage,
        getRankingType,
    }
})
