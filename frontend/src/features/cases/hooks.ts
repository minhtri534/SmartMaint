import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { useAuth } from '@/features/auth'
import { caseKeys, casesApi } from './api'
import { mockMe } from './mock'
import type { Actor } from './state-machine'

export function useCases() {
  return useQuery({ queryKey: caseKeys.list(), queryFn: casesApi.list })
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

/**
 * The signed-in user as the case rules need it (id + role + buildings in charge).
 * TODO(api): take this from /auth/me once it returns the person id and building assignments.
 */
export function useActor(): Actor | null {
  const { user } = useAuth()
  return user ? mockMe() : null
}

/** Ask the AI for a service type; nothing is saved (AI only suggests). */
export function useServiceTypeSuggestion() {
  return useMutation({ mutationFn: casesApi.suggestServiceType })
}
