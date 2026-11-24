import { useEffect, useRef, useState, type ReactNode } from 'react'
import { cn } from '@/shared/utils/cn'

/**
 * A small "?" that opens one short explanation of the figure beside it (meeting note 18/11). Click to open; a click
 * elsewhere or Esc closes it.
 */
export function HelpTip({
  label,
  align = 'right',
  children,
}: {
  /** What it explains, for screen readers: "About Waiting for staff". */
  label: string
  /** Which edge of the "?" the text lines up with. */
  align?: 'left' | 'right'
  children: ReactNode
}) {
  const [open, setOpen] = useState(false)
  const root = useRef<HTMLSpanElement>(null)

  useEffect(() => {
    if (!open) return
    const onDown = (e: MouseEvent) => {
      if (!root.current?.contains(e.target as Node)) setOpen(false)
    }
    const onKey = (e: KeyboardEvent) => {
      if (e.key === 'Escape') setOpen(false)
    }
    document.addEventListener('mousedown', onDown)
    document.addEventListener('keydown', onKey)
    return () => {
      document.removeEventListener('mousedown', onDown)
      document.removeEventListener('keydown', onKey)
    }
  }, [open])

  return (
    <span ref={root} className="relative inline-flex">
      <button
        type="button"
        aria-label={`About ${label}`}
        aria-expanded={open}
        onClick={() => setOpen((v) => !v)}
        className="border-control text-muted hover:text-ink hover:border-ink grid size-[18px] place-items-center rounded-full border text-[11px] leading-none transition-colors duration-150"
      >
        ?
      </button>
      {open && (
        <span
          role="note"
          className={cn(
            'bg-surface border-border text-ink rounded-control absolute top-6 z-30 w-60 border p-3 text-left text-xs leading-5 font-normal shadow-md',
            align === 'right' ? 'right-0' : 'left-0',
          )}
        >
          {children}
        </span>
      )}
    </span>
  )
}
