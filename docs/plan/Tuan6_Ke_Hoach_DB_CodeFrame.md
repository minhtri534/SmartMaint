# Tuần 6 — Kế hoạch Design DB + Code frame (v4: sở hữu theo luồng, số luồng chốt)

> Soạn: Claude cho Leader · 12/10/2026.
> **v4.2 (12/10, 20h45):** sửa 10 card + thêm 1 card theo mẫu R4/R5 thầy gửi (`claude/Doi_Chieu_Mau_R4_R5.md`): ERD **vật lý** (bỏ Chen); data dictionary bảng lõi + F2 + F3 (Trí); **Package Diagram** Backend + Web (Hùng, card mới) và Mobile (Tùng); kiến trúc đủ job nền / email / push / storage / logging; unit test N/A/B trong DoD card code; Schema.sql + Data.sql; BR F3 v2 dời tuần 7. Giờ: Trí ~24, Hùng 22, Huy 20, Tùng 20.
> **v4.1 (12/10 tối):** F2 đã vẽ, F3 dựng lại, F5 nguồn duy nhất cho bên thứ 3 — tuần 7 của Trí bỏ việc "vẽ F2"; thêm bảng `PlanOccurrence` vào phần bảng của Trí. Card Trello không đổi.
> **v4 (12/10, 17h):** dùng **số luồng chốt** theo thứ tự note thầy (`Danh_Sach_Luong_Chot.md`); **F2 Bảo trì định kỳ giao cho Trí** (Leader chốt). Giữ từ v2–v3: họp T2–T7 hằng ngày (nghỉ CN); chia ngang bằng ~20 giờ/người/tuần; mỗi người thiết kế + code đúng luồng mình; lõi dùng chung do Leader giữ; Huy nhận việc khó cùng chuẩn DoD.
> Trạng thái: phần phân công **chờ họp T3 13/10 xác nhận**; số luồng + chủ F2 + lịch họp **đã chốt**.

## 0. Hiện trạng (12/10)

- Mốc tuần 5 (R4 System + DB design, R5 trừ test case, code frame + DB script) **chưa nộp**. Repo Desktop rỗng.
- Template R4/R5 **không có** trên Drive / Material.
- Trello đứng từ 28/09.
- Luồng: **đủ 6 luồng đã có sơ đồ** (12/10 tối: F2 dựng mới, F3 dựng lại, F5 là nguồn duy nhất cho bên thứ 3 / thay thiết bị).

## 1. Mô hình: sở hữu theo luồng + lõi dùng chung

Mỗi chủ luồng tự làm đủ chuỗi cho luồng mình ở mỗi package: **bảng (data dictionary) → API → màn hình → sequence/class diagram (R4) → test case (R5) → BR liên quan (R3)**, và chốt OPEN của luồng mình.

5 điều kiện để mô hình không vỡ:

| # | Lỗ hổng | Cách vá |
|---|---|---|
| 1 | Các luồng dùng chung bảng Case + máy trạng thái | **Lõi do Trí giữ** (Auth/RBAC, Case + máy trạng thái, thông báo, audit, file đính kèm, AI gateway). Người khác gọi lõi; sửa bảng lõi → PR, Trí review |
| 2 | Luồng to nhỏ khác nhau | Ghép luồng lớn + nhỏ, xếp theo thời gian (mục 3) |
| 3 | Phụ thuộc thứ tự (F3 cần F1; F4/F5 mở rộng F3; F6 cần tất cả) | Tuần đầu, chủ luồng sau làm thiết kế + một mảnh code frame gần luồng mình; F3 chạy bằng seed data, không chờ màn F1 |
| 4 | Mobile cắt ngang mọi luồng | Tùng giữ toàn bộ mobile; chủ luồng viết spec màn mobile cho luồng mình |
| 5 | 4 kiểu code / 4 kiểu report | Module mẫu + trang mẫu + mẫu mục report; mọi PR qua Trí; ERD là **một mô hình** do Trí ghép |

## 2. Bản đồ sở hữu

