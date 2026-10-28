# Quy ước code frame SmartMaint

> Dựng 19/10/2026 (tuần 6, code frame); sửa 27/10 (backend 1 project, feature web gọn). Leader chốt: backend **chia theo module, 1 project**, **.NET 10 LTS**, DB dev = **Postgres Docker local**, web = **Tailwind + bộ thư viện tối thiểu**.
> Mọi module / trang mới phải theo đúng **module mẫu Auth** (backend), **trang mẫu Login** và **feature mẫu `cases`** (web). Muốn đổi quy ước → hỏi Trí trước, sửa file này trong cùng PR.

## 1. Phiên bản đã chốt

| Thành phần | Phiên bản | Ghim ở đâu |
|---|---|---|
| .NET SDK | 10.0.x (LTS, hỗ trợ tới 11/2028) | `backend/global.json` |
| ASP.NET Core / EF Core | 10.0.13 | `backend/Directory.Packages.props` |
| Npgsql EF provider | 10.0.3 | như trên |
| dotnet-ef (CLI) | 10.0.13 | `backend/dotnet-tools.json` (`dotnet tool restore`) |
| PostgreSQL | 17 (Docker `postgres:17-alpine`, cùng major với Supabase) | `docker-compose.yml` |
| Node.js | ≥ 24 | `frontend/package.json` → `engines` |
| React / Vite / TypeScript | 19 / 8 / 7 | `frontend/package-lock.json` |
| Router / data / form | react-router 8, TanStack Query 5, axios, react-hook-form 7 + zod 4 | như trên |
| CSS | Tailwind CSS 4 | như trên |
| Font | IBM Plex Sans + Plex Mono (`@fontsource/*` 5.3, OFL) — tự host | như trên |

Thư viện backend: FluentValidation (validate DTO), EFCore.NamingConventions (snake_case tự động), JwtBearer, Swashbuckle SwaggerUI (chỉ phần giao diện; tài liệu OpenAPI do `Microsoft.AspNetCore.OpenApi` sinh). **Không** dùng MediatR, AutoMapper, FluentAssertions (đã chuyển sang thu phí).
Thêm package mới (backend hay web) → hỏi Trí trước, thêm version vào `Directory.Packages.props` / `package.json` trong cùng PR.

## 2. Backend — cấu trúc

> Cập nhật 27/10: gộp 4 project (Domain / Application / Infrastructure / Api) thành **1 project `SmartMaint.Api`, chia theo thư mục**, cho nhóm dễ đọc. Quy tắc tầng vẫn giữ, chỉ không còn tách project.

```
backend/
├── SmartMaint.slnx
├── global.json                  ghim SDK
├── Directory.Build.props        net10.0, nullable, warning = error (áp cho mọi project)
├── Directory.Packages.props     version NuGet — mỗi package ghi version đúng 1 chỗ
├── dotnet-tools.json            dotnet-ef
├── Dockerfile                   image cho Railway
├── src/SmartMaint.Api/
│   ├── Controllers/             <Module>Controller.cs — tầng HTTP, mỏng, không chứa nghiệp vụ
│   ├── Modules/<Module>/        NGHIỆP VỤ của 1 module: I<Module>Service, <Module>Service, <Module>Errors, Dtos/, Validators/
│   │   └── ModuleRegistration.cs   đăng ký service của mọi module (1 dòng / module)
│   ├── Entities/                bảng dưới dạng class C#: Entity, AuditableEntity, <Nhóm>/<Entity>.cs (vd. Users/User.cs)
│   ├── Data/                    truy cập DB
│   │   ├── AppDbContext.cs
│   │   ├── Configurations/<Nhóm>/<Entity>Configuration.cs   tên bảng, cột, index, FK
│   │   ├── Conventions/         enum → text + CHECK
│   │   ├── Interceptors/        tự điền created_at / updated_at
│   │   ├── Migrations/          sinh bằng dotnet ef — không sửa tay
│   │   └── Seed/                chạy Data.sql khi dev
│   ├── Common/                  dùng chung mọi module
│   │   ├── Errors/              AppException, ErrorKind, GlobalExceptionHandler, ProblemFactory
│   │   ├── Security/            JWT, băm mật khẩu, ICurrentUser, [AuthorizeRoles]
│   │   ├── Validation/          ValidationFilter, quy tắc mật khẩu, paging validator
│   │   ├── Models/              PageRequest, PagedResult
│   │   ├── Configuration/       đọc file .env
│   │   └── Extensions/          AddApi / AddDatabase — Program.cs chỉ gọi các hàm này
│   └── Program.cs
└── tests/SmartMaint.UnitTests/
    ├── Common/                  TestType (N/A/B), fake, DbContext in-memory
    └── Modules/<Module>/        <Service>Tests.cs, <Dto>ValidatorTests.cs
```

