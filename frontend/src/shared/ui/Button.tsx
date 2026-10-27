import type { ButtonHTMLAttributes } from 'react'
import { Link, type LinkProps } from 'react-router'
import { cn } from '@/shared/utils/cn'
import { Spinner } from './Spinner'

type Variant = 'primary' | 'secondary' | 'danger' | 'ghost'

const VARIANTS: Record<Variant, string> = {
  primary: 'bg-primary text-white hover:bg-primary-hover',
  secondary: 'border border-line bg-surface text-ink hover:bg-canvas',
  danger: 'bg-danger text-white hover:opacity-90',
  ghost: 'text-muted hover:bg-canvas hover:text-ink',
}

const BASE = cn(
  'inline-flex h-9 items-center justify-center gap-2 rounded-md px-4 text-sm font-medium transition-colors',
  'focus-visible:ring-primary focus-visible:ring-2 focus-visible:ring-offset-2 focus-visible:outline-none',
  'disabled:cursor-not-allowed disabled:opacity-60',
)

interface ButtonProps extends ButtonHTMLAttributes<HTMLButtonElement> {
  variant?: Variant
  loading?: boolean
}

export function Button({
  variant = 'primary',
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

/** A router link that looks like a Button (e.g. "New request" in a page header). */
export function LinkButton({
  variant = 'primary',
  className,
  ...rest
}: LinkProps & { variant?: Variant }) {
  return <Link className={cn(BASE, VARIANTS[variant], className)} {...rest} />
}
