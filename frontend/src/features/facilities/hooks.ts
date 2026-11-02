import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { facilitiesApi, facilityKeys } from './api'

export function useFacilityTree() {
  return useQuery({ queryKey: facilityKeys.tree, queryFn: facilitiesApi.tree })
}

/** Every change refreshes the tree, the room pickers of other features and the audit log. */
export function useFacilityMutation<TVariables, TResult>(
  fn: (variables: TVariables) => Promise<TResult>,
) {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: fn,
    onSuccess: () => queryClient.invalidateQueries(),
  })
}
