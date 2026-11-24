import { useEffect, useRef, useState } from 'react'
import { Navigate, useNavigate, useParams } from 'react-router'
import { assetChangeDetailPath, paths } from '@/app/paths'
import { useFacilityTree } from '@/features/facilities'
import {
  Alert,
  Button,
  Card,
  CardHeader,
  ConfirmDialog,
  ErrorBlock,
  LinkButton,
  LoadingBlock,
  PageHeader,
} from '@/shared/ui'
import { formatDate } from '@/shared/utils/format'
import { assetChangesApi } from '../api'
import { ChangeTree } from '../components/ChangeTree'
import {
  useAssetTypeOptions,
  useChangeList,
  useChangeLists,
  useChangeMutation,
  useManufacturerOptions,
} from '../hooks'
import { toRequest, withoutLine, type Line } from '../tree'
import { CHANGE_KIND_LABELS } from '../types'
import type { ChangeListDetail } from '../types'

/**
 * Staff (meeting note 18/11, tree version 24/11): edit buildings, floors, rooms and assets on the tree. Every edit is a
 * line of the staff member's draft, saved right after it; nothing changes in the building until a manager approves the
 * list. /asset-changes/draft opens the newest draft (or starts one at the first edit); /asset-changes/:id/edit opens a
 * draft or a rejected list.
 */
export function ChangeEditorPage() {
  const routeId = Number(useParams().id) || null
  const lists = useChangeLists()
  const draftId =
    routeId ??
    lists.data
      ?.filter((l) => l.status === 'draft')
      .sort((a, b) => b.updatedAt.localeCompare(a.updatedAt))[0]?.id ??
    null
  const list = useChangeList(draftId)
  const tree = useFacilityTree()
  const types = useAssetTypeOptions()
  const makers = useManufacturerOptions()

  const failed = lists.error ?? list.error ?? tree.error ?? types.error ?? makers.error
  if (failed)
    return (
      <ErrorBlock
        what="the building tree"
        message={failed.message}
        onRetry={() => void Promise.all([lists.refetch(), list.refetch(), tree.refetch()])}
      />
    )
  if (
    (!routeId && !lists.data) ||
    (draftId && !list.data) ||
    !tree.data ||
    !types.data ||
    !makers.data
  )
    return <LoadingBlock rows={6} />
  // A sent or approved list is no longer edited here.
  if (list.data && !list.data.allowedActions.includes('edit'))
    return <Navigate to={assetChangeDetailPath(list.data.id)} replace />
  return (
    <Editor
      key={draftId ?? 'new'}
      list={list.data ?? null}
      tree={tree.data}
      types={types.data}
      makers={makers.data}
    />
  )
}

type SaveState = { status: 'saved' | 'saving' } | { status: 'error'; message: string }

