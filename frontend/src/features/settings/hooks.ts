import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { settingsApi, settingsKeys } from './api'

export function usePriorityLevels() {
  return useQuery({ queryKey: settingsKeys.priorityLevels, queryFn: settingsApi.priorityLevels })
}

export function useUpdateSla() {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: settingsApi.updateSla,
    onSuccess: () =>
      Promise.all([
        queryClient.invalidateQueries({ queryKey: settingsKeys.priorityLevels }),
        queryClient.invalidateQueries({ queryKey: ['audit'] }),
      ]),
  })
}
