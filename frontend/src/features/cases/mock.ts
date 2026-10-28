import { authKeys, type CurrentUser } from '@/features/auth'
import { ApiError } from '@/lib/api-error'
import { queryClient } from '@/lib/query-client'
import type { Role } from '@/shared/types/role'
import { findTransition, isInvolved, type Actor, type CaseAction } from './state-machine'
import type {
  ApproveCaseRequest,
  AssetRef,
  AssignCaseRequest,
  Building,
  CaseDetail,
  CaseStatus,
  CaseSummary,
  CompleteWorkRequest,
  CreateCaseRequest,
  PersonRef,
  RateCaseRequest,
  ReasonRequest,
  RejectCaseRequest,
  ServiceType,
  ServiceTypeSuggestion,
  TechnicianOption,
  Urgency,
} from './types'
import { REJECT_REASON_LABELS } from './types'

/*
 * MOCK BACKEND for the F3 sketch — delete this file when the Cases API exists.
 * It plays the role of CaseService: same row-level rules (state-machine.ts), same error codes.
 * Data is DEMO ONLY and kept in localStorage, so you can sign out, sign in as another role and
 * continue the same case. "Reset demo data" (MockDataBanner) puts the seed back.
 *
 * Who am I? The real signed-in user (from /auth/me) decides the role; each role is played by one
 * demo person below (the same names as the demo accounts in database/Data.sql).
 */

// ---------------------------------------------------------------------------
// Reference data (F1 owns the real tables)
// ---------------------------------------------------------------------------

const BUILDINGS: Building[] = [
  {
    id: 1,
    name: 'SmartMaint Tower A',
    floors: [
      {
        id: 10,
        name: 'Ground floor',
        rooms: [
          { id: 100, name: 'Main lobby' },
          { id: 101, name: 'Restroom G' },
        ],
      },
      {
        id: 15,
        name: 'Floor 5',
        rooms: [
          { id: 150, name: 'Room 501 · An Phat Co.' },
          { id: 151, name: 'Room 502 · Lumen Studio' },
          { id: 152, name: 'Corridor 5' },
        ],
      },
      {
        id: 22,
        name: 'Floor 12',
        rooms: [
          { id: 220, name: 'Room 1201 · Vista Law' },
          { id: 221, name: 'Meeting room 12' },
        ],
      },
    ],
  },
  {
    id: 2,
    name: 'Riverside Office B',
    floors: [
      {
        id: 33,
        name: 'Floor 3',
        rooms: [
          { id: 330, name: 'Room 301 · Minh Long JSC' },
          { id: 331, name: 'Room 302' },
        ],
      },
    ],
  },
]

const ASSETS: AssetRef[] = [
  { id: 500, name: 'Split AC Daikin 2HP (#AC-501)', roomId: 150 },
  { id: 501, name: 'LED ceiling panel (#LT-501)', roomId: 150 },
  { id: 502, name: 'Split AC Panasonic 1.5HP (#AC-502)', roomId: 151 },
  { id: 503, name: 'Water heater Ariston 30L (#WH-G1)', roomId: 101 },
  { id: 504, name: 'Distribution board DB-12 (#EL-12)', roomId: 220 },
  { id: 505, name: 'Passenger elevator 1 (#EV-01)', roomId: 100 },
]

const SERVICE_TYPES: ServiceType[] = [
  type(1, 'Electrical', 'Power outage in one area', 'high', false, 'building'),
  type(2, 'Electrical', 'Power outage in the whole building', 'emergency', true, 'building'),
  type(3, 'Electrical', 'Socket or switch not working', 'normal', false, 'building'),
  type(4, 'Electrical', 'Breaker keeps tripping', 'normal', false, 'building'),
  type(5, 'Electrical', 'Light not working', 'low', false, 'building'),
  type(10, 'Water', 'Leaking pipe or tap', 'normal', false, 'building', true),
  type(11, 'Water', 'Burst pipe or flooding', 'emergency', true, 'building'),
  type(12, 'Water', 'Clogged drain', 'normal', false, 'building'),
  type(13, 'Water', 'Toilet not flushing', 'normal', false, 'building'),
  type(20, 'Air conditioning', 'Not cooling', 'normal', false, 'building'),
  type(21, 'Air conditioning', 'Water dripping from unit', 'normal', false, 'building', true),
  type(30, 'Elevator', 'Person stuck in elevator', 'emergency', true, 'building'),
  type(31, 'Elevator', 'Elevator out of service', 'high', false, 'building'),
  type(40, 'Doors and locks', 'Door or lock broken', 'normal', false, 'building'),
  type(50, 'Ceiling and walls', 'Water stain on ceiling', 'low', false, 'building', true),
  type(
    51,
    'Ceiling and walls',
    'Tenant fit-out damage (partition, ceiling)',
    'low',
    false,
    'customer',
  ),
  type(60, 'Fire safety', 'False fire alarm', 'high', false, 'building'),
]

