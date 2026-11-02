import { keepPreviousData, useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { assetKeys, assetsApi } from './api'
import type { AssetFilters } from './types'

export function useAssetList(filters: AssetFilters) {
  return useQuery({
    queryKey: assetKeys.list(filters),
    queryFn: () => assetsApi.list(filters),
    placeholderData: keepPreviousData,
  })
}

export function useAssetsForLabels(filters: Omit<AssetFilters, 'page'>) {
  return useQuery({
    queryKey: assetKeys.labels(filters),
    queryFn: () => assetsApi.listForLabels(filters),
  })
}

export function useAsset(id: number, enabled = true) {
  return useQuery({ queryKey: assetKeys.detail(id), queryFn: () => assetsApi.get(id), enabled })
}

export function useQrTarget(id: number) {
  return useQuery({
    queryKey: assetKeys.qr(id),
    queryFn: () => assetsApi.qrTarget(id),
    retry: false,
  })
}

export function useAssetTypes() {
  return useQuery({ queryKey: assetKeys.types, queryFn: assetsApi.types, staleTime: Infinity })
}

export function useManufacturers(enabled: boolean) {
  return useQuery({
    queryKey: assetKeys.manufacturers,
    queryFn: assetsApi.manufacturers,
    enabled,
  })
}

export function usePlaces() {
  return useQuery({ queryKey: assetKeys.places, queryFn: assetsApi.places, staleTime: Infinity })
}

/** Every asset change refreshes asset queries, the facility tree counts and the audit log. */
export function useAssetMutation<TVariables, TResult>(
  fn: (variables: TVariables) => Promise<TResult>,
) {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: fn,
    onSuccess: () =>
      Promise.all([
        queryClient.invalidateQueries({ queryKey: assetKeys.all }),
        queryClient.invalidateQueries({ queryKey: ['facilities'] }),
        queryClient.invalidateQueries({ queryKey: ['audit'] }),
        queryClient.invalidateQueries({ queryKey: ['cases'] }),
      ]),
  })
}

export function useServiceTypeOptions(enabled: boolean) {
  return useQuery({
    queryKey: assetKeys.serviceTypes,
    queryFn: assetsApi.serviceTypes,
    staleTime: Infinity,
    enabled,
  })
}

/** A06 replacement requests (Admin: all; staff and managers: their buildings). */
export function useReplacements() {
  return useQuery({ queryKey: assetKeys.replacements, queryFn: assetsApi.replacements })
}
