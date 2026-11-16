// Report 4, sections 1 and 2: system architecture, package diagrams, physical ERD.
const architecture = {
  type: 'block',
  nodes: [
    { id: 'users', shape: 'actor', x: 20, y: 150, w: 110, h: 110, title: 'Users', lines: ['Customer, Staff,', 'Technician, Manager,', 'Administrator'] },
    { id: 'clients', shape: 'group', x: 170, y: 30, w: 260, h: 380, title: 'Clients' },
    { id: 'web', x: 195, y: 80, w: 210, h: 100, title: 'Web application', lines: ['React 19 + Vite (TypeScript)', 'TanStack Query, axios', 'Hosted on Vercel'] },
    { id: 'mobile', x: 195, y: 250, w: 210, h: 100, title: 'Mobile application', lines: ['Flutter, same 5 roles', '(next package)'], fill: '#f4f4f4' },
    { id: 'api', shape: 'group', x: 500, y: 30, w: 380, h: 415, title: 'SmartMaint.Api  (ASP.NET Core, .NET 10, Railway)' },
    { id: 'ctrl', x: 525, y: 100, w: 330, h: 60, title: 'Controllers', lines: ['REST endpoints, role check with [AuthorizeRoles]'] },
    { id: 'mod', x: 525, y: 185, w: 330, h: 70, title: 'Modules (business logic)', lines: ['Auth, Cases (F3), Catalog', 'F1, F2, F4, F5, F6, AI: next packages'] },
    { id: 'data', x: 525, y: 280, w: 330, h: 55, title: 'Data', lines: ['AppDbContext (EF Core + Npgsql), migrations, seed'] },
    { id: 'common', x: 525, y: 360, w: 330, h: 70, title: 'Common (used by every layer)', lines: ['JWT, password hashing, current user, RBAC', 'ProblemDetails errors, FluentValidation, paging'] },
    { id: 'db', shape: 'db', x: 960, y: 245, w: 200, h: 115, title: 'PostgreSQL 17', lines: ['Supabase (deployed)', 'Docker container (local)'] },
    { id: 'storage', x: 960, y: 110, w: 200, h: 70, title: 'Supabase Storage', lines: ['case photos (planned)'], fill: '#f4f4f4' },
    { id: 'ai', x: 960, y: 385, w: 200, h: 70, title: 'Replicate', lines: ['LLM API, async (planned)'], fill: '#f4f4f4' },
  ],
  edges: [
    { a: 'users', b: 'web', kind: 'flow', label: 'browser' },
    { a: 'users', b: 'mobile', kind: 'flow', label: 'app' },
    { a: 'web', b: 'ctrl', kind: 'flow', label: 'HTTPS, JSON\nJWT Bearer' },
    { a: 'mobile', b: 'ctrl', kind: 'flow', via: [[460, 300], [460, 145]], bp: [0, 0.75], dash: true },
    { a: 'ctrl', b: 'mod', kind: 'dep' },
    { a: 'mod', b: 'data', kind: 'dep' },
    { a: 'data', b: 'db', kind: 'flow', label: 'SQL (Npgsql)' },
    { a: 'mod', b: 'storage', kind: 'flow', ap: [1, 0.3], via: [[905, 206], [905, 145]], dash: true, label: 'HTTPS', lp: [920, 176] },
    { a: 'mod', b: 'ai', kind: 'flow', ap: [1, 0.7], via: [[925, 234], [925, 420]], dash: true, label: 'HTTPS', lp: [925, 380] },
  ],
}

