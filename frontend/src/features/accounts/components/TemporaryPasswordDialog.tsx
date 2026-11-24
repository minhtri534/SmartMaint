import { useState } from 'react'
import { Alert, Button, Dialog, DialogActions } from '@/shared/ui'

interface Props {
  name: string
  email: string | null
  /** Null when it went out by e-mail (meeting note 18/11); set when the e-mail could not be sent. */
  password: string | null
  onClose: () => void
}

/**
 * After a create or a reset: the temporary password goes by e-mail. Only when that failed (e-mail not set up, or the
 * server refused) is it shown here, once, for the Administrator to pass on.
 */
export function TemporaryPasswordDialog({ name, email, password, onClose }: Props) {
  const [copied, setCopied] = useState(false)
  const copy = () =>
    void navigator.clipboard.writeText(password ?? '').then(
      () => setCopied(true),
      () => setCopied(false),
    )

  return (
    <Dialog
      open
      onClose={onClose}
      title={password ? 'Temporary password' : 'Temporary password sent'}
      description={
        password
          ? `The e-mail could not be sent: give this password to ${name} yourself. They must change it the first time they sign in.`
          : `${name} gets it at ${email}, with how to sign in. They must change it the first time they sign in.`
      }
    >
      {password && (
        <>
          <div className="border-border bg-raised rounded-card flex items-center justify-between gap-3 border px-4 py-3">
            <span className="font-mono text-lg tracking-wide select-all">{password}</span>
            <Button variant="ghost" onClick={copy}>
              {copied ? 'Copied' : 'Copy'}
            </Button>
          </div>
          <Alert tone="notice">This is the only time the password is shown.</Alert>
        </>
      )}
      <DialogActions>
        <Button variant="primary" onClick={onClose}>
          Done
        </Button>
      </DialogActions>
    </Dialog>
  )
}
