import { zodResolver } from '@hookform/resolvers/zod'
import { useState } from 'react'
import { useFieldArray, useForm } from 'react-hook-form'
import { useAuth } from '@/features/auth'
import {
  Alert,
  Button,
  ConfirmDialog,
  Dialog,
  DialogActions,
  EmptyBlock,
  ErrorBlock,
  Icon,
  LoadingBlock,
  PageHeader,
  SearchInput,
  SortHeader,
  sortRows,
  TextField,
  useTableSort,
} from '@/shared/ui'
import { cn } from '@/shared/utils/cn'
import { maintenanceApi } from '../api'
import { useChecklists, useMaintenanceMutation } from '../hooks'
import { checklistSchema, type ChecklistFormValues } from '../schemas'
import type { Checklist } from '../types'

/**
 * PM-04 Checklist templates (ERD: CaseChecklists + ChecklistTasks). A plan copies the items of its checklist onto
 * every work order it makes. Administrator and Staff edit; the Manager reads. A checklist used by a plan cannot be
 * deleted (the work orders keep their own copy, so editing is safe).
 */
export function ChecklistsPage() {
  const { user } = useAuth()
  const list = useChecklists()
  const remove = useMaintenanceMutation(maintenanceApi.deleteChecklist)
  const [editing, setEditing] = useState<{ item: Checklist | null } | null>(null)
  const [deleting, setDeleting] = useState<Checklist | null>(null)
  const [openId, setOpenId] = useState<number | null>(null)
  const [search, setSearch] = useState('')
  const { sort, toggle } = useTableSort<'name' | 'plans'>({ key: 'name', dir: 'asc' })
  const words = search.trim().toLowerCase()
  const rows = sortRows(
    (list.data ?? []).filter(
      (c) =>
        !words ||
        c.name.toLowerCase().includes(words) ||
        c.tasks.some((t) => t.name.toLowerCase().includes(words)),
    ),
    sort,
    { name: (c) => c.name, plans: (c) => c.usedByPlans },
  )
  const canEdit = user?.role === 'admin' || user?.role === 'staff'

  return (
    <div className="flex flex-col gap-5">
      <PageHeader
        title="Checklists"
        description="The items a technician checks on each preventive work order."
        actions={
          canEdit && (
            <Button variant="primary" onClick={() => setEditing({ item: null })}>
              New checklist
            </Button>
          )
        }
      />
      <SearchInput
        label="Search checklists"
        placeholder="Checklist or item"
        value={search}
        onChange={(e) => setSearch(e.target.value)}
      />
      {list.isPending ? (
        <LoadingBlock rows={5} />
      ) : list.error ? (
        <ErrorBlock
          what="the checklists"
          message={list.error.message}
          onRetry={() => void list.refetch()}
        />
      ) : list.data.length === 0 ? (
        <EmptyBlock>No checklist yet.</EmptyBlock>
      ) : rows.length === 0 ? (
        <EmptyBlock>{`No checklist matches “${search}”.`}</EmptyBlock>
      ) : (
        <table className="w-full text-left text-sm">
          <thead>
            <tr className="border-border text-muted border-b text-xs">
              <SortHeader sortKey="name" sort={sort} onSort={toggle}>
                Checklist
              </SortHeader>
              <SortHeader sortKey="plans" sort={sort} onSort={toggle} align="right">
                used by plans
              </SortHeader>
              {canEdit && <th className="h-9" />}
            </tr>
          </thead>
          <tbody>
            {rows.map((c) => (
              <tr key={c.id} className="border-rule border-b align-top last:border-0">
                <td className="py-3 pr-4">
                  <button
                    type="button"
                    aria-expanded={openId === c.id}
                    onClick={() => setOpenId((id) => (id === c.id ? null : c.id))}
                    className="flex items-start gap-2 text-left"
                  >
                    <Icon
                      name="back"
                      className={cn(
                        'text-muted ease-snap mt-0.5 size-4 flex-none transition-transform duration-150',
                        openId === c.id ? 'rotate-90' : '-rotate-90',
                      )}
                    />
                    <span>
                      <span className="block font-medium">{c.name}</span>
                      <span className="text-muted block text-[13px]">
                        {c.tasks.length} {c.tasks.length === 1 ? 'item' : 'items'}
                      </span>
                    </span>
                  </button>
                  {openId === c.id && (
                    <ol className="mt-2 ml-6 flex list-decimal flex-col gap-1 pl-4 text-[13px]">
                      {c.tasks.map((t) => (
                        <li key={t.id ?? t.name}>{t.name}</li>
                      ))}
                    </ol>
                  )}
                </td>
                <td className="text-muted py-3 pr-4 text-right tabular-nums">{c.usedByPlans}</td>
                {canEdit && (
                  <td className="py-2 text-right whitespace-nowrap">
                    <Button variant="ghost" onClick={() => setEditing({ item: c })}>
                      Edit
                    </Button>
                    {c.usedByPlans === 0 && (
                      <Button variant="ghost" onClick={() => (remove.reset(), setDeleting(c))}>
                        Delete
                      </Button>
                    )}
                  </td>
                )}
              </tr>
            ))}
          </tbody>
        </table>
      )}

      {editing && (
        <Dialog
          open
          wide
          onClose={() => setEditing(null)}
          title={editing.item ? 'Edit checklist' : 'New checklist'}
          description="Work orders already made keep the items they were made with."
        >
          <ChecklistForm item={editing.item} onDone={() => setEditing(null)} />
        </Dialog>
      )}
      {deleting && (
        <ConfirmDialog
          open
          onClose={() => setDeleting(null)}
          title="Delete checklist"
          description="No plan uses it, so it can go."
          confirmLabel="Delete"
          onConfirm={() =>
            remove.mutate({ id: deleting.id }, { onSuccess: () => setDeleting(null) })
          }
          loading={remove.isPending}
          error={remove.error?.message}
        />
      )}
    </div>
  )
}

