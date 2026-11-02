import type { ReactNode } from 'react'
import { Badge, Card, CardHeader, type BadgeTone } from '@/shared/ui'
import { formatDate, formatDateTime, formatDuration } from '@/shared/utils/format'
import {
  COST_BEARER_LABELS,
  OUTSIDE_KIND_LABELS,
  OUTSIDE_STATUS_LABELS,
  RECOMMENDATION_LABELS,
  type Assessment,
  type DecisionInfo,
  type OutsideWorkStatus,
  type ThirdParty,
} from '../types'

const STATUS_TONES: Record<OutsideWorkStatus, BadgeTone> = {
  quoting: 'neutral',
  awaiting_decision: 'warning',
  working: 'accent',
  failed_check: 'negative',
  declined: 'neutral',
  done: 'positive',
  cancelled: 'neutral',
}

/** F5 step ①: what the technician found and recommends. */
export function AssessmentCard({ assessment }: { assessment: Assessment }) {
  return (
    <Card>
      <CardHeader title="Technician's assessment" aside={formatDateTime(assessment.assessedAt)} />
      <dl className="grid gap-4 text-sm sm:grid-cols-2">
        <Item label="Condition">{assessment.condition}</Item>
        <Item label="Cause">{assessment.cause}</Item>
        <Item label="Recommendation">{RECOMMENDATION_LABELS[assessment.recommendation]}</Item>
      </dl>
    </Card>
  )
}

/**
 * F5 step ③ decision board: facts, no money (G4, G5, N22). "Consider replacing" is a hint; the staff decide.
 * The age counts from when the asset was registered (the ERD has no install date).
 */
export function DecisionInfoCard({ info }: { info: DecisionInfo }) {
  const life = info.expectedLifeYears
  return (
    <Card>
      <CardHeader
        title="For the decision"
        aside={info.suggestReplace ? <Badge tone="warning">Consider replacing</Badge> : undefined}
      />
      <dl className="flex flex-col gap-3 text-sm">
        <Row label="Repairs in 12 months">
          <span className="tabular-nums">{info.repairsLast12Months}</span>
        </Row>
        <Row label="Reopened in 12 months">
          <span className="tabular-nums">{info.reopensLast12Months}</span>
        </Row>
        <Row label="Age">
          <span className="tabular-nums">
            {info.assetAgeYears} {life ? `of ${life} years` : 'years'}
          </span>
        </Row>
        <Row label="Warranty">
          {info.warrantyEndDate
            ? `${info.underWarranty ? 'until' : 'ended'} ${formatDate(info.warrantyEndDate)}`
            : '—'}
        </Row>
        {info.manufacturerName && (
          <Row label="Manufacturer">
            {info.manufacturerName}
            {info.manufacturerPhone && (
              <div className="text-muted text-xs">{info.manufacturerPhone}</div>
            )}
          </Row>
        )}
      </dl>
      {info.reasons.length > 0 && (
        <ul className="border-rule mt-4 flex list-disc flex-col gap-1 border-t pt-3 pl-4 text-[13px]">
          {info.reasons.map((r) => (
            <li key={r}>{r}</li>
          ))}
        </ul>
      )}
    </Card>
  )
}

/** Every outside party called on the case, newest first; the outside phase length is what F6 reports (N16). */
export function ThirdPartiesCard({ parties }: { parties: ThirdParty[] }) {
  const rows = [...parties].reverse()
  return (
    <Card>
      <CardHeader title="Outside party" aside={`${parties.length} in total`} />
      <ul className="flex flex-col">
        {rows.map((t) => (
          <li
            key={t.id}
            className="border-rule border-b py-3 text-sm first:pt-0 last:border-0 last:pb-0"
          >
            <div className="flex flex-wrap items-start justify-between gap-2">
              <div className="min-w-0">
                <div className="font-medium">{t.companyName}</div>
                <div className="text-muted text-[13px]">
                  {OUTSIDE_KIND_LABELS[t.kind]} · {t.job === 'replace' ? 'replace' : 'repair'} ·{' '}
                  {t.contact}
                </div>
              </div>
              <Badge tone={STATUS_TONES[t.status]}>{OUTSIDE_STATUS_LABELS[t.status]}</Badge>
            </div>
            <dl className="mt-2 grid gap-x-6 gap-y-1 text-[13px] sm:grid-cols-2">
              {t.appointmentAt && (
                <Line label="Appointment">{formatDateTime(t.appointmentAt)}</Line>
              )}
              {t.redoCount > 0 && <Line label="Redone">{t.redoCount}×</Line>}
              {t.quoteBearer && (
                <Line label="Quote paid by">{COST_BEARER_LABELS[t.quoteBearer]}</Line>
              )}
              {t.quoteSentTo && (
                <Line label="Quote sent to">
                  {t.quoteSentTo}
                  {t.quoteSentAt && ` · ${formatDateTime(t.quoteSentAt)}`}
                </Line>
              )}
              {t.decidedBy && (
                <Line label={t.decisionGo ? 'Accepted by' : 'Declined by'}>
                  {t.decidedBy}
                  {t.evidenceUrl && (
                    <>
                      {' · '}
                      <a
                        href={t.evidenceUrl}
                        target="_blank"
                        rel="noreferrer"
                        className="underline"
                      >
                        evidence
                      </a>
                    </>
                  )}
                </Line>
              )}
              <Line label="Outside phase">
                {t.finishedAt
                  ? formatDuration(
                      new Date(t.finishedAt).getTime() - new Date(t.startedAt).getTime(),
                    )
                  : `since ${formatDateTime(t.startedAt)}`}
              </Line>
            </dl>
            {t.note && <p className="text-muted mt-1 text-[13px]">{t.note}</p>}
          </li>
        ))}
      </ul>
    </Card>
  )
}

function Item({ label, children }: { label: string; children: ReactNode }) {
  return (
    <div>
      <dt className="text-muted mb-0.5 text-xs">{label}</dt>
      <dd className="whitespace-pre-line">{children}</dd>
    </div>
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

function Line({ label, children }: { label: string; children: ReactNode }) {
  return (
    <div className="flex gap-2">
      <dt className="text-muted">{label}</dt>
      <dd>{children}</dd>
    </div>
  )
}
