import { apiClient } from '@/lib/api-client'
import type { CatalogList } from './types'

/**
 * CatalogAdminController (Admin): every list has the same four endpoints
 * GET / POST api/catalog/{list}, PUT / DELETE api/catalog/{list}/{id}.
 */
export const catalogApi = {
  list: <T>(list: CatalogList) => apiClient.get<T[]>(`/catalog/${list}`).then((r) => r.data),
  save: <T>(list: CatalogList, id: number | null, body: unknown) =>
    (id === null
      ? apiClient.post<T>(`/catalog/${list}`, body)
      : apiClient.put<T>(`/catalog/${list}/${id}`, body)
    ).then((r) => r.data),
  remove: (list: CatalogList, id: number) =>
    apiClient.delete(`/catalog/${list}/${id}`).then(() => undefined),
}

export const catalogKeys = {
  list: (list: CatalogList) => ['catalog', list] as const,
}
