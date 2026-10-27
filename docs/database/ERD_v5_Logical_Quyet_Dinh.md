# ERD v5 Logical — Quyết định chốt từng bước (Leader + Claude)

> Bắt đầu 23/10/2026. Nguồn: trang "Logical ERD" trong `04_Design/ERD_SmartMaint_v5` (Hùng) — **bản Drive là bản chính từ 26/10**.
> 27/10: có đề tài update (`Smart_Corporate_Facility_Maintenance_Management_Platform_Update.docx`) — **Leader: bản update chưa phải bản cuối, có thể giảm scope; ưu tiên sửa LOGIC ERD trước, chỉnh bản update sau.** Sơ đồ F2 (12/10) không dùng nữa.
> ⚠️ Chưa đồng bộ vào PROJECT_CONTEXT.md.

## Góp ý của thầy (Leader kể lại 23/10)
- Thực thể tĩnh trước; 2 thực thể tĩnh M–M thì tách thực thể động ở giữa.
- Account → các bảng actor → bảng nghiệp vụ của actor.
- Rà bảng + quan hệ trước, cột sau.

## Quyết định
| # | Ngày | Quyết định | Ghi chú |
|---|---|---|---|
| E1 | 23/10 | Phạm vi = tòa văn phòng cho thuê; bỏ chung cư | Đề tài update có chung cư — để sau |
| E2 | 23/10 | Lỗi thường gặp theo loại thiết bị | Bản hiện tại dùng CaseCategories — để sau |
| E3 | 23/10 | Manufacturers chỉ lưu hãng sản xuất | |
| E4 | 23/10 | `HasCopyright` = chỉ hãng được bảo trì; thuê ngoài trên case = "lần này có thuê ngoài" (`TimeOutsourced`) | |
| E5 | 23/10 | Khách làm hư → xử lý ngoài hệ thống | |
| E6 | 23/10 | Không bảng Vendor | Đề tài update có Vendors — để sau |
| E7 | 23/10 | Officer ↔ Building M–M → `StaffBuildingAssignments` | Đổi tên 27/10 |
| E8 | 23/10 | `IsPrimary` — chỉ báo Officer chính | |
| E9 | 27/10 | Lưu chi phí (Costs + HourlyRate) — cần bổ sung loại chi phí, số lượng, đơn giá chép | |
| E10 | 27/10 | Manager duyệt kế hoạch (`ApprovingManager`) | Đề tài update chỉ ghi "review / supervise" |
| E11 | 27/10 | `Cases.TriggerWorkOrder` FK nullable → `MaintenanceWorkOrderDetails` (thay IsPreventative) — **Hùng đã sửa** | NULL = case sự cố |
| E12 | 27/10 | `TriggerStatuses` (ID, Name); Chờ duyệt / Đã duyệt / Từ chối… là dữ liệu, không phải cột | |
| E13 | 27/10 | Manager ↔ Building M–M → `ManagerBuildingAssignments` (bỏ `Manager.Building`) — **Hùng đã sửa** | Cột đang tên `Staff` → đổi `Manager` |
| — | 27/10 | **Để sau:** checklist theo loại thiết bị; kỳ bảo trì bỏ / dời | Leader |

