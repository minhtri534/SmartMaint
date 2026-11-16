import type { ComponentProps } from 'react'
import { cn } from '@/shared/utils/cn'
import { controlClass } from './control-class'
import { Icon } from './Icon'

/**
 * The search box of a filter row: 32px like FilterSelect, a magnifier inside, the label as its accessible name
 * (the placeholder says what it searches). Use TextField in forms.
 */
export function SearchInput({
  label,
  className,
  ...rest
}: Omit<ComponentProps<'input'>, 'type'> & { label: string }) {
  return (
    <div className={cn('relative min-w-56 flex-1 sm:max-w-sm', className)}>
      <Icon
        name="search"
        className="text-muted pointer-events-none absolute top-1/2 left-2.5 size-4 -translate-y-1/2"
      />
      <input type="search" aria-label={label} className={controlClass() + ' h-8 pl-8'} {...rest} />
    </div>
  )
}