function type(
  id: number,
  groupName: string,
  name: string,
  defaultUrgency: Urgency,
  isEmergency: boolean,
  defaultCostBearer: ServiceType['defaultCostBearer'],
  photoRequired = false,
): ServiceType {
  return { id, groupName, name, defaultUrgency, isEmergency, defaultCostBearer, photoRequired }
}

/** Keyword -> service type id, checked in order (more specific first). English + Vietnamese words. */
const KEYWORD_RULES: [RegExp, number][] = [
  [/(burst|flood|ngập|vỡ ống)/, 11],
  [/(stuck|trapped|kẹt).*(elevator|lift|thang)|(elevator|lift|thang).*(stuck|kẹt)/, 30],
  [/(elevator|lift|thang máy)/, 31],
  [/(air ?con|\bac\b|điều hòa|máy lạnh).*(drip|water|nước)|(drip|water).*(air ?con|\bac\b)/, 21],
  [/(air ?con|\bac\b|điều hòa|máy lạnh|cooling|warm air|không mát)/, 20],
  [/(leak|drip|rò|rỉ)/, 10],
  [/(clog|drain|tắc|nghẹt)/, 12],
  [/(toilet|flush|bồn cầu)/, 13],
  [/(breaker|trip|aptomat|cb nhảy)/, 4],
  [/(socket|outlet|switch|ổ cắm|công tắc)/, 3],
  [/(no power|power outage|mất điện)/, 1],
  [/(light|lamp|bulb|flicker|đèn)/, 5],
  [/(door|lock|latch|cửa|khóa)/, 40],
  [/(stain|ceiling|trần|thấm)/, 50],
  [/(fire alarm|smoke alarm|báo cháy)/, 60],
]

// ---------------------------------------------------------------------------
// Demo people. One per role plays "me"; the others fill the data.
// ---------------------------------------------------------------------------

const P = {
  customer: { id: 1001, fullName: 'Demo Customer' },
  bao: { id: 1003, fullName: 'Tran Quoc Bao' },
  hoa: { id: 1002, fullName: 'Le Thi Hoa' },
  officer: { id: 2001, fullName: 'Demo Facility Officer' },
  tuan: { id: 2002, fullName: 'Nguyen Van Tuan' },
  technician: { id: 3001, fullName: 'Demo Technician' },
  duc: { id: 3002, fullName: 'Pham Minh Duc' },
  long: { id: 3003, fullName: 'Vo Thanh Long' },
  manager: { id: 4001, fullName: 'Demo Facility Manager' },
} satisfies Record<string, PersonRef>

/** Officer in charge of each building (O02: primary officer; backup not modelled in the sketch). */
const OFFICER_OF_BUILDING: Record<number, PersonRef> = { 1: P.officer, 2: P.tuan }

const TECHNICIANS: { person: PersonRef; specialties: string[] }[] = [
  { person: P.technician, specialties: ['Electrical', 'Water'] },
  { person: P.duc, specialties: ['Air conditioning', 'Electrical'] },
  { person: P.long, specialties: ['Water', 'Doors and locks'] },
]

const ME_BY_ROLE: Partial<Record<Role, PersonRef & { buildingIds: number[] }>> = {
  customer: { ...P.customer, buildingIds: [] },
  officer: { ...P.officer, buildingIds: [1] },
  technician: { ...P.technician, buildingIds: [] },
  manager: { ...P.manager, buildingIds: [1, 2] },
}

/** The demo person played by the signed-in user. */
export function mockMe(): (Actor & PersonRef) | null {
  const user = queryClient.getQueryData<CurrentUser>(authKeys.me)
  const persona = user ? ME_BY_ROLE[user.role] : undefined
  return user && persona ? { ...persona, role: user.role } : null
}

function requireMe(): Actor & PersonRef {
  const me = mockMe()
  if (!me) throw new ApiError(403, 'auth.forbidden', 'This role has no access to cases.')
  return me
}

