/** Mirrors backend Modules/Facilities/Dtos. ERD: Buildings -> Floors -> Rooms. */
export interface RoomNode {
  id: number
  /** ERD: Rooms.RoomNumber ("501", "Lobby") */
  roomNumber: string
  assetCount: number
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
