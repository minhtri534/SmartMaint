# F3 — Cột / bảng backend đã thêm so với ERD v5 (để họp với Hùng)

> 28/10/2026. Backend Case (F3) đã chạy trên Postgres (migration `AddCasesAndRenameOfficerToStaff`). ERD v5 chưa đóng băng nên mọi thứ dưới đây là **đề xuất đang chạy thử**:
> họp ERD chốt giữ / bỏ cái nào thì sửa đúng chỗ đó (entity ở `backend/src/SmartMaint.Api/Entities/Cases`, mỗi cột có chú thích `NOT IN ERD v5`).
> Nguồn luồng: doc "Luồng Chính" (Drive, bản Leader sửa 28/10) + ERD Drive (`ERD_SmartMaint_v5`, sửa lần cuối 28/10 06:39: thêm `Assets.WarrantyEndDate`).

## 1. Đã khớp ERD (dùng nguyên)

| Bảng code | ERD | Ghi chú |
|---|---|---|
| `cases` | Cases | Title, Description, Status, PriorityLevel, Staff, AssignedTechnician, TimeSubmitted (`submitted_at`), StartTime (`started_at`), DueTime (`due_at`), TimeCompleted (`completed_at`), ActualCompletionHours (`actual_hours`), TimeOutsourced (`outsourced_at`) |
| `case_images` | CaseImages | |
| `case_status_history` | CaseStatusHistory | |
| `case_categories` | CaseCategories | |
| `priority_levels` | PriorityLevels | |
| `staff_building_assignments`, `manager_building_assignments` | StaffBuildingAssignments, ManagerBuildingAssignments | E7, E8, E13 |
| `buildings`, `floors`, `rooms`, `assets` | Buildings, Floors, Rooms, Assets | Bản tối thiểu để F3 chạy (Huy / F1 mở rộng) |

Khác cách đặt tên: bảng `Account` + `Staff/Customers/Technicians/Manager/Admins` của ERD = **một bảng `users`** (cột `role`) trong code; "Staff" là một role, không phải bảng riêng. Role `officer` đã **đổi tên thành `staff`** ở code, DB và seed.

## 2. Thêm vào bảng ERD đã có (NOT IN ERD v5)

| Bảng | Cột thêm | Vì sao (luồng) | Ghi chú |
|---|---|---|---|
| `cases` | `room_id` (FK rooms, bắt buộc) | Khách báo hỏng nhưng không biết thiết bị nào; ERD chỉ nối `Asset`, còn Customer chưa gắn phòng (điểm 11 của soát ERD) | Phạm vi dữ liệu của Staff / Manager đi qua `room → floor → building` |
| `cases` | `cost_bearer` (building / customer) | Staff chốt "bên chịu chi phí" khi duyệt (N23) | Có thể bỏ nếu E5 (khách làm hư xử lý ngoài hệ thống) áp dụng hết |
| `cases` | `closed_at` | Cửa sổ 7 ngày "chưa sửa xong" | Điểm 8 của soát ERD |
| `cases` | `root_cause`, `parts_used` | Ghi nhận sửa chữa: nguyên nhân + linh kiện ghi tay. Giờ công dùng `actual_hours` có sẵn (tự đo nếu Technician không nhập) | Bỏ "nội dung đã làm" theo doc 28/10. Nếu muốn lưu linh kiện đúng chuẩn thì dùng `Costs` (E9: thêm CostType, Quantity, UnitPrice) |
| `cases` | `hold_reason`, `hold_until` | On Hold phải có lý do cố định + ngày dự kiến tiếp tục (N13) | 4 lý do: chờ linh kiện / chờ khách / chờ Staff quyết định / chờ bên ngoài. `waiting_third_party` bật `outsourced_at` (cờ F5) |
| `cases` | `rating_stars`, `rating_comment`, `rated_at` | Khách đánh giá, 1 lần / case | Nếu muốn nhiều lần đánh giá (mở lại) thì tách bảng `CaseRatings` |
| `case_images` | `kind` (report / before / after) | Ảnh trước / sau sửa | `content` đang là data URL, TODO chuyển Supabase Storage |
| `case_status_history` | `from_status`, `changed_by_id`, `reason` | Biết ai duyệt / đóng / hủy / tạm dừng và vì sao (điểm 2 của soát ERD) | Thiếu hai cột sau là không truy vết được |
| `case_categories` | `group_name`, `is_emergency`, `default_priority_id`, `default_cost_bearer`, `photo_required` | Danh mục 2 cấp, cờ khẩn cấp → F4, bắt buộc ảnh (N24, N26, O05). F1 bị bỏ nên cần chỗ lưu | ERD mới có Name + Description |
| `priority_levels` | `code` (emergency / high / normal / low), `sla_hours` | Hạn SLA theo mức ưu tiên: `due_at = started_at + sla_hours` | Đang tính giờ thường; đếm giờ hành chính còn OPEN (BR-08, O04) |

## 3. Bảng mới

| Bảng | Thay cho | Lý do |
|---|---|---|
| `case_assignments` (case, technician, assigned_by, assigned_at, response, responded_at, decline_reason) | `Cases.AssignedTechnician` (chỉ người cuối) + `TechnicianRejections` | Giữ đủ lịch sử giao việc: giao lại, từ chối, mở lại (điểm 4 của soát ERD). Manager đếm lần từ chối bằng `response = declined`. Cột `cases.assigned_technician_id` vẫn giữ là "người đang giữ" |

## 4. Khác ERD có chủ ý

- **Trạng thái / mức ưu tiên là enum text + CHECK** (quy ước D-plan), không phải bảng tra `CaseStatuses`; máy trạng thái nằm trong code (`CaseTransitions.cs`). `priority_levels` vẫn là bảng vì có `sla_hours`.
- **Gộp case trùng bỏ**: theo doc 28/10 trùng thì Reject (lý do `duplicate`, ghi số case đã có trong ghi chú) nên **không cần** `Cases.ParentCase`.
- **SLA bắt đầu khi Technician bấm Start** (In Progress), không phải lúc duyệt. Mở lại thì xóa `started_at / due_at / completed_at / closed_at`, lịch sử vẫn còn trong `case_status_history`.
- **Mã hiển thị `CS-000123`** dựng từ `id`, không lưu cột riêng.

## 5. ERD còn thiếu / chưa dùng tới (không chặn F3)

Chưa làm trong backend F3 (đã có chỗ trong máy trạng thái / ghi chú): giao lại người khác (không phải từ chối), đổi mức khẩn cấp / dời hạn có lý do, khách báo "Chưa sửa xong" (hiện Staff tự mở lại trong 7 ngày), quyết định F5 ngoài cờ `waiting_third_party`, thông báo (`Notifications` chưa có đối tượng trỏ tới, thiếu `IsRead`), audit (`AuditLog` thiếu `TargetID`, chỉ nối Admin), chuyên môn Technician (`TechnicianSpecialty`), đính kèm lên Supabase Storage.

## 6. Câu hỏi còn mở cho họp

1. **Tòa có nhiều staff:** đang để *tất cả staff của tòa* đều thấy và xử lý được case; người duyệt đầu tiên thành `cases.staff_id`. `IsPrimary` hiện chưa dùng cho phân quyền, chỉ để hiển thị. Có đúng ý không?
2. **O07:** SLA có dừng khi On Hold "chờ khách" không? Đang **không** dừng.
3. Danh mục dịch vụ + cờ khẩn cấp: sau khi bỏ F1 thì ai (Admin?) sửa danh mục này, màn hình nào?
4. `cost_bearer` còn cần không (E5)?
