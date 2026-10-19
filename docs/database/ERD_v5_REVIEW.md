# Review ERD v5 — trang 2 (Logical) · TRẠNG THÁI: CHƯA CHỐT, KHÔNG DÙNG ĐỂ CODE

> Người review: Claude (cho Leader) · 19/10/2026
> File được review: `ERD_SmartMaint_v5.drawio` (Drive `04_Design`, sửa lần cuối 18/10, tác giả Hùng) — **Page-2** (logical, 22 bảng, 24 đường nối).
> Đối chiếu với: Page-1 của chính file này (Chen, khái niệm), D1–D16 (`docs/plan/Tuan6_Ke_Hoach_DB_CodeFrame.md` mục 6), 6 luồng F1–F6 (`docs/flows/`), Business Rules (`docs/requirements/SRS_Report3.md` mục 5.1).

## 0. Kết luận một câu

Page-2 **chưa phải là bản "logical" của Page-1**. Page-1 có khoảng 26 entity theo D1–D16, còn Page-2 chỉ có 22 bảng và **thiếu khoảng 13 entity lõi** của F2, F3 và F5. Ngoài ra Page-2 có vài lỗi thiết kế, nếu code theo sẽ phải đập lại ngay từ tuần 7. **Chưa được tạo migration theo Page-2.**

## 1. Lỗi nghiêm trọng (chặn code)

| # | Vấn đề | Vì sao sai | Đề xuất |
|---|---|---|---|
| E1 | **Thiếu bảng so với Page-1 và D1–D16:** Unit, BuildingAssignment (Officer phụ trách tòa), ServiceGroup / ServiceType, RootCause (Page-1 gọi là CauseType), CaseComment, CaseStatusHistory, CaseDueChange, Assignment, VendorHandoff, ChecklistTemplate / ChecklistItem, PreventivePlan / PlanAsset / PlanOccurrence, InspectionRecord, SystemSetting | Không có các bảng này thì F2 (bảo trì định kỳ) không chạy được, F6 không đo được KPI, và luồng F3 "giao → từ chối → giao lại" không lưu được lịch sử | Lấy Page-1 làm khung. Mỗi entity trên Page-1 phải có đúng 1 bảng trên Page-2 |
| E2 | **5 bảng con theo role** (Employees, Officers, Managers, Technicians, Admins) chỉ có đúng 1 cột `FK ID` | Bảng không có cột riêng thì không mang thông tin gì, lại khiến việc kiểm tra quyền (RBAC) phải JOIN 5 bảng. Muốn đổi role của một người thì phải xóa dòng ở bảng này rồi thêm dòng ở bảng khác. Đề là 5 role cố định, không có thuộc tính riêng theo role | Bỏ 5 bảng này. Thêm `users.role` (text enum: `customer, officer, technician, manager, admin` — D1). Thuộc tính riêng: `users.unit_id` (khách), `users.is_away` (D5) |
| E3 | **Cases bắt buộc FK Asset** | Rất nhiều sự cố không gắn với thiết bị nào: tắc cống hành lang, thấm trần, mất điện cả tầng. Page-1 có quan hệ "occurring at" (Room–Case) | `cases.room_id` NOT NULL, `cases.asset_id` NULL được |
| E4 | **Cases.AssignedTechnician là 1 cột** | Technician có quyền từ chối (BR-13) và Officer giao lại (DLG-05). Một cột thì chỉ lưu được người cuối cùng, mất lịch sử ai đã từ chối và lý do gì (D11) | Bảng `assignments` (case_id, technician_id, assigned_by, assigned_at, response, responded_at, reject_reason) |
| E5 | **Chỉ có TimeCreated / TimeStarted / TimeClosed** | F6 cần đo thời gian chờ duyệt, chờ giao, thời gian sửa, số lần reopen, thời gian On Hold… tức là cần mốc của **mọi** lần chuyển trạng thái (D13) | Bảng `case_status_history` (case_id, from_status, to_status, changed_by, changed_at, reason) |
| E6 | **Users có 2 cột `Username`, cột `Password`, không có Email / Role / trạng thái khóa** | Admin tạo tài khoản và gửi email, quên mật khẩu qua email (AUTH-02/03), khóa tài khoản (DLG-16, AUTH-06), bắt đổi mật khẩu lần đầu (AUTH-04). Lưu mật khẩu thô là lỗi bảo mật | `email` (unique), `full_name`, `password_hash`, `role`, `status` (active/locked/deactivated), `must_change_password`, `created_at` |
| E7 | **Tenants không nối với tòa / phòng / Unit** | Không biết khách thuộc tòa nào thì không định tuyến được case tới Officer phụ trách tòa đó (chốt 12/10: Officer theo tòa, bỏ hàng chờ chung) | Theo D2: `units` (building_id, code, contract_representative…), `rooms.unit_id`, `users.unit_id` |
| E8 | **Vendors bị dùng làm "Producer" (hãng sản xuất) của Asset** | Vendor trong F5 là **nhà thầu / bên thứ 3 nhận sửa chữa**, còn hãng sản xuất là chuyện khác. Gộp hai khái niệm thì một hãng (vd. Daikin) cũng sẽ hiện trong danh sách "chuyển bên thứ 3" | `assets.manufacturer` (text). Vendor chỉ dùng cho `vendor_handoffs` |
| E9 | **Cases.IsOutsourced là boolean** | Không biết đã chuyển cho ai, lúc nào, báo giá ra sao, khách đã xem chưa (F5, R01–R04) | Bảng `vendor_handoffs` (case_id, vendor_id, handed_at, quote_note, quote_file, customer_viewed_at, result…) — D10, không có cột tiền |
| E10 | **MaintenancePlans gộp "kế hoạch" với "một lần thực hiện"** (1 dòng = 1 asset + 1 thời điểm + 1 technician + Result) | F2 cần tách: **kế hoạch** (chu kỳ, cố định / trượt, mẫu checklist, Đang chạy / Tạm dừng / Kết thúc) → **kỳ** (đến hạn, cửa sổ ±10%, giữ / dời / bỏ) → **lệnh** (Case loại preventive) → **phiếu kiểm tra** (từng mục Đạt / Không đạt / N/A) | `preventive_plans`, `plan_assets`, `plan_occurrences`, `checklist_templates`, `checklist_items`, `inspection_records`, `inspection_results` — xem `docs/flows/F2_Bao_Tri_Dinh_Ky.md` mục 7 |
| E11 | **AuditLog chỉ FK tới Admin** | Audit phải ghi hành động của **mọi** user (duyệt, giao, đóng, mọi câu hỏi AI — mục 4b PROJECT_CONTEXT). Admin là người **đọc** log, không phải người duy nhất **tạo ra** log | `actor_user_id` → users, `action`, `entity_type`, `entity_id`, `data` (jsonb), `created_at` |

