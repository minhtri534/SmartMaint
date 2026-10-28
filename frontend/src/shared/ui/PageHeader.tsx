import type { ReactNode } from 'react'

interface PageHeaderProps {
  title: string
  /** One grey sentence about the data (the finding, the scope) — never instructions about the screen. */
  description?: ReactNode
  /** Real actions only, e.g. <LinkButton variant="primary" to={...}>New request</LinkButton>. */
  actions?: ReactNode
  /** Small line above the title, e.g. a breadcrumb link back to the list. */
  above?: ReactNode
}

/** Title row at the top of every signed-in page: the h1 is a noun at 22/600. */
export function PageHeader({ title, description, actions, above }: PageHeaderProps) {
  return (
    <div className="flex flex-wrap items-end justify-between gap-4">
      <div className="min-w-0">
        {above && <div className="text-muted mb-1 text-sm">{above}</div>}
        <h1 className="text-title font-semibold tracking-[-0.01em]">{title}</h1>
        {description && <p className="text-muted mt-1 text-sm">{description}</p>}
      </div>
      {actions && <div className="flex flex-wrap items-center gap-2">{actions}</div>}
    </div>
  )
}
