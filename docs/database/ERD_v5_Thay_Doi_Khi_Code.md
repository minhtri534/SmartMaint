# ERD v5 → code: mọi chỗ code khác ERD (để Hùng cập nhật ERD / họp nhóm)

> Bắt đầu 02/11/2026. Nguồn ERD: `ERD_SmartMaint_v5` trên Drive, trang **Logical ERD** (Hùng sửa lần cuối 29/10 10:53 giờ VN).
> **Thay cho** `F3_Cot_Bo_Sung_Cho_ERD.md` (28/10).
>
> Leader chốt 02/11:
> - DB làm **theo đúng ERD** (Account + bảng con theo role, bảng tra trạng thái, TechnicianRejections…), kể cả những chỗ bản soát 27/10 chê (AuditLog chỉ FK Admin, Notifications không IsRead…) — họp sửa sau.
> - Chỗ nào **thiếu cột thì tính năng không chạy được** → được thêm, nhưng phải ghi vào file này.
> - Mỗi phần code xong thì cập nhật file này trong cùng commit.

## 1. Quy ước chung (áp cho mọi bảng)

| ERD | Code | Lý do |
|---|---|---|
| Tên bảng / cột PascalCase (`CaseStatusHistory`, `TimeSubmitted`) | snake_case, bảng số nhiều (`case_status_history`, `submitted_at`) | Quy ước D-plan (CONVENTIONS.md mục 4) |
| FK đặt tên cột theo bảng đích (`Staff`, `Asset`) | thêm hậu tố `_id` (`staff_id`, `asset_id`) | như trên |
| Thời gian `TimeXxx` / `XxxTime` | `xxx_at`, kiểu `timestamptz` (UTC) | như trên |
| Không có | `created_at`, `updated_at` trên mọi bảng nghiệp vụ (không có ở bảng tra và bảng nối) | CLAUDE.md bắt buộc |
| Bảng tra `Roles`, `CaseStatuses` (ID, Name) | **Giữ là bảng thật + FK.** ID = giá trị enum C#, dòng dữ liệu do migration ghi (không sửa lúc chạy). Name = chữ snake_case API trả về (`in_progress`) | Đúng ERD; code vẫn đọc được `c.Status == CaseStatus.Pending` |
| Các cột liệt kê cố định khác (`status` của tài khoản, `kind` của ảnh, `hold_reason`…) | text + CHECK | Không có bảng tra trong ERD cho các cột này |

## 2. Bảng theo ERD — cột thêm (NOT IN ERD v5)