| Người | Sở hữu | Bảng sở hữu (ERD v4) | Màn hình chính |
|---|---|---|---|
| **Trí** | **Lõi** + **F2** bảo trì định kỳ + **F3** sự cố thông thường | User, Role, AuditLog, Notification, SystemSetting, Attachment, AIConversation; Case, CaseStatusHistory, Assignment, CaseComment, CaseDueChange; PreventivePlan, PlanAsset, PlanOccurrence, ChecklistTemplate, ChecklistItem, InspectionRecord | Khung web, Auth, Case Detail, tạo case, hàng chờ Officer, work order Technician, kế hoạch bảo trì, phiếu kiểm tra |
| **Huy** | **F1** cấu trúc tòa nhà, thiết bị, danh mục dịch vụ | Building, Floor, Room, Unit, BuildingAssignment, AssetCategory, Asset, ServiceGroup, ServiceType, RootCause | Cây tòa nhà, thiết bị (nhập file, QR, thanh lý), danh mục dịch vụ, phân công Officer theo tòa |
| **Tùng** | **F4** sự cố khẩn cấp + **toàn bộ Mobile** | Trường khẩn cấp trên Case (PR), OnCallSchedule (nếu thầy đồng ý K02) | Màn khẩn cấp (web), toàn bộ app Flutter |
| **Hùng** | **F5** sửa hay thay / bên thứ 3 + **F6** giám sát | Vendor, VendorHandoff; Asset.replaced_by (PR vào bảng của Huy); view/query KPI | Danh bạ vendor, chuyển bên thứ 3, Home theo role, dashboard Manager/Officer |

**Review chéo:** Huy ↔ Trí (F1 cấp dữ liệu cho F2, F3) · Tùng ↔ Trí (F4 mở rộng F3) · Hùng review dữ liệu của mọi người từ góc F6 ("bước này có ghi đủ dữ liệu để đo KPI không?").

**Sở hữu OPEN:** F1 A01–A07 → Huy · F3 O01–O17 → Trí · F4 K01–K08 → Tùng · F5 R01–R04, F6 S01–S05 → Hùng · F2 P01–P09 → Trí.

**Rủi ro quá tải Trí** (lõi + F2 + F3 + AI): F2 code ở Package 2 (sau khi F3 xong P1); luồng F2 **đã vẽ 12/10** nên tuần 7 của Trí nhẹ hơn (chỉ chốt P01–P09). Nếu tuần 8 trễ > 3 ngày: chuyển **AI gateway** (Package 3) cho Huy hoặc Hùng — quyết ở họp cuối tuần 8.

## 3. Lộ trình theo package

| Giai đoạn | Trí | Huy | Tùng | Hùng |
|---|---|---|---|---|
| **Tuần 6** code frame + R4/R5 | Lõi: backend + web skeleton, Auth, RBAC, deploy; ghép ERD v4 | F1: thiết kế bảng + seed; code Building/Floor/Room | Flutter skeleton + login/home/profile mobile; thiết kế F4 | F5/F6: thiết kế; code Quản lý user + Home theo role |
| **Tuần 7** Package 1 | **F3** lõi: case → duyệt → giao → làm → nghiệm thu → đóng; chốt OPEN F2 (P01–P09) | **F1**: thiết bị, danh mục dịch vụ, phân công Officer theo tòa | Mobile luồng F3 phía khách hàng; thiết kế chi tiết F4 | F5 thiết kế chi tiết; công thức KPI F6 |
| **Tuần 8–9** Package 2 | **F2**: kế hoạch, lịch, sinh work order, phiếu kiểm tra (+ AI gợi ý chu kỳ); F3 mở rộng (gộp case, On Hold, dời hạn) | **F1** phần còn lại: nhập file, QR, thay thế / thanh lý | **F4** web + backend; mobile Technician | **F5**: vendor, chuyển bên thứ 3, thay thiết bị |
| **Tuần 10–11** Package 3 | AI gateway + điểm AI trong F2/F3 | Test F1; hỗ trợ AI nếu Trí quá tải | Mobile Officer | **F6**: dashboard, KPI, cảnh báo |
| **Tuần 12–13** | System test, tích hợp | User Guide phần F1 | Mobile Manager/Admin | Test F5/F6 |

## 4. Task tuần 6 (13–18/10)

### Trí — Lõi + F3 (+ F2 từ tuần 7) (~24 giờ, cao hơn vì vai trò ghép / review)

