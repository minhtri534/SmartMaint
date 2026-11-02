import { zodResolver } from '@hookform/resolvers/zod'
import { useState, type ReactNode } from 'react'
import { useForm, type FieldValues, type Resolver } from 'react-hook-form'
import { useSearchParams } from 'react-router'
import {
  Alert,
  Button,
  ConfirmDialog,
  Dialog,
  DialogActions,
  EmptyBlock,
  ErrorBlock,
  LoadingBlock,
  PageHeader,
  Select,
  Tabs,
  TextField,
} from '@/shared/ui'
import { useCatalogDelete, useCatalogList, useCatalogSave } from '../hooks'
import {
  assetCategorySchema,
  assetTypeSchema,
  caseCategorySchema,
  manufacturerSchema,
  specialtySchema,
  type AssetCategoryValues,
  type AssetTypeValues,
  type CaseCategoryValues,
  type ManufacturerValues,
  type SpecialtyValues,
} from '../schemas'
import type {
  AssetCategory,
  AssetType,
  CaseCategory,
  CatalogList,
  Manufacturer,
  Specialty,
} from '../types'

const TABS = [
  { value: 'case-categories', label: 'Service types' },
  { value: 'asset-categories', label: 'Asset categories' },
  { value: 'asset-types', label: 'Asset types' },
  { value: 'manufacturers', label: 'Manufacturers' },
  { value: 'specialties', label: 'Specialties' },
] as const satisfies readonly { value: CatalogList; label: string }[]

const URGENCY_LABELS = {
  emergency: 'Emergency',
  high: 'High',
  normal: 'Normal',
  low: 'Low',
} as const

/**
 * ADM-06 Maintenance categories (Administrator): the lists every other screen chooses from. Service types are shared
 * by every building (A03). An item still in use cannot be deleted; rename it instead.
 */
export function CatalogPage() {
  const [params, setParams] = useSearchParams()
  const tab = TABS.find((t) => t.value === params.get('list'))?.value ?? 'case-categories'

  return (
    <div className="flex flex-col gap-5">
      <PageHeader
        title="Catalog"
        description="The lists people pick from when they report, add assets or create accounts."
      />
      <Tabs items={TABS} value={tab} onChange={(v) => setParams({ list: v }, { replace: true })} />
      {tab === 'case-categories' && (
        <ListSection<CaseCategory>
          list={tab}
          noun="service type"
          columns={['Service type', 'Default urgency', 'Rules']}
          cells={(c) => [
            <>
              <div className="font-medium">{c.name}</div>
              <div className="text-muted text-[13px]">{c.groupName}</div>
            </>,
            URGENCY_LABELS[c.defaultPriority],
            [
              c.isEmergency && 'Emergency (F4)',
              c.photoRequired && 'Photo required',
              c.defaultCostBearer === 'customer' && 'Customer pays',
            ]
              .filter(Boolean)
              .join(', ') || '—',
          ]}
          form={(item, done) => <CaseCategoryForm item={item} onDone={done} />}
        />
      )}
      {tab === 'asset-categories' && (
        <ListSection<AssetCategory>
          list={tab}
          noun="asset category"
          columns={['Category', 'Code']}
          cells={(c) => [
            c.categoryName,
            <span key="code" className="font-mono text-xs">
              {c.code}
            </span>,
          ]}
          form={(item, done) => <AssetCategoryForm item={item} onDone={done} />}
        />
      )}
      {tab === 'asset-types' && (
        <ListSection<AssetType>
          list={tab}
          noun="asset type"
          columns={['Type', 'Code']}
          cells={(t) => [
            <>
              <div className="font-medium">{t.name}</div>
              <div className="text-muted text-[13px]">{t.categoryName}</div>
            </>,
            <span key="code" className="font-mono text-xs">
              {t.code}
            </span>,
          ]}
          form={(item, done) => <AssetTypeForm item={item} onDone={done} />}
        />
      )}
      {tab === 'manufacturers' && (
        <ListSection<Manufacturer>
          list={tab}
          noun="manufacturer"
          columns={['Manufacturer', 'Contact']}
          cells={(m) => [m.name, [m.phoneNumber, m.email].filter(Boolean).join(' · ') || '—']}
          form={(item, done) => <ManufacturerForm item={item} onDone={done} />}
        />
      )}
      {tab === 'specialties' && (
        <ListSection<Specialty>
          list={tab}
          noun="specialty"
          columns={['Specialty']}
          cells={(s) => [s.name]}
          form={(item, done) => <SpecialtyForm item={item} onDone={done} />}
        />
      )}
    </div>
  )
}