| Bảng ERD → bảng code | Cột thêm | Vì sao |
|---|---|---|
| Account → `accounts` | `full_name` | Mọi màn hình hiện tên người, ERD chỉ có Username |
| | `email` (tùy chọn, unique) | Liên hệ / gửi thông báo sau này. **Đăng nhập bằng `username`** (theo ERD) |
| | `status` (active / deactivated) | Admin khóa tài khoản (ADM-03) |
| | `must_change_password` | AUTH-04: tài khoản Admin tạo phải đổi mật khẩu lần đầu |
| | `failed_login_count`, `locked_until`, `last_login_at` | BR-18: sai 5 lần khóa 15 phút |
| | `Password` → `password_hash` | Chỉ lưu hash |
| Customers → `customers` | `room_id` (FK rooms, bắt buộc) | Bản soát 27/10 điểm 11. Khách chỉ báo hỏng ở **phòng mình + khu chung của tòa mình** |
| Rooms → `rooms` | `is_common_area` | Phân biệt phòng thuê với sảnh / hành lang / WC — dùng cho luật trên |
| Floors → `floors` | (đổi kiểu) `floor_number` là **chuỗi** | Tầng "G", "B1", "12A" |
| PriorityLevels → `priority_levels` | `code` (emergency / high / normal / low), `sla_hours` | Code cần khóa cố định để xử lý; hạn SLA = bắt đầu + `sla_hours` |
| CaseCategories → `case_categories` | `group_name`, `is_emergency`, `default_priority_id`, `default_cost_bearer`, `photo_required` | Danh mục 2 cấp, cờ khẩn cấp → F4, bắt buộc ảnh (N24, N26, O05) |
| Cases → `cases` | `room_id` (bắt buộc) | Khách thường không biết tên thiết bị; ERD chỉ nối Asset. `asset_id` để tùy chọn |
| | `cost_bearer` | Staff chốt bên chịu chi phí khi duyệt (N23). **Hỏi lại:** còn cần không nếu E5 (khách làm hư xử lý ngoài hệ thống)? |
| | `closed_at` | Cửa sổ 7 ngày mở lại |
| | `root_cause`, `parts_used` | Biên bản sửa chữa (linh kiện ghi tay, không có kho) |
| | `hold_reason`, `hold_until` | On Hold phải có lý do + ngày dự kiến tiếp tục (N13) |
| | `rating_stars`, `rating_comment`, `rated_at` | Khách đánh giá 1 lần / case |
| | `not_fixed_reason`, `not_fixed_at` (Phần 4) | Khách báo "chưa sửa xong" trong 7 ngày sau đóng; chờ Staff mở lại hoặc từ chối |
| | `sla_warned_at`, `sla_overdue_notified_at` (Phần 4) | Job nền nhắc SLA chỉ báo 1 lần "còn 20% thời gian" và 1 lần "quá hạn"; xóa khi đổi hạn |
| Costs → `costs` (Phần 4) | theo ERD (Name, Description, Amount, Case) + `created_at` / `updated_at` | KTV ghi vật tư khi báo xong; hệ thống thêm dòng "Labour" = giờ × HourlyRate (E9, tính lúc ghi nên tăng lương không đổi chi phí cũ). Staff sửa được khi case đang Completed. Khách chỉ thấy chi phí khi khách là bên trả |
| CaseStatusHistory (Phần 4) | Thao tác **không đổi trạng thái** (đổi ưu tiên, dời hạn, "chưa sửa xong", sửa chi phí) vẫn ghi 1 dòng với `new_status` = trạng thái hiện tại + lý do | ERD không có bảng dời hạn / đổi ưu tiên riêng |
| | `requesting_customer_id` **cho phép trống** | Case sinh từ bảo trì định kỳ (F2) không có khách |
| Assets → `assets` | `retired_at` (Phần 3) | BR-10: thiết bị có lịch sử thì chỉ **thanh lý**, không xóa. Thanh lý = ẩn khỏi form báo hỏng, QR hết hiệu lực, lịch sử giữ nguyên; chặn khi còn case mở. Chưa có case thì được xóa |
| CaseImages → `case_images` | `kind` (report / before / after) | Ảnh lúc báo / trước / sau sửa |
| | `Image` = **đường dẫn file** trong Supabase Storage (bucket private), không phải URL | Web nhận URL ký tạm 1 giờ |
| CaseStatusHistory → `case_status_history` | `changed_by_id` (FK accounts), `reason` | Không có thì không biết ai duyệt / đóng / hủy / tạm dừng và vì sao (bản soát điểm 2). Trạng thái **trước** lấy từ dòng liền trước, không lưu cột |
| TechnicianRejections → `technician_rejections` | khóa chính (case_id, technician_id, rejected_at) | ERD chưa đánh khóa (bản soát điểm 15); cùng 1 KTV có thể từ chối lại sau khi mở lại |
| MaintenanceTriggers → `maintenance_triggers` (Phần 5) | `lead_days` (mặc định 3) | Leader 02/11: lịch cố định, **sinh work order trước hạn N ngày** |
| | `reject_reason` | Manager từ chối kế hoạch phải có lý do cho Staff (E10) |
| MaintenanceWorkOrderAssets → `maintenance_work_order_assets` (Phần 5) | `last_due_date`; khóa chính (work_order_detail_id, asset_id) | Mỗi thiết bị bắt đầu một ngày khác nhau nên "đã sinh tới kỳ nào" phải lưu theo thiết bị (bản soát điểm 3 + 15). `StartTime` / `EndTime` lưu kiểu **date** = ngày đến hạn đầu tiên / ngày cuối áp dụng |
| CaseTasks → `case_tasks` (Phần 5) | `name`, `description` chép từ `checklist_tasks` lúc sinh | Sửa checklist mẫu không làm đổi work order đã sinh |
| | `repair_case_id` (FK cases, tùy chọn) | Mục "Không đạt" → tạo case sửa (BR-07). Bản soát điểm 6 đề nghị `Cases.SourceTask`; code đặt chiều ngược lại cho gọn (1 mục ↔ tối đa 1 case sửa) — họp chọn 1 |