const backendPackages = {
  type: 'block',
  nodes: [
    { id: 'root', shape: 'group', x: 10, y: 10, w: 1060, h: 540, title: 'SmartMaint.Api (one ASP.NET Core project)' },
    { id: 'ctrl', shape: 'pkg', x: 40, y: 60, w: 230, h: 110, title: 'Controllers', lines: ['AuthController', 'CasesController', 'CatalogController'] },
    { id: 'mod', shape: 'pkg', x: 330, y: 60, w: 470, h: 270, title: 'Modules' },
    { id: 'auth', shape: 'pkg', x: 350, y: 100, w: 135, h: 210, title: 'Auth', lines: ['IAuthService', 'AuthService', 'AuthErrors', 'AuthOptions', 'Dtos/', 'Validators/'] },
    { id: 'cases', shape: 'pkg', x: 500, y: 100, w: 145, h: 210, title: 'Cases', lines: ['ICaseService', 'CaseService', 'CaseTransitions', 'CaseMapper', 'CaseErrors', 'Dtos/', 'Validators/'] },
    { id: 'catalog', shape: 'pkg', x: 660, y: 100, w: 125, h: 210, title: 'Catalog', lines: ['ICatalogService', 'CatalogService', 'Dtos/'] },
    { id: 'common', shape: 'pkg', x: 40, y: 250, w: 230, h: 260, title: 'Common', lines: ['Security/', '  JwtTokenService, PasswordHasher,', '  CurrentUser, AuthorizeRoles', 'Errors/', '  AppException, ProblemFactory,', '  GlobalExceptionHandler', 'Validation/ ValidationFilter', 'Models/ PageRequest, PagedResult', 'Extensions/, Configuration/'] },
    { id: 'data', shape: 'pkg', x: 330, y: 390, w: 320, h: 130, title: 'Data', lines: ['AppDbContext', 'Configurations/ (Users, Buildings, Cases)', 'Conventions/ (enum stored as text)', 'Interceptors/ (created_at, updated_at)', 'Migrations/, Seed/'] },
    { id: 'ent', shape: 'pkg', x: 710, y: 390, w: 330, h: 130, title: 'Entities', lines: ['Entity, AuditableEntity', 'Users/ User, UserRole, UserStatus', 'Buildings/ Building, Floor, Room, Asset, ...', 'Cases/ Case, CaseImage, CaseAssignment, ...'] },
  ],
  edges: [
    { a: 'ctrl', b: 'mod', kind: 'dep', label: '«use»', ap: [1, 0.55], bp: [0, 0.27] },
    { a: 'mod', b: 'data', kind: 'dep', label: '«use»', ap: [0.3, 1], bp: [0.55, 0] },
    { a: 'mod', b: 'ent', kind: 'dep', label: '«use»', ap: [0.85, 1], bp: [0.7, 0] },
    { a: 'data', b: 'ent', kind: 'dep', label: '«use»' },
    { a: 'ctrl', b: 'common', kind: 'dep', label: '«use»' },
    { a: 'mod', b: 'common', kind: 'dep', label: '«use»', ap: [0, 0.85], bp: [1, 0.2] },
  ],
}

const webPackages = {
  type: 'block',
  nodes: [
    { id: 'root', shape: 'group', x: 10, y: 10, w: 1000, h: 470, title: 'frontend/src (React + Vite)' },
    { id: 'app', shape: 'pkg', x: 40, y: 60, w: 230, h: 120, title: 'app', lines: ['App.tsx', 'router.tsx (routes per role)', 'navigation.ts, paths.ts'] },
    { id: 'feat', shape: 'pkg', x: 330, y: 60, w: 455, h: 240, title: 'features' },
    { id: 'auth', shape: 'pkg', x: 350, y: 100, w: 200, h: 180, title: 'auth', lines: ['pages/ LoginPage,', '  ChangePasswordPage', 'components/ AuthProvider,', '  RequireAuth', 'api.ts, hooks.ts,', 'schemas.ts, types.ts'] },
    { id: 'cases', shape: 'pkg', x: 565, y: 100, w: 200, h: 180, title: 'cases', lines: ['pages/ MyRequests, NewRequest,', '  CaseQueue, WorkOrders,', '  CaseDetail', 'components/ CaseActions, dialogs/', 'api.ts, hooks.ts, schemas.ts,', 'types.ts, sla.ts, suggest.ts'] },
    { id: 'shared', shape: 'pkg', x: 330, y: 350, w: 300, h: 110, title: 'shared', lines: ['ui/ Button, Card, Dialog, Table, ...', 'types/ role.ts', 'utils/ format.ts'] },
    { id: 'lib', shape: 'pkg', x: 700, y: 350, w: 280, h: 110, title: 'lib', lines: ['api-client.ts (axios + JWT header)', 'api-error.ts, token-storage.ts', 'query-client.ts, env.ts'] },
  ],
  edges: [
    { a: 'app', b: 'feat', kind: 'dep', label: '«use»', ap: [1, 0.5], bp: [0, 0.25] },
    { a: 'feat', b: 'shared', kind: 'dep', label: '«use»', ap: [0.3, 1], bp: [0.5, 0] },
    { a: 'feat', b: 'lib', kind: 'dep', label: '«use»', ap: [0.85, 1], bp: [0.4, 0] },
    { a: 'app', b: 'shared', kind: 'dep', label: '«use»', ap: [0.5, 1], via: [[155, 405]], bp: [0, 0.5] },
  ],
}

