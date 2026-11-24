import { keepPreviousData, useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { caseKeys, casesApi } from './api'
import { suggestServiceType } from './suggest'
import type { ServiceType, ServiceTypeSuggestion } from './types'

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

/** Staff: the followers dialog. The previous results stay on screen while the next search loads. */
export function useFollowerOptions(caseId: number, search: string, enabled: boolean) {
  return useQuery({
    queryKey: caseKeys.followerOptions(caseId, search),
    queryFn: () => casesApi.followerOptions(caseId, search),
    enabled,
    placeholderData: keepPreviousData,
  })
}

/** Staff: the technicians to choose from; for a case, the ones who fit it come first. */
export function useTechnicians(enabled: boolean, caseId?: number) {
  return useQuery({
    queryKey: caseKeys.technicians(caseId),
    queryFn: () => casesApi.technicians(caseId),
    enabled,
  })
}

/** Staff: ask who should take the case. Nothing changes until the staff member chooses. */
export function useTechnicianSuggestion() {
  return useMutation({ mutationFn: casesApi.technicianSuggestion })
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

/**
 * Ask the AI for a service type from the description (suggestion only, the customer decides).
 * When the AI is off or fails, the keyword rules answer instead, so the button always gives something (F3 section 4:
 * "if AI fails the screen still works").
 */
export function useServiceTypeSuggestion() {
  return useMutation({
    mutationFn: async ({
      description,
      serviceTypes,
    }: {
      description: string
      serviceTypes: ServiceType[]
    }): Promise<ServiceTypeSuggestion> => {
      try {
        const ai = await casesApi.requestSuggestion(description)
        if (ai.available) {
          return {
            serviceTypeId: ai.serviceTypeId,
            urgency: ai.urgency,
            tips: ai.tips,
            source: 'ai',
          }
        }
      } catch {
        // Too short a description (400), network trouble: fall through to the rules.
      }
      return suggestServiceType(description, serviceTypes)
    },
  })
}
