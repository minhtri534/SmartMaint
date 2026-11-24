import { keepPreviousData, useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { accountKeys, accountsApi } from './api'
import type { AccountListFilters } from './types'

export function useAdminOverview() {
  return useQuery({ queryKey: accountKeys.overview(), queryFn: accountsApi.overview })
}

export function useAccounts(filters: AccountListFilters) {
  return useQuery({
    queryKey: accountKeys.list(filters),
    queryFn: () => accountsApi.list(filters),
    placeholderData: keepPreviousData,
  })
}

export function useAccount(id: number, enabled = true) {
  return useQuery({ queryKey: accountKeys.detail(id), queryFn: () => accountsApi.get(id), enabled })
}

export function useBuildingOptions() {
  return useQuery({
    queryKey: accountKeys.buildings,
    queryFn: accountsApi.buildings,
    staleTime: Infinity,
  })
}

export function useSpecialties() {
  return useQuery({
    queryKey: accountKeys.specialties,
    queryFn: accountsApi.specialties,
    staleTime: Infinity,
  })
}

/**
 * Every account mutation refreshes all account queries (list, detail, overview) and the audit log.
 * Usage: const deactivate = useAccountMutation(accountsApi.deactivate)
 */
/** After a delete the account leaves the cache first, so its page does not ask for it again. */
export function useAccountMutation<TVariables, TResult>(
  fn: (variables: TVariables) => Promise<TResult>,
  options: { deletes?: boolean } = {},
) {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: fn,
    onSuccess: (_result, variables) => {
      if (options.deletes)
        queryClient.removeQueries({
          queryKey: accountKeys.detail((variables as { id: number }).id),
        })
      return Promise.all([
        queryClient.invalidateQueries({ queryKey: accountKeys.all }),
        queryClient.invalidateQueries({ queryKey: ['audit'] }),
      ])
    },
  })
}
