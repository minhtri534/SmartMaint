import type { ReactNode } from 'react'

/*
 * Settings-style form layout (taste app-screens §8.2, 16/11): each section is a row — its title and one grey
 * sentence on the left, its fields in a white card on the right — so a form uses the sheet's width instead of
 * a 720px column with an empty right half. Below 1024px the label column stacks above the card.
 */

const COLUMNS = 'grid gap-x-10 gap-y-3 lg:grid-cols-[minmax(0,240px)_minmax(0,1fr)]'

export function FormSection({
  title,
  description,
  aside,
  children,
}: {
  title: string
  /** What the section is for, or what its values cause — not how to fill it in. */
  description?: ReactNode
  /** A short fact on the right of the title, e.g. "2 chosen". */
  aside?: ReactNode
  children: ReactNode
}) {
  return (
    <section className={COLUMNS}>
      <div className="lg:pt-5">
        <div className="flex items-baseline justify-between gap-3 lg:block">
          <h2 className="text-section font-semibold">{title}</h2>
          {aside && <div className="text-muted text-xs tabular-nums lg:mt-1">{aside}</div>}
        </div>
        {description && <p className="text-muted mt-1 text-sm">{description}</p>}
      </div>
      <div className="bg-surface border-border rounded-card min-w-0 border p-5">{children}</div>
    </section>
  )
}

/** The form's buttons (and its error), under the field column, right-aligned. */
export function FormActions({ children, error }: { children: ReactNode; error?: ReactNode }) {
  return (
    <div className={COLUMNS}>
      <div className="col-start-1 flex flex-col gap-3 lg:col-start-2">
        {error}
        <div className="flex justify-end gap-2">{children}</div>
      </div>
    </div>
  )
}
