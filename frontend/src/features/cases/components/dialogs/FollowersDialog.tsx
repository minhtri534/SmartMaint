import { useDeferredValue, useState } from 'react'
import { Alert, Button, Dialog, DialogActions, LoadingBlock, SearchInput } from '@/shared/ui'
import { casesApi } from '../../api'
import { useCaseMutation, useFollowerOptions } from '../../hooks'
import type { CaseDetail, FollowerOption } from '../../types'

/**
 * Meeting note 18/11: who follows the case on the left, a search by username or e-mail on the right. Choosing a customer
 * moves them to the list; each one can be removed. Followers read the case and are told about it, but cannot act on it.
 * Both lists keep one fixed height and scroll, so the dialog does not grow with a long list.
 */
export function FollowersDialog({
  caseDetail,
  open,
  onClose,
}: {
  caseDetail: CaseDetail
  open: boolean
  onClose: () => void
}) {
  return (
    <Dialog
      open={open}
      onClose={onClose}
      wide
      title="Followers"
      description="Customers who see this case and are told about it."
    >
      {open && <FollowersForm caseId={caseDetail.id} onDone={onClose} />}
    </Dialog>
  )
}

const line = (p: FollowerOption) => [p.username, p.email].filter(Boolean).join(' · ')

/** Loads who follows the case now; the editor starts from that list. */
function FollowersForm({ caseId, onDone }: { caseId: number; onDone: () => void }) {
  const saved = useFollowerOptions(caseId, '', true)
  if (saved.error) return <Alert>{saved.error.message}</Alert>
  if (!saved.data) return <LoadingBlock rows={4} />
  return <FollowersEditor caseId={caseId} initial={saved.data.current} onDone={onDone} />
}

function FollowersEditor({
  caseId,
  initial,
  onDone,
}: {
  caseId: number
  initial: FollowerOption[]
  onDone: () => void
}) {
  const [search, setSearch] = useState('')
  const deferred = useDeferredValue(search.trim())
  const options = useFollowerOptions(caseId, deferred, true)
  const save = useCaseMutation(casesApi.setFollowers)
  const [chosen, setChosen] = useState(initial)
  const [removed, setRemoved] = useState<FollowerOption[]>([])

  if (options.error) return <Alert>{options.error.message}</Alert>
  if (!options.data) return <LoadingBlock rows={4} />

  const words = deferred.toLowerCase()
  const isChosen = (p: FollowerOption) => chosen.some((c) => c.id === p.id)
  // Someone just removed is not a server match yet (the server still counts them as following).
  const results = [
    ...options.data.matches,
    ...removed.filter(
      (p) =>
        !options.data.matches.some((m) => m.id === p.id) &&
        (!words || `${p.fullName} ${line(p)}`.toLowerCase().includes(words)),
    ),
  ].filter((p) => !isChosen(p))

  return (
    <div className="flex flex-col gap-4">
      <div className="grid gap-4 sm:grid-cols-2">
        <section aria-label="Following this case" className="flex min-w-0 flex-col gap-2">
          <h3 className="flex h-8 items-center text-sm font-medium">Following ({chosen.length})</h3>
          {chosen.length === 0 ? (
            <p className="text-muted border-control rounded-control flex h-72 items-center justify-center border border-dashed px-3 text-center text-sm">
              Nobody else follows this case.
            </p>
          ) : (
            <ul className="border-border rounded-control divide-rule h-72 divide-y overflow-y-auto overscroll-contain border">
              {chosen.map((p) => (
                <li key={p.id} className="flex items-center justify-between gap-2 px-3 py-2">
                  <Person person={p} />
                  <Button
                    variant="ghost"
                    aria-label={`Remove ${p.fullName}`}
                    onClick={() => {
                      setChosen(chosen.filter((c) => c.id !== p.id))
                      setRemoved((r) => [...r.filter((x) => x.id !== p.id), p])
                    }}
                  >
                    Remove
                  </Button>
                </li>
              ))}
            </ul>
          )}
        </section>

        <section aria-label="Find a customer" className="flex min-w-0 flex-col gap-2">
          <SearchInput
            label="Search customers"
            placeholder="Username or e-mail"
            value={search}
            onChange={(e) => setSearch(e.target.value)}
          />
          {results.length === 0 ? (
            <p className="text-muted border-control rounded-control flex h-72 items-center justify-center border border-dashed px-3 text-center text-sm">
              No customer found.
            </p>
          ) : (
            <ul className="border-border rounded-control divide-rule h-72 divide-y overflow-y-auto overscroll-contain border">
              {results.map((p) => (
                <li key={p.id} className="flex items-center justify-between gap-2 px-3 py-2">
                  <Person person={p} />
                  <Button
                    variant="secondary"
                    aria-label={`Add ${p.fullName}`}
                    onClick={() => setChosen([...chosen, p])}
                  >
                    Add
                  </Button>
                </li>
              ))}
            </ul>
          )}
        </section>
      </div>
      {save.error && <Alert>{save.error.message}</Alert>}
      <DialogActions>
        <Button variant="ghost" onClick={onDone}>
          Back
        </Button>
        <Button
          variant="primary"
          loading={save.isPending}
          onClick={() =>
            save.mutate({ id: caseId, customerIds: chosen.map((p) => p.id) }, { onSuccess: onDone })
          }
        >
          Save followers
        </Button>
      </DialogActions>
    </div>
  )
}

function Person({ person }: { person: FollowerOption }) {
  return (
    <div className="min-w-0 text-sm">
      <div className="truncate font-medium">{person.fullName}</div>
      <div className="text-muted truncate text-xs">{line(person)}</div>
    </div>
  )
}
