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
| | `requesting_customer_id` **cho phép trống** | Case sinh từ bảo trì định kỳ (F2) không có khách |
| CaseImages → `case_images` | `kind` (report / before / after) | Ảnh lúc báo / trước / sau sửa |
| | `Image` = **đường dẫn file** trong Supabase Storage (bucket private), không phải URL | Web nhận URL ký tạm 1 giờ |
| CaseStatusHistory → `case_status_history` | `changed_by_id` (FK accounts), `reason` | Không có thì không biết ai duyệt / đóng / hủy / tạm dừng và vì sao (bản soát điểm 2). Trạng thái **trước** lấy từ dòng liền trước, không lưu cột |
| TechnicianRejections → `technician_rejections` | khóa chính (case_id, technician_id, rejected_at) | ERD chưa đánh khóa (bản soát điểm 15); cùng 1 KTV có thể từ chối lại sau khi mở lại |

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
| AuditLog `Time` | `occurred_at`; bảng tra `audit_log_target_types` (account, priority_level) và `audit_log_target_operations` (sign_in, create, update, change_role, deactivate, reactivate, reset_password, unlock) | Dữ liệu bảng tra do migration ghi |
| Notifications(UserID, Details, TimeSent) — không IsRead, không trỏ case (bản soát điểm 12) | Trang Thông báo không có "chưa đọc", không bấm mở case được; mã case nằm trong chữ | `TimeSent` → `sent_at`; chỉ trong app, không email; giữ 90 ngày |
| AIConversation – Account 1–1, AIConversation – AIMessage 1–1 (bản soát điểm 5) | Đã tạo đúng 1–1 (`user_id` / `conversation_id` unique) → mỗi người chỉ 1 hội thoại, 1 tin nhắn | Chưa có tính năng AI; sửa ERD trước khi làm (AI chỉ cho Technician + Customer) |

## 4. Bảng ERD chưa tạo (sẽ tạo đúng phần của nó)

| Bảng ERD | Phần |
|---|---|
| Costs | Phần 4 — F3 còn lại / Phần 7 — F5 |
| MaintenanceTriggers, FrequencyType, TriggerStatuses, MaintenanceWorkOrderDetails, MaintenanceWorkOrderAssets, CaseChecklists, ChecklistTasks, CaseTasks, TaskStatuses, TaskImages, TaskNotes; `Cases.TriggerWorkOrder` | Phần 5 — F2 |

Đã tạo (Phần 2, migration `AddAuditNotificationsAi`): AuditLog (`audit_logs`), AuditLogTargetType, AuditLogTargetOperation, Notifications, AIConversation (`ai_conversations`), AIMessage (`ai_messages`).

Đã tạo đủ (Phần 1): Account, Roles, Customers, Staff, Technicians, Manager, Admins, Specialties, TechnicianSpecialty, Buildings, Floors, Rooms, AssetCategory, AssetType, Manufacturers, Assets, StaffBuildingAssignments, ManagerBuildingAssignments, PriorityLevels, CaseCategories, CaseStatuses, Cases, CaseImages, CaseStatusHistory, TechnicianRejections.

## 5. Việc cho máy dev của từng người

Migration cũ (`InitialAuth`, `AddCasesAndRenameOfficerToStaff`) đã **gộp thành 1 migration mới** `InitialSchemaErdV5` (chưa có DB chung nào chạy bản cũ). DB local cũ phải xóa một lần: `docker compose down -v` rồi chạy lại API (tự migrate + seed). Supabase: chạy `database/Schema.sql` rồi `database/Data.sql` trên DB trống.

Tài khoản demo đăng nhập bằng **username** (`admin`, `staff`, `technician`, `customer`…), mật khẩu như cũ — xem đầu `database/Data.sql`.

## Lịch sử
- 02/11 — Phần 1: tạo bảng theo ERD (mục 2–4), chuyển Auth + F3 sang schema mới, ảnh lên Supabase Storage.
- 02/11 — Phần 2: AuditLog, Notifications, bảng AI; Admin quản lý tài khoản, sửa giờ SLA; thông báo trong app ở các mốc F3.
