import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { catalogApi, catalogKeys } from './api'
import type { CatalogList } from './types'

export function useCatalogList<T>(list: CatalogList) {
  return useQuery({ queryKey: catalogKeys.list(list), queryFn: () => catalogApi.list<T>(list) })
}

/**
 * Save or delete in one list. Refreshes every query: the lists feed other screens' pickers
 * (report form, asset form, account form) and the audit log.
 */
export function useCatalogSave(list: CatalogList) {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: ({ id, body }: { id: number | null; body: unknown }) =>
      catalogApi.save(list, id, body),
    onSuccess: () => queryClient.invalidateQueries(),
  })
}

export function useCatalogDelete(list: CatalogList) {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: (id: number) => catalogApi.remove(list, id),
    onSuccess: () => queryClient.invalidateQueries(),
  })
}