**Chiều gọi (không được đảo):** `Controllers → Modules → Data / Entities`. Controller không đụng `AppDbContext`; `Entities` không tham chiếu thư mục nào khác. Dịch vụ bên ngoài (JWT, băm mật khẩu, sau này Storage / Replicate) đi qua interface trong `Common/` (`ITokenService`, `IPasswordHasher`, `ICurrentUser`) để test thay bằng fake.

**Ánh xạ sang Package Diagram (Report 4, mục 1.2):** Endpoints = `Controllers/` · Business (Commands / Queries) = `Modules/` · Domains = `Entities/` · Persistence = `Data/` · Infrastructures = `Common/Security`, `Common/Errors`... Mỗi package trên sơ đồ = 1 thư mục thật.

### 2.1 Luồng một request

```
HTTP → AuthController (mỏng) → ValidationFilter (FluentValidation tự chạy) → IAuthService → AppDbContext → Postgres
                                                     lỗi nghiệp vụ: throw AppException → GlobalExceptionHandler → ProblemDetails
```

### 2.2 Quy tắc bắt buộc

| # | Quy tắc |
|---|---|
| B1 | Controller: mỗi action gọi đúng **1** method service, không try/catch, không truy cập DbContext, không tự kiểm tra role. |
| B2 | Phân quyền theo role **chỉ** bằng `[AuthorizeRoles(UserRole.Admin, ...)]`. Mặc định mọi endpoint đã yêu cầu đăng nhập (fallback policy); endpoint công khai ghi `[AllowAnonymous]`. |
| B3 | Phạm vi dữ liệu (Officer chỉ thấy tòa mình phụ trách, Customer chỉ thấy case của mình) kiểm tra **trong service** bằng `ICurrentUser`, không ở controller. |
| B4 | Lỗi nghiệp vụ: `throw <Module>Errors.Xxx()` (trả `AppException` có `Code` dạng `module.reason`). Không throw `Exception` trơn, không tự `return BadRequest(...)`. |
| B5 | Validate input bằng validator đặt cạnh DTO (`Validators/<Dto>Validator.cs`). Validator tự chạy nhờ `ValidationFilter`, không gọi tay. |
| B6 | DTO là `record`, tách request / response; **không bao giờ trả entity** ra API. Mapping viết tay (`static From(entity)`). |
| B7 | Thời gian: lấy từ `TimeProvider` (inject), không gọi `DateTime.Now` / `DateTimeOffset.UtcNow`. Lưu UTC (`timestamptz`). |
| B8 | Mọi method I/O là `async`, nhận `CancellationToken`. |
| B9 | Biến môi trường mới → thêm vào `.env.example` (và `appsettings.json` nếu có giá trị mặc định không bí mật). Secret không bao giờ nằm trong `appsettings*.json`. |

### 2.3 Quy ước API

