import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { caseKeys, casesApi } from './api'
import { suggestServiceType } from './suggest'
import type { ServiceType } from './types'

/** `refetchInterval` (ms): pages that watch live work (the emergency board) poll. */
export function useCases(refetchInterval?: number) {
  return useQuery({ queryKey: caseKeys.list(), queryFn: casesApi.list, refetchInterval })
}

export function useCase(id: number, enabled = true) {
  return useQuery({ queryKey: caseKeys.detail(id), queryFn: () => casesApi.get(id), enabled })
}

export function useServiceTypes() {
  return useQuery({
    queryKey: caseKeys.serviceTypes,
    queryFn: casesApi.serviceTypes,
    staleTime: Infinity,
  })
}

export function useBuildings() {
  return useQuery({
    queryKey: caseKeys.buildings,
    queryFn: casesApi.buildings,
    staleTime: Infinity,
  })
}

export function useAssets(roomId: number | null) {
  return useQuery({
    queryKey: caseKeys.assets(roomId ?? 0),
    queryFn: () => casesApi.assets(roomId!),
    enabled: roomId !== null,
  })
}

/** Look an asset up by the code the customer typed (nothing is cached: each "Find" asks again). */
export function useAssetLookup() {
  return useMutation({ mutationFn: casesApi.assetByCode })
}

/** Staff: the customers of the case's building who may follow it (Edit form). */
export function useFollowerOptions(caseId: number, enabled: boolean) {
  return useQuery({
    queryKey: caseKeys.followerOptions(caseId),
    queryFn: () => casesApi.followerOptions(caseId),
    enabled,
  })
}

export function useTechnicians(enabled: boolean) {
  return useQuery({ queryKey: caseKeys.technicians, queryFn: casesApi.technicians, enabled })
}

/**
 * Every case mutation refreshes all case queries afterwards (list + detail), so every screen shows
 * the new status. Usage: const approve = useCaseMutation(casesApi.approve)
 */
export function useCaseMutation<TVariables, TResult>(
  fn: (variables: TVariables) => Promise<TResult>,
) {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: fn,
    onSuccess: () => queryClient.invalidateQueries({ queryKey: caseKeys.all }),
  })
}

/** Ask for a service type from the description; nothing is saved (suggestion only). */
export function useServiceTypeSuggestion() {
  return useMutation({
    mutationFn: ({
      description,
      serviceTypes,
    }: {
      description: string
      serviceTypes: ServiceType[]
    }) => Promise.resolve(suggestServiceType(description, serviceTypes)),
  })
}
