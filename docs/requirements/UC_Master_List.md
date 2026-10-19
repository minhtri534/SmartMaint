# Bảng UC tổng hợp (DRAFT) — để kiểm tra trước khi vẽ Use Case Diagram TỔNG

> Đây là bảng trung gian, tổng hợp CHÉO từ 3 nguồn gốc đã được leader chỉ định (Tùng, Hùng, Leader), cộng các fix nghiệp vụ đã chốt. KHÔNG bịa thêm UC nào ngoài các nguồn này. Cần leader rà lại phần "Không chắc chắn" ở cuối trước khi vẽ diagram thật.

## Nguồn dữ liệu đã đọc
1. Tùng — "Use Case Diagram_Maintenance.docx" (fileId 1J0pmc85tvZCeKRiR1nAsTmhFjYLCI3HW) — Common (User) + Employee + Technician.
2. Hùng — "Use case diagram manager admin" (fileId 1pw9--3NeBAT52tOwutJSDRE6Gh0LMUwt, .drawio, 3 trang: General/Manager/Admin) — dùng trang **Manager** và **Admin** (trang "General" chỉ là overview gộp, không dùng làm nguồn chi tiết).
3. Leader — Google Doc "Report3_Leader_Sections" (fileId 1ikTANFTfXVcDcCGQ7-S3Gsn3ffPe3oX6m7ZwOwY9e7w), mục 2.2.2 — Officer.
4. Đối chiếu quyền: Project doc `claude/SCREEN_INVENTORY.md` (81 màn hình, cột quyền theo role).

Không dùng "Core_Workflows_SmartMaint_Final.doc" theo đúng chỉ định.

## 1. Bảng UC tổng hợp

### Common (actor trừu tượng "User", Employee & Technician kế thừa) — 7 UC

| STT | Mã UC | Tên Use Case (EN) | Actor chính | Actor liên quan | Nguồn | Ghi chú |
|---|---|---|---|---|---|---|
| 1 | UC-COM-01 | Log In to System | User | Employee, Technician (kế thừa) | Tùng | |
| 2 | UC-COM-02 | Manage User Profile | User | Employee, Technician | Tùng | |
| 3 | UC-COM-03 | Use AI Assistant | User | Employee, Technician | Tùng | |
| 4 | UC-COM-04 | View System Notifications | User | Employee, Technician | Tùng | |
| 5 | UC-COM-05 | Global Search | User | Employee, Technician | Tùng | |
| 6 | UC-COM-06 | Reset Password | User | Employee, Technician | Tùng | |
| 7 | UC-COM-07 | Manage Personal Settings | User | Employee, Technician | Tùng | |

### Employee — 7 UC riêng (chưa gồm Common)

| STT | Mã UC | Tên Use Case (EN) | Actor chính | Actor liên quan | Nguồn | Ghi chú |
|---|---|---|---|---|---|---|
| 8 | UC-EMP-01 | Create Maintenance Request | Employee | — | Tùng | |
| 9 | UC-EMP-02 | Track Maintenance Requests | Employee | — | Tùng | |
| 10 | UC-EMP-03 | View Request Details | Employee | — | Tùng | UC cha, EMP-04..07 extend từ đây |
| 11 | UC-EMP-04 | Provide Additional Info | Employee | Officer (nhận info, ↔ OFF-04) | Tùng | extend EMP-03 |
| 12 | UC-EMP-05 | Rate Maintenance Service | Employee | — | Tùng | extend EMP-03 |
| 13 | UC-EMP-06 | Cancel Maintenance Request | Employee | — | Tùng | extend EMP-03 — **đã sửa**: điều kiện hủy đổi từ "chỉ khi Pending" thành "được hủy khi ở trạng thái Pending HOẶC Need more info" (fix đã chốt) |
| 14 | UC-EMP-07 | Report "Not Fixed" | Employee | Officer (xử lý reopen, ↔ OFF-10) | Tùng | extend EMP-03 |

### Technician — 11 UC riêng (chưa gồm Common)

