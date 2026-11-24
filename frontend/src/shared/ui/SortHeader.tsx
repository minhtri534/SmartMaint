import type { ReactNode } from 'react'
import { cn } from '@/shared/utils/cn'
import { Icon } from './Icon'
import type { SortState } from './table-sort'

/** A table heading that sorts its column; the arrow shows the direction. */
export function SortHeader<K extends string>({
  sortKey,
  sort,
  onSort,
  align = 'left',
  className,
  children,
}: {
  sortKey: K
  sort: SortState<K> | null
  onSort: (key: K) => void
  align?: 'left' | 'right'
  className?: string
  children: ReactNode
}) {
  const active = sort?.key === sortKey
  return (
    <th
      aria-sort={active ? (sort.dir === 'asc' ? 'ascending' : 'descending') : 'none'}
      className={cn('h-9 pr-4 font-normal', align === 'right' && 'text-right', className)}
    >
      <button
        type="button"
        onClick={() => onSort(sortKey)}
        className="hover:text-ink inline-flex items-center gap-1 transition-colors duration-150"
      >
        {children}
        <Icon
          name="back"
          className={cn(
            'size-3.5 transition-transform duration-150',
            active && sort.dir === 'asc' ? 'rotate-90' : '-rotate-90',
            !active && 'opacity-40',
          )}
        />
      </button>
    </th>
  )
}
