import { formatDuration } from '@/shared/utils/format'

/** 0.873 → "87%"; empty → "—". */
export const percent = (rate: number | null) => (rate === null ? '—' : `${Math.round(rate * 100)}%`)

export const minutes = (value: number | null) =>
  value === null ? '—' : formatDuration(value * 60_000)

export const hours = (value: number | null) =>
  value === null ? '—' : formatDuration(value * 3_600_000)

/**
 * Change against the previous period, in words (not colour only). `better` says which direction is good:
 * fewer minutes to respond is better, a higher on-time rate is better.
 */
export function change(
  now: number | null,
  before: number | null,
  better: 'up' | 'down',
  kind: 'rate' | 'number' | 'minutes' | 'hours',
): { text: string; good: boolean | null } | null {
  if (now === null || before === null) return null
  const diff = now - before
  if (Math.abs(diff) < (kind === 'rate' ? 0.005 : 0.05))
    return { text: 'same as before', good: null }
  const size = Math.abs(diff)
  const shown =
    kind === 'rate'
      ? `${Math.round(size * 100)} pts`
      : kind === 'minutes'
        ? minutes(size)
        : kind === 'hours'
          ? hours(size)
          : formatNumber(size)
  return {
    text: `${diff > 0 ? '↑ up' : '↓ down'} ${shown}`,
    good: diff > 0 === (better === 'up'),
  }
}

const formatNumber = (n: number) => (Number.isInteger(n) ? String(n) : n.toFixed(1))
