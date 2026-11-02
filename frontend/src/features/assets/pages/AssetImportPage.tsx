import { useState } from 'react'
import { Link } from 'react-router'
import { paths } from '@/app/paths'
import { Alert, Button, Card, CardHeader, LinkButton, PageHeader } from '@/shared/ui'
import { assetsApi } from '../api'
import { useAssetMutation } from '../hooks'
import type { ImportResult } from '../types'

const TEMPLATE =
  'building,floor,room,name,type_code,manufacturer,has_copyright,warranty_end_date\n' +
  'SmartMaint Tower A,5,501,Split AC Daikin 1HP (#AC-503),AC-SPLIT,Daikin,no,2027-12-31\n'

/**
 * AST-05 Import assets from a CSV file, in four steps: choose the file -> check it (nothing saved) -> fix the rows
 * the check names -> import (all rows or none). Excel can save a sheet as CSV.
 */
export function AssetImportPage() {
  const [fileName, setFileName] = useState<string | null>(null)
  const [csv, setCsv] = useState('')
  const [result, setResult] = useState<ImportResult | null>(null)
  const run = useAssetMutation(assetsApi.import)

  const choose = async (file: File | undefined) => {
    setResult(null)
    run.reset()
    if (!file) return
    setFileName(file.name)
    setCsv(await file.text())
  }
  const send = (dryRun: boolean) => run.mutate({ csv, dryRun }, { onSuccess: setResult })

  const checked = result && !result.imported
  const ready = checked && result.errorCount === 0 && result.rowCount > 0
  const template = `data:text/csv;charset=utf-8,${encodeURIComponent(TEMPLATE)}`

  return (
    <div className="flex max-w-[860px] flex-col gap-5">
      <PageHeader
        above={
          <LinkButton variant="ghost" to={paths.assets} className="-ml-3">
            Assets
          </LinkButton>
        }
        title="Import assets"
        description="Check the file first: nothing is saved until every row is correct."
      />

      <Card>
        <CardHeader
          title="1. Choose a CSV file"
          aside={
            <a
              href={template}
              download="assets-template.csv"
              className="text-sm underline-offset-2 hover:underline"
            >
              Download the template
            </a>
          }
        />
        <p className="text-muted mb-3 text-sm">
          Columns: building, floor, room, name, type_code, manufacturer, has_copyright (yes / no),
          warranty_end_date (2027-06-30). The building, floor, room, type code and manufacturer must
          already exist; see{' '}
          <Link to={paths.facilities} className="underline-offset-2 hover:underline">
            Buildings
          </Link>{' '}
          and{' '}
          <Link to={paths.catalog} className="underline-offset-2 hover:underline">
            Catalog
          </Link>
          . At most 1000 rows.
        </p>
        <input
          type="file"
          accept=".csv,text/csv"
          aria-label="CSV file"
          onChange={(e) => void choose(e.target.files?.[0])}
          className="text-sm"
        />
      </Card>

      <div className="flex justify-end gap-2">
        <Button
          disabled={!csv}
          loading={run.isPending && run.variables?.dryRun}
          onClick={() => send(true)}
        >
          2. Check {fileName ?? 'the file'}
        </Button>
        <Button
          variant="primary"
          disabled={!ready}
          loading={run.isPending && !run.variables?.dryRun}
          onClick={() => send(false)}
        >
          4. Import {ready ? `${result.rowCount} assets` : ''}
        </Button>
      </div>

      {run.error && <Alert>{run.error.message}</Alert>}
      {result?.imported && (
        <Alert tone="info">
          {result.rowCount} assets imported.{' '}
          <Link to={paths.assets} className="underline">
            See the list
          </Link>
          .
        </Alert>
      )}
      {checked && (
        <Card>
          <CardHeader
            title="3. Check result"
            aside={`${result.rowCount} rows, ${result.errorCount} with an error`}
          />
          {result.errorCount === 0 ? (
            <p className="text-sm">Every row is correct. Import them with step 4.</p>
          ) : (
            <ul className="flex flex-col">
              {result.rows
                .filter((r) => r.errors.length > 0)
                .map((r) => (
                  <li key={r.line} className="border-rule border-b py-2 text-sm last:border-0">
                    <span className="font-medium tabular-nums">Line {r.line}</span>
                    {r.name && <span className="text-muted"> · {r.name}</span>}
                    <ul className="text-negative mt-1 list-disc pl-5">
                      {r.errors.map((e) => (
                        <li key={e}>{e}</li>
                      ))}
                    </ul>
                  </li>
                ))}
            </ul>
          )}
        </Card>
      )}
    </div>
  )
}