- Route: `api/<module-số-nhiều-kebab>` (vd. `api/buildings`, `api/cases/{id}/assignments`). JSON camelCase. Enum là chuỗi snake_case giống DB (`"admin"`, `"on_hold"`).
- Danh sách: query `?page=1&pageSize=20` (`PageRequest`, tối đa 100) → trả `PagedResult<T>` `{ items, page, pageSize, totalCount }`.
- Lỗi: luôn là ProblemDetails + `code`. Client chỉ `switch` theo `code`, không theo câu chữ. Lỗi validate: `code = "validation.failed"`, `errors = { field: [messages] }`.
- Mã lỗi chung đã có: `auth.unauthenticated`, `auth.session_expired` (401), `auth.forbidden`, `auth.password_change_required` (403), `validation.failed` (400), `server.error` (500).

## 3. Cách tạo một module backend mới (vd. Buildings — card U4)

1. **Entity:** `Entities/Buildings/Building.cs` kế thừa `AuditableEntity`; enum riêng (`BuildingType.cs`).
2. **Bảng:** `Data/Configurations/Buildings/BuildingConfiguration.cs` — tên bảng snake_case số nhiều, độ dài chuỗi, index, FK, enum dùng `HasEnumText()` + `HasEnumCheck<T>()`.
3. **DbSet:** thêm `DbSet<Building> Buildings` vào `Data/AppDbContext.cs`.
4. **Migration:** `pwsh scripts/db-add-migration.ps1 AddBuildings` → tự sinh migration + cập nhật `database/Schema.sql`. Đọc lại file migration trước khi commit.
5. **Seed:** thêm section `buildings` vào `database/Data.sql` (idempotent, `ON CONFLICT DO NOTHING`, không ghi id).
6. **Nghiệp vụ:** `Modules/Buildings/` gồm `IBuildingService.cs`, `BuildingService.cs` (`internal sealed`), `BuildingErrors.cs`, `Dtos/`, `Validators/`. Đăng ký 1 dòng trong `Modules/ModuleRegistration.cs`.
7. **Controller:** `Controllers/BuildingsController.cs` theo mẫu `AuthController`, gắn `[AuthorizeRoles]` đúng ma trận quyền SRS.
8. **Test:** `tests/.../Modules/Buildings/BuildingServiceTests.cs` — mỗi API ≥ 1 Normal + 1 Abnormal + 1 Boundary.
9. Chạy `dotnet test backend` xanh → mở PR cho Trí.

> ⚠️ ERD chưa đóng băng (`docs/database/ERD_v5_REVIEW.md`): ngoài `users` và Building / Floor / Room, **chưa tạo migration cho bảng nghiệp vụ**.

## 4. Database

| Quy ước | Ví dụ |
|---|---|
| Bảng, cột: snake_case tiếng Anh, bảng số nhiều | `users`, `case_status_history`, `full_name` |
| C# đặt PascalCase, EF tự đổi sang snake_case | `FullName` → `full_name` |
| PK `bigint GENERATED ALWAYS AS IDENTITY` (`UseIdentityAlwaysColumn()`) | `id` |
| Mã hiển thị là cột riêng | `code = 'CS-000123'` |
| FK: `<bảng_số_ít>_id` | `building_id`, `assigned_by_user_id` |
| Thời gian `timestamptz` (UTC), tên `*_at` | `created_at`, `locked_until`, `resolved_at` |
| Enum: text + CHECK, giá trị snake_case | `role IN ('customer', ...)` |
| Mọi bảng nghiệp vụ kế thừa `AuditableEntity` | `created_at`, `updated_at` tự điền, có default `now()` |
| Tên ràng buộc do convention sinh | `pk_users`, `ix_users_email`, `ck_users_role`, `fk_rooms_floors_floor_id` |

**Hai file nộp kèm R4 / R5:**
- `database/Schema.sql` — **sinh tự động** từ migration (`scripts/db-schema.ps1`), idempotent. Không sửa tay.
- `database/Data.sql` — seed **viết tay**, nguồn duy nhất của dữ liệu mẫu. API tự chạy file này khi dev (sau migration).

**Môi trường:**
- Dev: Postgres Docker riêng mỗi máy (`docker compose up -d`). Xóa sạch: `docker compose down -v`.
- Supabase (deploy): chạy `Schema.sql` rồi `Data.sql` bằng SQL editor. API trên Railway **không** tự migrate.
- Không dùng Supabase Auth. Supabase Storage dùng cho file (module Attachment, làm sau).

