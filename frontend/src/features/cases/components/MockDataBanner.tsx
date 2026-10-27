import { useQueryClient } from '@tanstack/react-query'
import { ROLE_LABELS } from '@/shared/types/role'
import { Button } from '@/shared/ui'
import { caseKeys } from '../api'
import { mockMe, resetMockData } from '../mock'

/** Reminds everyone the F3 screens run on demo data. Remove together with mock.ts. */
export function MockDataBanner() {
  const queryClient = useQueryClient()
  const me = mockMe()

  return (
    <div className="border-warning/30 bg-warning-soft text-warning flex flex-wrap items-center justify-between gap-2 rounded-md border px-3 py-2 text-sm">
      <span>
        Demo data (no backend yet)
        {me && (
          <>
            {' '}
            — you act as <strong>{me.fullName}</strong> ({ROLE_LABELS[me.role]})
          </>
        )}
        .
      </span>
      <Button
        variant="ghost"
        className="h-7 px-2"
        onClick={() => {
          resetMockData()
          void queryClient.invalidateQueries({ queryKey: caseKeys.all })
        }}
      >
        Reset demo data
      </Button>
    </div>
  )
}