// ---------------------------------------------------------------------------
// Seed cases
// ---------------------------------------------------------------------------

const HOUR = 3_600_000
const ago = (hours: number) => new Date(Date.now() - hours * HOUR).toISOString()

/** Mock SLA in wall-clock hours. The real API counts business hours of the building (BR-08, O04). */
const SLA_HOURS: Record<Urgency, number> = { emergency: 2, high: 8, normal: 24, low: 72 }

function slaDue(fromIso: string, urgency: Urgency): string {
  return new Date(new Date(fromIso).getTime() + SLA_HOURS[urgency] * HOUR).toISOString()
}

function placeholderPhoto(label: string, color: string): string {
  const svg = `<svg xmlns="http://www.w3.org/2000/svg" width="320" height="220"><rect width="100%" height="100%" fill="${color}"/><text x="50%" y="50%" fill="#fff" font-family="sans-serif" font-size="20" text-anchor="middle" dominant-baseline="middle">${label}</text></svg>`
  return `data:image/svg+xml;charset=utf-8,${encodeURIComponent(svg)}`
}

function roomLocation(roomId: number): CaseDetail['location'] {
  for (const b of BUILDINGS) {
    for (const f of b.floors) {
      const room = f.rooms.find((r) => r.id === roomId)
      if (room) {
        return {
          buildingId: b.id,
          buildingName: b.name,
          floorName: f.name,
          roomId,
          roomName: room.name,
        }
      }
    }
  }
  throw new ApiError(400, 'validation.failed', 'Unknown room.', { roomId: ['Unknown room.'] })
}

type Step = [to: CaseStatus, by: PersonRef, hoursAgo: number, reason?: string]

interface SeedInput {
  id: number
  title: string
  description: string
  serviceTypeId: number
  roomId: number
  assetId?: number
  requester: PersonRef
  urgency?: Urgency
  hoursAgo: number
  steps?: Step[]
  technician?: PersonRef
  photo?: string
}

function seed(input: SeedInput): CaseDetail {
  const service = SERVICE_TYPES.find((s) => s.id === input.serviceTypeId)!
  const location = roomLocation(input.roomId)
  const urgency = input.urgency ?? service.defaultUrgency
  const submittedAt = ago(input.hoursAgo)
  const c: CaseDetail = {
    id: input.id,
    code: caseCode(input.id),
    title: input.title,
    description: input.description,
    status: 'pending',
    urgency,
    serviceTypeId: service.id,
    serviceTypeName: `${service.groupName} · ${service.name}`,
    location,
    asset: ASSETS.find((a) => a.id === input.assetId) ?? null,
    requester: input.requester,
    officer: OFFICER_OF_BUILDING[location.buildingId] ?? null,
    technician: null,
    submittedAt,
    dueAt: null,
    slaDueAt: null,
    slaStartedAt: null,
    completedAt: null,
    costBearer: service.defaultCostBearer,
    isEmergencyFlow: false,
    photos: input.photo ? [{ id: input.id * 10, url: input.photo, kind: 'report' }] : [],
    history: [{ at: submittedAt, from: null, to: 'pending', by: input.requester, reason: null }],
    assignments: [],
    repairReport: null,
    rating: null,
  }
  for (const [to, by, hoursAgo, reason] of input.steps ?? []) {
    const at = ago(hoursAgo)
    c.history.push({ at, from: c.status, to, by, reason: reason ?? null })
    c.status = to
    if (to === 'approved' && !c.slaDueAt) {
      c.slaStartedAt = at
      c.slaDueAt = c.dueAt = slaDue(at, urgency)
    }
    if (to === 'completed') c.completedAt = at
  }
  if (input.technician) c.technician = input.technician
  return c
}