## 5. Web — cấu trúc

> Cập nhật 27/10: mỗi feature gọn lại thành vài file cố định (`api.ts`, `hooks.ts`, `types.ts`...) thay cho thư mục `api/`, `hooks/`. Feature mẫu đầy đủ nhất: **`features/cases`** (luồng F3).

```
frontend/src/
├── main.tsx
├── index.css            Tailwind + design token (@theme): bg-frame, bg-surface, text-muted, border-border, bg-primary... (xem 5.4)
├── app/                 Khung ứng dụng
│   ├── App.tsx          provider (QueryClient, Auth) + RouterProvider
│   ├── router.tsx       cây route (guard theo role)
│   ├── paths.ts         mọi URL — không viết chuỗi URL ở chỗ khác
│   └── navigation.ts    mục sidebar theo role
├── lib/                 Hạ tầng không có UI: api-client (axios), api-error, query-client, env, token-storage
├── shared/              Dùng chung mọi feature
│   ├── ui/              Button, LinkButton, TextField, Select, TextArea, Dialog, ConfirmDialog, Tabs, Badge,
│   │                    Card, CardHeader, Alert, Spinner, PageHeader, Icon, ProductMark (import từ '@/shared/ui')
│   ├── layout/          AppLayout (sidebar trên khung + tờ trắng), AuthLayout
│   ├── types/           Role, ROLE_WORKSPACES...
│   └── utils/           cn, format (ngày giờ, thời lượng "3 h 10 m", "2 h ago"), apply-field-errors
└── features/<feature>/  Mỗi feature / luồng một thư mục
    ├── types.ts         kiểu dữ liệu, khớp DTO backend (+ nhãn hiển thị của enum)
    ├── api.ts           1 hàm / endpoint (comment ghi rõ METHOD + URL) + <feature>Keys (query keys)
    ├── hooks.ts         useXxx: TanStack Query query / mutation
    ├── schemas.ts       zod schema của các form
    ├── mock.ts          (chỉ khi API chưa có) backend giả — xem 5.3; features/cases đã bỏ mock
    ├── components/      component riêng của feature; dialogs/ cho các DLG-xx
    ├── pages/           <Name>Page.tsx — 1 file / màn hình (mã màn hình trong comment, vd. EMP-02)
    └── index.ts         public API của feature — feature khác chỉ import từ đây
```

Feature có luật riêng thì thêm 1 file đặt tên rõ ràng, vd. `features/cases/state-machine.ts` (bảng chuyển trạng thái F3 — mỗi dòng = 1 mũi tên trên sơ đồ Trang 5).

### 5.1 Quy tắc bắt buộc

| # | Quy tắc |
|---|---|
| W1 | Gọi API **chỉ** qua `features/<f>/api.ts` (dùng `apiClient`). Page không import axios / `apiClient`. |
| W2 | Dữ liệu server dùng TanStack Query (query key đặt trong `<f>Keys`). Không `useEffect` + `fetch` tự viết. |
| W3 | Form: react-hook-form + zod (`schemas.ts`); lỗi field từ server gắn bằng `applyFieldErrors`; lỗi khác hiển thị `<Alert>` theo `error.code`. Đọc giá trị form lúc render bằng `useWatch` (không dùng `watch()`). |
| W4 | Quyền: route bọc `<RequireAuth roles={[...]}>`, sidebar khai trong `navigation.ts` — cùng danh sách role với `[AuthorizeRoles]` của API. Nút không được dùng thì **ẩn** (SRS), không disable. |
| W5 | Import ngoài feature hiện tại dùng alias `@/...`; feature A không import file nội bộ của feature B (chỉ qua `index.ts`). |
| W6 | Style: class Tailwind dùng token trong `index.css`; không viết mã màu hex trong component; component dùng lại được đưa vào `shared/ui`. Luật giao diện ở 5.4. `cn()` chỉ nối class, **không** ghi đè: muốn đổi padding của `Card` thì dùng prop `padding`, đừng truyền `p-0`. |
| W7 | Biến môi trường web chỉ đọc trong `lib/env.ts`, khai trong `frontend/.env.example`, tiền tố `VITE_`. Không đặt secret ở web. |
| W8 | Trước PR: `npm run typecheck`, `npm run lint`, `npm run format`. |