## 2. Lỗi trung bình

| # | Vấn đề | Đề xuất |
|---|---|---|
| M1 | AssetCategories chỉ có cột ID, không có tên. Đồng thời tồn tại cả AssetTypes, không rõ khác gì Category | Giữ 1 bảng `asset_categories` (name, code, `requires_inspection` — D15). Checklist template gắn theo category (D16) |
| M2 | MaintenanceType dùng `Name` làm PK | Khóa tự nhiên dạng chuỗi, đổi tên là vỡ FK. Dùng `id bigint identity` |
| M3 | Statuses là bảng tra cứu, trong khi D7 chốt enum dạng text (11 trạng thái) | Thống nhất: `cases.status text` + CHECK constraint, enum trong C#. Dùng bảng tra cứu cũng được, miễn chọn một cách và ghi vào quy ước |
| M4 | Cases thiếu nhiều cột đã chốt: `code` (mã hiển thị), `building_id`, `service_type_id`, `priority` / mức khẩn cấp (F4), `sla_due_at`, `adjusted_due_at`, `hold_reason`, `expected_resume_at`, `parent_case_id` (gộp case — D8), `root_cause_id`, `work_done`, `parts_text`, `labor_hours` (D12), `contained_at`, `resolved_at` (D14), `source` (customer / officer / preventive), `plan_occurrence_id`, `rating` | Bổ sung theo D7–D14 |
| M5 | Images chỉ thuộc Case, không có loại ảnh, người upload, đường dẫn Storage | `attachments` (owner_type/owner_id hoặc case_id + inspection_id, `kind` before/after/evidence/quote, `storage_path`, `uploaded_by`, `created_at`) — Supabase Storage chỉ lưu file, DB lưu đường dẫn |
| M6 | Notifications thiếu `is_read`, `type`, liên kết tới case | Thêm `type`, `case_id` (NULL được), `read_at` |
| M7 | AIConversation lưu từng tin nhắn nhưng không có phiên hội thoại, không có `prediction_id` / `status` (Replicate chạy bất đồng bộ), không có ngữ cảnh case | `ai_conversations` (user_id, role_scope, case_id) + `ai_messages` (conversation_id, sender, content, prediction_id, status, created_at) |
| M8 | Rooms thiếu `area_type` (khu chung / thuê — D4); Buildings thiếu `type`, giờ làm việc (D3) | Bổ sung |
| M9 | Assets thiếu `status` (đang dùng / hỏng / ngừng / thanh lý — BR-10), `qr_code`, `installed_at`, `warranty_until`, `replaced_by_asset_id` (D15) | Bổ sung |
| M10 | Cases.AssignedOfficer cố định trên case | Sau D5, Officer phụ trách được suy ra từ `building_assignments` (Primary / Backup). Nếu muốn giữ `handled_by_officer_id` thì ghi rõ đây là người **đã thao tác**, không phải người **được phân** |

## 3. Lỗi trình bày (sẽ bị hỏi khi bảo vệ R4)

- Nhãn cardinality `1` / `M` đặt **rời** khỏi đường nối, kéo bảng là lệch. ERD vật lý nên dùng ký hiệu crow's foot gắn liền đường nối.
- Cột chưa có **kiểu dữ liệu**, NULL / NOT NULL, UNIQUE. R4 (mẫu SP25SE136) cần cột + kiểu + PK/FK.
- Tên bảng dạng PascalCase số nhiều (`Cases`, `AssetTypes`), trong khi D-quy ước chốt **snake_case tiếng Anh**. Chọn một kiểu và áp dụng cho cả nhóm.
- Không bảng nào có `created_at` / `updated_at`.

## 4. Việc cần làm trước khi code DB

1. Hùng và Trí ngồi lại, dựng lại Page-2 **từ Page-1** (mỗi entity thành 1 bảng), áp D1–D16 và sửa E1–E11.
2. Mỗi chủ luồng tự kiểm tra bảng của luồng mình theo câu hỏi: *"Bước nào trong sơ đồ luồng ghi dữ liệu vào bảng nào, cột nào?"*. Không trả lời được thì là thiếu cột.
3. Hùng kiểm tra thêm từ góc F6: *"Mỗi KPI tính từ cột nào của bảng nào?"*
4. Đóng băng → tạo migration EF Core đầu tiên → xuất lại ERD từ DB thật cho R4.

Cho tới khi xong bước 4: **chỉ code phần không phụ thuộc schema chi tiết** (Auth với bảng `users` tối thiểu, khung web, khung API, Building / Floor / Room).