function seedCases(): CaseDetail[] {
  const cases = [
    seed({
      id: 101,
      title: 'Air conditioner blows warm air',
      description:
        'The AC in our office has been running since 8am but the room is still 30°C. No error light on the unit.',
      serviceTypeId: 20,
      roomId: 150,
      assetId: 500,
      requester: P.customer,
      hoursAgo: 2,
    }),
    seed({
      id: 102,
      title: 'Two lights out in corridor 5',
      description:
        'The two ceiling lights near the fire exit are off. The corridor is dark at night.',
      serviceTypeId: 5,
      roomId: 152,
      requester: P.bao,
      hoursAgo: 5,
    }),
    seed({
      id: 98,
      title: 'Water leaking under the washbasin',
      description: 'Water is dripping from the pipe under the second washbasin, the floor is wet.',
      serviceTypeId: 10,
      roomId: 101,
      requester: P.bao,
      urgency: 'high',
      hoursAgo: 7,
      photo: placeholderPhoto('Leak under washbasin', '#0e7490'),
      steps: [['approved', P.officer, 5]],
    }),
    seed({
      id: 95,
      title: 'Socket next to the printer is dead',
      description: 'The double socket on the left wall gives no power. Other sockets are fine.',
      serviceTypeId: 3,
      roomId: 220,
      requester: P.customer,
      hoursAgo: 9,
      technician: P.technician,
      steps: [
        ['approved', P.officer, 8],
        ['assigned', P.officer, 7],
      ],
    }),
    seed({
      id: 93,
      title: 'AC dripping water on the desk',
      description: 'Water drips from the indoor unit onto the desk below every few minutes.',
      serviceTypeId: 21,
      roomId: 151,
      assetId: 502,
      requester: P.customer,
      hoursAgo: 22,
      technician: P.technician,
      photo: placeholderPhoto('Water on desk', '#475569'),
      steps: [
        ['approved', P.officer, 21],
        ['assigned', P.officer, 20],
        ['accepted', P.technician, 19],
        ['in_progress', P.technician, 2],
      ],
    }),
    seed({
      id: 91,
      title: 'Toilet in restroom G keeps running',
      description: 'The toilet in the second stall does not stop filling after flushing.',
      serviceTypeId: 13,
      roomId: 101,
      requester: P.bao,
      hoursAgo: 50,
      technician: P.long,
      steps: [
        ['approved', P.officer, 49],
        ['assigned', P.officer, 48],
        ['approved', P.technician, 47, 'Plumbing is not my main skill, please give it to Long.'],
        ['assigned', P.officer, 46],
        ['accepted', P.long, 45],
      ],
    }),
    seed({
      id: 90,
      title: 'Breaker trips when the kettle is on',
      description:
        'The pantry breaker trips every time we use the kettle and the microwave together.',
      serviceTypeId: 4,
      roomId: 220,
      assetId: 504,
      requester: P.customer,
      hoursAgo: 30,
      technician: P.duc,
      steps: [
        ['approved', P.officer, 26],
        ['assigned', P.officer, 25],
        ['accepted', P.duc, 24],
        ['in_progress', P.duc, 8],
        ['completed', P.duc, 4],
      ],
    }),
    seed({
      id: 87,
      title: 'Meeting room door does not lock',
      description: 'The key turns but the latch does not move.',
      serviceTypeId: 40,
      roomId: 221,
      requester: P.customer,
      hoursAgo: 120,
      technician: P.long,
      steps: [
        ['approved', P.officer, 119],
        ['assigned', P.officer, 118],
        ['accepted', P.long, 117],
        ['in_progress', P.long, 100],
        ['completed', P.long, 98],
        ['closed', P.officer, 96],
      ],
    }),
    seed({
      id: 85,
      title: 'Ceiling panel flickering',
      description: 'The LED panel above my desk flickers all day.',
      serviceTypeId: 5,
      roomId: 150,
      assetId: 501,
      requester: P.customer,
      hoursAgo: 200,
      technician: P.technician,
      steps: [
        ['approved', P.officer, 199],
        ['assigned', P.officer, 198],
        ['accepted', P.technician, 197],
        ['in_progress', P.technician, 180],
        ['completed', P.technician, 178],
        ['closed', P.officer, 170],
      ],
    }),
    seed({
      id: 80,
      title: 'Something is broken',
      description: 'Please check.',
      serviceTypeId: 12,
      roomId: 150,
      requester: P.customer,
      hoursAgo: 90,
      steps: [
        [
          'rejected',
          P.officer,
          88,
          `${REJECT_REASON_LABELS.missing_info}: which drain, and where? Please add a photo.`,
        ],
      ],
    }),
    seed({
      id: 99,
      title: 'Room 301 has no power',
      description: 'All sockets and lights in room 301 are off since this morning.',
      serviceTypeId: 1,
      roomId: 330,
      requester: P.hoa,
      hoursAgo: 1,
    }),
  ]

  // Assignment history, repair reports and ratings for the seeded cases.
  const byId = new Map(cases.map((c) => [c.id, c]))
  const assignment = (
    t: PersonRef,
    hours: number,
    response: 'waiting' | 'accepted' | 'declined',
    declineReason: string | null = null,
  ) => ({
    technician: t,
    assignedBy: P.officer,
    assignedAt: ago(hours),
    response,
    respondedAt: response === 'waiting' ? null : ago(hours - 1),
    declineReason,
  })
  byId.get(95)!.assignments = [assignment(P.technician, 7, 'waiting')]
  byId.get(93)!.assignments = [assignment(P.technician, 20, 'accepted')]
  byId.get(91)!.assignments = [
    assignment(
      P.technician,
      48,
      'declined',
      'Plumbing is not my main skill, please give it to Long.',
    ),
    assignment(P.long, 46, 'accepted'),
  ]
  byId.get(90)!.assignments = [assignment(P.duc, 25, 'accepted')]
  byId.get(87)!.assignments = [assignment(P.long, 118, 'accepted')]
  byId.get(85)!.assignments = [assignment(P.technician, 198, 'accepted')]

  byId.get(90)!.repairReport = {
    workDone: 'Moved the pantry sockets to a new 20A circuit and replaced the old 16A breaker.',
    rootCause: 'Kettle and microwave on one 16A circuit (about 3.2 kW).',
    partsUsed: '1× MCB 20A Schneider, 6 m of 2.5 mm² cable',
    laborHours: 2.5,
  }
  byId.get(90)!.photos = [
    { id: 901, url: placeholderPhoto('Before: old 16A breaker', '#9a3412'), kind: 'before' },
    { id: 902, url: placeholderPhoto('After: new 20A circuit', '#15803d'), kind: 'after' },
  ]
  for (const id of [87, 85]) {
    byId.get(id)!.repairReport = {
      workDone: id === 87 ? 'Replaced the lock cylinder.' : 'Replaced the LED driver.',
      rootCause: id === 87 ? 'Worn latch spring.' : 'Driver failure.',
      partsUsed: id === 87 ? '1× lock cylinder' : '1× LED driver 40W',
      laborHours: 1,
    }
  }
  byId.get(85)!.rating = { stars: 5, comment: 'Fixed the same day, thank you!', ratedAt: ago(160) }
  return cases
}

