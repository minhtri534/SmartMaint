import { useState } from 'react'

export type SortDirection = 'asc' | 'desc'
export interface SortState<K extends string> {
  key: K
  dir: SortDirection
}

/** Which column a table is sorted by. Pressing a column again reverses it. */
export function useTableSort<K extends string>(initial: SortState<K> | null = null) {
  const [sort, setSort] = useState<SortState<K> | null>(initial)
  const toggle = (key: K) =>
    setSort((s) => ({ key, dir: s?.key === key && s.dir === 'asc' ? 'desc' : 'asc' }))
  return { sort, toggle }
}

/** The rows in the chosen order; empty values always come last. */
export function sortRows<T, K extends string>(
  rows: readonly T[],
  sort: SortState<K> | null,
  values: Record<K, (row: T) => string | number | null>,
): T[] {
  if (!sort) return [...rows]
  const value = values[sort.key]
  const factor = sort.dir === 'asc' ? 1 : -1
  return [...rows].sort((a, b) => {
    const x = value(a)
    const y = value(b)
    if (x === null || y === null) return x === y ? 0 : x === null ? 1 : -1
    return (
      factor *
      (typeof x === 'number' && typeof y === 'number' ? x - y : String(x).localeCompare(String(y)))
    )
  })
}