// ----- one list: table + add / edit dialog + delete -------------------------------------------------------------

function ListSection<T extends { id: number; usedBy: number }>({
  list,
  noun,
  columns,
  cells,
  form,
}: {
  list: CatalogList
  noun: string
  columns: string[]
  cells: (item: T) => ReactNode[]
  form: (item: T | null, done: () => void) => ReactNode
}) {
  const items = useCatalogList<T>(list)
  const remove = useCatalogDelete(list)
  const [editing, setEditing] = useState<{ item: T | null } | null>(null)
  const [deleting, setDeleting] = useState<T | null>(null)

  if (items.isPending) return <LoadingBlock rows={6} />
  if (items.error)
    return (
      <ErrorBlock
        what={`the ${noun}s`}
        message={items.error.message}
        onRetry={() => void items.refetch()}
      />
    )

  return (
    <>
      <div className="flex justify-end">
        <Button variant="primary" onClick={() => setEditing({ item: null })}>
          New {noun}
        </Button>
      </div>
      {items.data.length === 0 ? (
        <EmptyBlock>{`No ${noun} yet.`}</EmptyBlock>
      ) : (
        <table className="w-full text-left text-sm">
          <thead>
            <tr className="border-border text-muted border-b text-xs">
              {columns.map((c) => (
                <th key={c} className="h-9 pr-4 font-normal">
                  {c}
                </th>
              ))}
              <th className="h-9 pr-4 text-right font-normal">used by</th>
              <th className="h-9" />
            </tr>
          </thead>
          <tbody>
            {items.data.map((item) => (
              <tr key={item.id} className="border-rule border-b align-top last:border-0">
                {cells(item).map((cell, i) => (
                  <td key={i} className="py-3 pr-4">
                    {cell}
                  </td>
                ))}
                <td className="text-muted py-3 pr-4 text-right tabular-nums">{item.usedBy}</td>
                <td className="py-2 text-right whitespace-nowrap">
                  <Button variant="ghost" onClick={() => setEditing({ item })}>
                    Edit
                  </Button>
                  {item.usedBy === 0 && (
                    <Button variant="ghost" onClick={() => (remove.reset(), setDeleting(item))}>
                      Delete
                    </Button>
                  )}
                </td>
              </tr>
            ))}
          </tbody>
        </table>
      )}

      {editing && (
        <Dialog
          open
          onClose={() => setEditing(null)}
          title={`${editing.item ? 'Edit' : 'New'} ${noun}`}
        >
          {form(editing.item, () => setEditing(null))}
        </Dialog>
      )}
      {deleting && (
        <ConfirmDialog
          open
          onClose={() => setDeleting(null)}
          title={`Delete ${noun}`}
          description="Nothing uses it, so it can go."
          confirmLabel="Delete"
          onConfirm={() => remove.mutate(deleting.id, { onSuccess: () => setDeleting(null) })}
          loading={remove.isPending}
          error={remove.error?.message}
        />
      )}
    </>
  )
}

/** The shared shape of the five small forms: RHF + zod, save, Back / Save buttons. */
function useCatalogForm<V extends FieldValues>(
  list: CatalogList,
  resolver: Resolver<V>,
  defaults: V,
) {
  const save = useCatalogSave(list)
  const form = useForm<V>({ resolver, defaultValues: defaults as never })
  return { save, form }
}

