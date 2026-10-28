import { cn } from '@/shared/utils/cn'

export interface TabItem<T extends string> {
  value: T
  label: string
  count?: number
}

interface TabsProps<T extends string> {
  items: readonly TabItem<T>[]
  value: T
  onChange: (value: T) => void
}

/** Underline tabs on a full-width 1px rule; the selected one gets a 2px ink line. For filtering a list. */
export function Tabs<T extends string>({ items, value, onChange }: TabsProps<T>) {
  return (
    <div role="tablist" className="border-border flex gap-6 overflow-x-auto border-b">
      {items.map((item) => {
        const active = item.value === value
        return (
          <button
            key={item.value}
            type="button"
            role="tab"
            aria-selected={active}
            onClick={() => onChange(item.value)}
            className={cn(
              'relative flex items-center gap-1.5 py-2.5 text-sm whitespace-nowrap transition-colors',
              active ? 'text-ink font-medium' : 'text-muted hover:text-ink',
            )}
          >
            {item.label}
            {item.count !== undefined && (
              <span className="text-muted tabular-nums">{item.count}</span>
            )}
            {active && <span className="bg-ink absolute inset-x-0 -bottom-px h-0.5" />}
          </button>
        )
      })}
    </div>
  )
}
