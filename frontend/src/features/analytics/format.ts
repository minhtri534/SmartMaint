import { formatDuration } from '@/shared/utils/format'

/** 0.873 → "87%"; empty → "—". */
export const percent = (rate: number | null) => (rate === null ? '—' : `${Math.round(rate * 100)}%`)

export const minutes = (value: number | null) =>
  value === null ? '—' : formatDuration(value * 60_000)

export const hours = (value: number | null) =>
  value === null ? '—' : formatDuration(value * 3_600_000)
