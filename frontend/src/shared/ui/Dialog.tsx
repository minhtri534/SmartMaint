import { useEffect, useRef, type ReactNode } from 'react'
import { Alert } from './Alert'
import { Button } from './Button'

interface DialogProps {
  open: boolean
  onClose: () => void
  title: string
  description?: string
  children: ReactNode
}

/**
 * Modal built on the native <dialog> element (Esc and the backdrop are handled by the browser).
 * Screen catalog dialogs (DLG-xx) use this: put the form inside, call onClose after the mutation succeeds.
 */
export function Dialog({ open, onClose, title, description, children }: DialogProps) {
  const ref = useRef<HTMLDialogElement>(null)

  useEffect(() => {
    const dialog = ref.current
    if (!dialog) return
    if (open && !dialog.open) dialog.showModal()
    if (!open && dialog.open) dialog.close()
  }, [open])

  return (
    <dialog
      ref={ref}
      onClose={onClose}
      className="bg-surface text-ink m-auto w-full max-w-lg rounded-lg p-0 shadow-xl backdrop:bg-black/40"
    >
      {open && (
        <div className="flex flex-col gap-4 p-6">
          <div>
            <h2 className="text-lg font-semibold">{title}</h2>
            {description && <p className="text-muted mt-1 text-sm">{description}</p>}
          </div>
          {children}
        </div>
      )}
    </dialog>
  )
}

/** Right-aligned button row at the bottom of a dialog form. */
export function DialogActions({ children }: { children: ReactNode }) {
  return <div className="mt-2 flex justify-end gap-2">{children}</div>
}

interface ConfirmDialogProps {
  open: boolean
  onClose: () => void
  title: string
  description?: string
  children?: ReactNode
  confirmLabel: string
  onConfirm: () => void
  loading?: boolean
  /** Message of a failed attempt, shown above the buttons. */
  error?: string | null
}

/** Yes / no question with no form fields (e.g. "Verify and close"). */
export function ConfirmDialog({
  open,
  onClose,
  title,
  description,
  children,
  confirmLabel,
  onConfirm,
  loading,
  error,
}: ConfirmDialogProps) {
  return (
    <Dialog open={open} onClose={onClose} title={title} description={description}>
      {children}
      {error && <Alert>{error}</Alert>}
      <DialogActions>
        <Button variant="secondary" onClick={onClose}>
          Back
        </Button>
        <Button loading={loading} onClick={onConfirm}>
          {confirmLabel}
        </Button>
      </DialogActions>
    </Dialog>
  )
}
