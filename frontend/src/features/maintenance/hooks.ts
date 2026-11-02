import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { maintenanceApi, maintenanceKeys } from './api'

export function useChecklists() {
  return useQuery({ queryKey: maintenanceKeys.checklists, queryFn: maintenanceApi.checklists })
}

export function usePlans() {
  return useQuery({ queryKey: maintenanceKeys.plans, queryFn: maintenanceApi.plans })
}

export function usePlan(id: number, enabled = true) {
  return useQuery({
    queryKey: maintenanceKeys.plan(id),
    queryFn: () => maintenanceApi.plan(id),
    enabled,
  })
}

export function usePlanAssets() {
  return useQuery({ queryKey: maintenanceKeys.assets, queryFn: maintenanceApi.assets })
}

export function useTechnicianOptions() {
  return useQuery({ queryKey: maintenanceKeys.technicians, queryFn: maintenanceApi.technicians })
}

export function useServiceTypeOptions() {
  return useQuery({
    queryKey: maintenanceKeys.serviceTypes,
    queryFn: maintenanceApi.serviceTypes,
    staleTime: Infinity,
  })
}

/** Every F2 change refreshes plans, checklists, notifications and the cases (work orders) lists. */
export function useMaintenanceMutation<TVariables, TResult>(
  fn: (variables: TVariables) => Promise<TResult>,
) {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: fn,
    onSuccess: () =>
      Promise.all([
        queryClient.invalidateQueries({ queryKey: maintenanceKeys.all }),
        queryClient.invalidateQueries({ queryKey: ['cases'] }),
      ]),
  })
}
