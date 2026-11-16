import { motion } from 'motion/react'
import { useId } from 'react'
import { cn } from '@/shared/utils/cn'
import { slide } from '@/shared/utils/motion'

export interface SegmentedOption<T extends string> {
  value: T
  label: string
}

/**
 * Pill segmented control for a few mutually exclusive choices (a period, a view): a tinted track, the chosen
 * option on a white pill that slides to the new choice. Use FilterSelect when there are more than ~5 options.
 */
export function Segmented<T extends string>({
  label,
  options,
  value,
  onChange,
  className,
}: {
  /** Accessible name of the group. */
  label: string
  options: readonly SegmentedOption<T>[]
  value: T
  onChange: (value: T) => void
  className?: string
}) {
  const group = useId()
  return (
    <div
      role="radiogroup"
      aria-label={label}
      className={cn('bg-active inline-flex h-8 items-center gap-0.5 rounded-full p-0.5', className)}
    >
      {options.map((o) => {
        const on = o.value === value
        return (
          <button
            key={o.value}
            type="button"
            role="radio"
            aria-checked={on}
            onClick={() => onChange(o.value)}
            className={cn(
              'relative isolate h-7 rounded-full px-3 text-[13px] whitespace-nowrap transition-colors duration-150',
              on ? 'text-ink font-medium' : 'text-muted hover:text-ink',
            )}
          >
            {on && (
              <motion.span
                layoutId={`segmented-${group}`}
                transition={slide}
                className="bg-surface absolute inset-0 -z-10 rounded-full shadow-[0_1px_2px_rgb(15_27_45/0.12)]"
              />
            )}
            {o.label}
          </button>
        )
      })}
    </div>
  )
}