### 5.2 Cách tạo một trang mới (vd. danh sách tòa nhà)

1. `features/buildings/types.ts` — kiểu khớp DTO backend.
2. `features/buildings/api.ts` — hàm gọi endpoint + `buildingKeys`.
3. `features/buildings/hooks.ts` — `useBuildings()` (`useQuery`) / `useCreateBuilding()` (`useMutation`).
4. `features/buildings/pages/BuildingListPage.tsx` — theo mẫu `auth/pages/LoginPage` (form) hoặc `cases/pages/MyRequestsPage` (danh sách).
5. `features/buildings/index.ts` — export page.
6. `app/paths.ts` thêm path; `app/router.tsx` thêm route (bọc `RequireAuth roles`); `app/navigation.ts` thêm mục sidebar nếu cần.

### 5.3 Làm web trước khi có API (dữ liệu giả)

Khi bảng / API chưa có (ERD chưa đóng băng), làm như `features/cases` đã làm ở giai đoạn đầu (28/10 đã nối API thật và xóa `mock.ts`):
- `types.ts` viết theo **nhu cầu màn hình**, mỗi field ghi chú `ERD: Bảng.Cột` hoặc `NOT IN ERD v5` → mang đi họp ERD; thêm / cắt field nào thì sửa đúng chỗ đó.
- `mock.ts` đóng vai backend: cùng luật phạm vi dữ liệu, cùng mã lỗi (`case.invalid_transition`...), lưu localStorage để đăng xuất / đổi role vẫn giữ dữ liệu. Có banner "Demo data" + nút **Reset demo data**.
- `api.ts` gọi `mock.*`, comment ghi sẵn endpoint thật. Khi API xong: thay thân hàm bằng `apiClient...`, xóa `mock.ts` + banner — **pages và hooks không đổi** (đúng như đã xảy ra với cases).
- **Server quyết nút nào hiện:** API trả `allowedActions` cho người gọi (máy trạng thái + role + phạm vi dữ liệu); web chỉ vẽ nút theo danh sách đó, không tự lặp lại luật (W4).

### 5.4 Giao diện — theo skill `design-taste-frontend` (chốt 28/10)

Skill nằm ở `.claude/skills/design-taste-frontend/` (MIT, bản chép từ Leonxlnx/taste-skill, xem `VENDORED.txt`). Màn hình của mình là **app screen**, nên luật nằm ở `references/app-screens.md` của skill. Style đã rút: **framed-sheet** (seed 1994434881). Token nằm hết trong `index.css`.

| # | Quy tắc giao diện |
|---|---|
| U1 | Khung: sidebar 228px nằm trên **nền khung xám xanh thép** (`bg-frame`); trang nằm trên **tờ trắng** viền 1px, bo 16px, **không đổ bóng**. Dưới 1024px sidebar gập thành menu. |
| U2 | **Mỗi màn hình chỉ 1 nút chính** (`variant="primary"`, gần đen). Còn lại `secondary` (viền), `ghost` (chữ), `danger` (viền đỏ, cho hủy / xóa). |
| U3 | **Màu nhấn teal chỉ dùng cho dữ liệu** (thanh SLA, bộ lọc đang chọn) và focus. Không tô màu sidebar, tab, nút. Đỏ chỉ cho cái đang quá hạn / lỗi — tối đa 1 dấu đỏ mỗi đối tượng. |
| U4 | Trạng thái = **pill nền nhạt chữ đậm cùng màu** (`Badge`), 1 pill mỗi dòng. Mức khẩn cấp chỉ in khi trên mức Normal, dạng chữ. |
| U5 | Tiêu đề trang là **danh từ** 22px ("Cases", "My requests"), không "Welcome". Dòng phụ là **phát hiện từ dữ liệu** ("2 new requests to review, 1 past the deadline"), không phải hướng dẫn dùng màn hình. |
| U6 | Bảng: tiêu đề cột 12px xám, viết thường; không sọc ngựa vằn; số căn phải `tabular-nums`; dòng 2 xám 13px. Thiếu giá trị = dấu `—` xám. Dưới 768px bảng thành danh sách 2 dòng, không bỏ trường nào. |
| U7 | Không gradient, không glow / kính mờ, không icon trong ô màu, không emoji (kể cả ★ ✓ — dùng SVG). Icon viền 16px một bộ (`Icon`). |
| U8 | Font: IBM Plex Sans (gói `@fontsource`, không gọi Google). IBM Plex Mono **chỉ** cho mã (CS-000123). |
| U9 | Mỗi khối có đủ trạng thái: đang tải (thanh xám đúng kích thước, không shimmer), lỗi (1 câu + nút Retry trong khối), rỗng (khung nét đứt + 1 câu). |

