import { zodResolver } from '@hookform/resolvers/zod'
import { useState } from 'react'
import { useForm } from 'react-hook-form'
import { Link, useNavigate, useParams } from 'react-router'
import { useFacilityTree } from '@/features/facilities'
import { assetChangeEditPath, assetDetailPath, paths } from '@/app/paths'
import {
  Alert,
  Badge,
  Button,
  ConfirmDialog,
  Dialog,
  DialogActions,
  ErrorBlock,
  LinkButton,
  LoadingBlock,
  PageHeader,
  Tabs,
  TextArea,
} from '@/shared/ui'
import { formatDate, formatDateTime } from '@/shared/utils/format'
import { assetChangesApi } from '../api'
import { ChangeListStatusBadge } from '../components/ChangeListStatusBadge'
import { ChangeTree } from '../components/ChangeTree'
import { useChangeList, useChangeMutation } from '../hooks'
import { rejectChangeListSchema, type RejectChangeListValues } from '../schemas'
import { CHANGE_KIND_LABELS, type ChangeItem, type ChangeListDetail } from '../types'

/**
 * One change list: the building tree with its changes marked (the manager reviews it the way the staff wrote it,
 * meeting note 18/11), and the list of its lines with why a line cannot be applied now.
 */
export function ChangeListDetailPage() {
  const id = Number(useParams().id)
  const list = useChangeList(id)
  if (list.isPending) return <LoadingBlock rows={5} />
  if (list.error)
    return (
      <ErrorBlock
        what="the change list"
        message={list.error.message}
        onRetry={() => void list.refetch()}
      />
    )
  return <ListView l={list.data} />
}

type Confirm = 'submit' | 'approve' | 'delete'

const CONFIRMS: Record<Confirm, { title: string; text: string; label: string }> = {
  submit: {
    title: 'Send for approval',
    text: 'The managers of the buildings are told. You can no longer change the list unless a manager rejects it.',
    label: 'Send',
  },
  approve: {
    title: 'Approve change list',
    text: 'Every change is made now, all together. If one cannot be made, none is.',
    label: 'Approve',
  },
  delete: {
    title: 'Delete change list',
    text: 'The list and its lines are deleted. Nothing in the building changes.',
    label: 'Delete',
  },
}