function FormShell({
  onSubmit,
  error,
  saving,
  onDone,
  children,
}: {
  onSubmit: () => void
  error?: string
  saving: boolean
  onDone: () => void
  children: ReactNode
}) {
  return (
    <form
      onSubmit={(e) => {
        e.preventDefault()
        onSubmit()
      }}
      noValidate
      className="flex flex-col gap-4"
    >
      {children}
      {error && <Alert>{error}</Alert>}
      <DialogActions>
        <Button variant="ghost" onClick={onDone}>
          Back
        </Button>
        <Button type="submit" variant="primary" loading={saving}>
          Save
        </Button>
      </DialogActions>
    </form>
  )
}

function CaseCategoryForm({ item, onDone }: { item: CaseCategory | null; onDone: () => void }) {
  const { save, form } = useCatalogForm<CaseCategoryValues>(
    'case-categories',
    zodResolver(caseCategorySchema),
    {
      groupName: item?.groupName ?? '',
      name: item?.name ?? '',
      description: item?.description ?? '',
      isEmergency: item?.isEmergency ?? false,
      defaultPriority: item?.defaultPriority ?? 'normal',
      defaultCostBearer: item?.defaultCostBearer ?? 'building',
      photoRequired: item?.photoRequired ?? false,
    },
  )
  const { register, handleSubmit, formState } = form
  const e = formState.errors
  return (
    <FormShell
      onSubmit={handleSubmit((v) =>
        save.mutate(
          { id: item?.id ?? null, body: { ...v, description: v.description || null } },
          { onSuccess: onDone },
        ),
      )}
      error={save.error?.message}
      saving={save.isPending}
      onDone={onDone}
    >
      <div className="grid gap-4 sm:grid-cols-2">
        <TextField
          label="Group"
          hint="e.g. Water"
          error={e.groupName?.message}
          {...register('groupName')}
        />
        <TextField
          label="Name"
          hint="e.g. Leaking pipe or tap"
          error={e.name?.message}
          {...register('name')}
        />
      </div>
      <TextField
        label="Description"
        hint="Optional"
        error={e.description?.message}
        {...register('description')}
      />
      <div className="grid gap-4 sm:grid-cols-2">
        <Select label="Default urgency" {...register('defaultPriority')}>
          {(['emergency', 'high', 'normal', 'low'] as const).map((u) => (
            <option key={u} value={u}>
              {URGENCY_LABELS[u]}
            </option>
          ))}
        </Select>
        <Select label="Who pays by default" {...register('defaultCostBearer')}>
          <option value="building">The building</option>
          <option value="customer">The customer</option>
        </Select>
      </div>
      <label className="flex items-center gap-2 text-sm">
        <input type="checkbox" className="accent-primary size-4" {...register('isEmergency')} />
        Emergency: goes straight to the emergency flow (F4)
      </label>
      <label className="flex items-center gap-2 text-sm">
        <input type="checkbox" className="accent-primary size-4" {...register('photoRequired')} />A
        photo is required when reporting
      </label>
    </FormShell>
  )
}

function AssetCategoryForm({ item, onDone }: { item: AssetCategory | null; onDone: () => void }) {
  const { save, form } = useCatalogForm<AssetCategoryValues>(
    'asset-categories',
    zodResolver(assetCategorySchema),
    {
      code: item?.code ?? '',
      categoryName: item?.categoryName ?? '',
      description: item?.description ?? '',
    },
  )
  const { register, handleSubmit, formState } = form
  return (
    <FormShell
      onSubmit={handleSubmit((v) =>
        save.mutate(
          { id: item?.id ?? null, body: { ...v, description: v.description || null } },
          { onSuccess: onDone },
        ),
      )}
      error={save.error?.message}
      saving={save.isPending}
      onDone={onDone}
    >
      <TextField
        label="Code"
        hint="e.g. HVAC"
        error={formState.errors.code?.message}
        {...register('code')}
      />
      <TextField
        label="Name"
        error={formState.errors.categoryName?.message}
        {...register('categoryName')}
      />
      <TextField label="Description" hint="Optional" {...register('description')} />
    </FormShell>
  )
}

