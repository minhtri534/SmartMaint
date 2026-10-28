import { useQueryClient } from '@tanstack/react-query'
import { caseKeys } from '../api'
import { mockMe, resetMockData } from '../mock'

/** Quiet notice that the F3 screens run on demo data. Remove together with mock.ts. */
export function MockDataBanner() {
  const queryClient = useQueryClient()
  const me = mockMe()

  return (
    <div
      role="status"
      className="border-notice-border bg-pill-warning-bg text-pill-warning-ink rounded-control flex flex-wrap items-center justify-between gap-x-3 gap-y-1 border px-3 py-1.5 text-xs"
    >
      <span>Demo data, no backend yet{me && <> · you act as {me.fullName}</>}</span>
      <button
        type="button"
        className="font-medium underline-offset-2 hover:underline"
        onClick={() => {
          resetMockData()
          void queryClient.invalidateQueries({ queryKey: caseKeys.all })
        }}
      >
        Reset demo data
      </button>
    </div>
  )
}