| STT | Mã UC | Tên Use Case (EN) | Actor chính | Actor liên quan | Nguồn | Ghi chú |
|---|---|---|---|---|---|---|
| 15 | UC-TEC-01 | Manage Assigned Work Orders | Technician | — | Tùng | |
| 16 | UC-TEC-02 | View Work Order Details | Technician | — | Tùng | UC cha, TEC-03..07 extend từ đây |
| 17 | UC-TEC-03 | Accept Work Order | Technician | — | Tùng | extend TEC-02 |
| 18 | UC-TEC-04 | Reject Work Order | Technician | Officer (nhận lại case, ↔ OFF-05/06) | Tùng | extend TEC-02 — **đã sửa**: ghi rõ điều kiện "chỉ được reject trước khi Accept work order" (BR-13, fix đã chốt) |
| 19 | UC-TEC-05 | Update Work Order Progress | Technician | — | Tùng | extend TEC-02 |
| 20 | UC-TEC-06 | Discuss Work Order | Technician | — | Tùng | extend TEC-02 |
| 21 | UC-TEC-07 | Complete Work Order | Technician | Officer (verify, ↔ OFF-09) | Tùng | extend TEC-02 |
| 22 | UC-TEC-08 | View Work History | Technician | — | Tùng | |
| 23 | UC-TEC-09 | Perform Routine Inspection | Technician | — | Tùng | UC cha, TEC-10..11 extend từ đây |
| 24 | UC-TEC-10 | Update Inspection Checklist | Technician | — | Tùng | extend TEC-09 |
| 25 | UC-TEC-11 | Complete Routine Inspection | Technician | Officer (case mới tự sinh nếu fail) | Tùng | extend TEC-09 — liên hệ Non-Screen Function "Inspection Failure → New Case" |

### Facility Officer — 23 UC (22 gốc từ Leader + 1 bổ sung theo fix đã chốt)

| STT | Mã UC | Tên Use Case (EN) | Actor chính | Actor liên quan | Nguồn | Ghi chú |
|---|---|---|---|---|---|---|
| 26 | UC-OFF-01 | Claim Case from Queue | Officer | — | Leader | |
| 27 | UC-OFF-02 | Approve Maintenance Request | Officer | Employee | Leader | extend OFF-23 (View Case Details) |
| 28 | UC-OFF-03 | Reject Maintenance Request | Officer | Employee | Leader | extend OFF-23 |
| 29 | UC-OFF-04 | Request More Information | Officer | Employee (↔ EMP-04) | Leader | extend OFF-23 |
| 30 | UC-OFF-05 | Assign Technician | Officer | Technician | Leader | extend OFF-23 |
| 31 | UC-OFF-06 | Reassign Technician | Officer | Technician | Leader | extend OFF-23 |
| 32 | UC-OFF-07 | Track Case Progress | Officer | Manager (xem read-only, ↔ MGR-02) | Leader | |
| 33 | UC-OFF-08 | Cancel Case | Officer | — | Leader | extend OFF-23 |
| 34 | UC-OFF-09 | Verify and Close Case | Officer | Technician (↔ TEC-07) | Leader | extend OFF-23 |
| 35 | UC-OFF-10 | Handle Reopen Request | Officer | Employee (↔ EMP-07), Technician | Leader | extend OFF-23 |
| 36 | UC-OFF-11 | Create Case for Employee | Officer | Employee | Leader | |
| 37 | UC-OFF-12 | View Asset List and Details | Officer | — | Leader | **đã sửa**: bản gốc ghi "Thêm/nhập hàng loạt thiết bị" — sai quyền vì đây là việc của Admin theo BR-09; đổi thành chỉ xem, theo fix đã chốt |
| 38 | UC-OFF-13 | Change Asset Status | Officer | — | Leader | **đã sửa**: bản gốc ghi "Đổi trạng thái / thanh lý thiết bị" — bỏ phần "thanh lý" vì đây là việc của Admin theo BR-10; chỉ giữ đổi trạng thái |
| 39 | UC-OFF-14 | Print Asset QR Labels | Officer | — | Leader | |
| 40 | UC-OFF-15 | Create/Edit Preventive Plan | Officer | — | Leader | |
| 41 | UC-OFF-16 | Pause/Resume/End Preventive Plan | Officer | — | Leader | |
| 42 | UC-OFF-17 | Generate Preventive Work Order Now | Officer | — | Leader | |
| 43 | UC-OFF-18 | View Preventive Calendar | Officer | — | Leader | |
| 44 | UC-OFF-19 | View Inspection Records | Officer | Technician (↔ TEC-09..11) | Leader | |
| 45 | UC-OFF-20 | View KPI Dashboard | Officer | — | Leader | |
| 46 | UC-OFF-21 | View Failure Trends | Officer | — | Leader | |
| 47 | UC-OFF-22 | Export Reports | Officer | — | Leader | |
| 48 | UC-OFF-23 | View Case Details | Officer | — | **Bổ sung theo fix đã chốt** | Không có trong bản gốc Leader — đây là UC cha mà OFF-02,03,04,05,06,08,09,10 «extend» từ đó, theo yêu cầu đã chốt |