function ChecklistForm({ item, onDone }: { item: Checklist | null; onDone: () => void }) {
  const save = useMaintenanceMutation(maintenanceApi.saveChecklist)
  const {
    register,
    control,
    handleSubmit,
    formState: { errors },
  } = useForm<ChecklistFormValues>({
    resolver: zodResolver(checklistSchema),
    defaultValues: {
      name: item?.name ?? '',
      description: item?.description ?? '',
      tasks: item?.tasks.map((t) => ({ name: t.name })) ?? [{ name: '' }],
    },
  })
  const tasks = useFieldArray({ control, name: 'tasks' })

  const onSubmit = handleSubmit((v) =>
    save.mutate(
      {
        id: item?.id ?? null,
        name: v.name,
        description: v.description || null,
        tasks: v.tasks.map((t) => ({ name: t.name, description: null })),
      },
      { onSuccess: onDone },
    ),
  )

  return (
    <form onSubmit={onSubmit} noValidate className="flex flex-col gap-4">
      <TextField
        label="Name *"
        placeholder="e.g. Split AC monthly service"
        error={errors.name?.message}
        {...register('name')}
      />
      <TextField label="Description" hint="Optional" {...register('description')} />
      <fieldset className="flex flex-col gap-2">
        <legend className="mb-1 text-sm font-medium">Items *</legend>
        {tasks.fields.map((row, i) => (
          <div key={row.id} className="flex items-start gap-2">
            <span className="text-muted mt-2.5 w-6 text-right text-sm tabular-nums">{i + 1}.</span>
            <TextField
              label={`Item ${i + 1}`}
              className="flex-1 [&_label]:sr-only"
              placeholder="e.g. Clean the filters"
              error={errors.tasks?.[i]?.name?.message}
              {...register(`tasks.${i}.name`)}
            />
            <Button
              variant="ghost"
              onClick={() => tasks.remove(i)}
              disabled={tasks.fields.length === 1}
              aria-label={`Remove item ${i + 1}`}
            >
              Remove
            </Button>
          </div>
        ))}
        {errors.tasks?.root?.message && (
          <p className="text-negative text-xs">{errors.tasks.root.message}</p>
        )}
        <div>
          <Button variant="secondary" onClick={() => tasks.append({ name: '' })}>
            Add item
          </Button>
        </div>
      </fieldset>
      {save.error && <Alert>{save.error.message}</Alert>}
      <DialogActions>
        <Button variant="ghost" onClick={onDone}>
          Back
        </Button>
        <Button type="submit" variant="primary" loading={save.isPending}>
          Save
        </Button>
      </DialogActions>
    </form>
  )
}