// ---------------------------------------------------------------------------
// Store (localStorage, so the demo survives sign-out / reload)
// ---------------------------------------------------------------------------

const STORAGE_KEY = 'smartmaint.mock.f3.v4'

function load(): CaseDetail[] {
  try {
    const raw = localStorage.getItem(STORAGE_KEY)
    if (raw) return JSON.parse(raw) as CaseDetail[]
  } catch {
    // Storage blocked or corrupted: fall back to the seed.
  }
  return seedCases()
}

let cases = load()

function save() {
  try {
    localStorage.setItem(STORAGE_KEY, JSON.stringify(cases))
  } catch {
    // Quota full (large photos) or storage blocked: keep working in memory.
  }
}

export function resetMockData() {
  cases = seedCases()
  save()
}

function caseCode(id: number) {
  return `CS-${String(id).padStart(6, '0')}`
}

/** Fake network: async, slightly slow, and never hands out the stored object itself. */
async function respond<T>(value: T): Promise<T> {
  await new Promise((resolve) => setTimeout(resolve, 250))
  return structuredClone(value)
}

function toSummary(c: CaseDetail): CaseSummary {
  return {
    id: c.id,
    code: c.code,
    title: c.title,
    status: c.status,
    urgency: c.urgency,
    serviceTypeName: c.serviceTypeName,
    location: c.location,
    requester: c.requester,
    officer: c.officer,
    technician: c.technician,
    submittedAt: c.submittedAt,
    dueAt: c.dueAt,
    slaStartedAt: c.slaStartedAt,
    completedAt: c.completedAt,
    isEmergencyFlow: c.isEmergencyFlow,
  }
}

/** Data scope (B3): which cases may this person see at all? */
function visibleTo(me: Actor, c: CaseDetail): boolean {
  switch (me.role) {
    case 'customer':
      return c.requester.id === me.id
    case 'officer':
    case 'manager':
      return me.buildingIds.includes(c.location.buildingId)
    case 'technician':
      return c.technician?.id === me.id || c.assignments.some((a) => a.technician.id === me.id)
    default:
      return false
  }
}