### Facility Manager — 10 UC (từ file Hùng, trang "Manager")

> File gốc không có mã UC, chỉ có tên tiếng Việt — tự đánh số UC-MGR-01..10 theo thứ tự xuất hiện trong file, cần Leader xác nhận thứ tự này có ý nghĩa.

| STT | Mã UC | Tên Use Case (EN) | Tên gốc (VI, Hùng) | Actor chính | Actor liên quan | Nguồn | Ghi chú |
|---|---|---|---|---|---|---|---|
| 49 | UC-MGR-01 | View and Analyze Dashboard | Xem và phân tích dashboard | Manager | — | Hùng | |
| 50 | UC-MGR-02 | View All Cases List | Xem chi tiết danh sách tất cả case | Manager | Officer (↔ OFF-07) | Hùng | Theo SCREEN_INVENTORY, màn OFF-03/MGR-02 đã gộp 1 màn (Manager chỉ xem/Officer full quyền) — cần quyết định khi vẽ: 1 UC oval dùng chung hay 2 UC riêng |
| 51 | UC-MGR-03 | View Case Details | Xem chi tiết một case | Manager | Officer (↔ OFF-23) | Hùng | Tương tự — cùng ý nghĩa với UC-OFF-23 nhưng Manager chỉ xem |
| 52 | UC-MGR-04 | View Asset List | Xem danh sách tài sản | Manager | — | Hùng | |
| 53 | UC-MGR-05 | View Asset Details | Xem thông tin một tài sản | Manager | — | Hùng | |
| 54 | UC-MGR-06 | View Facility Tree | Xem sơ đồ địa điểm | Manager | — | Hùng | |
| 55 | UC-MGR-07 | View Preventive Plan Details | Xem chi tiết kế hoạch bảo trì định kỳ | Manager | — | Hùng | |
| 56 | UC-MGR-08 | Analyze Technician Performance | Phân tích hiệu suất kỹ thuật viên | Manager | — | Hùng | |
| 57 | UC-MGR-09 | Analyze Failure Trends | Phân tích xu hướng hư hỏng | Manager | — | Hùng | |
| 58 | UC-MGR-10 | Export Reports | Xuất báo cáo | Manager | — | Hùng | |

### Administrator — 9 UC (7 gốc từ file Hùng trang "Admin" + 2 bổ sung theo fix đã chốt)

| STT | Mã UC | Tên Use Case (EN) | Tên gốc (VI, Hùng) | Actor chính | Actor liên quan | Nguồn | Ghi chú |
|---|---|---|---|---|---|---|---|
| 59 | UC-ADM-01 | Manage Users | Quản lý người dùng | Admin | — | Hùng | |
| 60 | UC-ADM-02 | Manage Maintenance Categories | Quản lý danh mục bảo trì | Admin | — | Hùng | |
| 61 | UC-ADM-03 | View Audit Log | Xem nhật ký hoạt động hệ thống | Admin | — | Hùng | BR-11: Admin không mở được nội dung case |
| 62 | UC-ADM-04 | Manage Facility Tree | Quản lý sơ đồ địa điểm | Admin | — | Hùng | |
| 63 | UC-ADM-05 | Manage Assets (incl. Retire) | Quản lý tài sản | Admin | Officer (↔ OFF-12/13, chỉ xem/đổi trạng thái) | Hùng | Gộp create/edit/retire vào 1 UC theo file gốc; BR-09/BR-10 xác nhận đúng quyền |
| 64 | UC-ADM-06 | Import Assets from File | Nhập tài sản từ tập(tin) | Admin | — | Hùng | |
| 65 | UC-ADM-07 | Generate Asset QR Codes | Tạo mã QR cho tài sản | Admin | — | Hùng | |
| 66 | UC-ADM-08 | Configure SLA and Rules | (không có) | Admin | — | **Bổ sung theo fix đã chốt** | Không có trong file gốc Hùng — liên quan màn ADM-07 (SLA and Rules Settings) |
| 67 | UC-ADM-09 | View Roles and Permissions | (không có) | Admin | — | **Bổ sung theo fix đã chốt** | Không có trong file gốc Hùng — chỉ xem/tham khảo, liên quan màn ADM-05 (Roles and Permissions - reference) |

## 2. Tổng số UC theo actor

