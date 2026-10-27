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

/** Horizontal tab bar for filtering a list (not for routing). */
export function Tabs<T extends string>({ items, value, onChange }: TabsProps<T>) {
  return (
    <div role="tablist" className="border-line flex gap-1 overflow-x-auto border-b">
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
              '-mb-px flex items-center gap-2 border-b-2 px-3 py-2 text-sm whitespace-nowrap',
              active
                ? 'border-primary text-primary font-medium'
                : 'text-muted hover:text-ink border-transparent',
            )}
          >
            {item.label}
            {item.count !== undefined && (
              <span
                className={cn(
                  'rounded-full px-1.5 text-xs',
                  active ? 'bg-primary-soft' : 'bg-canvas',
                )}
              >
                {item.count}
              </span>
            )}
          </button>
        )
      })}
    </div>
  )
}
