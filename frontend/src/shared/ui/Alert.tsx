import type { ReactNode } from 'react'
import { cn } from '@/shared/utils/cn'

type Tone = 'danger' | 'info'

const TONES: Record<Tone, string> = {
  danger: 'border-danger/30 bg-danger-soft text-danger',
  info: 'border-primary/30 bg-primary-soft text-primary',
}

export function Alert({ tone = 'danger', children }: { tone?: Tone; children: ReactNode }) {
  return (
    <div role="alert" className={cn('rounded-md border px-3 py-2 text-sm', TONES[tone])}>
      {children}
    </div>
  )
}
