import { useQuery } from '@tanstack/react-query'
import { facilitiesApi, facilityKeys } from './api'

export function useFacilityTree() {
  return useQuery({ queryKey: facilityKeys.tree, queryFn: facilitiesApi.tree })
}