## 3. Chỗ code theo ERD nguyên bản nhưng có hệ quả (biết trước, họp quyết)

| Chỗ | Hệ quả | Đang xử lý tạm |
|---|---|---|
| Giao việc chỉ có `Cases.AssignedTechnician` + `TechnicianRejections` (bỏ bảng `case_assignments` của bản 28/10) | Mất lịch sử "ai từng được giao" khi giao lại; F6 đếm hiệu suất KTV theo người cuối | Dòng lịch sử trạng thái *Assigned* ghi lý do "Assigned to <tên KTV>." để còn dấu vết |
| Account – Roles vẽ 1–1 | Làm đúng 1–1 thì mỗi role chỉ có 1 người → **code làm M–1** (nhiều tài khoản cùng 1 role) | Ghi nhận, coi như lỗi vẽ |
| Bảng con theo role (Customers, Staff, Technicians, Manager, Admins) | Mỗi tài khoản có đúng 1 dòng ở bảng con khớp role (bản soát điểm 17) | EF ánh xạ kiểu **TPT**: tạo tài khoản là tạo luôn dòng bảng con; role và bảng con không thể lệch |
| Tên bảng `Manager` | Code dùng `managers` (số nhiều như các bảng khác) | — |
| Khóa chính các bảng con | Tên ràng buộc là `PK_customers`, `PK_staff`… (EF không cho đặt tên kiểu `pk_` cho TPT) | Chỉ là tên, không ảnh hưởng |
| **Đổi role** (Leader 02/11: cho đổi, nhưng chỉ khi tài khoản chưa có lịch sử) | Mỗi role là 1 bảng con nên không "chuyển" được dòng: code xóa tài khoản cũ rồi tạo lại ở bảng role mới, cùng username + mật khẩu → **id đổi** | "Có lịch sử" = có case / dòng lịch sử trạng thái / lần từ chối / dòng audit. Có rồi thì phải khóa TK cũ, tạo TK mới |
| AuditLog chỉ FK `AdminID`, không có `TargetID` (bản soát điểm 1) | Chỉ ghi được hành động của **Admin**: tạo / sửa / đổi role / khóa / mở / reset mật khẩu / mở khóa tài khoản, sửa giờ SLA, Admin đăng nhập. Không ghi được Staff duyệt, KTV làm… | Bản ghi bị sửa nằm trong JSON `old_value` / `new_value` (có id) |
| AuditLog `Time` | `occurred_at`; bảng tra `audit_log_target_types` (account, priority_level, building, floor, room, asset, case_category, asset_category, asset_type, manufacturer, specialty) và `audit_log_target_operations` (sign_in, create, update, change_role, deactivate, reactivate, reset_password, unlock, delete, retire, import) | Dữ liệu bảng tra do migration ghi |
| CaseCategories không gắn tòa (A03) | Danh mục dịch vụ **dùng chung mọi tòa** | ADM-06 |
| Trạng thái thiết bị của Screen Catalog (Active / Under repair / Out of service do Officer đổi) | Không làm: ERD không có cột; Leader 02/11 chỉ cho Admin sửa thiết bị. "Đang hỏng" hiện ra từ số case mở của thiết bị | — |
| Notifications(UserID, Details, TimeSent) — không IsRead, không trỏ case (bản soát điểm 12) | Trang Thông báo không có "chưa đọc", không bấm mở case được; mã case nằm trong chữ | `TimeSent` → `sent_at`; chỉ trong app, không email; giữ 90 ngày |
| AIConversation – Account 1–1, AIConversation – AIMessage 1–1 (bản soát điểm 5) | Đã tạo đúng 1–1 (`user_id` / `conversation_id` unique) → mỗi người chỉ 1 hội thoại, 1 tin nhắn | Chưa có tính năng AI; sửa ERD trước khi làm (AI chỉ cho Technician + Customer) |
| MaintenanceTriggers không có tòa nhà | Tòa của kế hoạch **suy ra từ thiết bị**; mọi thiết bị của 1 kế hoạch phải cùng 1 tòa (để biết Manager nào duyệt, Staff nào thấy) | Kiểm tra khi lưu |
| Trigger – WorkOrderDetails là 1–N | Màn hình làm **1 checklist / 1 kế hoạch** (đúng 1 dòng detail) | ERD vẫn cho nhiều detail, chưa dùng |
| MaintenanceTriggers không có cột tên | Tên kế hoạch hiển thị = "tên checklist · tên tòa" | Không thêm cột |
| Lịch sử trạng thái của work order sinh tự động | `case_status_history.changed_by_id` bắt buộc → dòng "Assigned / Approved" do job ghi **đứng tên Staff lập kế hoạch** | — |
| Sửa kế hoạch đang chạy | Quay lại *Chờ duyệt* (Manager duyệt lại); thiết bị giữ nguyên thì giữ `last_due_date` nên không sinh trùng | — |

