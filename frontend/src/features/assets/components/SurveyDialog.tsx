import { zodResolver } from '@hookform/resolvers/zod'
import { useForm } from 'react-hook-form'
import { useNavigate } from 'react-router'
import { caseDetailPath } from '@/app/paths'
import { Alert, Button, Dialog, DialogActions, LoadingBlock, Select, TextArea } from '@/shared/ui'
import { assetsApi } from '../api'
import { useAssetMutation, useServiceTypeOptions } from '../hooks'
import { surveySchema, type SurveyFormValues } from '../schemas'
import type { AssetDetail, ServiceTypeOption } from '../types'

/**
 * F5 way 2: the staff open a survey for an asset that fails often. It is an approved case with no customer, not
 * counted in the KPIs; the staff assign a technician, who assesses "repair or replace" as in way 1.
 */
export function SurveyDialog({ asset, onClose }: { asset: AssetDetail; onClose: () => void }) {
  const types = useServiceTypeOptions(true)
  return (
    <Dialog
      open
      onClose={onClose}
      title="Open a survey"
      description={`${asset.name} had ${asset.repairsLast12Months} repairs in 12 months. A technician assesses whether to repair or replace it.`}
    >
      {types.data ? (
        <SurveyForm asset={asset} types={types.data} onDone={onClose} />
      ) : types.error ? (
        <Alert>{types.error.message}</Alert>
      ) : (
        <LoadingBlock rows={2} />
      )}
    </Dialog>
  )
}

function SurveyForm({
  asset,
  types,
  onDone,
}: {
  asset: AssetDetail
  types: ServiceTypeOption[]
  onDone: () => void
}) {
  const navigate = useNavigate()
  const open = useAssetMutation(assetsApi.openSurvey)
  const {
    register,
    handleSubmit,
    formState: { errors },
  } = useForm<SurveyFormValues>({
    resolver: zodResolver(surveySchema),
    defaultValues: { serviceTypeId: '', note: '' },
  })

  return (
    <form
      onSubmit={handleSubmit((v) =>
        open.mutate(
          { assetId: asset.id, serviceTypeId: Number(v.serviceTypeId), note: v.note || null },
          { onSuccess: (c) => void navigate(caseDetailPath(c.id)) },
        ),
      )}
      noValidate
      className="flex flex-col gap-4"
    >
      <Select
        label="Service type *"
        placeholder="Choose…"
        error={errors.serviceTypeId?.message}
        {...register('serviceTypeId')}
      >
        {types.map((t) => (
          <option key={t.id} value={t.id}>
            {t.groupName} · {t.name}
          </option>
        ))}
      </Select>
      <TextArea
        label="What to look at"
        placeholder="Optional, e.g. Third time the compressor trips this year"
        rows={3}
        {...register('note')}
      />
      {open.error && <Alert>{open.error.message}</Alert>}
      <DialogActions>
        <Button variant="ghost" onClick={onDone}>
          Back
        </Button>
        <Button type="submit" variant="primary" loading={open.isPending}>
          Open survey
        </Button>
      </DialogActions>
    </form>
  )
}
