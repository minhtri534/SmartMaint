import type { ReactNode } from 'react'
import { cn } from '@/shared/utils/cn'

/**
 * danger — something failed (inside the block that failed, never a full-screen banner).
 * info   — a neutral note.
 * notice — a quiet amber notice (e.g. "demo data"); never used as a ground.
 */
type Tone = 'danger' | 'info' | 'notice'

const TONES: Record<Tone, string> = {
  danger: 'border-negative/30 bg-pill-negative-bg text-pill-negative-ink',
  info: 'border-border bg-raised text-ink',
  notice: 'border-notice-border bg-pill-warning-bg text-pill-warning-ink',
}

export function Alert({ tone = 'danger', children }: { tone?: Tone; children: ReactNode }) {
  return (
    <div
      role={tone === 'danger' ? 'alert' : 'status'}
      className={cn('rounded-control border px-3 py-2 text-sm', TONES[tone])}
    >
      {children}
    </div>
  )
}
