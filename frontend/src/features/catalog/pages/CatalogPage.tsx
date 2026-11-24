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
  SearchInput,
  Select,
  SortHeader,
  sortRows,
  Tabs,
  TextField,
  useTableSort,
} from '@/shared/ui'
import { cn } from '@/shared/utils/cn'
import { useCatalogArchive, useCatalogDelete, useCatalogList, useCatalogSave } from '../hooks'
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
  { value: 'case-categories', label: 'Fault categories' },
  { value: 'asset-categories', label: 'Asset categories' },
  { value: 'asset-types', label: 'Asset types' },
  { value: 'manufacturers', label: 'Manufacturers' },
  { value: 'specialties', label: 'Specialties' },
] as const satisfies readonly { value: CatalogList; label: string }[]

/**
 * ADM-06 Maintenance categories (Administrator): the lists every other screen chooses from, shared by every building
 * (A03). An item still in use cannot be deleted. A fault category in use and any specialty can be archived instead:
 * it leaves the pick lists and stays on the records that have it.
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
          noun="fault category"
          countLabel="number of cases"
          archive="when-used"
          nameOf={(c) => c.name}
          columns={['Fault category', 'Asset type']}
          cells={(c) => [
            <span key="name" className="font-medium">
              {c.name}
            </span>,
            c.assetTypeName ?? 'All asset types',
          ]}
          form={(item, done) => <CaseCategoryForm item={item} onDone={done} />}
        />
      )}
      {tab === 'asset-categories' && (
        <ListSection<AssetCategory>
          list={tab}
          noun="asset category"
          countLabel="number of assets"
          countOf={(c) => c.assetCount}
          nameOf={(c) => c.categoryName}
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
          countLabel="number of assets"
          countOf={(t) => t.assetCount}
          nameOf={(t) => t.name}
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
          countLabel="number of assets"
          nameOf={(m) => m.name}
          columns={['Manufacturer', 'Contact']}
          cells={(m) => [m.name, [m.phoneNumber, m.email].filter(Boolean).join(' · ') || '—']}
          form={(item, done) => <ManufacturerForm item={item} onDone={done} />}
        />
      )}
      {tab === 'specialties' && (
        <ListSection<Specialty>
          list={tab}
          noun="specialty"
          countLabel="number of technicians"
          archive="always"
          nameOf={(x) => x.name}
          columns={['Specialty']}
          cells={(s) => [s.name]}
          form={(item, done) => <SpecialtyForm item={item} onDone={done} />}
        />
      )}
    </div>
  )
}

// ----- one list: table + add / edit dialog + delete -------------------------------------------------------------

function ListSection<T extends { id: number; usedBy: number; archivedAt?: string | null }>({
  list,
  noun,
  countLabel,
  countOf = (item) => item.usedBy,
  archive,
  nameOf,
  columns,
  cells,
  form,
}: {
  list: CatalogList
  noun: string
  /** What the number column counts. */
  countLabel: string
  countOf?: (item: T) => number
  /** Which rows offer Archive; left out = the list cannot be archived. */
  archive?: 'when-used' | 'always'
  /** What the search looks in and the first column sorts by. */
  nameOf: (item: T) => string
  columns: string[]
  cells: (item: T) => ReactNode[]
  form: (item: T | null, done: () => void) => ReactNode
}) {
  const items = useCatalogList<T>(list)
  const remove = useCatalogDelete(list)
  const setArchived = useCatalogArchive(list)
  const [editing, setEditing] = useState<{ item: T | null } | null>(null)
  const [deleting, setDeleting] = useState<T | null>(null)
  const [archiving, setArchiving] = useState<T | null>(null)
  const [search, setSearch] = useState('')
  const { sort, toggle } = useTableSort<'name' | 'count'>()

  if (items.isPending) return <LoadingBlock rows={6} />
  if (items.error)
    return (
      <ErrorBlock
        what={`the ${noun}s`}
        message={items.error.message}
        onRetry={() => void items.refetch()}
      />
    )

  const words = search.trim().toLowerCase()
  const rows = sortRows(
    items.data.filter((item) => !words || nameOf(item).toLowerCase().includes(words)),
    sort,
    { name: nameOf, count: countOf },
  )

  return (
    <>
      <div className="flex flex-wrap items-center justify-between gap-2">
        <SearchInput
          label={`Search ${noun}`}
          placeholder="Search by name"
          value={search}
          onChange={(e) => setSearch(e.target.value)}
        />
        <Button variant="primary" onClick={() => setEditing({ item: null })}>
          New {noun}
        </Button>
      </div>
      {items.data.length === 0 ? (
        <EmptyBlock>{`No ${noun} yet.`}</EmptyBlock>
      ) : rows.length === 0 ? (
        <EmptyBlock>{`No ${noun} matches “${search}”.`}</EmptyBlock>
      ) : (
        <table className="w-full text-left text-sm">
          <thead>
            <tr className="border-border text-muted border-b text-xs">
              {columns.map((c, i) =>
                i === 0 ? (
                  <SortHeader key={c} sortKey="name" sort={sort} onSort={toggle}>
                    {c}
                  </SortHeader>
                ) : (
                  <th key={c} className="h-9 pr-4 font-normal">
                    {c}
                  </th>
                ),
              )}
              <SortHeader sortKey="count" sort={sort} onSort={toggle} align="right">
                {countLabel}
              </SortHeader>
              <th className="h-9" />
            </tr>
          </thead>
          <tbody>
            {rows.map((item) => (
              <tr
                key={item.id}
                className={cn(
                  'border-rule border-b align-top last:border-0',
                  item.archivedAt && 'text-muted',
                )}
              >
                {cells(item).map((cell, i) => (
                  <td key={i} className="py-3 pr-4">
                    {cell}
                    {i === 0 && item.archivedAt && (
                      <span className="text-muted ml-2 text-xs">Archived</span>
                    )}
                  </td>
                ))}
                <td className="text-muted py-3 pr-4 text-right tabular-nums">{countOf(item)}</td>
                {/* Delete and Archive sit before Edit, so every Edit lands on the same right edge. */}
                <td className="py-2 text-right whitespace-nowrap">
                  {item.usedBy === 0 && (
                    <Button
                      variant="ghost"
                      className="text-muted! hover:text-negative!"
                      onClick={() => (remove.reset(), setDeleting(item))}
                    >
                      Delete
                    </Button>
                  )}
                  {item.archivedAt ? (
                    <Button
                      variant="ghost"
                      loading={setArchived.isPending && setArchived.variables?.id === item.id}
                      onClick={() => setArchived.mutate({ id: item.id, archived: false })}
                    >
                      Restore
                    </Button>
                  ) : (
                    <>
                      {(archive === 'always' || (archive === 'when-used' && item.usedBy > 0)) && (
                        <Button
                          variant="ghost"
                          className="text-muted! hover:text-ink!"
                          onClick={() => (setArchived.reset(), setArchiving(item))}
                        >
                          Archive
                        </Button>
                      )}
                      <Button variant="ghost" onClick={() => setEditing({ item })}>
                        Edit
                      </Button>
                    </>
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
      {archiving && (
        <ConfirmDialog
          open
          onClose={() => setArchiving(null)}
          title={`Archive ${noun}`}
          description="It leaves the pick lists. Records that already use it keep it, and you can restore it later."
          confirmLabel="Archive"
          onConfirm={() =>
            setArchived.mutate(
              { id: archiving.id, archived: true },
              { onSuccess: () => setArchiving(null) },
            )
          }
          loading={setArchived.isPending}
          error={setArchived.error?.message}
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

/** The asset type select keeps its saved value only if its options exist when the form mounts: load them first. */
function CaseCategoryForm({ item, onDone }: { item: CaseCategory | null; onDone: () => void }) {
  const types = useCatalogList<AssetType>('asset-types')
  if (types.error) return <Alert>{types.error.message}</Alert>
  if (!types.data) return <LoadingBlock rows={3} />
  return <CaseCategoryFields item={item} types={types.data} onDone={onDone} />
}

function CaseCategoryFields({
  item,
  types,
  onDone,
}: {
  item: CaseCategory | null
  types: AssetType[]
  onDone: () => void
}) {
  const { save, form } = useCatalogForm<CaseCategoryValues>(
    'case-categories',
    zodResolver(caseCategorySchema),
    {
      name: item?.name ?? '',
      description: item?.description ?? '',
      assetTypeId: item?.assetTypeId ? String(item.assetTypeId) : '',
    },
  )
  const { register, handleSubmit, formState } = form
  const e = formState.errors
  return (
    <FormShell
      onSubmit={handleSubmit((v) =>
        save.mutate(
          {
            id: item?.id ?? null,
            body: {
              name: v.name,
              description: v.description || null,
              assetTypeId: v.assetTypeId ? Number(v.assetTypeId) : null,
            },
          },
          { onSuccess: onDone },
        ),
      )}
      error={save.error?.message}
      saving={save.isPending}
      onDone={onDone}
    >
      <TextField
        label="Name"
        hint="e.g. Not cooling"
        error={e.name?.message}
        {...register('name')}
      />
      <Select
        label="Asset type"
        hint="Customers see this fault only for assets of this type."
        {...register('assetTypeId')}
      >
        <option value="">All asset types</option>
        {types.map((t) => (
          <option key={t.id} value={t.id}>
            {t.name}
          </option>
        ))}
      </Select>
      <TextField
        label="Description"
        hint="Optional"
        error={e.description?.message}
        {...register('description')}
      />
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

/** The category select keeps its saved value only if its options exist when the form mounts: load them first. */
function AssetTypeForm({ item, onDone }: { item: AssetType | null; onDone: () => void }) {
  const categories = useCatalogList<AssetCategory>('asset-categories')
  if (categories.error) return <Alert>{categories.error.message}</Alert>
  if (!categories.data) return <LoadingBlock rows={4} />
  return <AssetTypeFields item={item} categories={categories.data} onDone={onDone} />
}

function AssetTypeFields({
  item,
  categories,
  onDone,
}: {
  item: AssetType | null
  categories: AssetCategory[]
  onDone: () => void
}) {
  const { save, form } = useCatalogForm<AssetTypeValues>(
    'asset-types',
    zodResolver(assetTypeSchema),
    {
      categoryId: item ? String(item.categoryId) : '',
      code: item?.code ?? '',
      name: item?.name ?? '',
      description: item?.description ?? '',
      expectedLifeYears: item?.expectedLifeYears ? String(item.expectedLifeYears) : '',
    },
  )
  const { register, handleSubmit, formState } = form
  return (
    <FormShell
      onSubmit={handleSubmit((v) =>
        save.mutate(
          {
            id: item?.id ?? null,
            body: {
              ...v,
              categoryId: Number(v.categoryId),
              description: v.description || null,
              expectedLifeYears: v.expectedLifeYears ? Number(v.expectedLifeYears) : null,
            },
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
        {categories.map((c) => (
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
