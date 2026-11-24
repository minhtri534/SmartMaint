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
| CaseCategories → `case_categories` | `group_name`, `is_emergency`, `default_priority_id`, `photo_required` | Danh mục 2 cấp (họp 07/11: web gọi là **Category** + **Problem**), cờ khẩn cấp → F4, bắt buộc ảnh (N24, N26, O05). `default_cost_bearer` **đã bỏ** (họp 07/11) |
| Cases → `cases` | `room_id` (bắt buộc) | Khách thường không biết tên thiết bị; ERD chỉ nối Asset. `asset_id` để tùy chọn |
| | ~~`cost_bearer`~~ **đã bỏ** (họp 07/11) | Không ghi nhận ai chịu chi phí (out-of-scope #5, #6) |
| | `planned_start_at`, `planned_end_at` (họp 07/11) | Staff hẹn giờ bắt đầu / kết thúc trong form Approve / Edit; hiện ở lịch của KTV khi chọn người. **Không phải** hạn SLA (`due_at` vẫn tính từ lúc bắt đầu làm) |
| | `previous_case_id` (FK cases, họp 07/11) | Đổi KTV khi KTV cũ đã nhận việc = **tạo case mới** nối về case cũ; case cũ **Closed** (Leader 09/11). Từ 24/11 (ghi chú họp 18/11): case cũ **giữ cả danh sách task**, task chưa xong mang trạng thái `moved`, case mới nhận bản sao của chúng (kèm note, hình) |
| | `reassigned_at` (ghi chú họp 18/11) | Lúc case bị đóng vì chuyển sang case mới cho KTV khác. Case như vậy **không bao giờ mở lại** (mở lại case mới). Case đã Closed chỉ xem, thao tác duy nhất còn lại là Reopen (Staff, trong 7 ngày) |
| | `closed_at` | Cửa sổ 7 ngày mở lại |
| | `root_cause`, `parts_used` | Biên bản sửa chữa (linh kiện ghi tay, không có kho) |
| | `hold_reason`, `hold_until` | On Hold phải có lý do + ngày dự kiến tiếp tục (N13) |
| | `rating_stars`, `rating_comment`, `rated_at` | Khách đánh giá 1 lần / case |
| | `not_fixed_reason`, `not_fixed_at` (Phần 4) | Khách báo "chưa sửa xong" trong 7 ngày sau đóng; chờ Staff mở lại hoặc từ chối |
| | `sla_warned_at`, `sla_overdue_notified_at` (Phần 4) | Job nền nhắc SLA chỉ báo 1 lần "còn 20% thời gian" và 1 lần "quá hạn"; xóa khi đổi hạn |
| Costs → `costs` (Phần 4) | theo ERD (Name, Description, Amount, Case) + `created_at` / `updated_at` | KTV ghi vật tư khi báo xong; hệ thống thêm dòng "Labour" = giờ × HourlyRate (E9, tính lúc ghi nên tăng lương không đổi chi phí cũ). Staff sửa được khi case đang Completed. Khách chỉ thấy chi phí khi khách là bên trả |
| | `receipt_code`, `receipt_image` (họp 07/11, phần Technician) | KTV tự ghi chi phí đã trả (tiêu đề, mô tả, số tiền) kèm **biên lai**: số biên lai và / hoặc ảnh (đường dẫn trong Storage), phải có ít nhất 1. Staff vẫn sửa được toàn bộ danh sách; dòng giữ lại thì giữ biên lai. Dòng "Labour" do hệ thống tính lại mỗi lần báo xong |
| Cases → `cases` (họp 07/11, phần Technician) | `clock_started_at` | Đồng hồ giờ làm: chạy khi trạng thái **In progress**, dừng khi On hold / Completed / chuyển bên thứ 3 / hủy; cộng dồn vào `actual_completion_hours` (ERD). KTV gõ tay tổng giờ được (quên bấm) |
| | `assessment_*` (dùng lại) | Bỏ "phiếu đánh giá" của KTV: nút "cần bên thứ 3" lưu **mô tả** → `assessment_condition`, **lý do** → `assessment_cause`, ảnh → `case_images.kind = assessment`; `assessment_recommendation` để trống |
| | `root_cause`, `parts_used`, `hold_reason`, `hold_until` | **Không còn ghi** (họp 07/11: báo xong không phải điền gì; On hold không lý do / ngày). Giữ cột để đọc dữ liệu cũ |
| CaseStatusHistory (Phần 4, sửa họp 07/11) | **Chỉ ghi khi đổi trạng thái** (họp 07/11 "bỏ lưu lịch sử custom"). Ngoại lệ giữ dòng cùng trạng thái: nhật ký khẩn cấp F4, "Deadline moved", "Not an emergency" (F6 đếm từ đây) | ERD không có bảng dời hạn / hạ mức riêng |
| CaseStatuses → `case_statuses` (họp 07/11) | **Bỏ dòng 3 `assigned`** (còn 10 trạng thái). Staff chọn KTV ngay khi Approve; case có KTV chưa nhận việc = *Approved* | Migration đổi dữ liệu cũ 3 → 2 (cả `case_status_history`) |
| Bảng mới `case_followers` (case_id, customer_id, added_at) (họp 07/11, "phải sửa lại ERD") | Nhiều khách theo dõi 1 case (vd. request trùng bị reject → khách đó theo dõi case cũ). Khách theo dõi chỉ xem + nhận thông báo, không thao tác | Thay cho gộp case (merge) |
| CaseTasks → `case_tasks` (họp 07/11) | **Mọi case đều có task** (không chỉ F2): Staff nhập khi Approve / Edit, bỏ trống thì 1 task = tiêu đề case. Thêm `done_by` (technician / third_party) + `done_at` | Staff đánh dấu task do bên thứ 3 làm; KTV không sửa được task đó. Migration tạo 1 task cho case đã duyệt chưa có task |
| | `requesting_customer_id` **cho phép trống** | Case sinh từ bảo trì định kỳ (F2) không có khách |
| | `emergency_at` (Phần 6, F4) | Lúc case thành khẩn cấp (tạo với loại dịch vụ khẩn cấp, hoặc Staff nâng mức). Đồng hồ **24/7** tính từ đây: `due_at` = `emergency_at` + giờ SLA khẩn cấp. Xóa khi hạ mức về F3 |
| | `acknowledged_at`, `made_safe_at` (Phần 6) | 2 mốc của K06: Staff bấm "Đã nhận" (phản hồi đầu), KTV bấm "Đã an toàn" (khống chế xong). Mốc thứ 3 "khắc phục xong" = `completed_at` có sẵn. Chỉ mốc khắc phục có hạn |
| | `merged_into_case_id` (FK cases, Phần 6) | N5 gộp case khẩn cấp trùng: case gộp chuyển **Cancelled** + trỏ về case chính, không tính KPI; khách được báo khi case chính đóng |
| | `backup_alerted_at`, `manager_alerted_at`, `accept_reminded_at` (Phần 6) | Job nền chỉ báo 1 lần: 10 phút chưa ai "Đã nhận" → Staff dự phòng; 30 phút → Manager; KTV 10 phút chưa nhận việc → nhắc KTV + Staff |
| | `incident_damage`, `incident_lessons`, `incident_follow_up` (none / preventive_plan / replacement_review), `incident_reported_at` (Phần 6) | K08 báo cáo sau sự cố, **không bắt buộc**, Staff viết sau khi đóng; case khẩn cấp xong trễ hạn thì nhắc viết |
| | `assessment_condition`, `assessment_cause`, `assessment_recommendation` (repairable / replace / specialist), `assessed_at` (Phần 7, F5) | Bước ①: KTV thấy vượt khả năng / nên thay → phiếu đánh giá **bắt buộc** (ảnh = `case_images.kind` = `assessment`) → On Hold "chờ Staff quyết định" |
| | `is_survey` (Phần 7) | F5 cách 2: Staff mở case khảo sát cho thiết bị hỏng thường xuyên (≥ 3 lần / 12 tháng); không có khách, F6 không tính KPI |
| | `replacement_model`, `replacement_serial` (Phần 7) | Bước ⑨: bên thứ 3 đã thay thiết bị → KTV ghi model / serial thiết bị mới khi báo xong (bắt buộc) |
| CaseImages → `case_images` | thêm giá trị `kind` = `assessment` (Phần 7) | Ảnh phiếu đánh giá F5 |
| AssetType → `asset_types` | `expected_life_years` (Phần 7, tùy chọn, 1–100) | R03: tuổi thọ chuẩn, Admin nhập ở Catalog; bảng thông tin quyết định so tuổi thiết bị với tuổi thọ |
| Assets → `assets` | `replaces_asset_id` (FK assets, Phần 7) | A07: thiết bị mới "thay cho" thiết bị cũ; trang thiết bị hiện "Replaces / Replaced by" |
| Assets → `assets` | `retired_at` (Phần 3) | BR-10: thiết bị có lịch sử thì chỉ **thanh lý**, không xóa. Thanh lý = ẩn khỏi form báo hỏng, QR hết hiệu lực, lịch sử giữ nguyên; chặn khi còn case mở. Chưa có case thì được xóa |
| CaseImages → `case_images` | `kind` (report / before / after) | Ảnh lúc báo / trước / sau sửa |
| | `Image` = **đường dẫn file** trong Supabase Storage (bucket private), không phải URL | Web nhận URL ký tạm 1 giờ |
| CaseStatusHistory → `case_status_history` | `changed_by_id` (FK accounts), `reason` | Không có thì không biết ai duyệt / đóng / hủy / tạm dừng và vì sao (bản soát điểm 2). Trạng thái **trước** lấy từ dòng liền trước, không lưu cột |
| TechnicianRejections → `technician_rejections` | khóa chính (case_id, technician_id, rejected_at) | ERD chưa đánh khóa (bản soát điểm 15); cùng 1 KTV có thể từ chối lại sau khi mở lại |
| MaintenanceTriggers → `maintenance_triggers` (Phần 5) | `lead_days` (mặc định 3) | Leader 02/11: lịch cố định, **sinh work order trước hạn N ngày** |
| | `reject_reason` | Manager từ chối kế hoạch phải có lý do cho Staff (E10) |
| | `name`, `description`, `archived_at` | Họp 18/11 (kiểu UpKeep): plan có tên (thành tiêu đề work order), ghi chú cho technician, và nút Archive (khóa vĩnh viễn). Dòng `trigger_statuses` id 5 đổi tên `ended` → `archived` |
| MaintenanceWorkOrderDetails → `maintenance_work_order_details` (Phần 5) | `frequency`, `frequency_type_id` (FK `frequency_types`), `lead_days`, `start_date`, `end_date` | Họp 18/11: **mỗi plan có nhiều trigger** — mỗi dòng detail giờ là 1 trigger với lịch riêng (mỗi X ngày/tuần/tháng/năm, ngày bắt đầu, ngày hết hạn; gia hạn = dời `end_date`, không cần duyệt lại). Checklist / giờ / mức ưu tiên vẫn ở dòng detail như ERD, các trigger của một plan cùng giá trị. `maintenance_triggers.frequency`, `frequency_type_id`, `lead_days` vẫn giữ (= trigger đầu) cho API cũ |
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
| AIConversation – Account 1–1, AIConversation – AIMessage 1–1 (bản soát điểm 5) | **Đã đổi 16/11 thành 1–N cả hai** (bỏ unique trên `user_id`, `conversation_id`; migration `AiConversationsOneToMany`). `ai_messages.message` nới 500 → 2000 ký tự (= mô tả case dài nhất). Gợi ý loại sự cố ở trang New request = 1 hội thoại `context_type = case_request` + 1 tin nhắn (câu hỏi = mô tả). Chatbot (Leader 16/11: mọi role trừ Admin) = hội thoại `context_type = chat`, mỗi lượt hỏi–đáp 1 dòng `ai_messages`; "New chat" mở hội thoại mới. `reply` = câu trả lời của model, trống khi AI lỗi / quá giờ | Hùng sửa ERD thành 1–N. Chỉ ghi khi đã gọi model (có token); Admin chưa có màn hình xem metadata |
| MaintenanceTriggers không có tòa nhà | Tòa của kế hoạch **suy ra từ thiết bị**; mọi thiết bị của 1 kế hoạch phải cùng 1 tòa (để biết Manager nào duyệt, Staff nào thấy) | Kiểm tra khi lưu |
| Trigger – WorkOrderDetails là 1–N | Màn hình làm **1 checklist / 1 kế hoạch** (đúng 1 dòng detail) | ERD vẫn cho nhiều detail, chưa dùng |
| MaintenanceTriggers không có cột tên | Tên kế hoạch hiển thị = "tên checklist · tên tòa" | Không thêm cột |
| Lịch sử trạng thái của work order sinh tự động | `case_status_history.changed_by_id` bắt buộc → dòng "Assigned / Approved" do job ghi **đứng tên Staff lập kế hoạch** | — |
| Không có bảng lịch trực (K02 / K05, Leader 02/11: không làm) | Case khẩn cấp báo **Staff chính** của tòa (`staff_building_assignments.is_primary`) ngay, 24/7; dự phòng sau 10 phút | Tòa không có Staff chính thì báo mọi Staff của tòa |
| Nhật ký khẩn cấp (gọi bên thứ 3 N3, biện pháp tạm N2 / G2, cập nhật cho khách) không có bảng | Ghi thành **dòng lịch sử giữ nguyên trạng thái** (`case_status_history.reason` bắt đầu bằng loại dòng) | Bảng bên thứ 3 đầy đủ để F5 (Phần 7) |
| Bên thứ 3 / hãng bảo hành (N3) — ERD chỉ có `Cases.TimeOutsourced` | **Bảng mới `case_third_parties`** (Phần 7): loại (warranty / contract / quote), việc (repair / replace), trạng thái (quoting → awaiting_decision → working → failed_check / done; declined; cancelled), công ty, liên hệ, giờ hẹn, số lần làm lại, báo giá (ai trả, gửi cho ai, lúc nào, đã nhắc chưa), quyết định (đồng ý / từ chối, ai quyết, ảnh bằng chứng). Nhiều dòng / case | Không lưu tiền, không kho, không Manager duyệt (N2, N22, BR-12). `created_at` → `finished_at` = pha bên thứ 3 để F6 báo riêng (N16). `outsourced_at` = lần chuyển đầu tiên |
| Trạng thái khi chờ bên thứ 3 (R02, Leader 02/11) | **On Hold** lý do `waiting_staff_decision` (chờ Staff) → `waiting_third_party` (đang ở bên ngoài); KTV kiểm tra đạt → In Progress | Hạn SLA không dừng; Staff dời hạn có lý do như F3. KTV không tự "Tiếp tục" khi đang chờ 2 lý do này |
| Kênh Staff → Admin khi thay thiết bị (A06) | **Bảng mới `asset_replacement_requests`** (Phần 7): Staff đóng case đã thay → tạo đề nghị; Admin đăng ký thiết bị mới (cùng phòng, `replaces_asset_id`) và thanh lý thiết bị cũ trong 1 bước, hoặc bỏ qua có lý do | Admin thấy mã case để trích dẫn, **không mở được case** (BR-11) |
| Asset / địa điểm qua duyệt (họp 18/11, quyết định 7) — ERD không có | **Bảng mới `asset_change_lists`** (tòa, tiêu đề, ghi chú, `status` draft / pending / approved / rejected, staff lập, manager duyệt, lúc gửi, lúc duyệt, lý do từ chối) + **`asset_change_items`** (thứ tự, `kind` add / rename / delete floor, add / rename / delete room, add / edit / retire / delete asset, `key` + `parent_key` để phòng mới nằm trên tầng mới cùng danh sách, các cột đích `floor_id` / `room_id` / `asset_id` (SET NULL khi bị xóa), giá trị mới, `target_label` ghi lại tên lúc lập) | Staff của tòa lập, Manager của tòa duyệt cả danh sách (áp dụng tất cả cùng lúc qua service cũ, ghi audit như trước) hoặc từ chối có lý do. Admin chỉ xem; Admin vẫn tạo / sửa tòa nhà |
| Khách chịu chi phí báo giá (R04 = O13) | Staff ghi "đã gửi báo giá cho <đại diện>" + quyết định + ảnh bằng chứng; quá 3 ngày chưa trả lời → nhắc Staff 1 lần | Không có tài khoản đại diện hợp đồng |
| Thống kê F6 (Phần 8) không có bảng sự kiện riêng | Số lần dời hạn, số case hạ mức, thời gian On Hold theo lý do được **đếm từ dòng `case_status_history`** theo tiền tố lý do chung (`CaseHistoryTags` trong code); tính ngay khi mở trang, không có bảng tổng hợp | Không thêm bảng / cột. Đổi câu lý do thì phải giữ tiền tố |
| Tuổi thiết bị | ERD không có ngày lắp đặt → tuổi tính từ `assets.created_at` | Họp: có thêm `installed_at` không |
| Case khẩn cấp vào thẳng Approved | Dòng lịch sử đầu tiên là *Approved* do chính khách (người tạo) đứng tên, lý do "approved automatically" | `changed_by_id` bắt buộc |
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
- 02/11 — Phần 6 (F4): 11 cột khẩn cấp trên `cases` (mốc, gộp, nhắc, báo cáo sau sự cố) (migration `AddEmergencyFlow`; điền `emergency_at` cho case khẩn cấp cũ).
- 02/11 — Phần 7 (F5): bảng `case_third_parties`, `asset_replacement_requests`; cột đánh giá / khảo sát / thiết bị thay trên `cases`, `asset_types.expected_life_years`, `assets.replaces_asset_id`, ảnh `assessment` (migration `AddRepairOrReplace`).
- 03/11 — Phần 8 (F6): không đổi DB; thống kê đọc từ bảng có sẵn (ghi chú ở mục 3).
- 09/11 — Họp 07/11 phần Staff (migration `StaffWorkOrderMeeting0711`): bỏ trạng thái `assigned`; bỏ `cases.cost_bearer`, `case_categories.default_cost_bearer`; thêm `cases.planned_start_at / planned_end_at / previous_case_id`, `case_tasks.done_by / done_at`, bảng `case_followers`; lịch sử chỉ ghi khi đổi trạng thái (mục 2). Gộp case (`merged_into_case_id`) và quy trình bên thứ 3 nhiều bước (`case_third_parties`) **không còn dùng**, giữ cột / bảng để đọc dữ liệu cũ — họp quyết có xóa hẳn không.
- 09/11 — Họp 07/11 phần Technician (migration `TechnicianWorkClockAndReceipts`): `cases.clock_started_at`, `costs.receipt_code / receipt_image`; ảnh task có chú thích (`task_images.notes` của ERD); bỏ biên bản sửa chữa và lý do On hold khi KTV báo; "hoàn thành checklist" gộp vào "Completed".
- 16/11 — AI (Phần lõi, Trí): bảng AI 1–1 → 1–N, `ai_messages.message` 2000 ký tự (migration `AiConversationsOneToMany`); gợi ý loại sự cố cho Customer qua Replicate (mục 3).
- 18/11 — Họp 18/11, phần tiếp nhận yêu cầu (migration `IntakeRulesMeeting1811`, chỉ thêm, API cũ vẫn chạy): `case_categories.asset_type_id` (FK `asset_types`, trống = mọi loại asset), `case_categories.archived_at`, `specialties.archived_at`; `customers.room_id` cho phép trống (khách không còn phòng thuê). Các cột không còn dùng nhưng **chưa xóa** vì API đang chạy trên Railway còn đọc: `case_categories.is_emergency`, `default_priority_id`, `photo_required`, `rooms.is_common_area`.
- 18/11 — Họp 18/11, phần màn hình case (migration `CaseScreensMeeting1811`, chỉ thêm): `costs.added_by_id` (FK `accounts`, trống với dòng cũ) để technician chỉ sửa chi phí mình nhập. Case bị loại vì trùng dùng lại cột `cases.merged_into_case_id` (migration điền cho các case trùng cũ) nên không tính vào số liệu. Checklist không còn chọn service type: cột `case_checklists.category_id` vẫn giữ, checklist mới dùng chung fault category đã archive "General · Preventive maintenance".
- 19/11 — Họp 18/11, kế hoạch bảo trì kiểu UpKeep (migration `MaintenanceTriggersMeeting1811`, chỉ thêm, cột mới có mặc định nên API cũ vẫn ghi được): `maintenance_triggers.name`, `description`, `archived_at`; `maintenance_work_order_details.frequency`, `frequency_type_id`, `lead_days`, `start_date`, `end_date` (migration điền từ plan và asset cho dữ liệu cũ, đặt tên plan cũ = "checklist · tòa nhà"); `trigger_statuses` id 5 `ended` → `archived`.
- 19/11 — Họp 18/11, asset và địa điểm qua duyệt (migration `AssetChangeListsMeeting1811`, chỉ thêm 2 bảng mới): `asset_change_lists`, `asset_change_items` (mục 3). API cũ không biết 2 bảng này nên vẫn chạy. `audit_logs` vẫn chỉ ghi thao tác của Admin như ERD (FK `admins`): thay đổi do Manager duyệt không ghi audit, chính danh sách (ai lập, ai duyệt, lúc nào, từng dòng) là bản ghi.
- 24/11 — Asset và địa điểm sửa theo **cây** (ghi chú họp cập nhật, migration `AssetTreeChangesMeeting1811`, chỉ thêm): `buildings.deleted_at`, `floors.deleted_at`, `rooms.deleted_at` (xóa tòa / tầng / phòng = xóa mềm, ẩn khỏi cây và danh sách chọn; xóa asset = retire như cũ); `asset_change_items.building_id` (FK, SET NULL) và `address` (thêm / sửa / xóa tòa nhà cũng qua danh sách, Staff đề xuất, Manager duyệt; tòa mới tự gán Staff đề xuất làm primary và Manager duyệt); `asset_change_lists.building_id` cho phép trống (một danh sách phủ nhiều tòa của Staff); `kind` thêm `add_building`, `edit_building`, `delete_building`. Index unique của tên tòa / số tầng / số phòng **giữ nguyên** (không lọc `deleted_at`) để API cũ và seed cũ chạy y nguyên: tên của chỗ đã xóa mềm vẫn bị giữ.
- 24/11 — Ghi chú họp 18/11, case đóng và đổi KTV (migration `CaseReassignClosureMeeting1811`, chỉ thêm): `cases.reassigned_at` (migration điền = `closed_at` cho case cũ đã có case nối tiếp); bảng tra `task_statuses` thêm dòng id 5 `moved` (task chưa xong khi case chuyển sang KTV khác). API cũ không đọc cột mới; task `moved` chỉ có ở case đổi KTV sau khi code mới chạy.