function AssetTypeForm({ item, onDone }: { item: AssetType | null; onDone: () => void }) {
  const categories = useCatalogList<AssetCategory>('asset-categories')
  const { save, form } = useCatalogForm<AssetTypeValues>(
    'asset-types',
    zodResolver(assetTypeSchema),
    {
      categoryId: item ? String(item.categoryId) : '',
      code: item?.code ?? '',
      name: item?.name ?? '',
      description: item?.description ?? '',
    },
  )
  const { register, handleSubmit, formState } = form
  return (
    <FormShell
      onSubmit={handleSubmit((v) =>
        save.mutate(
          {
            id: item?.id ?? null,
            body: { ...v, categoryId: Number(v.categoryId), description: v.description || null },
          },
          { onSuccess: onDone },
        ),
      )}
      error={save.error?.message}
      saving={save.isPending}
      onDone={onDone}
    >
      <Select
        label="Category"
        placeholder="Choose a category"
        error={formState.errors.categoryId?.message}
        {...register('categoryId')}
      >
        {categories.data?.map((c) => (
          <option key={c.id} value={c.id}>
            {c.categoryName}
          </option>
        ))}
      </Select>
      <TextField
        label="Code"
        hint="Used in the import file, e.g. AC-SPLIT"
        error={formState.errors.code?.message}
        {...register('code')}
      />
      <TextField label="Name" error={formState.errors.name?.message} {...register('name')} />
      <TextField label="Description" hint="Optional" {...register('description')} />
    </FormShell>
  )
}

function ManufacturerForm({ item, onDone }: { item: Manufacturer | null; onDone: () => void }) {
  const { save, form } = useCatalogForm<ManufacturerValues>(
    'manufacturers',
    zodResolver(manufacturerSchema),
    {
      name: item?.name ?? '',
      phoneNumber: item?.phoneNumber ?? '',
      email: item?.email ?? '',
    },
  )
  const { register, handleSubmit, formState } = form
  return (
    <FormShell
      onSubmit={handleSubmit((v) =>
        save.mutate(
          {
            id: item?.id ?? null,
            body: { name: v.name, phoneNumber: v.phoneNumber || null, email: v.email || null },
          },
          { onSuccess: onDone },
        ),
      )}
      error={save.error?.message}
      saving={save.isPending}
      onDone={onDone}
    >
      <TextField label="Name" error={formState.errors.name?.message} {...register('name')} />
      <TextField
        label="Phone"
        hint="Optional"
        error={formState.errors.phoneNumber?.message}
        {...register('phoneNumber')}
      />
      <TextField
        label="E-mail"
        hint="Optional"
        error={formState.errors.email?.message}
        {...register('email')}
      />
    </FormShell>
  )
}

function SpecialtyForm({ item, onDone }: { item: Specialty | null; onDone: () => void }) {
  const { save, form } = useCatalogForm<SpecialtyValues>(
    'specialties',
    zodResolver(specialtySchema),
    {
      name: item?.name ?? '',
    },
  )
  const { register, handleSubmit, formState } = form
  return (
    <FormShell
      onSubmit={handleSubmit((v) =>
        save.mutate({ id: item?.id ?? null, body: v }, { onSuccess: onDone }),
      )}
      error={save.error?.message}
      saving={save.isPending}
      onDone={onDone}
    >
      <TextField
        label="Name"
        hint="e.g. Elevator"
        error={formState.errors.name?.message}
        {...register('name')}
      />
    </FormShell>
  )
}