function ListView({ l }: { l: ChangeListDetail }) {
  const navigate = useNavigate()
  const tree = useFacilityTree()
  const [view, setView] = useState<'tree' | 'lines'>('tree')
  const [confirm, setConfirm] = useState<Confirm | null>(null)
  const [rejecting, setRejecting] = useState(false)
  const submit = useChangeMutation(assetChangesApi.submit)
  const approve = useChangeMutation(assetChangesApi.approve)
  const remove = useChangeMutation(assetChangesApi.remove, { deletes: true })
  const running = confirm === 'submit' ? submit : confirm === 'approve' ? approve : remove
  const can = (a: ChangeListDetail['allowedActions'][number]) => l.allowedActions.includes(a)
  const open = (c: Confirm) => {
    ;({ submit, approve, delete: remove })[c].reset()
    setConfirm(c)
  }
  const problems = l.items.filter((i) => i.problem !== null).length

  return (
    <div className="flex max-w-[1040px] flex-col gap-5">
      <PageHeader
        above={
          <LinkButton variant="ghost" to={paths.assetChanges} className="-ml-3">
            Asset changes
          </LinkButton>
        }
        title={l.title}
        description={
          <>
            <span className="font-mono">{l.code}</span> ·{' '}
            {l.buildingNames.join(', ') || 'no building'} · {l.itemCount}{' '}
            {l.itemCount === 1 ? 'change' : 'changes'} by {l.staff.fullName}
          </>
        }
        actions={
          <>
            {can('delete') && (
              <Button variant="danger" onClick={() => open('delete')}>
                Delete
              </Button>
            )}
            {can('reject') && (
              <Button variant="secondary" onClick={() => setRejecting(true)}>
                Reject
              </Button>
            )}
            {can('edit') && (
              <LinkButton variant="secondary" to={assetChangeEditPath(l.id)}>
                Edit
              </LinkButton>
            )}
            {can('submit') && (
              <Button variant="primary" disabled={problems > 0} onClick={() => open('submit')}>
                Send for approval
              </Button>
            )}
            {can('approve') && (
              <Button variant="primary" disabled={problems > 0} onClick={() => open('approve')}>
                Approve
              </Button>
            )}
          </>
        }
      />

      <div className="flex flex-wrap items-center gap-x-4 gap-y-1 text-sm">
        <ChangeListStatusBadge status={l.status} />
        {l.submittedAt && <span className="text-muted">Sent {formatDateTime(l.submittedAt)}</span>}
        {l.reviewer && l.reviewedAt && (
          <span className="text-muted">
            {l.status === 'approved' ? 'Approved' : 'Rejected'} by {l.reviewer.fullName},{' '}
            {formatDate(l.reviewedAt)}
          </span>
        )}
      </div>

      {problems > 0 && (
        <Alert>
          {problems} {problems === 1 ? 'change cannot' : 'changes cannot'} be made as the building
          is now (see the lines marked below).{' '}
          {can('edit') ? 'Edit the list to fix them.' : 'Reject the list so the staff fix it.'}
        </Alert>
      )}
      {l.status === 'rejected' && l.rejectReason && (
        <Alert tone="notice">
          Rejected: {l.rejectReason}
          {can('edit') && ' Edit the list and send it again.'}
        </Alert>
      )}
      {l.status === 'draft' && (
        <Alert tone="info">Draft: only you see it until you send it for approval.</Alert>
      )}
      {l.status === 'pending' && !can('approve') && (
        <Alert tone="info">Waiting for a manager of {l.buildingNames.join(', ')}.</Alert>
      )}
      {l.status === 'approved' && (
        <Alert tone="info">Approved: every change below is in place.</Alert>
      )}
      {l.note && (
        <div className="text-sm">
          <div className="text-muted mb-1 text-xs">Note</div>
          <p className="whitespace-pre-line">{l.note}</p>
        </div>
      )}

      <Tabs
        items={[
          { value: 'tree', label: 'On the tree' },
          { value: 'lines', label: 'All changes', count: l.items.length },
        ]}
        value={view}
        onChange={setView}
      />
      {view === 'tree' ? (
        tree.data ? (
          // read only; after approval the changes are in the tree itself
          <ChangeTree
            tree={tree.data}
            lines={l.status === 'approved' ? [] : l.items}
            types={[]}
            makers={[]}
          />
        ) : (
          <LoadingBlock rows={4} />
        )
      ) : (
        <ol className="border-border bg-surface rounded-card flex flex-col border">
          {l.items.map((i) => (
            <ItemRow key={i.id} item={i} items={l.items} approved={l.status === 'approved'} />
          ))}
        </ol>
      )}

      {confirm && (
        <ConfirmDialog
          open
          onClose={() => setConfirm(null)}
          title={CONFIRMS[confirm].title}
          description={CONFIRMS[confirm].text}
          confirmLabel={CONFIRMS[confirm].label}
          loading={running.isPending}
          error={running.error?.message}
          onConfirm={() =>
            confirm === 'delete'
              ? remove.mutate(
                  { id: l.id },
                  { onSuccess: () => void navigate(paths.assetChanges, { replace: true }) },
                )
              : running.mutate({ id: l.id }, { onSuccess: () => setConfirm(null) })
          }
        />
      )}
      {rejecting && (
        <Dialog
          open
          onClose={() => setRejecting(false)}
          title="Reject change list"
          description="The staff member reads your reason, fixes the list and sends it again."
        >
          <RejectForm listId={l.id} onDone={() => setRejecting(false)} />
        </Dialog>
      )}
    </div>
  )
}

