import type { ButtonHTMLAttributes } from 'react'
import { Link, type LinkProps } from 'react-router'
import { cn } from '@/shared/utils/cn'
import { Spinner } from './Spinner'

/**
 * primary   — the blue-to-cyan of the Sign in button. ONE per screen: the main job (Approve, Send request, Report completed).
 * secondary — white with a 1px border: every other action.
 * ghost     — text only: "Back", "Reset demo data".
 * danger    — outlined in the negative colour: destructive actions (Reject, Cancel case, Decline).
 */
type Variant = 'primary' | 'secondary' | 'ghost' | 'danger'

// ui/mercato-style: buttons are raised keys (a light top edge, a short shadow under them).
const VARIANTS: Record<Variant, string> = {
  primary:
    'brand-fill border border-transparent font-semibold shadow-[0_2px_8px_-2px_rgb(30_130_220/0.45)]',
  secondary: 'bump text-ink hover:bg-raised',
  ghost: 'border border-transparent text-ink hover:bg-raised',
  danger: 'border border-negative/40 bg-surface text-negative hover:bg-pill-negative-bg',
}

const BASE = cn(
  'inline-flex h-8 items-center justify-center gap-2 rounded-control px-3 text-sm font-medium whitespace-nowrap',
  // press feedback: a 3% squeeze, 150ms strong ease-out (emil-design-eng)
  'transition-[color,background-color,border-color,transform] duration-150 ease-snap active:scale-[0.97]',
  'disabled:cursor-not-allowed disabled:opacity-60 disabled:active:scale-100',
)

interface ButtonProps extends ButtonHTMLAttributes<HTMLButtonElement> {
  variant?: Variant
  loading?: boolean
}

export function Button({
  variant = 'secondary',
  loading = false,
  disabled,
  className,
  children,
  type = 'button',
  ...rest
}: ButtonProps) {
  return (
    <button
      type={type}
      disabled={disabled || loading}
      className={cn(BASE, VARIANTS[variant], className)}
      {...rest}
    >
      {loading && <Spinner className="size-4" />}
      {children}
    </button>
  )
}

/** A router link that looks like a Button (e.g. "New request" in a title row). */
export function LinkButton({
  variant = 'secondary',
  className,
  ...rest
}: LinkProps & { variant?: Variant }) {
  return <Link className={cn(BASE, VARIANTS[variant], className)} {...rest} />
}
