import type { ServiceType, ServiceTypeSuggestion } from './types'

/*
 * TODO(ai): stand-in for the AI suggestion (F3 section 4, point 1). The AI gateway (Replicate, async) is not built yet,
 * so this matches keywords. The real one will ask the API and only ever suggest: the customer decides.
 * Rules are checked in order (specific first). Each points at a service type by "Group · Name".
 */
const RULES: [RegExp, string][] = [
  [/(burst|flood|ngập|vỡ ống)/, 'Water · Burst pipe or flooding'],
  [
    /(stuck|trapped|kẹt).*(elevator|lift|thang)|(elevator|lift|thang).*(stuck|kẹt)/,
    'Elevator · Person stuck in elevator',
  ],
  [/(elevator|lift|thang máy)/, 'Elevator · Elevator out of service'],
  [
    /(air ?con|\bac\b|điều hòa|máy lạnh).*(drip|water|nước)|(drip|water).*(air ?con|\bac\b)/,
    'Air conditioning · Water dripping from unit',
  ],
  [
    /(air ?con|\bac\b|điều hòa|máy lạnh|cooling|warm air|không mát)/,
    'Air conditioning · Not cooling',
  ],
  [/(leak|drip|rò|rỉ)/, 'Water · Leaking pipe or tap'],
  [/(clog|drain|tắc|nghẹt)/, 'Water · Clogged drain'],
  [/(toilet|flush|bồn cầu)/, 'Water · Toilet not flushing'],
  [/(breaker|trip|aptomat|cb nhảy)/, 'Electrical · Breaker keeps tripping'],
  [/(socket|outlet|switch|ổ cắm|công tắc)/, 'Electrical · Socket or switch not working'],
  [/(no power|power outage|mất điện)/, 'Electrical · Power outage in one area'],
  [/(light|lamp|bulb|flicker|đèn)/, 'Electrical · Light not working'],
  [/(door|lock|latch|cửa|khóa)/, 'Doors and locks · Door or lock broken'],
  [/(stain|ceiling|trần|thấm)/, 'Ceiling and walls · Water stain on ceiling'],
  [/(fire alarm|smoke alarm|báo cháy)/, 'Fire safety · False fire alarm'],
]

export function suggestServiceType(
  description: string,
  serviceTypes: ServiceType[],
): ServiceTypeSuggestion {
  const text = description.toLowerCase()
  const hit = RULES.find(([pattern]) => pattern.test(text))
  const match = hit && serviceTypes.find((s) => `${s.groupName} · ${s.name}` === hit[1])

  const tips: string[] = []
  if (text.length < 40) tips.push('Add more detail: what exactly happens, and since when?')
  if (!/(desk|stall|unit|left|right|near|next to|corner|room|floor|bàn|góc|cạnh)/.test(text)) {
    tips.push(
      'Say exactly where it is (which desk, unit or stall) so the technician finds it fast.',
    )
  }
  return { serviceTypeId: match?.id ?? null, tips }
}
