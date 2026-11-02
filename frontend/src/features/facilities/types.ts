/** Mirrors backend Modules/Facilities/Dtos. ERD: Buildings -> Floors -> Rooms. */
export interface RoomNode {
  id: number
  /** ERD: Rooms.RoomNumber ("501", or a short name for a common area) */
  roomNumber: string
  /** NOT IN ERD v5: lobby, corridor, restroom... */
  isCommonArea: boolean
  assetCount: number
  /** Customers renting this room. */
  tenants: string[]
}

export interface FloorNode {
  id: number
  /** ERD: Floors.FloorNumber ("G", "5") */
  floorNumber: string
  /** "Floor 5" */
  name: string
  rooms: RoomNode[]
}

export interface BuildingNode {
  id: number
  name: string
  address: string | null
  floors: FloorNode[]
}

export interface BuildingRequest {
  name: string
  address: string | null
}

export interface FloorRequest {
  buildingId: number
  floorNumber: string
}

export interface RoomRequest {
  floorId: number
  roomNumber: string
  isCommonArea: boolean
}