## 4. Bảng ERD chưa tạo

Không còn: đã tạo hết bảng của ERD v5.

Đã tạo (Phần 5, migration `AddPreventiveMaintenance`): MaintenanceTriggers, FrequencyType (`frequency_types`), TriggerStatuses, MaintenanceWorkOrderDetails, MaintenanceWorkOrderAssets, CaseChecklists, ChecklistTasks, CaseTasks, TaskStatuses, TaskImages, TaskNotes; `Cases.TriggerWorkOrder` (`trigger_work_order_id`).

Đã tạo (Phần 2, migration `AddAuditNotificationsAi`): AuditLog (`audit_logs`), AuditLogTargetType, AuditLogTargetOperation, Notifications, AIConversation (`ai_conversations`), AIMessage (`ai_messages`).

Đã tạo đủ (Phần 1): Account, Roles, Customers, Staff, Technicians, Manager, Admins, Specialties, TechnicianSpecialty, Buildings, Floors, Rooms, AssetCategory, AssetType, Manufacturers, Assets, StaffBuildingAssignments, ManagerBuildingAssignments, PriorityLevels, CaseCategories, CaseStatuses, Cases, CaseImages, CaseStatusHistory, TechnicianRejections.

## 5. Việc cho máy dev của từng người

Migration cũ (`InitialAuth`, `AddCasesAndRenameOfficerToStaff`) đã **gộp thành 1 migration mới** `InitialSchemaErdV5` (chưa có DB chung nào chạy bản cũ). DB local cũ phải xóa một lần: `docker compose down -v` rồi chạy lại API (tự migrate + seed). Supabase: chạy `database/Schema.sql` rồi `database/Data.sql` trên DB trống.

Tài khoản demo đăng nhập bằng **username** (`admin`, `staff`, `technician`, `customer`…), mật khẩu như cũ — xem đầu `database/Data.sql`.

## Lịch sử
- 02/11 — Phần 1: tạo bảng theo ERD (mục 2–4), chuyển Auth + F3 sang schema mới, ảnh lên Supabase Storage.
- 02/11 — Phần 2: AuditLog, Notifications, bảng AI; Admin quản lý tài khoản, sửa giờ SLA; thông báo trong app ở các mốc F3.
- 02/11 — Phần 3 (F1): `assets.retired_at`; thêm dòng bảng tra audit cho F1 (migration `AddAssetRetirementAndAuditTargets`).
- 02/11 — Phần 4 (F3 còn lại): bảng `costs`, cột "chưa sửa xong" và nhắc SLA trên `cases` (migration `AddCostsNotFixedAndSlaReminders`).
- 02/11 — Phần 5 (F2): 11 bảng bảo trì định kỳ + `cases.trigger_work_order_id`; cột thêm `lead_days`, `reject_reason`, `last_due_date`, `case_tasks.name/description/repair_case_id` (migration `AddPreventiveMaintenance`).