## Soát logic bản Drive 27/10 14:28 (Hùng) — đã đọc toàn bộ trang Logical
### Nghiêm trọng (hệ thống không trả lời được câu hỏi cốt lõi)
1. **AuditLog không có TargetID** (chỉ TargetType + Operation) → biết "có ai sửa 1 Case" nhưng không biết case nào. Và chỉ FK Admin → hành động của Officer / Tech / Manager (duyệt, đóng, duyệt kế hoạch) không audit được. Sửa: `AccountID` thay `AdminID` + `TargetID`.
2. **CaseStatusHistory thiếu ChangedBy + Reason, chưa nối Cases** → không biết ai duyệt / ai xác nhận đóng / vì sao On Hold, Reject, Reopen.
3. **`LastTriggered` đặt ở Trigger nhưng ngày bắt đầu lại theo từng thiết bị** (`MaintenanceWorkOrderAssets.StartTime`, kiểu UpKeep) → máy bắt đầu khác ngày thì hạn kế tiếp khác nhau, 1 cột ở Trigger không đủ. Sửa: `LastGeneratedAt` / `NextDueAt` đặt ở `MaintenanceWorkOrderAssets`; ghi rõ StartTime / EndTime = ngày bắt đầu / kết thúc áp dụng cho thiết bị đó.
4. **Giao việc chỉ lưu người cuối** (`AssignedTechnician`) + `TechnicianRejections` chỉ ghi lần từ chối → Officer đổi người (không phải từ chối) hoặc Reopen giao người khác thì mất dấu; F6 hiệu suất KTV tính sai. Sửa: bảng `CaseAssignments` (Case, Technician, AssignedBy, AssignedAt, Response, Reason) thay cả 2.
5. **Lực lượng quan hệ sai:** Account–Roles 1–1 (phải M–1), Account–AIConversation 1–1 (phải 1–M), AIConversation–AIMessage 1–1 (phải 1–M) → hiện tại mỗi user chỉ 1 cuộc hội thoại, mỗi hội thoại 1 tin nhắn.

### Mức vừa
6. Case sửa sinh từ mục checklist Không đạt không biết nguồn → `Cases.SourceTask` (FK CaseTasks, nullable).
7. Nhiều khách báo cùng 1 lỗi → nhiều case, F6 đếm hỏng gấp nhiều lần → `Cases.ParentCase` (gộp case).
8. Thiếu `TimeClosed`; Reopen ghi đè StartTime / TimeCompleted → thời gian lấy từ CaseStatusHistory, cột trên Cases chỉ là cache.
9. Costs: thiếu CostType, Quantity, UnitPrice; giờ công = giờ × HourlyRate hiện tại → tăng lương làm sai chi phí cũ → chép đơn giá vào dòng chi phí.
10. MaintenanceTriggers: không có tòa → không chặn Officer thêm thiết bị tòa khác, không xác định được Manager nào duyệt; thiếu ApprovedAt / RejectReason; Staff / Status chưa đánh chữ FK.
11. Customers không gắn khu vực (phòng / tòa) → không giới hạn được khách chỉ báo thiết bị khu mình.
12. Notifications không trỏ tới đối tượng (case / kế hoạch), không có IsRead.
13. `TaskStatuses` chưa rõ là tiến độ (Chưa làm / Xong) hay kết quả (Đạt / Không đạt / N/A).

### Nhỏ (lỗi vẽ)
14. 2 đường chưa dính bảng: CaseStatuses → Cases, Technicians → Cases.
15. Khóa ghép chưa đánh PK: StaffBuildingAssignments, ManagerBuildingAssignments, TechnicianSpecialty, TechnicianRejections, MaintenanceWorkOrderAssets (cặp Detail + Asset phải duy nhất).
16. `ManagerBuildingAssignments.Staff` nối sang Manager → đổi tên cột.
17. Account.Role + bảng con (Customers / Technicians…) → cần quy tắc: mỗi Account có đúng 1 bảng con khớp Role.

## Lịch sử
- Bản 26/10 22:32: bỏ Requests, thêm CaseStatusHistory, TechnicianRejections, CaseTasks, CaseChecklists, F2 kiểu trigger, Costs có Amount.
- Bản 24/10 21:32: WorkOrders, WorkOrderHistory, CommonFaults, TechnicianSchedule — đã bị thay.
- Đối chiếu đề tài update 27/10 (để sau): Requests tách WO, Vendors, Part Usage, Assets.Status / Criticality, KnowledgeDocuments; 4 điểm lệch phạm vi (chung cư, Mobile chỉ Customer + Tech, OpenAI / Gemini, Manager "review" chứ không "approve").

## Ghi chú kỹ thuật
- Đọc bản Drive bằng connector (fileId `1KNhHzFT_6ET6VNlpQ-TeZnqilxVVZjuI`); parse bảng + đường nối bằng script; so sánh diff giữa các bản.
