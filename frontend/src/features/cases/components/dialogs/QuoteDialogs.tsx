import { zodResolver } from '@hookform/resolvers/zod'
import { Controller, useForm, useWatch } from 'react-hook-form'
import { Alert, Button, Dialog, DialogActions, Select, TextField } from '@/shared/ui'
import { casesApi } from '../../api'
import { useCaseMutation } from '../../hooks'
import {
  quoteDecisionSchema,
  recordQuoteSchema,
  type QuoteDecisionFormValues,
  type RecordQuoteFormValues,
} from '../../schemas'
import { COST_BEARERS, COST_BEARER_LABELS, type CaseDetail } from '../../types'
import { PhotoPicker } from '../PhotoPicker'

interface Props {
  caseDetail: CaseDetail
  open: boolean
  onClose: () => void
}

/**
 * The contractor's quote arrived (R04). No amount is stored: the building decides outside the system, or the
 * staff forward the quote to the customer's contract representative (email, paper) and write down to whom.
 */
export function RecordQuoteDialog({ caseDetail, open, onClose }: Props) {
  return (
    <Dialog
      open={open}
      onClose={onClose}
      title="Quote received"
      description="The amount stays outside the system. If the customer does not answer in 3 days you are reminded."
    >
      <RecordQuoteForm caseDetail={caseDetail} onDone={onClose} />
    </Dialog>
  )
}

function RecordQuoteForm({ caseDetail, onDone }: { caseDetail: CaseDetail; onDone: () => void }) {
  const record = useCaseMutation(casesApi.recordQuote)
  const {
    register,
    control,
    handleSubmit,
    formState: { errors },
  } = useForm<RecordQuoteFormValues>({
    resolver: zodResolver(recordQuoteSchema),
    defaultValues: { bearer: caseDetail.costBearer, sentTo: '', note: '' },
  })
  const bearer = useWatch({ control, name: 'bearer' })

  return (
    <form
      onSubmit={handleSubmit((v) =>
        record.mutate(
          {
            id: caseDetail.id,
            bearer: v.bearer,
            sentTo: v.bearer === 'customer' ? v.sentTo : null,
            note: v.note || null,
          },
          { onSuccess: onDone },
        ),
      )}
      noValidate
      className="flex flex-col gap-4"
    >
      <Select label="Who pays" {...register('bearer')}>
        {COST_BEARERS.map((b) => (
          <option key={b} value={b}>
            {COST_BEARER_LABELS[b]}
          </option>
        ))}
      </Select>
      {bearer === 'customer' && (
        <TextField
          label="Forwarded to *"
          placeholder="e.g. Ms Lan, office manager of ACME"
          hint="The contract representative of the tenant (O13)."
          error={errors.sentTo?.message}
          {...register('sentTo')}
        />
      )}
      <TextField label="Note" hint="Optional" {...register('note')} />
      {record.error && <Alert>{record.error.message}</Alert>}
      <DialogActions>
        <Button variant="ghost" onClick={onDone}>
          Back
        </Button>
        <Button type="submit" variant="primary" loading={record.isPending}>
          Save
        </Button>
      </DialogActions>
    </form>
  )
}

/** F5 step ⑥: the payer's decision on the quote, with evidence. Yes → the party works; no → you choose again. */
export function QuoteDecisionDialog({ caseDetail, open, onClose }: Props) {
  return (
    <Dialog
      open={open}
      onClose={onClose}
      title="Quote decision"
      description="Write down who decided and keep a photo of the email or signed paper."
    >
      <QuoteDecisionForm caseDetail={caseDetail} onDone={onClose} />
    </Dialog>
  )
}

function QuoteDecisionForm({ caseDetail, onDone }: { caseDetail: CaseDetail; onDone: () => void }) {
  const decide = useCaseMutation(casesApi.decideQuote)
  const party = caseDetail.thirdParties.find((t) => t.status === 'awaiting_decision')
  const {
    register,
    control,
    handleSubmit,
    formState: { errors },
  } = useForm<QuoteDecisionFormValues>({
    resolver: zodResolver(quoteDecisionSchema),
    defaultValues: { go: 'yes', decidedBy: party?.quoteSentTo ?? '', note: '', evidence: [] },
  })

  return (
    <form
      onSubmit={handleSubmit((v) =>
        decide.mutate(
          {
            id: caseDetail.id,
            go: v.go === 'yes',
            decidedBy: v.decidedBy,
            note: v.note || null,
            evidence: v.evidence[0] ?? null,
          },
          { onSuccess: onDone },
        ),
      )}
      noValidate
      className="flex flex-col gap-4"
    >
      <fieldset className="flex flex-col gap-2 text-sm">
        <legend className="mb-1 font-medium">Decision</legend>
        <label className="flex items-center gap-2">
          <input type="radio" value="yes" className="accent-primary size-4" {...register('go')} />
          Accepted: {party?.companyName ?? 'the party'} goes ahead
        </label>
        <label className="flex items-center gap-2">
          <input type="radio" value="no" className="accent-primary size-4" {...register('go')} />
          Declined: choose another way
        </label>
      </fieldset>
      <TextField
        label="Decided by *"
        placeholder="Name and role"
        error={errors.decidedBy?.message}
        {...register('decidedBy')}
      />
      <TextField label="Note" hint="Optional" {...register('note')} />
      <Controller
        control={control}
        name="evidence"
        render={({ field }) => (
          <PhotoPicker
            label="Evidence"
            max={1}
            value={field.value}
            onChange={field.onChange}
            hint="A photo of the email or the signed paper."
          />
        )}
      />
      {decide.error && <Alert>{decide.error.message}</Alert>}
      <DialogActions>
        <Button variant="ghost" onClick={onDone}>
          Back
        </Button>
        <Button type="submit" variant="primary" loading={decide.isPending}>
          Save decision
        </Button>
      </DialogActions>
    </form>
  )
}