| Mã | Card | Hạn | Giờ | DoD |
|---|---|---|---|---|
| L1 | Chốt D1–D16 → Decision Log v2; báo thầy trễ + gửi 6 câu hỏi | T3 13/10 | 3 | Decision Log v2 trên `00_Admin`; tin nhắn thầy lưu `06_MeetingNotes` |
| L2 | Ghép **ERD v4 vật lý** (cột, kiểu, PK/FK — không Chen) + **data dictionary bảng lõi + F2 + F3** (F2 theo `F2_Bao_Tri_Dinh_Ky.md` mục 7); sau migration xuất ERD từ DB thật | T5 15/10 | 6 | `04_Design/ERD_SmartMaint_v4.drawio`; data dictionary |
| L3 | Backend skeleton + Auth (module mẫu) + RBAC + deploy Railway↔Supabase; **chốt pattern** (layered / CQRS); unit test mẫu N/A/B; **Login class + sequence** (mục mẫu R4) | T5 15/10 | 9 | Swagger online, đăng nhập được; README "cách tạo module" |
| L4 | Web skeleton (router theo role, layout, API client, trang lỗi) + **1 trang mẫu**; deploy Vercel | T5 15/10 | 4 | Link Vercel gọi được API; README "cách tạo trang" |
| L5 | Migration đợt 1 + **Schema.sql + Data.sql**; review / merge PR | T7 17/10 | 2 | 2 file .sql trong repo |
| L6 | R4 kiến trúc hệ thống (đủ job nền, email, push, Storage, logging; Context Diagram thêm Email) + ghép R4 nháp. **BR F3 v2 dời tuần 7** | CN 18/10 | 2 | R4 nháp |

### Huy — F1 (~20 giờ)

| Mã | Card | Hạn | Giờ | DoD |
|---|---|---|---|---|
| U1 | Nắm F1 bản dựng lại: trình bày lại ở họp + chốt hướng A01, A03, A05 (nguồn thật) | T3 13/10 | 3 | Trình bày 10 phút; ghi kết quả vào OPEN F1 |
| U2 | Đề xuất bảng F1 + **data dictionary** các bảng F1 (kiểu, null, khóa, ràng buộc, mô tả, BR nguồn) | T4 14/10 | 4 | Gửi Trí trước 21h T4 để ghép ERD |
| U3 | Seed data F1: 2 tòa (văn phòng + chung cư), tầng, phòng (khu chung / thuê), Unit, thiết bị, danh mục dịch vụ 2 cấp | T6 16/10 | 2 | CSV đúng tên cột |
| U4 | **Code Building / Floor / Room** (backend theo module mẫu + web theo trang mẫu), chỉ Admin sửa, Officer / Technician xem | T7 17/10 | 8 | Chạy online; RBAC đúng; PR được review. **Checkpoint T5:** API danh sách Building chạy |
| U5 | R5 khung mục 1–3 (scope F1–F6, ma trận cấp test có Integration) + **2 file Excel chung** (Unit Test N/A/B, Test Report UAT theo luồng) + test case F1; BR F1 v2 | CN 18/10 | 3 | Khung R5; 2 file Excel; ≥ 8 UAT F1 + unit test N/A/B |

### Tùng — F4 + Mobile (~20 giờ)

| Mã | Card | Hạn | Giờ | DoD |
|---|---|---|---|---|
| T1 | Lấy template R4 + R5 lên Drive; dọn Trello | T3 13/10 | 1 | 2 template trên Drive |
| T2 | Nắm F4 bản dựng lại: trình bày ở họp + research K02/K05 (lịch trực 24/7, nguồn thật) | T4 14/10 | 4 | Trình bày 10 phút; K02/K05 có URL |
| T3 | Đề xuất dữ liệu F4 (trường khẩn cấp trên Case, mốc khống chế / khắc phục, lịch trực nếu có) + data dictionary phần F4 | T4 14/10 | 2 | Gửi Trí trước 21h T4 |
| T4 | Flutter skeleton + Login / Home theo role / Profile mobile gọi API thật | T7 17/10 | 10 | Chạy trên emulator với user seed |
| T5 | R4 **Package Diagram Mobile** + bảng mô tả; BR F4 v2 (BR-01, BR-08) | CN 18/10 | 3 | Gửi Trí ghép |

### Hùng — F5 + F6 (~22 giờ)