**Kiểm tra trước PR** (chạy ở thư mục gốc repo):
- `node .claude/skills/design-taste-frontend/scripts/lint.mjs --app frontend/src` → phải **0 FAIL**.
- `node .claude/skills/design-taste-frontend/scripts/inspect.mjs --app <url>` → mở trang thật ở 1440 / 768 / 390, lưu ảnh vào `.taste-skill/` (đã gitignore). Trang cần đăng nhập thì nhờ Trí chạy bản có gắn token.
- WARN nào giữ lại phải giải thích được trong 1 câu (vd. mono cho mã case, bóng của dialog).

## 6. Unit test (yêu cầu Report 5)

- Mỗi test gắn đúng 1 trait: `[Trait(TestType.Key, TestType.Normal | Abnormal | Boundary)]`. Mỗi API ≥ 1 N + 1 A + 1 B.
- Tên: `Method_Scenario_ExpectedResult` (vd. `Login_FifthWrongAttempt_LocksAccountFor15Minutes`).
- Service test dùng `TestDbContextFactory` (EF in-memory) + fake trong `Common/Fakes.cs`; Arrange / Act / Assert.
- Boundary = giá trị ngay sát giới hạn (7 vs 8 ký tự, lần sai thứ 4 vs 5, đúng lúc hết khóa).
- Lọc khi làm báo cáo: `dotnet test backend --filter "Type=Boundary"`.

## 7. Đặt tên

| Thứ | Quy ước | Ví dụ |
|---|---|---|
| C# class / method / property | PascalCase | `AuthService.LoginAsync` |
| C# field private | `_camelCase` | `_options` |
| Method async | hậu tố `Async` | `GetCurrentUserAsync` |
| Mã lỗi | `module.reason` snake_case | `case.invalid_transition` |
| Migration | PascalCase mô tả thay đổi | `AddBuildings`, `AddCaseStatusHistory` |
| React component / page | PascalCase, file trùng tên | `LoginPage.tsx` |
| Hook | `useXxx`, gom trong `hooks.ts` của feature | `useLogin`, `useCases` |
| File TS không phải component | kebab-case | `api.ts`, `state-machine.ts`, `api-client.ts` |

## 8. Việc còn để sau (không thuộc code frame)

- Audit log mọi lần đăng nhập (BR-16) — chờ bảng `audit_logs` sau khi ERD đóng băng (đã để `TODO(audit)` trong `AuthService`).
- Quên / đặt lại mật khẩu qua email (AUTH-02/03), kết thúc các phiên khác khi đổi mật khẩu — cần module Email + cột phiên.
- Overlay AUTH-05 (cảnh báo trước 5 phút, quay lại đúng trang) — hiện chỉ đưa về Login kèm thông báo.
- Module Attachment (Supabase Storage), Notification, AI gateway (Replicate), job nền.
- Mobile Flutter (Tùng) — thư mục `mobile/` chưa dựng.