| Nhóm | Số UC | Ghi chú |
|---|---|---|
| Common (User, dùng chung Employee+Technician) | 7 | Không double-count vào Employee/Technician bên dưới |
| Employee (riêng) | 7 | + 7 Common nếu tính đầy đủ theo actor = 14 |
| Technician (riêng) | 11 | + 7 Common nếu tính đầy đủ theo actor = 18 |
| Facility Officer | 23 | 22 gốc + 1 bổ sung (View Case Details) |
| Facility Manager | 10 | |
| Administrator | 9 | 7 gốc + 2 bổ sung (SLA, Roles) |
| **TỔNG (không double-count Common)** | **67** | 7+7+11+23+10+9 = 67 |

**So sánh với con số 67 đã ghi trong tài liệu dự án: KHỚP CHÍNH XÁC.**

Phát hiện đáng chú ý: nếu chỉ cộng UC có sẵn trong 3 file gốc thô (chưa áp dụng fix bổ sung UC-OFF-23, UC-ADM-08, UC-ADM-09) thì tổng chỉ là **64** (7+7+11+22+10+7). Con số 67 trong tài liệu dự án khớp đúng bằng 64 + 3 UC bổ sung theo các fix đã chốt. Điều này cho thấy con số 67 có thể đã được tính với giả định các fix này đã áp dụện sẵn — nên việc áp dụng đúng 3 fix bổ sung trên là hợp lý và cần thiết để khớp số liệu, không phải suy đoán thêm.

## 3. Những điểm KHÔNG chắc chắn / cần Leader xác nhận

1. **Trang "General" trong file .drawio của Hùng** (3 trang: General/Manager/Admin) là một overview gộp cả 5 actor với các UC diễn đạt tổng quát hơn (VD: "Xử lý yêu cầu bảo trì", "Theo dõi trạng thái yêu cầu", "Giám sát hiệu suất hoạt động bảo trì") — KHÔNG dùng trang này làm nguồn UC chi tiết cho Manager/Admin vì nó không đúng cấp actor. Đã chỉ dùng 2 trang "Manager" và "Admin". Cần Leader xác nhận cách hiểu này đúng.
2. **Mã UC cho Manager/Admin**: file gốc Hùng không có mã UC, chỉ có tên tiếng Việt trên hình oval. Tôi tự đánh số UC-MGR-01..10 và UC-ADM-01..07 theo thứ tự xuất hiện trong XML (không chắc có phản ánh đúng thứ tự logic nghiệp vụ của Hùng hay không).
3. **UC-ADM-05 "Quản lý tài sản"**: file Hùng gộp chung create/edit/retire vào 1 UC. Theo SCREEN_INVENTORY (AST-04 Create/Edit Asset vs DLG-17 Confirm Retire or Delete là 2 màn khác nhau) và BR-10 (retire có điều kiện riêng: bị chặn nếu còn case mở), có thể cần tách UC-ADM-05 thành 2 UC riêng ("Manage Assets" và "Retire Asset") khi vẽ diagram — hiện tại giữ gộp theo đúng dữ liệu gốc, chưa tự tách vì không muốn suy diễn thêm.
4. **Trùng lặp ý nghĩa Manager ↔ Officer**: UC-MGR-02/UC-MGR-03 (View All Cases / View Case Details, Manager, chỉ xem) trùng nội dung với UC-OFF-07/UC-OFF-23 (Officer, có thao tác) — SCREEN_INVENTORY xác nhận 2 màn hình OFF-03/MGR-02 đã được gộp thành 1 màn (khác nhau ở quyền hành động). Khi vẽ 1 diagram tổng, cần quyết định: dùng chung 1 UC oval với 2 actor line nối vào (Manager + Officer), hay giữ 2 UC riêng theo actor. Đây là quyết định thiết kế diagram, chưa có trong dữ liệu nguồn nên chưa tự quyết.
5. Không đọc được nội dung hình ảnh minh họa trong Google Doc "Report3_Leader_Sections" (Hình 1 Context Diagram, Hình 2 UC Diagram Officer, Hình 3 ERD) — chỉ đọc được các bảng text, nhưng bảng 2.2.2 đã liệt kê đủ text mô tả nên không ảnh hưởng tới bảng UC.
6. File .docx của Tùng chỉ cung cấp link Google Drive tới hình UC Diagram Employee/Technician (không phải ảnh nhúng trực tiếp đọc được) — chỉ dùng được các bảng mô tả text (đã đủ dữ liệu để lập bảng).
