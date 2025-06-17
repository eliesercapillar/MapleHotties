<script setup lang="ts">
import { HoverCardRoot, type HoverCardRootEmits, type HoverCardRootProps, useForwardPropsEmits } from 'reka-ui'

interface ExtendedHoverCardProps extends HoverCardRootProps {
  openDelay?: number
  closeDelay?: number
}

const props = withDefaults(defineProps<ExtendedHoverCardProps>(), {
  openDelay: 200,    
  closeDelay: 50,
})
const emits = defineEmits<HoverCardRootEmits>()

const forwarded = useForwardPropsEmits(props, emits)
</script>

<template>
  <HoverCardRoot v-bind="forwarded">
    <slot />
  </HoverCardRoot>
</template>
