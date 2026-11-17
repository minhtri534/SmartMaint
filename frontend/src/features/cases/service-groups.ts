import type { ChipHue, IconName } from '@/shared/ui'

/**
 * How a service group looks (ui/mercato-style, 17/11): one icon and one chip hue per group of the catalogue, used
 * by the case cards and the home pages. A group missing here falls back to the wrench.
 */
const GROUPS: Record<string, { icon: IconName; hue: ChipHue }> = {
  Water: { icon: 'drop', hue: 'blue' },
  Electrical: { icon: 'bolt', hue: 'orange' },
  'Air conditioning': { icon: 'snow', hue: 'blue' },
  Elevator: { icon: 'lift', hue: 'violet' },
  'Doors and locks': { icon: 'door', hue: 'violet' },
  'Fire safety': { icon: 'flame', hue: 'red' },
  'Ceiling and walls': { icon: 'wall', hue: 'green' },
}

/** "Water · Leaking pipe" → "Water". */
export const groupOf = (serviceTypeName: string) => serviceTypeName.split(' · ')[0] ?? ''

export function groupStyle(group: string): { icon: IconName; hue: ChipHue } {
  return GROUPS[group] ?? { icon: 'wrench', hue: 'orange' }
}

/** Tinted chip classes per hue (the hues are slots of the blue family, see index.css). */
export const CHIP_CLASSES: Record<ChipHue, string> = {
  orange: 'bg-chip-orange/15 text-chip-orange',
  violet: 'bg-chip-violet/15 text-chip-violet',
  green: 'bg-chip-green/15 text-chip-green',
  blue: 'bg-chip-blue/15 text-chip-blue',
  red: 'bg-negative/15 text-negative',
}