| Mã | Card | Hạn | Giờ | DoD |
|---|---|---|---|---|
| H1 | Nắm F5 + F6 bản dựng lại: trình bày ở họp; chốt hướng S03 ("đúng lịch") có nguồn | T3 13/10 | 3 | Trình bày 10 phút |
| H2 | Đề xuất bảng F5 (Vendor, VendorHandoff) + **bảng KPI F6**: mỗi chỉ số → công thức → cần cột nào của bảng nào | T4 14/10 | 5 | Gửi Trí trước 21h T4; chỉ ra cột còn thiếu |
| H3 | **Code Quản lý người dùng** (Admin: danh sách / tạo / sửa / khóa) backend + web, theo mẫu | T7 17/10 | 8 | Chạy online; validate theo ADM-03; PR được review. **Checkpoint T5:** API danh sách user chạy |
| H4 | Home theo role (khung, ô KPI giữ chỗ cho F6) | T7 17/10 | 2 | 5 role vào đúng Home |
| H5 | BR F5/F6 v2 + test case cho Quản lý user (2 file Excel chung) | CN 18/10 | 2 | ≥ 8 UAT + unit test N/A/B |
| H6 | **R4 Package Diagram Backend + Web** + bảng Package Descriptions, vẽ theo skeleton thật (card mới 12/10) | CN 18/10 | 2 | 2 sơ đồ + bảng, gửi Trí |

**Chuỗi phụ thuộc:** T4 21h — 4 người gửi đề xuất bảng → T5 Trí ghép ERD v4 → **T6 16/10 buổi dài: đóng băng ERD v4** → T7 migration. Song song: T5 có module mẫu + trang mẫu → Huy, Hùng code từ T5. **T7 17/10 buổi dài: demo nội bộ online.** **CN 18/10:** gửi thầy R4 nháp + R5 khung + link code frame + DB script.

## 5. Lịch họp: T2–T7 hằng ngày, nghỉ CN (đã chốt)

- Họp ngắn 20 phút: cập nhật Trello trước; mỗi người 3 câu (xong gì / làm gì / kẹt gì).
- Buổi dài: T6 16/10 đóng băng ERD v4; T7 17/10 demo nội bộ.
- T3–T5: mỗi chủ luồng trình bày lại luồng của mình (10 phút / người).

## 6. Quyết định schema D1–D16 (chờ họp T3 chốt)

D1 role khách = `Customer` · D2 `Unit` + đại diện hợp đồng · D3 `Building.type` + giờ làm việc · D4 `Room.area_type` · D5 `BuildingAssignment` (Primary/Backup) + `User.is_away`, bỏ claim · D6 `ServiceGroup` + `ServiceType` (sao chép mặc định vào Case) · D7 11 trạng thái + `hold_reason` + `expected_resume_at` · D8 `Case.parent_case_id` · D9 `sla_due_at` + `adjusted_due_at` + `CaseDueChange` · D10 `Vendor` + `VendorHandoff`, không cột tiền · D11 `Assignment` (lịch sử giao + lý do từ chối) · D12 `work_done`, `root_cause_id`, `parts_text`, `labor_hours`, `CaseAttachment.kind` · D13 `CaseStatusHistory` · D14 mức khẩn cấp + `contained_at` / `resolved_at`, lịch trực để Could · D15 `Asset.replaced_by_asset_id`, `requires_inspection` · D16 `ChecklistTemplate` theo loại thiết bị (thuộc F2).
Nguyên tắc: câu trả lời của thầy chỉ làm đổi 1 giá trị enum / 1 cột nullable / 1 dòng cấu hình. Quy ước: snake_case tiếng Anh, `bigint identity` + mã hiển thị, `timestamptz`, enum dạng text, EF Core code-first → `dotnet ef migrations script`. Không có Register (Admin tạo tài khoản, đúng R3).

## 7. Rủi ro

1. Trí quá tải (lõi + F2 + F3 + AI) → xem mục 2; AI gateway có thể chuyển người ở tuần 10.
2. Huy code lần đầu nhưng F1 là dữ liệu nền → F3 chạy bằng seed data; checkpoint T5.
3. Lõi thành nút thắt → module mẫu + trang mẫu xong T5; sửa bảng lõi bằng PR.
4. R3 ↔ R4 lệch → Decision Log v2 + BR v2 (gom từ 4 chủ luồng) xong trước khi nộp R4.
5. Scope phình mà effort giữ 100 man-day → mỗi chủ luồng đề xuất Must / Should / Could cho luồng mình trong tuần 7.
6. ✅ AI cho F2 đã có trong sơ đồ (3 ô AI, 12/10). Rủi ro còn lại: P03 (văn bản PCCC hiện hành) và P04 (N ngày "vừa sửa") cần thầy chốt.
7. Mâu thuẫn PROJECT_CONTEXT (16/09 "đã xong code frame" vs repo rỗng) → xác nhận repo `Documents/GitHub/SmartMaint`.