function findVisible(id: number, me: Actor): CaseDetail {
  const c = cases.find((x) => x.id === id)
  if (!c || !visibleTo(me, c)) throw new ApiError(404, 'case.not_found', 'Case not found.')
  return c
}

/** Checks the state machine + row rule, then records the status change. */
function transition(id: number, action: CaseAction, reason: string | null = null): CaseDetail {
  const me = requireMe()
  const c = findVisible(id, me)
  const rule = findTransition(action, me.role, c.status)
  if (!rule || !isInvolved(me, c) || c.isEmergencyFlow) {
    throw new ApiError(
      409,
      'case.invalid_transition',
      'This action is no longer possible. The case may have been changed by someone else. Reload the page.',
    )
  }
  if (rule.to) {
    c.history.push({
      at: new Date().toISOString(),
      from: c.status,
      to: rule.to,
      by: { id: me.id, fullName: me.fullName },
      reason,
    })
    c.status = rule.to
  }
  return c
}

function currentAssignment(c: CaseDetail) {
  return c.assignments.findLast((a) => a.response === 'waiting')
}

// ---------------------------------------------------------------------------
// Endpoints (called by api.ts)
// ---------------------------------------------------------------------------

export const mock = {
  serviceTypes: () => respond(SERVICE_TYPES),
  buildings: () => respond(BUILDINGS),
  assets: (roomId: number) => respond(ASSETS.filter((a) => a.roomId === roomId)),

  /** Stand-in for the AI suggestion (F3 section 4, point 1): simple keyword rules instead of an LLM. */
  suggestServiceType: (description: string): Promise<ServiceTypeSuggestion> => {
    const text = description.toLowerCase()
    const hit = KEYWORD_RULES.find(([pattern]) => pattern.test(text))
    const tips: string[] = []
    if (text.length < 40) tips.push('Add more detail: what exactly happens, and since when?')
    if (!/(desk|stall|unit|left|right|near|next to|corner|room|floor|bàn|góc|cạnh)/.test(text)) {
      tips.push(
        'Say exactly where it is (which desk, unit or stall) so the technician finds it fast.',
      )
    }
    return respond({ serviceTypeId: hit?.[1] ?? null, tips })
  },

  technicians: () =>
    respond<TechnicianOption[]>(
      TECHNICIANS.map((t) => ({
        ...t.person,
        specialties: t.specialties,
        openCases: cases.filter(
          (c) =>
            c.technician?.id === t.person.id &&
            !['closed', 'cancelled', 'completed'].includes(c.status),
        ).length,
      })),
    ),

  list: () => {
    const me = requireMe()
    return respond(
      cases
        .filter((c) => visibleTo(me, c))
        .sort((a, b) => b.submittedAt.localeCompare(a.submittedAt))
        .map(toSummary),
    )
  },

  get: (id: number) => respond(findVisible(id, requireMe())),

  create: (body: CreateCaseRequest) => {
    const me = requireMe()
    if (me.role !== 'customer')
      throw new ApiError(403, 'auth.forbidden', 'Only customers report here.')
    const service = SERVICE_TYPES.find((s) => s.id === body.serviceTypeId)
    if (!service) {
      throw new ApiError(400, 'validation.failed', 'Choose a service type.', {
        serviceTypeId: ['Choose a service type.'],
      })
    }
    const id = Math.max(0, ...cases.map((c) => c.id)) + 1
    const now = new Date().toISOString()
    const location = roomLocation(body.roomId)
    const requester = { id: me.id, fullName: me.fullName }
    const c: CaseDetail = {
      id,
      code: caseCode(id),
      title: body.title,
      description: body.description,
      // N26: an emergency service type is still saved, but F4 handles it (isEmergencyFlow), not F3.
      status: 'pending',
      urgency: service.defaultUrgency,
      serviceTypeId: service.id,
      serviceTypeName: `${service.groupName} · ${service.name}`,
      location,
      asset: ASSETS.find((a) => a.id === body.assetId) ?? null,
      requester,
      officer: OFFICER_OF_BUILDING[location.buildingId] ?? null,
      technician: null,
      submittedAt: now,
      dueAt: null,
      slaDueAt: null,
      slaStartedAt: null,
      completedAt: null,
      costBearer: service.defaultCostBearer,
      isEmergencyFlow: service.isEmergency,
      photos: body.photos.map((url, i) => ({ id: id * 10 + i, url, kind: 'report' as const })),
      history: [
        {
          at: now,
          from: null,
          to: 'pending',
          by: requester,
          reason: service.isEmergency ? 'Emergency service type, handed over to F4.' : null,
        },
      ],
      assignments: [],
      repairReport: null,
      rating: null,
    }
    cases.push(c)
    save()
    return respond(c)
  },

  cancel: (id: number, body: ReasonRequest) => {
    transition(id, 'cancel', body.reason)
    save()
    return respond(undefined)
  },

  approve: (id: number, body: ApproveCaseRequest) => {
    const c = transition(id, 'approve')
    c.urgency = body.urgency
    c.costBearer = body.costBearer
    if (body.urgency === 'emergency') {
      // Officer escalates: the case leaves F3 for the emergency flow (F4, Tùng).
      c.isEmergencyFlow = true
      c.history.at(-1)!.reason = 'Escalated to Emergency, handed over to F4.'
    }
    c.slaStartedAt = new Date().toISOString()
    c.slaDueAt = c.dueAt = slaDue(c.slaStartedAt, body.urgency)
    save()
    return respond(undefined)
  },

  reject: (id: number, body: RejectCaseRequest) => {
    const label = REJECT_REASON_LABELS[body.reason]
    transition(id, 'reject', body.note ? `${label}: ${body.note}` : label)
    save()
    return respond(undefined)
  },

  assign: (id: number, body: AssignCaseRequest) => {
    const tech = TECHNICIANS.find((t) => t.person.id === body.technicianId)
    if (!tech) {
      throw new ApiError(400, 'validation.failed', 'Choose a technician.', {
        technicianId: ['Choose a technician.'],
      })
    }
    const me = requireMe()
    const c = transition(id, 'assign')
    c.technician = tech.person
    c.assignments.push({
      technician: tech.person,
      assignedBy: { id: me.id, fullName: me.fullName },
      assignedAt: new Date().toISOString(),
      response: 'waiting',
      respondedAt: null,
      declineReason: null,
    })
    save()
    return respond(undefined)
  },

  accept: (id: number) => {
    const c = transition(id, 'accept')
    const a = currentAssignment(c)
    if (a) Object.assign(a, { response: 'accepted', respondedAt: new Date().toISOString() })
    save()
    return respond(undefined)
  },

  decline: (id: number, body: ReasonRequest) => {
    const c = transition(id, 'decline', body.reason)
    const a = currentAssignment(c)
    if (a) {
      Object.assign(a, {
        response: 'declined',
        respondedAt: new Date().toISOString(),
        declineReason: body.reason,
      })
    }
    c.technician = null
    save()
    return respond(undefined)
  },

  start: (id: number) => {
    transition(id, 'start')
    save()
    return respond(undefined)
  },

  complete: (id: number, body: CompleteWorkRequest) => {
    const c = transition(id, 'complete')
    const { beforePhotos, afterPhotos, ...report } = body
    c.repairReport = report
    c.completedAt = new Date().toISOString()
    const nextId = Math.max(0, ...c.photos.map((p) => p.id)) + 1
    c.photos.push(
      ...beforePhotos.map((url, i) => ({ id: nextId + i, url, kind: 'before' as const })),
      ...afterPhotos.map((url, i) => ({
        id: nextId + beforePhotos.length + i,
        url,
        kind: 'after' as const,
      })),
    )
    save()
    return respond(undefined)
  },

  close: (id: number) => {
    transition(id, 'close')
    save()
    return respond(undefined)
  },

  reopen: (id: number, body: ReasonRequest) => {
    const me = requireMe()
    const c = transition(id, 'reopen', body.reason)
    // BR-04: by default the same technician gets it back and must accept again.
    if (c.technician) {
      c.assignments.push({
        technician: c.technician,
        assignedBy: { id: me.id, fullName: me.fullName },
        assignedAt: new Date().toISOString(),
        response: 'waiting',
        respondedAt: null,
        declineReason: null,
      })
    }
    c.slaStartedAt = new Date().toISOString()
    c.slaDueAt = c.dueAt = slaDue(c.slaStartedAt, c.urgency)
    save()
    return respond(undefined)
  },

  rate: (id: number, body: RateCaseRequest) => {
    const c = transition(id, 'rate')
    if (c.rating) throw new ApiError(409, 'case.already_rated', 'You have already rated this case.')
    c.rating = { ...body, ratedAt: new Date().toISOString() }
    save()
    return respond(undefined)
  },
}