// Physical ERD of the tables that exist today (migrations InitialAuth + AddCasesAndRenameOfficerToStaff).
// created_at / updated_at are left out of the boxes (every table except the assignment / history tables has them).
const T = 'timestamptz'
const erd = {
  type: 'erd',
  nodes: [
    { id: 'sba', name: 'staff_building_assignments', x: 20, y: 20, cols: ['PK FK|staff_id|bigint', 'PK FK|building_id|bigint', '|is_primary|boolean'] },
    { id: 'mba', name: 'manager_building_assignments', x: 20, y: 125, cols: ['PK FK|manager_id|bigint', 'PK FK|building_id|bigint'] },
    { id: 'b', name: 'buildings', x: 290, y: 20, cols: ['PK|id|bigint', 'UQ|name|varchar(100)', '|address|varchar(200)'] },
    { id: 'f', name: 'floors', x: 290, y: 120, cols: ['PK|id|bigint', 'FK|building_id|bigint', '|name|varchar(50)'] },
    { id: 'r', name: 'rooms', x: 290, y: 220, cols: ['PK|id|bigint', 'FK|floor_id|bigint', '|name|varchar(100)'] },
    { id: 'a', name: 'assets', x: 290, y: 320, cols: ['PK|id|bigint', 'FK|room_id|bigint', '|name|varchar(150)'] },
    { id: 'cat', name: 'case_categories', x: 290, y: 430, cols: ['PK|id|bigint', '|group_name|varchar(60)', '|name|varchar(100)', '|description|varchar(300)', '|is_emergency|boolean', 'FK|default_priority_id|bigint', '|default_cost_bearer|varchar(32)', '|photo_required|boolean'] },
    { id: 'pl', name: 'priority_levels', x: 290, y: 610, cols: ['PK|id|bigint', 'UQ|code|varchar(32)', '|name|varchar(50)', '|sla_hours|integer'] },
    { id: 'c', name: 'cases', x: 600, y: 20, cols: ['PK|id|bigint', 'FK|requesting_customer_id|bigint', 'FK|room_id|bigint', 'FK|asset_id|bigint', 'FK|category_id|bigint', '|title|varchar(120)', '|description|varchar(2000)', '|status|varchar(32)', 'FK|priority_level_id|bigint', '|cost_bearer|varchar(32)', 'FK|staff_id|bigint', 'FK|assigned_technician_id|bigint', `|submitted_at|${T}`, `|started_at|${T}`, `|due_at|${T}`, `|completed_at|${T}`, `|closed_at|${T}`, '|actual_hours|numeric(5,2)', `|outsourced_at|${T}`, '|root_cause|varchar(1000)', '|parts_used|varchar(500)', '|hold_reason|varchar(32)', '|hold_until|date', '|rating_stars|integer', '|rating_comment|varchar(500)', `|rated_at|${T}`] },
    { id: 'users', name: 'users', x: 600, y: 520, cols: ['PK|id|bigint', 'UQ|email|varchar(254)', '|full_name|varchar(100)', '|phone|varchar(20)', '|role|varchar(32)', '|status|varchar(32)', '|password_hash|varchar(256)', '|must_change_password|boolean', '|failed_login_count|integer', `|locked_until|${T}`, `|last_login_at|${T}`] },
    { id: 'ca', name: 'case_assignments', x: 900, y: 20, cols: ['PK|id|bigint', 'FK|case_id|bigint', 'FK|technician_id|bigint', 'FK|assigned_by_id|bigint', `|assigned_at|${T}`, '|response|varchar(32)', `|responded_at|${T}`, '|decline_reason|varchar(600)'] },
    { id: 'ch', name: 'case_status_history', x: 900, y: 200, cols: ['PK|id|bigint', 'FK|case_id|bigint', '|from_status|varchar(32)', '|to_status|varchar(32)', 'FK|changed_by_id|bigint', '|reason|varchar(600)', `|changed_at|${T}`] },
    { id: 'ci', name: 'case_images', x: 900, y: 365, cols: ['PK|id|bigint', 'FK|case_id|bigint', '|kind|varchar(32)', '|content|text'] },
  ],
  edges: [
    { a: 'b', b: 'sba', kind: 'rel' },
    { a: 'b', b: 'mba', kind: 'rel', ap: [0, 0.8], via: [[250, 82], [250, 156]], bp: [1, 0.5] },
    { a: 'users', b: 'sba', kind: 'rel', ap: [0.1, 1], via: [[623, 745], [8, 745], [8, 58]], bp: [0, 0.5] },
    { a: 'users', b: 'mba', kind: 'rel', ap: [0.05, 1], via: [[611.5, 735], [14, 735], [14, 156]], bp: [0, 0.5] },
    { a: 'b', b: 'f', kind: 'rel' },
    { a: 'f', b: 'r', kind: 'rel' },
    { a: 'r', b: 'a', kind: 'rel' },
    { a: 'r', b: 'c', kind: 'rel', ap: [1, 0.5], bp: [0, 0.15] },
    { a: 'a', b: 'c', kind: 'rel', from: 'zero-one', ap: [1, 0.5], bp: [0, 0.184] },
    { a: 'cat', b: 'c', kind: 'rel', ap: [1, 0.2], bp: [0, 0.217] },
    { a: 'pl', b: 'cat', kind: 'rel' },
    { a: 'pl', b: 'c', kind: 'rel', ap: [1, 0.5], via: [[565, 656], [565, 176]], bp: [0, 0.353] },
    { a: 'users', b: 'c', kind: 'rel', ap: [0.2, 0], bp: [0.2, 1], label: 'customer' },
    { a: 'users', b: 'c', kind: 'rel', from: 'zero-one', ap: [0.5, 0], bp: [0.5, 1], label: 'staff' },
    { a: 'users', b: 'c', kind: 'rel', from: 'zero-one', ap: [0.8, 0], bp: [0.8, 1], label: 'technician' },
    { a: 'c', b: 'ca', kind: 'rel', ap: [1, 0.1], bp: [0, 0.25] },
    { a: 'c', b: 'ch', kind: 'rel', ap: [1, 0.45], bp: [0, 0.2] },
    { a: 'c', b: 'ci', kind: 'rel', ap: [1, 0.8], bp: [0, 0.25] },
    { a: 'users', b: 'ca', kind: 'rel', ap: [1, 0.3], via: [[1120, 579], [1120, 86]], bp: [1, 0.43], label: 'technician,\nassigned by', lp: [1120, 500] },
    { a: 'users', b: 'ch', kind: 'rel', ap: [1, 0.4], via: [[1100, 599], [1100, 296]], bp: [1, 0.7], label: 'changed by', lp: [1100, 420] },
  ],
}

export default { architecture, 'packages-backend': backendPackages, 'packages-web': webPackages }