function Editor({
  list,
  tree,
  types,
  makers,
}: {
  list: ChangeListDetail | null
  tree: NonNullable<ReturnType<typeof useFacilityTree>['data']>
  types: NonNullable<ReturnType<typeof useAssetTypeOptions>['data']>
  makers: NonNullable<ReturnType<typeof useManufacturerOptions>['data']>
}) {
  const navigate = useNavigate()
  const [lines, setLines] = useState<Line[]>(list?.items ?? [])
  const [save, setSave] = useState<SaveState>({ status: 'saved' })
  const [discarding, setDiscarding] = useState(false)
  const saver = useChangeMutation(assetChangesApi.save)
  const remove = useChangeMutation(assetChangesApi.remove, { deletes: true })

  // One save at a time: the first one creates the draft, the next ones must use its id.
  const listId = useRef<number | null>(list?.id ?? null)
  const latest = useRef(lines)
  const busy = useRef(false)
  const again = useRef(false)
  const title = useRef(list?.title ?? `Changes of ${formatDate(new Date().toISOString())}`)
  const [code, setCode] = useState(list?.code ?? null)
  const [savedId, setSavedId] = useState(list?.id ?? null)

  const flush = async () => {
    if (busy.current) {
      again.current = true
      return
    }
    busy.current = true
    setSave({ status: 'saving' })
    try {
      do {
        again.current = false
        const sent = latest.current
        const saved = await saver.mutateAsync({
          id: listId.current,
          title: title.current,
          note: list?.note ?? null,
          items: sent.map(toRequest),
        })
        listId.current = saved.id
        setSavedId(saved.id)
        setCode(saved.code)
        // What the server found about each line (its problem, where its target is) joins the lines kept here.
        const byKey = new Map(saved.items.map((i) => [i.key, i]))
        setLines((current) =>
          current.map((l) => {
            const s = byKey.get(l.key)
            return s ? { ...l, problem: s.problem, at: s.at } : l
          }),
        )
      } while (again.current)
      setSave({ status: 'saved' })
    } catch (e) {
      setSave({
        status: 'error',
        message: e instanceof Error ? e.message : 'The draft was not saved.',
      })
    } finally {
      busy.current = false
    }
  }

  // Saved shortly after each edit (several quick edits make one save).
  const [version, setVersion] = useState(0)
  useEffect(() => {
    if (version === 0) return
    const timer = setTimeout(() => void flush(), 350)
    return () => clearTimeout(timer)
    // flush reads refs only
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [version])

  const change = (next: Line[]) => {
    latest.current = next
    setLines(next)
    setVersion((v) => v + 1)
  }

  const problems = lines.filter((l) => l.problem).length
  const status =
    save.status === 'saving'
      ? 'Saving…'
      : save.status === 'error'
        ? 'Not saved'
        : savedId
          ? 'Saved'
          : 'Nothing changed yet'

  return (
    <div className="flex max-w-[1040px] flex-col gap-5">
      <PageHeader
        above={
          <LinkButton variant="ghost" to={paths.assetChanges} className="-ml-3">
            Asset changes
          </LinkButton>
        }
        title="Edit buildings and assets"
        description={
          <>
            {code && <span className="font-mono">{code}</span>}
            {code && ' · '}
            {lines.length} {lines.length === 1 ? 'change' : 'changes'} · {status}. Nothing changes
            until a manager approves the list.
          </>
        }
        actions={
          <>
            {savedId && (
              <Button variant="danger" onClick={() => (remove.reset(), setDiscarding(true))}>
                Discard draft
              </Button>
            )}
            <Button
              variant="primary"
              disabled={lines.length === 0 || save.status !== 'saved' || !savedId}
              onClick={() => void navigate(assetChangeDetailPath(savedId!))}
            >
              Review and send
            </Button>
          </>
        }
      />

      {list?.status === 'rejected' && list.rejectReason && (
        <Alert tone="notice">Rejected: {list.rejectReason} Fix it here, then send it again.</Alert>
      )}
      {save.status === 'error' && <Alert>{save.message}</Alert>}
      {problems > 0 && (
        <Alert tone="notice">
          {problems} {problems === 1 ? 'change cannot' : 'changes cannot'} be made as the buildings
          are now: see the red notes on the tree and the list below it.
        </Alert>
      )}

      <ChangeTree tree={tree} lines={lines} onChange={change} types={types} makers={makers} />

      {/* Every line that cannot be made, also one the tree cannot show (its place is gone). */}
      {problems > 0 && (
        <Card>
          <CardHeader title="Changes that cannot be made" aside={`${problems}`} />
          <ul className="flex flex-col text-sm">
            {lines
              .filter((l) => l.problem)
              .map((l) => (
                <li
                  key={l.key}
                  className="border-rule flex items-start justify-between gap-3 border-b py-2.5 first:pt-0 last:border-0"
                >
                  <div className="min-w-0">
                    <span className="font-medium">{CHANGE_KIND_LABELS[l.kind]}</span>{' '}
                    {l.name ?? l.targetLabel ?? ''}
                    <p className="text-negative text-[13px]">{l.problem}</p>
                  </div>
                  <Button variant="ghost" onClick={() => change(withoutLine(lines, l.key))}>
                    Remove
                  </Button>
                </li>
              ))}
          </ul>
        </Card>
      )}

      {discarding && savedId && (
        <ConfirmDialog
          open
          onClose={() => setDiscarding(false)}
          title="Discard draft"
          description="Every change of this draft is dropped. Nothing in the buildings changes."
          confirmLabel="Discard"
          loading={remove.isPending}
          error={remove.error?.message}
          onConfirm={() =>
            remove.mutate(
              { id: savedId },
              { onSuccess: () => void navigate(paths.assetChanges, { replace: true }) },
            )
          }
        />
      )}
    </div>
  )
}
