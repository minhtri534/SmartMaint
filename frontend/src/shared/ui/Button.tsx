import type { ButtonHTMLAttributes } from 'react'
import { Link, type LinkProps } from 'react-router'
import { cn } from '@/shared/utils/cn'
import { Spinner } from './Spinner'

/**
 * primary   — near-black fill. ONE per screen: the main job (Approve, Send request, Report completed).
 * secondary — white with a 1px border: every other action.
 * ghost     — text only: "Back", "Reset demo data".
 * danger    — outlined in the negative colour: destructive actions (Reject, Cancel case, Decline).
 */
type Variant = 'primary' | 'secondary' | 'ghost' | 'danger'

const VARIANTS: Record<Variant, string> = {
  primary: 'bg-primary text-white hover:bg-primary-hover border border-transparent',
  secondary: 'border border-control bg-surface text-ink hover:bg-raised',
  ghost: 'border border-transparent text-ink hover:bg-raised',
  danger: 'border border-negative/40 bg-surface text-negative hover:bg-pill-negative-bg',
}

const BASE = cn(
  'inline-flex h-8 items-center justify-center gap-2 rounded-control px-3 text-sm font-medium whitespace-nowrap',
  'transition-colors duration-150 active:translate-y-px',
  'disabled:cursor-not-allowed disabled:opacity-60',
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
