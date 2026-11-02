import { useState } from 'react'
import { Alert, Button, Card, ErrorBlock, LoadingBlock, PageHeader, TextField } from '@/shared/ui'
import { usePriorityLevels, useUpdateSla } from '../hooks'
import type { PriorityLevel } from '../types'

/**
 * ADM-07 SLA settings (Leader 02/11: only the SLA hours of each urgency level).
 * A new value counts from the next time a technician starts work; due dates already set stay.
 */
export function SlaSettingsPage() {
  const levels = usePriorityLevels()

  return (
    <div className="flex max-w-[720px] flex-col gap-5">
      <PageHeader
        title="SLA settings"
        description="Hours from the moment a technician starts work to the deadline. Changes apply to work started after saving."
      />
      {levels.isPending ? (
        <LoadingBlock rows={4} />
      ) : levels.error ? (
        <ErrorBlock
          what="the urgency levels"
          message={levels.error.message}
          onRetry={() => void levels.refetch()}
        />
      ) : (
        <Card padding="none">
          <ul className="flex flex-col">
            {levels.data.map((l) => (
              <SlaRow key={l.id} level={l} />
            ))}
          </ul>
        </Card>
      )}
    </div>
  )
}

function SlaRow({ level }: { level: PriorityLevel }) {
  const [hours, setHours] = useState(String(level.slaHours))
  const update = useUpdateSla()
  const value = Number(hours)
  const valid = Number.isInteger(value) && value >= 1 && value <= 720
  const changed = valid && value !== level.slaHours

  return (
    <li className="border-rule flex flex-col gap-2 border-b px-5 py-4 last:border-0">
      <div className="flex flex-wrap items-center justify-between gap-3">
        <div className="min-w-0">
          <div className="font-medium">{level.name}</div>
          {level.description && <div className="text-muted text-[13px]">{level.description}</div>}
        </div>
        <div className="flex items-end gap-2">
          <TextField
            label="Hours"
            inputMode="numeric"
            className="w-24"
            value={hours}
            onChange={(e) => setHours(e.target.value)}
            error={valid ? undefined : 'Between 1 and 720.'}
          />
          <Button
            variant="secondary"
            disabled={!changed}
            loading={update.isPending}
            onClick={() => update.mutate({ id: level.id, slaHours: value })}
          >
            Save
          </Button>
        </div>
      </div>
      {update.error && <Alert>{update.error.message}</Alert>}
    </li>
  )
}