/** One line: the kind, what it changes and the new values, then its problem if any. */
function ItemRow({
  item: i,
  items,
  approved,
}: {
  item: ChangeItem
  items: ChangeItem[]
  approved: boolean
}) {
  const parent = i.parentKey ? items.find((o) => o.key === i.parentKey) : undefined
  const isAsset = i.kind === 'add_asset' || i.kind === 'edit_asset'
  // Where a new thing goes: a new place of this list, or the existing one (its label).
  const where = parent
    ? `in the new ${parent.kind === 'add_building' ? 'building' : parent.kind === 'add_floor' ? 'floor' : 'room'} ${parent.name}`
    : i.targetLabel && `in ${i.targetLabel}`
  const what =
    i.kind === 'add_building'
      ? `Building ${i.name}`
      : i.kind === 'add_floor'
        ? `Floor ${i.name}`
        : i.kind === 'add_room'
          ? `Room ${i.name}`
          : i.kind === 'add_asset'
            ? (i.name ?? '')
            : (i.targetLabel ?? 'Deleted since')
  const moved = i.kind === 'edit_asset' && (parent || (i.at && i.roomId !== i.at.roomId))
  const details = [
    (i.kind === 'add_floor' || i.kind === 'add_room' || i.kind === 'add_asset') && where,
    (i.kind === 'add_building' || i.kind === 'edit_building') && `Address: ${i.address ?? 'none'}`,
    i.kind === 'edit_building' && `Name: ${i.name}`,
    (i.kind === 'rename_floor' || i.kind === 'rename_room') && `New name: ${i.name}`,
    i.kind === 'edit_asset' && i.name && `Name: ${i.name}`,
    moved && (parent ? `Moves ${where}` : 'Moves to another room (see the tree)'),
    isAsset && i.typeName && `Type: ${i.typeName}`,
    isAsset && `Maker: ${i.manufacturerName ?? 'unknown'}`,
    isAsset && `Warranty: ${i.warrantyEndDate ? formatDate(i.warrantyEndDate) : 'none'}`,
    isAsset && i.hasCopyright && 'Copyrighted',
  ].filter(Boolean)

  return (
    <li className="border-rule flex gap-4 border-b px-5 py-4 last:border-0">
      <span className="text-muted w-6 flex-none pt-0.5 text-right text-sm tabular-nums">
        {i.position}.
      </span>
      <div className="flex min-w-0 flex-1 flex-col gap-1">
        <div className="flex flex-wrap items-center gap-2">
          <Badge
            tone={
              i.kind.startsWith('delete') || i.kind === 'retire_asset'
                ? 'negative'
                : i.kind.startsWith('add')
                  ? 'positive'
                  : 'accent'
            }
          >
            {CHANGE_KIND_LABELS[i.kind]}
          </Badge>
          {approved && i.assetId && i.kind !== 'delete_asset' ? (
            <Link
              to={assetDetailPath(i.assetId)}
              className="font-medium underline-offset-2 hover:underline"
            >
              {what}
            </Link>
          ) : (
            <span className="font-medium">{what}</span>
          )}
        </div>
        {details.length > 0 && <p className="text-muted text-[13px]">{details.join(' · ')}</p>}
        {i.problem && <p className="text-negative text-[13px]">{i.problem}</p>}
      </div>
    </li>
  )
}

function RejectForm({ listId, onDone }: { listId: number; onDone: () => void }) {
  const reject = useChangeMutation(assetChangesApi.reject)
  const {
    register,
    handleSubmit,
    formState: { errors },
  } = useForm<RejectChangeListValues>({
    resolver: zodResolver(rejectChangeListSchema),
    defaultValues: { reason: '' },
  })
  return (
    <form
      onSubmit={handleSubmit((v) => reject.mutate({ id: listId, ...v }, { onSuccess: onDone }))}
      noValidate
      className="flex flex-col gap-4"
    >
      <TextArea
        label="Reason *"
        placeholder="e.g. Room 503 is called 5-03 on the floor plans."
        rows={3}
        error={errors.reason?.message}
        {...register('reason')}
      />
      {reject.error && <Alert>{reject.error.message}</Alert>}
      <DialogActions>
        <Button variant="ghost" onClick={onDone}>
          Back
        </Button>
        <Button type="submit" variant="danger" loading={reject.isPending}>
          Reject
        </Button>
      </DialogActions>
    </form>
  )
}
