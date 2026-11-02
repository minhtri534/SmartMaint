import { useState } from 'react'
import { Alert, Button, Dialog, DialogActions } from '@/shared/ui'

interface Props {
  open: boolean
  username: string
  password: string
  onClose: () => void
}

/** Shows a temporary password once (create account, reset password). The server never shows it again. */
export function TemporaryPasswordDialog({ open, username, password, onClose }: Props) {
  const [copied, setCopied] = useState(false)
  const copy = () =>
    void navigator.clipboard.writeText(password).then(
      () => setCopied(true),
      () => setCopied(false),
    )

  return (
    <Dialog
      open={open}
      onClose={onClose}
      title="Temporary password"
      description={`Give it to ${username}. They must change it the first time they sign in.`}
    >
      <div className="border-border bg-raised rounded-card flex items-center justify-between gap-3 border px-4 py-3">
        <span className="font-mono text-lg tracking-wide select-all">{password}</span>
        <Button variant="ghost" onClick={copy}>
          {copied ? 'Copied' : 'Copy'}
        </Button>
      </div>
      <Alert tone="notice">This is the only time the password is shown.</Alert>
      <DialogActions>
        <Button variant="primary" onClick={onClose}>
          Done
        </Button>
      </DialogActions>
    </Dialog>
  )
}
