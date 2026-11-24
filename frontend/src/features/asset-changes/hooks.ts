import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { assetChangeKeys, assetChangesApi } from './api'

export function useChangeLists() {
  return useQuery({ queryKey: assetChangeKeys.list, queryFn: assetChangesApi.list })
}

export function useChangeList(id: number | null) {
  return useQuery({
    queryKey: assetChangeKeys.detail(id ?? 0),
    queryFn: () => assetChangesApi.get(id!),
    enabled: id !== null,
  })
}

/** The assets in service in an existing room (none for a room the list adds). */
export function useRoomAssets(roomId: number | null) {
  return useQuery({
    queryKey: assetChangeKeys.roomAssets(roomId ?? 0),
    queryFn: () => assetChangesApi.roomAssets(roomId!),
    enabled: roomId !== null,
  })
}

export function useAssetTypeOptions() {
  return useQuery({
    queryKey: assetChangeKeys.types,
    queryFn: assetChangesApi.types,
    staleTime: Infinity,
  })
}

export function useManufacturerOptions() {
  return useQuery({
    queryKey: assetChangeKeys.manufacturers,
    queryFn: assetChangesApi.manufacturers,
    staleTime: Infinity,
  })
}

/**
 * Every change list action refreshes the lists, and (an approval changes the building) the facility tree, the assets
 * and the notifications. After a delete the deleted list leaves the cache first, so its page does not ask for it again.
 */
export function useChangeMutation<TVariables, TResult>(
  fn: (variables: TVariables) => Promise<TResult>,
  options: { deletes?: boolean } = {},
) {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: fn,
    onSuccess: (_result, variables) => {
      if (options.deletes)
        queryClient.removeQueries({
          queryKey: assetChangeKeys.detail((variables as { id: number }).id),
        })
      return Promise.all(
        [assetChangeKeys.all, ['facilities'], ['assets'], ['notifications']].map((queryKey) =>
          queryClient.invalidateQueries({ queryKey }),
        ),
      )
    },
  })
}
