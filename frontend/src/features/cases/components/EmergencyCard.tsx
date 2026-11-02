import type { ReactNode } from 'react'
import { Link } from 'react-router'
import { caseDetailPath } from '@/app/paths'
import { Card, CardHeader } from '@/shared/ui'
import { formatDateTime, formatDuration } from '@/shared/utils/format'
import { FOLLOW_UP_LABELS, type CaseDetail, type IncidentReport } from '../types'

/**
 * F4 milestones on one card: reported → acknowledged (first response) → made safe → fixed. Each shows how long it
 * took from the report; the step still waiting shows how long it has been waiting.
 */
export function EmergencyCard({ c, now }: { c: CaseDetail; now: number }) {
  const e = c.emergency
  if (!e) return null
  const start = new Date(e.startedAt).getTime()
  const since = (at: string | null) => (at ? formatDuration(new Date(at).getTime() - start) : null)
  const steps: { label: string; at: string | null }[] = [
    { label: 'Acknowledged', at: e.acknowledgedAt },
    { label: 'Made safe', at: e.madeSafeAt },
    { label: 'Fixed', at: c.completedAt },
  ]
  const firstOpen = steps.findIndex((s) => !s.at)
  const stopped = c.status === 'cancelled' || c.status === 'rejected'

  return (
    <Card>
      <CardHeader title="Emergency" aside="24/7 clock" />
      <dl className="flex flex-col gap-3 text-sm">
        <Row label="Reported">{formatDateTime(e.startedAt)}</Row>
        {steps.map((s, i) => (
          <Row key={s.label} label={s.label}>
            {s.at ? (
              <span className="flex flex-col items-end">
                {formatDateTime(s.at)}
                <span className="text-muted text-xs tabular-nums">after {since(s.at)}</span>
              </span>
            ) : i === firstOpen && !stopped ? (
              <span className="text-warning tabular-nums">
                waiting {formatDuration(now - start)}
              </span>
            ) : (
              <span className="text-muted">—</span>
            )}
          </Row>
        ))}
      </dl>
      {c.mergedCases.length > 0 && (
        <p className="border-rule mt-4 border-t pt-3 text-[13px]">
          Same incident also reported as{' '}
          {c.mergedCases.map((m, i) => (
            <span key={m.id}>
              {i > 0 && ', '}
              <Link to={caseDetailPath(m.id)} className="font-mono underline underline-offset-2">
                {m.code}
              </Link>
            </span>
          ))}
          .
        </p>
      )}
    </Card>
  )
}

export function IncidentReportCard({ report }: { report: IncidentReport }) {
  return (
    <Card>
      <CardHeader title="Post-incident report" aside={formatDateTime(report.reportedAt)} />
      <dl className="grid gap-4 text-sm sm:grid-cols-2">
        <div>
          <dt className="text-muted mb-0.5 text-xs">Damage</dt>
          <dd className="whitespace-pre-line">{report.damage}</dd>
        </div>
        <div>
          <dt className="text-muted mb-0.5 text-xs">Cause and lessons</dt>
          <dd className="whitespace-pre-line">{report.lessons}</dd>
        </div>
        <div className="sm:col-span-2">
          <dt className="text-muted mb-0.5 text-xs">What next</dt>
          <dd>{FOLLOW_UP_LABELS[report.followUp]}</dd>
        </div>
      </dl>
    </Card>
  )
}

function Row({ label, children }: { label: string; children: ReactNode }) {
  return (
    <div className="flex items-baseline justify-between gap-3">
      <dt className="text-muted">{label}</dt>
      <dd className="text-right">{children}</dd>
    </div>
  )
}
