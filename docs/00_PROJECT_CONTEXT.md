# PROJECT CONTEXT — FA26SE147
# Smart Corporate Facility Maintenance Management Platform
# (Nền tảng / Hệ thống quản lý bảo trì cơ sở vật chất doanh nghiệp thông minh)

> **Mục đích file này:** Giữ context đầy đủ cho AI / nhóm khi chat mới hoặc quên lịch sử.  
> **Cập nhật lần cuối:** 2026-10-12 (lần 4, tối) — **Đủ sơ đồ 6 luồng**: dựng **F2 bảo trì định kỳ** (4 trang, 3 ô AI, OPEN P01–P09, `claude/F2_Bao_Tri_Dinh_Ky.md`) và **dựng lại F3** (5 trang; file Leader bỏ vào folder là bản 05/10 cũ); **chốt F5 là nguồn duy nhất** cho bảo hành / bên thứ 3 / báo giá / thay thiết bị (F3 Trang 3 rút gọn → F5). Xem Decisions Log 12/10 (5). Trước đó (lần 3): chốt số luồng theo thứ tự note thầy (F1 Huy · F2, F3 Trí · F4 Tùng · F5, F6 Hùng) ở `claude/Danh_Sach_Luong_Chot.md`; sở hữu theo luồng; họp T2–T7; kế hoạch tuần 6 ở `claude/Tuan6_Ke_Hoach_DB_CodeFrame.md`.  
> **Trước đó cùng ngày:** (lần 2) đối chiếu bài tuần 4 + dựng lại 4 luồng (nay là F1, F4, F5, F6 — `claude/Doi_Chieu_Tuan4_F1_F4_F5_F6.md`); (lần 1) luồng sự cố thông thường v3 (nay là F3 — `claude/F3_Su_Co_Thong_Thuong.md`).  
> **Trước đó:** 2026-10-05 — Vẽ xong luồng sự cố thông thường bản đầu (4 trang, 22 OPEN — đã bị thay bởi F3 v3, file đã xóa 12/10).  
> **Trước đó:** 2026-09-30 — Lập Scrum Product Backlog Excel (71 module, 5 sprint, 100 man-day).  
> **Trước đó:** 2026-09-29 — **Audit chéo R1/R2/R3/Catalog và sửa trực tiếp trên Drive** (chi tiết mục 8g): R1+R2 thêm dòng RoC, khớp scope AI bắt buộc + Mobile 5 role; R3 (Google Doc trên Drive) sửa 63 chỗ + thêm phạm vi Mobile, ERD **17 entity**; Catalog VI+EN trên Drive **gộp MGR-02 vào OFF-03** (81 màn); upload `04_Design/ERD_SmartMaint_v3.drawio`.  
> **Cập nhật trước đó:** 2026-09-28 — **Report 3 (SRS) viết lại mục 3.2 trở đi theo Feature** (138 → **77 trang**, theo đúng cấu trúc mẫu trường "3.2 <<Feature Name>> / 3.2.x <<Function Name>>" thay vì liệt kê từng màn hình); ERD thêm entity **Floor**; **Backend đổi sang .NET (C#)** (Leader xác nhận); Screen Authorization/Descriptions dựng lại từ catalog 81 màn — chi tiết mục 8f.  
> **Leader:** minh tri (minhtritradetf2@gmail.com)  
> **Trạng thái hiện tại:** **Tuần 6 (12–18/10/2026).** ⚠️ **Mốc tuần 5 (Report 4 System + DB design, Report 5 trừ test case, code frame + database script) CHƯA NỘP** — kế hoạch bù: nộp CN 18/10 (`claude/Tuan6_Ke_Hoach_DB_CodeFrame.md`). Template R4/R5 chưa có trên Drive/Material (Tùng đi lấy). Repo `SmartMaint` trong folder Desktop chỉ có `.git` rỗng. Package 1 (case core = F1 + F3) hạn hết tuần 7. **Report 1 + Report 2 đã hoàn thành và ĐÃ CẬP NHẬT theo scope mới** (AI bắt buộc + RBAC-scoped cho all role, Mobile full parity 5 role) — file `Report1_Project_Introduction_SmartMaint.docx` và `Report2_Project_Management_Plan_SmartMaint.docx` (lưu `03_Reports/` trên Drive). Effort ước lượng chốt ở **100 man-day** (Leader chủ động giữ mức này, không ép lên 133 dù scope AI+Mobile mở rộng — xem mục 8f/16 ngày 28/09). **Report 3 (SRS) đã có bản đầy đủ, đã rút gọn mục 3.2+ theo Feature** `Report3_Software_Requirement_Specification_SmartMaint.docx` (28/09, 77 trang) — **chưa sửa theo note thầy 12/10** (bối cảnh tòa nhà cho thuê/chung cư, 11 trạng thái, Officer theo tòa…).  
> **Nguồn làm việc nhóm:** Google Drive. **Tài liệu trường:** `MaterialOfCapStoneProject/` (local + bản trên Drive).  
> **Leader/AI only:** `00_Admin/_LEADER_ONLY_PreCode_Plan.md` — **không** upload lên Drive nhóm.  
> **Lịch tuần dự án:** Tuần 1 bắt đầu tuần có ngày 08–14/09/2026 (T2 08/09 → CN 14/09).

---

## 1. Thông tin hành chính đề tài

| Mục | Nội dung |
|---|---|
| Mã đề | **FA26SE147** (ghi ngắn "SE147" trong report) |
| Tên EN | Smart Corporate Facility Maintenance Management Platform |
| Tên VN (form) | Hệ thống quản lý bảo trì cơ sở vật chất doanh nghiệp thông minh |
| Tên VN (file thầy) | Nền tảng quản lý bảo trì cơ sở vật chất doanh nghiệp thông minh |
| **Group name (dùng trong report)** | **SmartMaint** — không có mã lớp riêng |
| Thời gian KLTN | 09/2026 → 03/2027 |
| Ngành | Software Engineering (SE) / Information Technology |
| GVHD chính (dùng trong report) | **Trần Thanh Nguyên** — nguyentt15@fe.edu.vn |
| GVHD (có trong 1 bản, không đưa vào Report Team) | Trần Trọng Huỳnh — huynhtt@fpt.edu.vn |
| File đề gốc VN | Trên Drive: `01_Idea/` (tên file theo bản nhóm upload) |
| File đề EN gốc | FA26SE147_*_nguyentt15.docx (cũng ưu tiên bản trên Drive) |
| Existing systems benchmark (dùng cho Report 1, mục 3) | **UpKeep** và **Fiix** (2 CMMS thương mại, research 2026-09-14) |

### Tags dự án (gợi ý đã dùng khi đăng ký)
Facility Maintenance, CMMS, Asset Management, Ticketing, Web Application, Dashboard, Preventive Maintenance, AI Chatbot

---

## 2. Bài toán (Context) — nói 1 phút

> ⚠️ **Cập nhật 12/10 (note thầy):** bối cảnh đổi thành **ban quản lý tòa nhà văn phòng cho thuê + chung cư**; Officer/Technician/Manager là nhân viên ban tòa nhà; **Employee = khách hàng (người thuê / cư dân)**. Không quản lý kho, tiền, duyệt thuê ngoài. Technician chỉ sửa sự cố thông thường; vượt khả năng / cần thay → bên thứ 3 (hệ thống chỉ ghi nhận + chuyển báo giá). Chi tiết `claude/F3_Su_Co_Thong_Thuong.md`. R1/R3 chưa sửa theo bối cảnh mới.

Doanh nghiệp quản lý tòa nhà văn phòng với nhiều tài sản: phòng họp, máy lạnh, đèn, thang máy, máy in, thiết bị mạng.

**Helpdesk hiện tại** chủ yếu chỉ **ghi nhận** yêu cầu bảo trì → yếu ở:
1. Lập kế hoạch bảo trì
2. Theo dõi quá trình sửa chữa
3. Ra quyết định dựa trên dữ liệu (thiết bị nào hay hỏng, KTV nào hiệu quả, sửa hay thay)

**Đề xuất:** nền tảng quản lý **toàn bộ vòng đời Maintenance Case**  
(báo sự cố → phân công → sửa → xác nhận → đóng) + dashboard cho Facility Manager.

**AI:** chỉ tích hợp qua **API có sẵn** (Replicate — xem mục 8), **bắt buộc** từ 14/09, hỗ trợ tư vấn — **không** train model, **không** thay quyết định của người.

---

## 3. Giải pháp — 5 module

1. **Asset & Facility Management** — Building, **Floor**, Room, Asset; lịch sử bảo trì; lịch kiểm tra định kỳ  
2. **Maintenance Case Management** — lõi hệ thống: tạo case, giao KTV, theo dõi, xác nhận, đóng  
3. **Preventive Maintenance** — kế hoạch định kỳ, nhắc nhở, ghi lịch sử (chủ động, không đợi hỏng)  
4. **Analytics & Decision Support** — dashboard KPI, hiệu suất KTV, xu hướng hỏng  
5. **AI-assisted Maintenance Support (BẮT BUỘC, không còn optional — chốt 2026-09-14)** — hỏi đáp, gợi ý troubleshooting, tóm tắt kiến thức qua API; hiển thị cho **cả 5 role**, mỗi role bị giới hạn theo đúng phạm vi RBAC của mình (xem bảng AI Access Rules ở mục 4b)  

### 3b. Sáu luồng nghiệp vụ — BẢN CHỐT 12/10/2026 (theo thứ tự note thầy)

| Số | Luồng | Phụ trách (phân tích + thiết kế + code) | Luồng của thầy |
|---|---|---|---|
| F1 | Cấu trúc tòa nhà, thiết bị và danh mục dịch vụ | Huy | Luồng 1 |
| F2 | Bảo trì định kỳ (có AI) | Trí | Luồng 2 |
| F3 | Xử lý sự cố thông thường (có AI) | Trí | Luồng 3 |
| F4 | Xử lý sự cố khẩn cấp (có AI) | Tùng | Luồng 3 |
| F5 | Sửa hay thay / chuyển bên thứ 3 | Hùng | Luồng 3 |
| F6 | Giám sát hiệu suất và thống kê | Hùng | Luồng 4 |

Lõi dùng chung (Auth/RBAC, Case + máy trạng thái, thông báo, audit, file đính kèm, AI gateway): Trí. Mobile toàn bộ: Tùng. Bảng đối chiếu số cũ (brief 03/10, bài tuần 4): `claude/Danh_Sach_Luong_Chot.md`. ⚠️ Mọi tài liệu cũ hơn 12/10 (lần 3) dùng số khác — đọc kèm bảng đối chiếu.

---

## 4. Actors (5 role) — rất quan trọng

| Role | Việc chính |
|---|---|
| **Employee** | Gửi request, upload ảnh, theo dõi trạng thái, nhận notif |
| **Facility Officer** | Duyệt request, giao Technician, giám sát tiến độ, **verify & đóng** |
| **Maintenance Technician** | Nhận/từ chối work order, sửa tại hiện trường, cập nhật status, upload bằng chứng, ghi nguyên nhân/linh kiện/giờ công |
| **Facility Manager** | Dashboard, KPI, giám sát hiệu suất (không điều phối hằng ngày) |
| **Administrator** | User, facility/asset, cấu hình danh mục bảo trì |

### Phân biệt Officer vs Technician (điểm từng bị mơ hồ)
- **Officer = điều phối / quyết định / verify trên hệ thống** (không ra sửa máy).  
- **Technician = thực thi tại hiện trường**; tạo dữ liệu thực (ảnh, nguyên nhân, linh kiện, giờ công).  
- Officer "giám sát tiến độ" = **đọc** những gì Technician nhập.  
- Technician báo Complete ≠ Closed; Officer phải **Verify** mới đóng (hoặc Reopen).
- **Officer KHÔNG tạo/sửa/nhập thiết bị** (chỉ Admin — BR-09) và **không thanh lý (retire)** thiết bị (chỉ Admin — BR-10); Officer chỉ xem thiết bị + đổi trạng thái (DLG-18) + in nhãn QR. *(Đã sửa lại UC Officer theo điểm này ngày 28/09 — mục 8f.)*

### Deliverable theo role (CẬP NHẬT 2026-09-14 — thay thế quyết định cũ)
- **Web VÀ Mobile (Flutter) đều phục vụ đủ 5 role, full parity.** Không còn tách "Web = Officer/Manager/Admin, Mobile = Employee/Technician" như bản cũ nữa.
- Mỗi role dùng được toàn bộ chức năng của mình (submit/approve/assign/verify case, quản lý asset, xem dashboard, quản lý user) trên **cả 2 nền tảng**, RBAC áp dụng nhất quán trên cả Web và Mobile.
- **Impact:** tăng đáng kể effort Mobile (Flutter phải làm thêm màn hình cho Officer/Manager/Admin, không chỉ Employee/Technician) — xem WBS Report 2 (mục 7, đã tăng từ 19 lên 36 man-day).

## 4b. AI Access Rules (RBAC-scoped) — chốt 2026-09-14

Nguyên tắc chung: **AI không được cấp quyền nào ngoài quyền RBAC hiện có của role đó** — AI chỉ là một "kênh hỏi đáp" trên dữ liệu mà role vốn đã truy vấn được qua hệ thống, không phải một nguồn quyền mới. Mọi câu hỏi/trả lời AI đều ghi audit log.

| Role | AI được trả lời trong phạm vi |
|---|---|
| Employee | Case của chính mình; FAQ chung cách dùng app; mẹo xử lý sự cố phổ biến (không gắn case người khác) |
| Maintenance Technician | Case/Work Order đang được giao cho mình; gợi ý troubleshooting theo loại asset; không xem hiệu suất/case của Technician khác |
| Facility Officer | Case trong hàng chờ/phạm vi mình xử lý (đúng bằng phạm vi RBAC Officer đã có); tóm tắt case theo building/asset; không xem thông tin cá nhân nhạy cảm ngoài phạm vi công việc |
| Facility Manager | Dữ liệu KPI/thống kê tổng hợp đã hiển thị trên dashboard; không hỏi được chi tiết cá nhân của từng Employee ngoài phạm vi thống kê |
| Administrator | Câu hỏi cấu hình hệ thống, danh mục; phạm vi rộng nhất nhưng vẫn bị audit log toàn bộ |

**Cơ chế kỹ thuật (để nói chuyện với thầy/note kiến trúc):** AI luôn nhận context đã được filter theo cùng lớp RBAC/query-scope mà backend áp dụng cho role đó — tức là AI "nhìn thấy" đúng bằng những gì role đó vốn query được từ DB, không tự ý mở rộng.

**Note thầy 08/10 về AI:** "AI chỉ hỗ trợ trong lĩnh vực lập kế hoạch bảo trì và quy trình bảo trì, đề xuất bảo trì, định vị sự cố"; "Luồng 2, 3 có thể lồng ghép AI" — theo thứ tự của thầy là **F2 bảo trì định kỳ** và **F3/F4 xử lý sự cố**. F3 có 3 điểm AI; **F2 (vẽ 12/10) có 3 điểm AI**: gợi ý chu kỳ + checklist theo loại thiết bị, gợi ý giữ / dời kỳ khi thiết bị vừa sửa (G3), gợi ý nguyên nhân mục checklist Không đạt. AI chỉ gợi ý, không tự đổi lịch / bỏ kỳ / chuyển Out of service.

---

## 5. Workflow nghiệp vụ (đã thảo luận)

### Happy path (gợi ý đã thống nhất hướng)
```
Employee tạo request
  → Pending
Facility Officer duyệt
  → Approved (+ notif Employee)
Officer giao Technician
  → Assigned (+ notif Technician)
Technician Accept
  → Accepted / In Progress
Technician làm việc, cập nhật, upload evidence
  → Completed
Officer Verify đạt
  → Verified → Closed (+ notif Employee)
```

### Nhánh phụ cần có
- Officer **Reject** (kèm lý do)  
- Officer **Need More Info** (trả Employee bổ sung)  
- Technician **Reject** assignment (Officer giao người khác)  
- Officer **Reopen** sau Completed nếu chưa đạt  
- (Optional) On Hold khi thiếu linh kiện  

### Status chính thức (chốt theo catalog, dùng trong Report 3 mục 5.4.1 — 28/09)
⚠️ **12/10:** thầy yêu cầu **bỏ Need more info** (thiếu thông tin → Reject, khách gửi lại) → còn **11 trạng thái**; R3 chưa sửa (bảng thay đổi ở `claude/F3_Su_Co_Thong_Thuong.md` mục 7). Bản cũ: 12 trạng thái: **Pending, Need more info, Approved, Assigned, Accepted, In Progress, On Hold, Completed, Closed, Reopened, Rejected, Cancelled.** (Bảng trạng thái WS1 của Tùng — `Maintenance_Case_Status.docx` — là bản cũ hơn, thiếu Accepted/On Hold/Reopened/Cancelled và dùng tên "Additional Information required"; SRS dùng bộ 12 trạng thái của catalog.)

### Case vs Work Order (khuyến nghị V1)
- **Maintenance Case** = 1 vấn đề nghiệp vụ (phiếu sự cố)  
- **Work Order** = lệnh giao KTV thực hiện  
- **V1 đơn giản: 1 Case = 1 Work Order** (tránh phức tạp sớm)

### Duyệt vs Giao việc
- **Đã chốt: tách 2 bước** (Approved rồi mới Assigned) để đo được thời gian chờ giao việc — BR-20 trong Report 3; DLG-01 và DLG-04 riêng, có tick "Assign next" cho nhanh.

### Notification (đã bàn)
Không chỉ 1 notif lúc duyệt. Các mốc chính: tạo request → Officer; duyệt/từ chối/đòi info → Employee; giao việc → Technician; Complete → Officer; Closed → Employee; Tech reject → Officer. (Bảng đầy đủ 20 mốc ở Report 3 mục 5.4.2.)

### Hàng chờ Officer
Nên dùng **queue chung**: ai rảnh mở xử lý trước (tránh gán cứng 1 Officer nếu chưa cần). *(Đã chốt — BR-19.)* ⚠️ **12/10: ĐỔI** — Leader chốt theo note thầy: **mỗi Officer phụ trách 1+ tòa, chỉ Officer phụ trách nhận thông báo và xử lý; bỏ hàng chờ chung** (BR-19 phải sửa; claim BR-02 giữ/bỏ: O02).

---

## 6. Dữ liệu (Dataset theo đề)

- **Master:** Buildings, Floors, Rooms, Assets, Employees  
- **Transaction:** Maintenance Cases, Work Orders, Inspection Records  
- **Analytical:** KPI bảo trì, thống kê sửa chữa, báo cáo hiệu suất  

### 6b. Domain model cho ERD (Report 3, mục 3.1.5 — chốt dần khi code schema thật)

**🆕 13/10 sáng — ERD khái niệm v4 cho R3 (Chen, đúng style v3) đã vẽ:** 26 entity / 34 quan hệ theo D1–D16 đề xuất + F2 (thêm Unit, ServiceGroup, ServiceType thay IssueType, CaseStatusHistory, CaseDueChange, Assignment, Vendor, VendorHandoff, ChecklistTemplate, PlanOccurrence; quan hệ mới: leasing, belonging, responsible for (Officer–Building, thay claim), occurring at (Room–Case), merging (gộp case), replacing (thay thiết bị), generating (kỳ bảo trì → Case), recording, requesting/assigning/performing thay "handling"). File: máy Leader `Claude outputs/ERD_v4_Report3/` (Leader up Drive 04_Design). **Chưa sửa chữ / bảng Entities Description / ảnh trong R3 Google Doc** vì phiên này không có connector Google Docs. Sau họp chốt D1–D16 tối 13/10 chỉnh lại nếu có thay đổi.

**🆕 12/10:** ERD v4 đang thiết kế theo 16 quyết định D1–D16 (Customer, Unit, BuildingAssignment, ServiceGroup/ServiceType, 11 trạng thái + hold_reason, parent_case_id, CaseDueChange, Vendor/VendorHandoff, Assignment, CaseStatusHistory, ChecklistTemplate…) — chờ họp T3 13/10 chốt; chi tiết `claude/Tuan6_Ke_Hoach_DB_CodeFrame.md` mục 6. Mỗi chủ luồng đề xuất bảng của luồng mình, Trí ghép, đóng băng T6 16/10.

**✅ Cập nhật 2026-09-29 — ERD còn 17 entity (Leader chọn "sửa ERD đầy đủ"):** thêm **IssueType** (Case M—1 IssueType, quan hệ `categorizing`), **RootCause** (Case M—1 RootCause, `explaining`), **SystemSetting** (User/Admin 1—M SystemSetting, `configuring` — cho ADM-07 SLA/rules); **PreventivePlan M—M Asset** (`scheduling`, một kế hoạch áp cho nhiều thiết bị). Đã thay ảnh ERD trong R3 (mục 3.1.5), thêm 3 dòng Entities Description (#15–17), và upload file nguồn **`04_Design/ERD_SmartMaint_v3.drawio`** (Drive fileId `1PrYmlkyhuBIeLyWzbACiZ1vQoBWPyi07`) — file `ERD.drawio` cũ của Leader vẫn giữ nguyên, không ghi đè.

*(Bản cũ, trước 29/09:)* **14 entity** (đã đối chiếu PROJECT_CONTEXT + SCREEN_INVENTORY + catalog `content.json`): Building, **Floor**, Room, AssetCategory, Asset, User, Case (1 Case = 1 Work Order), CaseComment, CaseAttachment, PreventivePlan, InspectionRecord, AuditLog, AIConversation (giữ là entity riêng, không gộp vào AuditLog, vì cần lưu prediction_id/status/rating mà AuditLog dạng text không lưu được), Notification.

**✅ Đã fix 2026-09-28 (Leader xác nhận):** thêm entity **Floor** giữa Building và Room — Building 1—M Floor (quan hệ `having`), Floor 1—M Room (quan hệ `consisting`) — vì AST-01 "Facility Tree" và DLG-15 quản lý tầng là bản ghi riêng (Hùng phát hiện 25/09). Sửa trực tiếp trên file `ERD.drawio` của Leader (Drive `04_Design/`), giữ nguyên toàn bộ bố cục cũ, chỉ chèn Floor vào nhánh bên trái. File mới nằm trong `Report3_drawio_sources.zip` — **Leader cần upload đè lên `04_Design/ERD.drawio`** (AI chưa ghi đè file trên Drive).

---

## 7. NFR (phi chức năng)

- Response < 2s thao tác thường  
- JWT + RBAC  
- Cloud-ready  
- Audit logging  
- Web + Mobile responsive  

*(Bản đầy đủ theo template — External Interfaces, Usability, Reliability, Performance, Security, Maintainability — đã viết ở Report 3 mục 4, dựa trên `NFR_QA.docx` tuần 1 + catalog.)*

---

## 8. Tech stack — ĐÃ CHỐT (2026-09-14 tối; **backend đổi .NET 2026-09-28**)

| Tầng | Lựa chọn cuối | Lý do ngắn gọn |
|---|---|---|
| **Web frontend** | **React + Vite** | App nằm sau login toàn bộ (không cần SEO/SSR) → không cần Next.js; Vite build nhanh, deploy free đơn giản, giảm độ phức tạp không cần thiết cho 1 người code solo tốc độ cao |
| **Mobile** | **Flutter** (deliverable bắt buộc trong đề) | Không đổi |
| **Backend** | **.NET (C#)** — *đổi từ NestJS, Leader xác nhận 2026-09-28 (Report 2 trên Drive đã ghi .NET từ 24/09)* | Lý do chọn .NET: **chưa ghi lại** — cần Leader bổ sung (xem mục 8c, 17) |
| **Database + Storage** | **Supabase** (Postgres + Storage) | Gộp DB và file storage (ảnh evidence từ Case) vào 1 platform free, đỡ setup/quản lý nhiều service riêng lẻ. Backend .NET chỉ connect Postgres qua connection string bình thường; JWT/RBAC vẫn tự code riêng theo đúng NFR đã thiết kế, không phụ thuộc Supabase Auth |
| **Deployment** | **Railway** (host .NET backend) + **Vercel** (host React frontend) | Free tier đủ cho demo/capstone; tách frontend/backend host riêng theo đúng thông lệ |
| **AI** | **Replicate** (model LLM dạng chat — Leader đã có credit nạp sẵn) | Khác OpenAI/Gemini ở chỗ Replicate API mặc định là **async** (tạo prediction → poll hoặc webhook lấy kết quả), không trả kết quả sync ngay lập tức như chat completion thông thường → cần thiết kế backend AI module xử lý polling/webhook thay vì gọi đồng bộ 1 bước |

**Ràng buộc chọn đề của Leader:** tránh phụ thuộc bên thứ ba (công ty thật, payment live, IoT/hardware). Demo bằng **mock/seed data** là đủ.

**Note kiến trúc AI (Replicate, quan trọng):** vì Replicate xử lý prediction bất đồng bộ, module AI-assisted (mục 4b) cần: (1) endpoint tạo request → trả về prediction id ngay; (2) FE poll trạng thái hoặc BE dùng webhook Replicate để cập nhật; (3) audit log ghi cả lúc tạo request và lúc có kết quả. **Đã ghi rõ trong Report 3** (3.1.4 Non-Screen Functions, mục Feature liên quan AI, 4.1).

**✅ Đã sửa 2026-09-28 (chốt bản cuối: giữ tổng 100 man-day, KHÔNG lên 133):** Report 2 dòng Deployment (NestJS → .NET) + text mục 1.2 khớp lại đúng bảng WBS (100). Chi tiết bên dưới.

**Sửa Report 2 (Deployment + effort text) — 2026-09-28:** Tải bản docx thật trên Drive (`03_Reports/`, fileId `1jICczKFKrnZ...`) về, đối chiếu số liệu: bảng WBS trong file thực tế tổng **100** (Auth 8, Asset 5, Case 33, PM 11, Analytics 16, AI 8, Mobile 19) nhưng đoạn văn mục 1.2 lại ghi "133 man-days" — 2 chỗ lệch nhau. **Thử đầu tiên:** khôi phục Auth/Asset về 11/13 (bản gốc 14/09) + tăng AI lên 13, Mobile lên 36 (đúng 2 quyết định 14/09) để bảng khớp đúng 133 — **nhưng Leader phản hồi: không muốn ép effort lên 133, muốn giữ tổng ở mức 100 ngày** (thực tế hơn cho 1 người code chính + AI-assist). **Đã sửa lại theo đúng ý Leader:** GIỮ NGUYÊN bảng WBS gốc (100, không thêm/bớt dòng nào), chỉ sửa đoạn văn mục 1.2 từ "133 man-days" → "100 man-days" cho khớp đúng bảng. Sửa dòng Deployment "Railway (NestJS backend API)" → "Railway (.NET backend API)". Ghi 1 dòng Record of Changes mới (28/09, M, mô tả 2 chỗ sửa: Deployment + khớp lại effort text). Đã validate schema (`validate.py --original` PASS, +4 paragraph) + render PDF kiểm tra cả 2 chỗ bằng mắt — sạch. File cuối đã gửi Leader qua chat — **Leader cần tự upload đè lên `03_Reports/Report2_Project_Management_Plan_SmartMaint.docx`** (AI chưa ghi/ghi đè trực tiếp file trên Drive, theo quy tắc ở mục 13).

⚠️ **Lưu ý quan trọng cho các report sau (R3 SRS effort ref, R4, R5...):** tổng effort chính thức của dự án là **100 man-day** (KHÔNG phải 133 — con số 133 chỉ xuất hiện thoáng qua trong 1 lần sửa rồi bị Leader bác lại cùng ngày 28/09, không dùng nữa). Nếu chỗ nào khác trong Report 1/3 có nhắc "133 man-day" thì cần rà lại và sửa về 100.

### 8c. Lập luận so sánh stack (dùng khi bảo vệ hội đồng) — chốt 2026-09-17, **cần viết lại phần backend**

> ⚠️ **2026-09-28:** backend đã đổi sang **.NET (C#)** → gạch đầu dòng "NestJS" bên dưới **không còn đúng**, giữ lại chỉ để tham khảo lịch sử. Cần viết lại lập luận ".NET (không phải NestJS/Spring Boot/Express)" trước khi bảo vệ.

Để trả lời tốt câu "tại sao chọn cái này mà không phải cái khác" trước hội đồng, logic chung xuyên suốt mọi lựa chọn là 2 tiêu chí: (1) đúng bản chất bài toán, (2) tối ưu cho ràng buộc thật của nhóm (1 người code chính tốc độ cao nhờ AI-assist, 10 tuần nội bộ) chứ không chạy theo công nghệ hot nhất.

- **React + Vite (không phải Next.js/Angular/Vue):** toàn bộ app nằm sau login, không cần SEO/SSR → dùng Next.js là trả thêm phức tạp không cần thiết. Angular nặng convention, hợp team lớn hơn 1 người code solo. Vite nhanh hơn CRA, hệ sinh thái React phổ biến nhất để AI coding assistant generate chính xác.
- ~~**NestJS (không phải Express thuần/Spring Boot/.NET):** cùng TypeScript với frontend → share type/DTO, giảm lỗi lệch kiểu dữ liệu. So Express thuần: NestJS ép kiến trúc modular (module/controller/service/DI) ngay từ đầu, cần thiết khi có 5 role RBAC + nhiều module. So Spring Boot/.NET (quen thuộc hơn ở trường): chậm hơn khi code solo tốc độ cao và không chia sẻ type với FE TypeScript.~~ *(superseded 28/09 — backend = .NET)*
- **Supabase/Postgres (không phải Firebase/MongoDB):** dữ liệu quan hệ chặt (Case–Asset–Room–Floor–Building–User có khóa ngoại rõ ràng) hợp SQL hơn NoSQL; MongoDB phải tự dựng lại referential integrity; Firebase khóa hệ sinh thái Google, khó migrate. Supabase gộp DB + Storage giảm số dịch vụ phải quản lý khi làm một mình. JWT/RBAC vẫn tự code, không dùng Supabase Auth.
- **Railway + Vercel (không phải AWS/Azure/GCP):** free tier đủ demo capstone, tách FE/BE deploy riêng đúng thông lệ. AWS/Azure mạnh hơn nhưng chi phí cấu hình (VPC/IAM/load balancer) không cần thiết vì đề không yêu cầu chứng minh năng lực DevOps hạ tầng lớn.
- **Replicate (không phải OpenAI/Gemini trực tiếp):** ràng buộc thực tế là Leader đã có credit sẵn; về kỹ thuật, Replicate là API bất đồng bộ (prediction + poll/webhook) khác OpenAI/Gemini đồng bộ — đây là điểm có thể chứng minh hiểu đúng kiến trúc async khi trình bày Design, không phải điểm yếu.

*(Ghi chú: chưa có file `docs/tech-stack-rationale.md` riêng trong repo — nếu cần bản đầy đủ hơn để in/nộp kèm SDD thì viết thêm sau.)*

---

## 8b. Figma Design (Web — Case core) — tạo 2026-09-16, nâng cấp thẩm mỹ cùng ngày

**File:** SmartMaint - Web Wireframes (Case Core) — https://www.figma.com/design/j0eLoc9xgwdGCTKlOyrTTI

**Lịch sử:** Bản đầu là wireframe thô (xám, không style). Leader góp ý đúng là còn sơ sài/thiếu thẩm mỹ — đã nâng cấp bằng cách dựng **mini design system** trước (trang "Design System" trong file: color palette lấy cảm hứng Vercel/Linear + màu status semantic cho domain CMMS, type scale Inter, spacing 4px-grid, radius, component mẫu: button/input/status pill có chấm màu/card/sidebar item), rồi re-skin lại các màn theo đúng hệ thống đó — tham khảo layout pattern thực tế của CMMS (UpKeep/Fiix: sidebar nav theo role, dashboard tách task-level vs strategic data, status không chỉ dựa màu mà có icon/label đi kèm) + nguyên tắc thiết kế SaaS tối giản kiểu Linear/Vercel (border mảnh thay vì shadow nặng, nhiều khoảng trắng, 1 màu accent nhất quán).

**Trạng thái Figma (tạm dừng — hết quota):**
- ✅ Đã re-skin: Employee Submit & Track, Officer Case Queue & Assign, Technician My Work Orders
- ⏳ Còn 1 màn (Officer Verify & Close) ở bản wireframe cũ, chưa re-skin
- ❌ **Figma team của Leader đang ở gói Starter, giới hạn chỉ 20 lượt gọi API/tháng (đã dùng hết)** — không dùng tiếp được cho tới khi nâng gói (cần seat Dev/Full, không phải "View") hoặc sang tháng sau reset quota. Không nên cố gọi thêm Figma MCP tool cho tới khi 1 trong 2 điều kiện đó xảy ra.

**Pivot 2026-09-16 (cùng ngày):** Vì bị chặn Figma, và Leader góp ý bản Figma còn thiếu icon/hình ảnh/tính sinh động, đã chuyển sang làm **bản UI prototype bằng HTML** (không phụ thuộc Figma, không giới hạn), có icon SVG thật, avatar, trạng thái có icon+màu, khung upload ảnh, và **có thể bấm chuyển đổi qua lại giữa 4 role ngay trên 1 trang** (Employee / Officer Queue / Technician / Officer Verify) để xem trực quan hơn wireframe tĩnh.

**Link prototype (Artifact, còn hiệu lực lâu dài, xem lại được):** https://claude.ai/artifact/AEkfTMEqFfi4CRmkzHMQSX

**Bản v2 (cùng ngày, sau góp ý "còn chán, thiếu icon/hình ảnh"):** thêm banner giới thiệu có hoạ tiết blueprint mờ (gợi domain facility/building) + caption đổi theo từng role khi bấm tab (giải thích luôn vai trò/vị trí trong workflow — dùng được trực tiếp khi thuyết trình bảo vệ); thêm stat tile đầu mỗi màn (số liệu tóm tắt kiểu dashboard thật); thêm **stepper hiển thị vị trí case trong vòng đời** (Submitted → Approve → Assigned → In Progress → Verified) ngay trong panel chi tiết — vừa đẹp hơn vừa minh hoạ trực quan đúng state machine đã thiết kế ở SRS; ảnh evidence đổi từ ô xám sang tile màu gradient + tên file giả lập.

**Bản v3 (2026-09-16, sau góp ý "vẫn nhìn AI-like, tham khảo web mạnh về UI design"):** Nhận thấy bản v2 (gradient tím/indigo, font Inter/Plus Jakarta, card bo tròn + đổ bóng khắp nơi) đúng là rơi vào đúng các mô-típ "thiết kế AI generic" thường gặp. Đã nghiên cứu lại cách các sản phẩm chuyên nghiệp thật sự thiết kế (Raycast: nền gần đen + 1 màu nhấn duy nhất; Stripe/Datadog/Sentry: màu chỉ dùng cho ý nghĩa trạng thái, không trang trí; Linear: bố cục thoáng, ít viền/chrome; HubSpot: số liệu quan trọng in to, nổi bật) và đổi hẳn hướng thiết kế sang phong cách **"industrial / asset-tag"** — nền tối gần đen làm mặc định (vẫn có bản sáng dự phòng theo hệ điều hành), **1 màu cam an toàn (safety-orange) duy nhất** làm điểm nhấn (hợp chủ đề bảo trì thiết bị, thay cho tím/indigo đại trà), đổi font sang **IBM Plex Sans + IBM Plex Mono** (mono dùng riêng cho mã case/số liệu/nhãn bước — gợi cảm giác "tem thiết bị kỹ thuật"), bỏ banner gradient hero, bỏ hầu hết đổ bóng, đổi trạng thái chọn/active từ nền màu đặc sang **vạch màu mỏng bên trái** (sidebar nav, dòng đang chọn) — phẳng và tiết chế hơn, gần với ngôn ngữ thiết kế của công cụ chuyên nghiệp thật. Đã chụp màn hình kiểm tra cả bản tối và bản sáng trước khi publish, không phát hiện lỗi cần sửa. Đã republish cùng 1 link Artifact (không đổi URL).

**Bản v4 (2026-09-16, sau góp ý "sidebar còn thừa, AI Assistant nên là icon riêng, Profile nên nằm trong dropdown avatar"):** Bỏ 2 mục "AI Assistant" và "Profile" khỏi sidebar (không cần thiết phải chiếm chỗ cố định). AI Assistant chuyển thành **nút icon nổi (floating chat button)** ở góc dưới-phải màn hình, hiện xuyên suốt mọi tab, có nhãn "AI Assistant" hiện ra khi rê chuột — gọn và giống pattern chat-widget quen thuộc hơn. Mục Profile chuyển vào **dropdown menu khi bấm vào avatar/tên user** ở góc trên-phải (kèm thêm mục Log out), sidebar giờ chỉ còn đúng các mục điều hướng chức năng chính theo từng role. Đã kiểm tra tương tác (mở/đóng dropdown, hover icon) bằng script trước khi publish. Đã republish cùng URL Artifact.

**Bản v5 (2026-09-16, mở rộng đủ 5 role + các module còn thiếu):** Sau khi Leader xong "code frame" và chuyển hẳn sang dùng Claude để code, đã mở rộng prototype từ 4 màn Case-core sang **9 màn, đủ 5 role theo RBAC đã thiết kế**:
- Thêm 3 module cho Facility Officer (điều hướng được từ sidebar Officer, đã nối `data-target` thật, không còn là nav-item trang trí): **Assets & Facilities** (danh sách thiết bị/phòng theo building + panel chi tiết có lịch sử bảo trì), **Preventive Maintenance** (danh sách kế hoạch bảo trì định kỳ, hạn tới/quá hạn, nút "Generate work order now"), **Analytics** (dashboard: cases theo status/building bằng bar chart, sparkline thời gian xử lý trung bình 8 tuần, workload từng kỹ thuật viên — màu dùng lại đúng bộ token status/accent sẵn có, không thêm bảng màu mới, theo đúng nguyên tắc "màu chỉ mang nghĩa trạng thái" đã tham khảo từ Stripe/Datadog).
- Thêm 2 tab role mới ở thanh trên: **Manager** (portfolio overview toàn bộ building: chi phí, SLA, case cần duyệt chi phí vượt hạn mức Officer) và **Admin** (quản lý user/role, form mời user, bảng RBAC 5 role tham khảo trực tiếp từ SRS).
- Thanh case-bar trên cùng giờ dùng chung 1 cơ chế điều hướng cho cả tab-trên lẫn nav-item-sidebar (bấm đâu cũng tự cập nhật tiêu đề/mô tả đúng ngữ cảnh).
- Đã kiểm tra bằng script Playwright: click hết 9 màn + module nav-items + dropdown + nút AI, không phát sinh lỗi JS.
Đã republish cùng URL Artifact (v5).

**Phạm vi (đã đủ):** Cả 9 màn / 5 role (Employee, Facility Officer ×5 màn, Technician, Manager, Admin) đã có trên HTML prototype. Mobile vẫn chưa làm (theo kế hoạch, để tuần 7). Bản HTML này dùng làm nguồn hình ảnh chính cho Report 4 (SDD) thay vì Figma, trừ khi sau này nâng gói Figma và muốn đồng bộ lại. *(Lưu ý: prototype v5 có "Manager duyệt chi phí" — catalog/SRS đã chốt Manager chỉ xem, không có duyệt chi phí (BR-12); khi dùng ảnh prototype cho Report 4 cần bỏ phần này.)*

---

## 8d. Repo GitHub & scaffold — bắt đầu 2026-09-17

- Repo **`SmartMaint`** đã tạo trên GitHub, clone local tại `C:\Users\ADMIN\Documents\GitHub\SmartMaint` trên máy Leader (Windows, device `desktop-iduilg2`). VS Code + extension Claude Code đã kết nối được vào repo này (xem Decisions Log 2026-09-17 — lỗi PATH đã fix bằng cách restart VS Code hoàn toàn).
- **2026-09-17:** Đã ghi `README.md` đầu tiên vào repo (qua Claude, ghi trực tiếp lên máy Leader) — gồm: giới thiệu đồ án, bảng nhóm/role, 5 vai trò RBAC, 5 module, bảng tech stack tóm tắt, cấu trúc thư mục dự kiến (`backend/` NestJS theo module domain, `frontend/` React+Vite theo feature, `mobile/` Flutter, `docs/`), và ghi chú trạng thái "đang scaffold, chưa có code thật". **⚠️ README vẫn ghi NestJS → cần sửa sang .NET (C#) (28/09).**
- **⚠️ 12/10:** thư mục `SmartMaint` trong folder Desktop `KLTN_FA26SE147_FacilityMaintenance` chỉ có `.git` rỗng; Decisions Log 16/09 ghi "Leader đã xong code frame" mâu thuẫn với mục này — **cần Leader xác nhận repo thật ở `Documents/GitHub/SmartMaint`** trước khi scaffold.
- Cấu trúc thư mục dự kiến (ghi trong README, sẽ hiện thực hoá khi scaffold code thật — phần backend cần viết lại theo cấu trúc .NET):
  - `backend/src/modules/{auth,user,case,asset,preventive-maintenance,analytics,ai}` + `common/` (guard/decorator/filter dùng chung) + `config/`
  - `frontend/src/features/{case,asset,preventive-maintenance,analytics,admin}` + `shared/` + `lib/`
  - `mobile/` (Flutter, Tùng phụ trách)
  - `docs/` (kiến trúc, ERD, tech-stack rationale)
- **Nguyên tắc tổ chức code đã chốt** (áp dụng ngay từ lúc scaffold, không đợi sau): chia theo domain/feature chứ không theo loại file, tách rõ controller/service/dto trong backend, RBAC làm guard/decorator (hoặc policy/attribute bên .NET) dùng chung thay vì check tay từng chỗ, linter/formatter thống nhất style, có `.env.example` liệt kê đủ biến môi trường — nhằm đảm bảo các tiêu chí chất lượng phần mềm chuẩn ISO/IEC 25010 (Modularity, Reusability, Analysability, Modifiability, Testability) + Readability/Consistency, phục vụ tốt cả phần code thật lẫn phần trình bày SDD lúc bảo vệ. *(Đã đưa vào Report 3 mục 4.2.5 Maintainability.)*
- **12/10 — mô hình code mới:** sở hữu theo luồng (mục 3b); Trí làm **module mẫu (backend) + trang mẫu (web)** trước, các mem tạo module/trang theo mẫu; mọi PR qua Trí review; ai sửa bảng lõi (Case…) phải PR.
- **Còn thiếu / việc tiếp theo:** chưa scaffold code thật (`backend/`, `frontend/` mới chỉ có trong README); chưa có `docs/tech-stack-rationale.md` riêng; chưa setup Supabase project thật; chưa upload README này lên Drive nhóm.

---

## 8e. Report 3 (SRS) — phần Leader + Screen Catalog audit (2026-09-21 → 2026-09-25)

**Screen Catalog (82 → 81 màn):** Ban đầu đã sinh `SmartMaint_Web_Screen_Catalog_EN/VI.docx` (82 màn hình, catalog nguồn ở `content.json`) + `SCREEN_INVENTORY.md` lưu trong project. Khi Hùng làm phần Use Case của mình (Manager/Admin) thì phát hiện **OFF-03 (Case Tracking)** và **MGR-02 (All Cases)** trùng gần như 100% (cùng tabs/filter/cột/phạm vi dữ liệu "mọi case ở mọi trạng thái", chỉ khác Manager không có row-menu hành động) — đúng việc bảng Screen Authorization (3.1.3) vốn đã thiết kế để xử lý (ẩn/hiện hành động theo role), không cần 2 screen ID riêng. **Đã gộp MGR-02 vào OFF-03** trong `content.json` (roles thêm Manager=Read only, purpose/entry/rules/comps ghép nội dung MGR-02 vào, thêm dòng "Manager view"), xóa MGR-02 khỏi sitemap + mọi tham chiếu chéo, build lại 2 file docx, cập nhật `SCREEN_INVENTORY.md`. **Tổng còn 81 màn (58 page + 23 dialog).**

Cùng đợt audit, 2 điểm khác Hùng nêu đã xác nhận **không cần sửa** (đúng thiết kế): (1) "Thanh lý" (Retire) thiết bị khác Delete thật — giữ lại vì Case/PreventivePlan/InspectionRecord đều tham chiếu `asset_id`, xóa thật sẽ mất lịch sử/audit; retire là soft-delete chuẩn CMMS, đã có sẵn trong BR-10. (2) Global Search (COM-06) không trùng OFF-03/MGR-02 — search bar dùng chung khác hẳn trang list đầy đủ.

**Entity Floor:** ✅ đã thêm ngày 28/09 (mục 6b).

**Report3_Leader_Sections.docx (phần Leader tự làm: 1. Product Overview/Context Diagram, 2.1 Actors, 2.2 UC Officer, 3.1.4 Non-Screen Functions, 3.1.5 ERD, 5.1 Business Rules):**
- Đã đối chiếu lại **toàn bộ** với file template gốc `Report3_Software Requirement Specification.docx` (không chỉ theo mục lục — mục lục bị lệch: "Business Rules" thật ở **5.1**, không phải 1.2; mục 1 không có tách 1.1/1.2). Sửa: thêm đoạn mô tả sản phẩm ở mục 1 (template yêu cầu overview trước Context Diagram); bảng "Entities Description" (3.1.5) thêm cột `#` cho khớp mẫu; header bảng UC (2.2.2) sửa `Actor`→`Actors`, `Mô tả`→`Use Case Description`; header Business Rules sửa `Rule ID`→`ID` cho khớp mẫu chữ; UC diagram vẽ lại nối Officer tới **đủ 22** use case (trước chỉ nối 4 đường đại diện).
- **Context Diagram + ERD ban đầu vẽ bằng matplotlib không đúng style bản mẫu trường** (bản mẫu: Context Diagram kiểu DFD — external entity là hình chữ nhật (không phải hình que actor), hệ thống là hình oval, nhiều mũi tên 1 chiều đặt tên cụ thể; ERD kiểu Chen notation — box chỉ ghi tên entity, quan hệ là hình thoi có tên + cardinality 1/M). **Đã vẽ lại cả 2 bằng draw.io** đúng 2 style trên; Leader chỉnh tay tiếp và lưu bản cuối ở Drive `04_Design/Context.drawio` + `04_Design/ERD.drawio` (25/09).
- ⚠️ **Phát hiện 28/09:** file `Report3_Leader_Sections.docx` đã gửi hôm 24/09 thực ra vẫn nhúng **ảnh matplotlib cũ** của Context Diagram và ERD (script vẽ cũ tự chạy lại khi sửa UC diagram và ghi đè ảnh). Không ảnh hưởng nữa vì Report 3 bản đầy đủ (mục 8f) dùng đúng 2 file draw.io của Leader trên Drive — nhưng **đừng dùng lại ảnh trong `Report3_Leader_Sections.docx`**.
- **Bài học quy trình:** từ nay vẽ Context Diagram / ERD / diagram kiểu "phải khớp mẫu trường" thì dùng **draw.io** chứ không tự vẽ bằng matplotlib — matplotlib chỉ dùng cho chart/số liệu thống kê thật (Analytics), không dùng cho diagram kiến trúc/UML cần đúng ký hiệu chuẩn.

---

## 8f. Report 3 (SRS) bản đầy đủ — 2026-09-28 (viết + rút gọn cùng ngày)

**File:** `Report3_Software_Requirement_Specification_SmartMaint.docx` (77 trang, tiếng Anh — bản đầu 138 trang, đã rút gọn cùng ngày, xem bên dưới) + `Report3_drawio_sources.zip` (ERD.drawio có Floor, UC Officer/Manager/Admin, 8 Screens Flow). Làm **trực tiếp trên bản sao template trường** (file `03_Reports/Report3_Software Requirement Specification.docx` trên Drive — bản đã có 3.1.2/3.1.3 nhóm điền), giữ nguyên style/bảng/heading của template, chỉ thay phần hướng dẫn và bảng mẫu. Bìa theo Report 2 ("Ho Chi Minh City, September 2026", Project Code SE147, Group SmartMaint). Mục lục tĩnh (giống template) đã điền số trang thật.

**Phạm vi đã làm (toàn bộ template, không chỉ partial tuần 3 — Leader yêu cầu "làm đầy đủ"):** I. Record of Changes; 1. Product Overview + Context Diagram; 2.1 Actors; 2.2 Use Cases (6 UC diagram + bảng 67 UC); 3.1.1 Screens Flow (8 sơ đồ, phủ đủ 81 màn); 3.1.2 Screen Descriptions (81 màn); 3.1.3 Screen Authorization (81 màn × 5 role); 3.1.4 Non-Screen Functions (8); 3.1.5 ERD (14 entity); **3.2 trở đi theo Feature** (xem phần rút gọn bên dưới); 4.1 External Interfaces, 4.2.1–4.2.5 (Usability, Reliability, Performance, Security, Maintainability); 5.1 Business Rules (BR-01..BR-20); 5.2 Common Requirements (CR-01..12); 5.3 Messages (MSG01..22); 5.4.1 Case Status Life Cycle (12 status); 5.4.2 Notifications (20 mốc).

**Nguồn & review từng phần:**
- **Context Diagram + ERD:** dùng đúng file draw.io của Leader trên Drive (`04_Design/`), render bằng engine draw.io chính thức; ERD chỉ thêm Floor.
- **UC Employee + Technician (Tùng, `02_Tasks/Tuần3`):** giữ nguyên diagram của Tùng (render từ file .drawio của Tùng). Sửa trong bảng mô tả: (1) bảng docx của Tùng bị lệch cột (mô tả rơi vào cột Relationship); (2) UC-EMP-06 Cancel: Tùng ghi "chỉ khi Pending" → catalog cho hủy **trước khi duyệt (Pending hoặc Need more info)**; (3) UC-TEC-04 Reject ghi rõ chỉ trước khi Accept (BR-13).
- **UC Manager + Admin (Hùng, `02_Tasks/Tuần3`):** bản của Hùng là tiếng Việt → dịch sang tiếng Anh và vẽ lại cùng ký hiệu với diagram của Tùng (cho đồng bộ toàn report). **Bổ sung 2 UC Admin còn thiếu** so với catalog: UC-ADM-08 Configure SLA and Rules (ADM-07), UC-ADM-09 View Roles and Permissions (ADM-05). Đổi mã UC-01..17 → UC-MGR-xx / UC-ADM-xx cho thống nhất.
- **UC Officer (Leader):** dịch sang tiếng Anh, vẽ lại cùng ký hiệu; **sửa 2 lỗi lệch catalog**: UC "Thêm/nhập hàng loạt thiết bị" (chỉ Admin — BR-09) → đổi thành "View Asset List and Details"; UC "Đổi trạng thái/thanh lý thiết bị" → chỉ "Change Asset Status" (thanh lý chỉ Admin — BR-10). Thêm UC-OFF-02 View Case Details làm gốc cho các UC «extend» (Approve/Reject/Need info/Assign/Reassign/Cancel/Verify/Reopen) → Officer còn **23 UC**.
- **3.1.2 + 3.1.3 (bản nhóm đã điền trong template trên Drive):** **3.1.3 sai nặng** — gần như mọi dòng chỉ đánh X ở cột Employee (vd. Officer Home, User List, Approve Case đều ghi chỉ Employee có quyền). Cả 2 bảng vẫn là 82 màn (còn MGR-02). → Dựng lại cả 2 từ catalog 81 màn; 3.1.2 giữ cột Feature nhóm đã viết, mô tả lấy từ purpose của catalog; 3.1.3 dùng ký hiệu X / X* (giới hạn) / R (chỉ xem) / P (public) theo roles của catalog.
- **3.1.1 Screens Flow** (Hùng chưa nộp): tự dựng từ 7 sitemap catalog + thêm sơ đồ Sign-in; ký hiệu đúng template (page = chữ nhật, dialog = oval); thay nhãn "Decision dialogs..." bằng các dialog thật để **đủ 81 màn**.
- File `Core_Workflows_SmartMaint_Final.doc` (email tung682004@gmail.com, không thuộc task nào) — **không dùng** theo quyết định Leader.
- Record of Changes: dòng "First version of Screen Descriptions and Screen Authorization" ghi người phụ trách là "SmartMaint team" vì không xác định được ai điền — Leader sửa lại nếu biết.

**Kỹ thuật (để lần sau làm nhanh):** render file .drawio ra PNG bằng `viewer-static.min.js` (lấy từ repo github.com/jgraph/drawio qua sparse checkout) chạy trong Chromium headless (Playwright), gọi thẳng `new Graph()` + `mxCodec.decode` (không dùng GraphViewer vì nó chờ tải MathJax từ mạng bị chặn); file nhiều trang phải tách riêng từng `mxGraphModel` trước khi decode (trùng id giữa các trang). Ghép Word bằng python-docx trên bản sao template (clone bảng mẫu, clone heading), build 2 lượt để lấy số trang cho mục lục. Script ở scratchpad `r3final/` (assemble.py, content_r3.py, render/…).

### Rút gọn mục 3.2 trở đi theo Feature — 2026-09-28 (cùng ngày, sau góp ý review)

Leader gửi 3 góp ý review sau khi nhận bản 138 trang: (1) hỏi UC diagram có cần gộp 1 bản tổng thay vì theo actor không; (2) mục 3.2 trở đi quá chi tiết cho hội đồng đọc; (3) yêu cầu tự audit như hội đồng khó tính.

- **UC diagram:** đọc lại đúng câu hướng dẫn gốc template mục 2.2.1 — *"Provide the UC diagram(s)... You can have multiple UC diagrams for the system"* — template **cho phép nhiều diagram**, không bắt 1 diagram tổng. Với 67 UC, gộp 1 diagram sẽ không đọc được. **Giữ nguyên** 6 diagram (chung + theo actor), không đổi.
- **Mục 3.2 trở đi:** đo lại thực tế — bản 138 trang có 3.2–3.14 (81 màn, mỗi màn 3 bảng con) chiếm **~75% tổng nội dung**. Đối chiếu mẫu gốc: template chỉ show 1 ví dụ (Manage Products) theo cấu trúc **"3.2 <<Feature Name>> → 3.2.x <<Function Name>>"** — nghĩa là viết theo **Feature** (nhóm nghiệp vụ), không phải liệt kê từng màn hình. **Đã viết lại hoàn toàn** theo Feature: gom 81 màn hình (+ 23 dialog lồng vào function cha mở ra nó, không tách heading riêng) thành **16 Feature (3.2–3.17), 43 Function**. Mỗi Function chỉ còn: Function trigger (1 dòng), Function description (2-3 câu), Screen layout (wireframe, chỉ 1 ảnh đại diện/function), Function Details (đoạn văn ngắn 4-8 câu + 3-6 bullet tham chiếu mã BR-xx) — bỏ 3 bảng con, chỉ giữ **1 bảng gọn** cho 4 trường hợp thật sự phức tạp (Case Detail: bảng trạng thái→hành động; Create/Edit User, Create/Edit Asset, Preventive Plan: bảng validate trường). Cuối mỗi Function có dòng "Screens covered: …" để truy vết. **138 → 77 trang.** Đã verify bằng script: cả 81 screen ID đều xuất hiện đủ trong bản mới (không sót màn nào), `validate.py --original master_template.docx` PASS (không phát sinh lỗi schema mới so với bản gốc).
- **Audit hội đồng khó tính:** kiểm tra chéo — 67 UC khớp bảng 2.2.2, 81 màn khớp 3.1.2/3.1.3, 20 Business Rules (BR-01→20) đều định nghĩa đủ ở 5.1 và không rule nào bị tham chiếu mà thiếu định nghĩa, hết sạch từ "NestJS/Node.js" còn sót, Officer về asset đúng chỉ View + Change status, figure numbering 1→16 liền mạch. Không phát hiện lỗi nội dung/số liệu nào khác ngoài vấn đề cấu trúc 3.2 đã sửa ở trên.
- File cũ 138 trang **không còn dùng** — bản 77 trang là bản chính thức mới nhất, đã gửi Leader.
- Nội dung Feature/Function mới (`content_r3_features.py`) và bước lắp ráp mới trong `assemble.py` vẫn ở scratchpad `r3final/` cùng chỗ với bản trước, cùng cách build (`build.sh`, 2 lượt để lấy đúng số trang mục lục).

---

## 8g. Audit chéo + sửa đồng bộ R1/R2/R3/Catalog trên Drive — 2026-09-28 → 2026-09-29

Leader yêu cầu "sửa hết theo thứ tự" sau bản audit; AI sửa **trực tiếp trên file Drive** (Google Docs editor), mỗi bước verify bằng export text + diff.

**Quyết định của Leader trong đợt này:** (1) ERD sửa đầy đủ → 17 entity, PreventivePlan M—M Asset; (2) **Administrator chỉ thấy metadata của sự kiện AI** (ai hỏi, lúc nào, role, trạng thái) — không đọc nội dung câu hỏi/trả lời; (3) UC-GEN: **giữ hình diagram, chỉ sửa chữ**; (4) **gộp MGR-02 vào OFF-03 trên cả catalog VI và EN**; (5) phạm vi R3 thiếu Mobile → **thêm câu phạm vi Mobile (Flutter, 5 role)**.

- **R1** (`03_Reports`, docx `1qMXO-pzM2UowdH6XRVWJFFyTTl6pCGDN`): thêm dòng Record of Changes 28/09 — AI-assisted bắt buộc + RBAC-scoped cho 5 role (Replicate), FE-11 Web+Mobile parity 5 role, thêm Floor vào facility registry.
- **R2** (docx `1jICczKFKrnZ_lygpna0rGFMb5ycsRhd1`): 133 → **100 man-days**; NestJS → **.NET**; "optional AI" → "AI-assisted support"; Building/Floor/Room CRUD; text WBS 7.1/7.2; FE-01..FE-11; danh sách folder Drive thật (00_Admin … 07_Deliverables); "prepaid Replicate credit"; mốc "Report 3 update (supervisor feedback)"; RoC 28/09.
- **R3** (Google Doc `1x78jLp3r0Bml6tL9dSNb_OXlcDMnlcIAvkfqxkEublU`): 63 chỗ sửa (câu phạm vi Mobile, tham chiếu mục 3.2–3.18, metadata AI cho Admin, tham chiếu BR, cửa sổ hủy case, text UC-GEN, messages, lead time, Pass/Fail/N/A, QR…); bảng trạng thái Case Detail sửa ai thao tác được ở Pending / Need more info / Approved-Reopened / Closed; bảng Notifications bổ sung (Employee khi Officer tạo hộ, Reopened/declined, COM-08/TEC-03); 5.4.1 thêm đoạn "Who moves each status"; thêm **MSG23–25**; Entities thêm #15–17; ảnh ERD 17 entity; RoC: xóa dòng trùng "SmartMaint team 25/09", thêm dòng 29/09.
- **Catalog VI** (`1vM9TIJD7ntASezlVKhZdN6rpMthFUT26DN7NltzRY4Q`) + **EN** (`1qA_-vnYxQ1prKEMUDWtZ1Exgk5xOQVRl4ICiPShdPwY`): xóa MGR-02 khỏi danh sách màn, bảng phân quyền (10.7) và phần đặc tả; OFF-03 role "Officer, Manager (read only / chỉ xem)", "Who can open it" = Officer (Full), Manager (Read only), bảng quyền OFF-03 cột Manager = **R**; mọi tham chiếu MGR-02 → OFF-03 (có link bookmark); tổng **81 màn (58 page)**, MGR còn 1 màn.

**Còn tồn (chưa sửa được / cần Leader):**
- Ảnh **sitemap mục 8 của catalog** vẫn vẽ MGR-02 (ảnh tĩnh, không sửa được bằng editor) — cần vẽ lại/thay ảnh.
- **Mục lục R3** có thể lệch số trang sau khi sửa — mở Google Docs cập nhật lại mục lục.
- R3 L803 (phạm vi claim của Officer) — bỏ qua, chưa sửa, chờ Leader xác nhận.
- Catalog EN: link OFF-03 ở dòng "MGR-01, OFF-03 (when opened by the Manager)…" phủ cả cụm "(when opened by the Manager)" — chấp nhận được.
- Ô "R" mới ở bảng quyền OFF-03 (VI+EN) chưa tô màu nền như các ô R khác.

---

## 9. RBL (Research-Based Learning) — đề gợi ý 3 hướng

1. AI-assisted vs tra cứu truyền thống  
2. **Maintenance Case Prioritization** (severity tiên khuyến nghị — dễ viết, ít phụ thuộc AI)  
3. Maintenance Analytics  

Chưa vote chốt — làm cuối tuần 2.

---

## 10. Phương châm làm việc của Leader / nhóm

1. **Bám milestone + template nhà trường (SEP490 Student Guide)** — mọi deliverable map sang Report 1→7.  
2. Hiểu idea trước (cả team đọc FULL đề) — nhưng **không trì hoãn Report** vì "chưa hiểu hết".  
3. Phân tích → SRS → Design; **Code frame bắt buộc ~cuối tuần 5** (theo guide), không đợi tuần 8.  
4. AI bắt buộc (từ 14/09); core là workflow Case.  
5. Cắt scope sớm nếu cần.  
6. **English is official language** cho toàn bộ document CP và buổi bảo vệ (theo Student Guide).

### Nội quy thầy (đã phổ biến) + quy định CP
- Họp team **T2–T7 hằng ngày, nghỉ CN** (đổi từ T3 + T6 ngày 12/10/2026); họp thầy theo lịch; nghỉ 3 buổi không lý do → dropout  
- Mỗi ngày **3–4 tiếng**  
- Leader giao task + track; không ôm việc công khai; chủ động  
- **7 Reports** (không phải 9): Intro, PMP, SRS, SDD, STD, SUG, Final/SP  
- Điểm: OGA (7 reports) 50% + TDA (defense) 50% — trọng số SRS/Design/Test/Impl cao  

### Nhóm — thông tin liên hệ (dùng cho bảng Project Team trong report)

| Full Name | Student ID | Role | Email | Mobile |
|---|---|---|---|---|
| Quách Minh Trí | SE193495 | Leader | minhtritradetf2@gmail.com | 0918179106 |
| Nguyễn Quang Tùng | SE183817 | Member | tungnqse183817@fpt.edu.vn | 0838263176 |
| Nguyễn Võ Mạnh Hùng | SE193568 | Member | usernameabc124@gmail.com | 0902757793 |
| Nguyễn Hoàng Huy | SE193540 | Member | nguyenthang.moc@gmail.com | 0342117105 |

### Phân vai — CẬP NHẬT 12/10/2026: sở hữu theo luồng (thay bảng phân vai cũ)

| Người | Sở hữu | Ghi chú |
|---|---|---|
| **Leader (Trí)** | Lõi dùng chung + **F2** bảo trì định kỳ + **F3** sự cố thông thường + AI gateway; ghép ERD, review mọi PR, ghép 7 report | Rủi ro quá tải: AI gateway có thể chuyển người ở tuần 10 |
| **Tùng** | **F4** khẩn cấp + toàn bộ **Mobile** Flutter | |
| **Hùng** | **F5** sửa hay thay / bên thứ 3 + **F6** giám sát & thống kê; review dữ liệu từ góc KPI | Code thật (Quản lý user, Home theo role…) |
| **Huy** | **F1** cấu trúc tòa nhà, thiết bị, danh mục dịch vụ | 12/10: Huy tự đề nghị nhận nhiều việc và việc khó hơn → nhận việc phân tích + thiết kế + code thật, cùng chuẩn DoD; có checkpoint giữa chừng |

Nguyên tắc chia: ngang bằng **~20 giờ/người/tuần**; mỗi chủ luồng làm đủ chuỗi bảng → API → màn hình → sequence/class (R4) → test case (R5) → BR (R3) và chốt OPEN của luồng mình. *(Bảng phân vai cũ 14/09 — Leader code Web + report, Hùng chỉ review, Huy chỉ test/docs — không dùng nữa.)*

---

## 10b. Tài liệu chính thức nhà trường (QUAN TRỌNG)

**Local path:** `MaterialOfCapStoneProject/1_SEP490/` (bản trên Drive: `MaterialOfCapStoneProject/1_SEP490/`)

| File | Dùng để |
|---|---|
| `SEP490 StudentGuide_Fall 2023.docx` | Quy định + **bảng milestone tuần 1→15** + mô tả 7 reports |
| `Report1_Project Introduction.docx` | Template Report 1 |
| `Report2_Project Management Plan.docx` | Template Report 2 (PMP) |
| `Report2_Sample Project Schedule.pdf/.pod` | Sample schedule (RUP gợi ý, không bắt buộc) |
| `Report3_Software Requirement Specification.docx` | Template Report 3 (SRS) |
| `Report3_Project Tracking.xlsx` | WBS / Issues / Defects / Q&A |
| `Project Weekly Report_GroupName.xlsx` | Weekly report |

⚠️ **Chưa có template Report 4, Report 5** (cả local lẫn Drive) — Tùng đi lấy (card tuần 6).

**12/10 tối — thầy gửi mẫu Final Report nhóm SP25SE136 FreeDriver** (Leader giữ file PDF 209 trang): dùng làm **khung R4/R5** trong lúc chờ template chính thức. Phân tích cấu trúc + đối chiếu card tuần 6 ở `claude/Doi_Chieu_Mau_R4_R5.md`. Tóm tắt: **R4** = 1.1 System Architecture (sơ đồ triển khai, có job nền / mail / storage / log) · 1.2 Package Diagram (web, backend, mobile) + bảng mô tả · 2. Database Design (**ERD vật lý** + Table Descriptions) · 3. Detailed Design (class + sequence theo chức năng). **R5** = Scope · Strategy (loại test, ma trận cấp test, công cụ) · Plan (nhân lực, môi trường, mốc) · Test cases (2 file Excel: Unit Test N/A/B + Test Report UAT) · Test reports.

> Lưu ý: bản guide ghi **Fall 2023** — cấu trúc 7 reports + milestone là chuẩn SEP490; guide này vẫn dùng được cho khóa Fall 2026 (đã xác nhận với Leader 2026-09-14), không cần hỏi lại GVHD về lệch lịch.

### 7 Reports (OGA)
| # | Report | Trọng số OGA |
|---|---|---|
| 1 | Project Introduction | 4% |
| 2 | Project Management Plan (SPMP) | 8% |
| 3 | Software Requirements (SRS) | 16% |
| 4 | Software Design (SDD) | 18% |
| 5 | Testing (STD) | 18% |
| 6 | User Guides (SUG) | 4% |
| 7 | Implementation / Final Product | 32% |

### Milestone nhà trường (chuẩn — ưu tiên hơn lịch nội bộ cũ)

| Tuần | Deliverable chính | Output | Trạng thái |
|---|---|---|---|
| **1** | Project Introduction | **Report 1** | ✅ Done 14/09/2026 |
| **2** | Project Plan | **Report 2** (+ schedule) | ✅ Done sớm 14/09/2026 (due thật 21/09) — ⚠️ còn lệch stack/WBS, xem mục 8 |
| **3** | Overall Requirement | **Report 3 một phần**: Part I + II.1 Product Overview + II.2 User Requirements + II.3.1 System Functional Overview; cập nhật R1/R2; **Project Schedule/Tracking** | ✅ **Report 3 bản đầy đủ 28/09/2026, đã rút gọn 3.2+ theo Feature (77 trang)** — mục 8f. Còn: cập nhật R2, Tracking sheet |
| **5** | Overall Software Design | **Report 4** (System + DB design); **Report 5** (trừ Test Cases/Reports); **Demo code package** (login, register, home, profile, user CRUD frame…); cập nhật R1–3; tracking | ❌ **Trễ** — kế hoạch bù nộp CN 18/10 (`claude/Tuan6_Ke_Hoach_DB_CodeFrame.md`) |
| **7** | Software Package 1 | Code iteration 1 + cập nhật R3–5 (FR, class/seq, UT/IT, defects) | Chưa làm — nội dung: F1 + F3 |
| **9** | Software Package 2 | Tương tự iteration 2 | Chưa làm — F2, F4, F5 |
| **11** | Software Package 3 | Tương tự iteration 3 | Chưa làm — F6, AI |
| **13** | Full Software Package | Full + System Test | Chưa làm |
| **14** | User Guides | **Report 6** | Chưa làm |
| **15** | Final Package | **Report 7** + slides bảo vệ | Chưa làm |

### Tracking bắt buộc (theo guide)
- Schedule: **MS Project hoặc ProjectLibre** (sample trong material) — *2026-09-14: tạm dùng bảng Deliverables đơn giản trong Report 2 thay vì file Gantt riêng, Trello dùng để track task hằng ngày.*  
- Task chi tiết: Trello (đã có) OK — ⚠️ **12/10: board không cập nhật từ 28/09**, các bài vẽ luồng tuần 4–5 không có card  
- Tracking sheet: WBS, Issues, Defects, Q&A (`Report3_Project Tracking.xlsx`) — nên convert Google Sheet share GVHD — **chưa setup thật, cần làm sớm**
- Weekly Report template có sẵn (trên Drive đã có `Project Weekly Report_SmartMaint - FA26SE147`)  

---

## 11. Milestone nội bộ — ĐÃ CHỈNH theo nhà trường (2026-09-10, cập nhật 2026-09-14)

> Lịch "M1 hiểu idea → code tuần 8" **lệch guide**. Giữ phần hiểu idea / WS2 như **nguyên liệu**, nhưng **deadline deliverable theo bảng tuần 1–15 ở mục 10b**.

### Gap / việc tuần hiện tại (Tuần 3)
| Việc | Trạng thái | Action |
|---|---|---|
| Report 1 | ✅ **Done** — file `Report1_Project_Introduction_SmartMaint.docx` | Đã có trên Drive `03_Reports/` |
| Report 2 | ✅ **Done sớm, đã sửa lệch 28/09** — file `Report2_Project_Management_Plan_SmartMaint.docx` | Đã sửa Deployment (NestJS → .NET) + text mục 1.2 khớp lại bảng WBS (giữ tổng **100** man-day, không ép lên 133); Leader upload đè lên Drive |
| Report 3 | ✅ **Bản đầy đủ 28/09, đã rút gọn 77 trang** — xem mục 8f | Leader review, upload Drive `03_Reports/`, nhóm đọc lại phần của mình |
| Code frame | — | Tuần 5 theo guide |
| English docs | ✅ R1 + R2 + R3 đã viết English | R4 trở đi tiếp tục English |
| Tracking + Weekly Report | Chưa setup thật | Setup sớm |
| Repo GitHub `SmartMaint` | 🟡 Đã tạo, đã có README.md (2026-09-17) | Sửa README sang .NET; scaffold code thật |

### WS2 map sang report nhà trường (không bỏ)
| WS2 | Vào report |
|---|---|
| 5 role | R3 – Actors / authorization |
| State machine Case | R3 – UC + business rules + FR Case (5.4.1) |
| Scope V1 IN/OUT | R1 – Scope & Limitations; R2 – scope/estimation |
| NFR | R3 – Quality attributes (4.2) |
| Idea Summary / tóm tắt | R1 – Background / Vision |

### Lộ trình gần (đã align trường)

> **Cập nhật 2026-09-14 (tối):** Mục tiêu nội bộ đổi sang **hoàn thành mức Package 3 (Case + Asset + Preventive + Analytics + AI) sớm ở tuần 10**, thay vì kéo dài đến tuần 13 như khung gốc. Lý do: tránh tình trạng vẫn còn code sát ngày bảo vệ hội đồng (tuần 15) — dồn 5 tuần cuối (11-15) thành buffer thật để test kỹ, ổn định, và tập dượt bảo vệ. **Lịch nộp report chính thức theo trường (R1→R7, các tuần 1/2/3/5/7/9/11/13/14/15) không đổi** — chỉ tiến độ code nội bộ chạy trước lịch nộp.
> **⚠️ 12/10:** tiến độ thực tế đã trễ ~1 tuần (code frame chưa có ở tuần 5) → lộ trình mới theo package/luồng ở `claude/Tuan6_Ke_Hoach_DB_CodeFrame.md` mục 3 (P1 = F1 + F3 tuần 7; P2 = F2 + F4 + F5 tuần 9; P3 = F6 + AI tuần 11). Mốc nội bộ "Package 3 ở tuần 10" khó giữ.

| Tuần | Focus nhóm | Mốc trường |
|---|---|---|
| **1 (Done)** | **Report 1 EN ✅**; **Report 2 EN ✅ (làm sớm)**; setup Drive/Tracking; WS2 Ready nếu đã align | **R1 due hết tuần 1 — đạt** |
| **2 (Done)** | Chạy/chốt WS2; setup Tracking sheet + Weekly Report thật; polish R2 nếu cần; bắt đầu scaffold repo GitHub (README ✅) | **R2 due 21/09 — đạt** |
| **3 (Done)** | **Report 3 bản đầy đủ ✅ (28/09), đã rút gọn 3.2+**; cập nhật R1/R2 | **R3 partial due — đạt (vượt phạm vi)** |
| 4–5 | Review 1 → phân tích lại 6 luồng theo góp ý hội đồng + note thầy | **R4 + code frame due tuần 5 — TRỄ** |
| **6 (đang chạy)** | ERD v4 + code frame + R4 nháp + R5 khung (sở hữu theo luồng) | Bù mốc tuần 5 (CN 18/10) |
| 7 | Package 1: F1 + F3 chạy trên Web (F2 đã vẽ xong 12/10) | **Package 1 tuần 7** |
| 8–9 | F2 bảo trì định kỳ, F4 khẩn cấp, F5 bên thứ 3; mobile Technician | **Package 2 tuần 9** |
| 10–11 | F6 giám sát + AI; mobile Officer | **Package 3 tuần 11** |
| 12–13 | Mobile Manager/Admin; system test | Full (13) |
| 14–15 | User guide + Final + **tập dượt bảo vệ nhiều lần** | R6, R7 |

---

## 12. Trello

| Mục | Giá trị |
|---|---|
| Board | **KLTN** |
| Link | https://trello.com/b/QxXtubvy/kltn |

### Cột (trái → phải)
`Rules & Links` → `Backlog` → `Ready` → `Doing` → `Review` → `Done` → `Blocked`

| Cột | Nghĩa |
|---|---|
| Backlog | Việc đã nghĩ, **chưa** làm tuần này |
| Ready | Việc **được phép** làm tuần này (Leader kéo từ Backlog) |
| Doing | Đang làm (tối đa 1–2 card/người) |
| Review | Xong, chờ check |
| Done | Đã đạt DoD |
| Blocked | Kẹt (ghi lý do) |

**DoD = Definition of Done:** checklist "xong thật" trên mỗi card; thiếu 1 mục = chưa Done.

**Rule:** Việc không có trên Trello = chưa tồn tại. Member không tự lấy từ Backlog.

### Card tuần kickoff (đã chạy)
- 4 card WS1: đọc FULL đề + tóm tắt 6 mục  
- Card Leader: Setup Drive + Idea Summary nhóm  
- Rules & Links: `WorkFlow`

### Card WS2 (Backlog — kéo Ready sau họp T3 align)
| Card | Owner gợi ý | Deliverable |
|---|---|---|
| `WS2 \| Huy \| Bảng 5 role` | Huy | Bảng Role / làm / không làm / app |
| `WS2 \| Tung \| Sơ đồ trạng thái Case` | Tung | State machine + bảng status |
| `WS2 \| Hùng \| Scope V1` | Hùng | IN/OUT theo 5 module + demo + TBD |
| `WS2 \| Mem \| NFR + câu hỏi` | Chốt ở họp | Checklist NFR + list hỏi đã gom |
| `WS2 \| Leader \| Idea Summary nhóm v1` | Leader | Gộp 4 bản + nối deliverable WS2 |

Hướng dẫn chi tiết: Drive `07_Tasks/_Huong_dan/` (file `WS2_Mo_ta_task_chi_tiet.md` + `paste_WS2_*.txt`).

**Lưu ý:** Có 1 card `Tuần 2 | Tùng | Research UX mobile CMMS cho cả 5 role` trong Backlog — nội dung là research 2-3 app CMMS mobile + sketch ý tưởng cho Officer/Manager/Admin mobile, nộp trước T6 19/09. **Đã xác nhận không trùng** với phần Web UI prototype Leader+Claude đã làm (mục 8b) — Tùng làm research/sketch cho mobile-native UX, khác phạm vi với Web desktop prototype dùng tham khảo workflow/RBAC. Không cần comment gì thêm trên card này (Leader đã quyết định 2026-09-16).

**Lưu ý 2026-09-14:** Session Claude hiện đã kết nối MCP Trello thật (đọc/ghi board trực tiếp) — có thể dùng để tạo/kéo card thay vì chỉ soạn text để Leader tự dán. **Không thao tác gì trên Trello trừ khi Leader yêu cầu lại.**

---

## 13. Google Drive (nguồn chính) + local (phụ)

### Quy tắc từ 2026-09-08
- **Nguồn sự thật (source of truth) = Google Drive của nhóm.**  
  Mọi tài liệu, tóm tắt, nộp task, Idea Summary, note họp… làm và lưu trên Drive.
- Folder Desktop local (`KLTN_FA26SE147_FacilityMaintenance`) **chỉ còn mirror/backup cũ** — có thể lệch cấu trúc hoặc thiếu file mới.  
  **AI / Leader khi cần xem dữ liệu mới → mở Drive, không mặc định tin bản local.**
- **AI phải rà hết các folder Drive trước khi làm** (đặc biệt `02_Tasks/TuầnX` — bài nộp của mem, `04_Design` — diagram gốc, `00_Admin/ProblemNote` — câu hỏi của mem). **Không vẽ lại / làm lại thứ đã có trên Drive** (bài học 28/09: suýt vẽ lại Context Diagram trong khi Leader đã có bản chuẩn ở `04_Design`).
- Cấu trúc thư mục trên Drive đã chỉnh lại một chút cho dễ làm việc với member (vẫn giữ tinh thần các mục `00_Admin` … `07_Tasks`).
- **Link Drive nhóm:** https://drive.google.com/drive/folders/10Wsj2DINkE_lM_OpFXHQSrzx07am6J3X?usp=sharing  
  (Nên dán thêm vào card Rules & Links trên Trello nếu chưa có.)
- **Lưu ý riêng cho repo code:** Repo GitHub `SmartMaint` là source of truth **riêng cho code** (không phải Drive) — Drive vẫn là source of truth cho report/tài liệu/note họp. README.md của repo có thể copy 1 bản lên Drive `04_Design/` hoặc `00_Admin/` để tham khảo nhưng bản gốc luôn là bản trong repo.

### Cấu trúc thư mục trên Drive (thực tế 2026-09-28)

```
00_Admin/          — PROJECT_CONTEXT.md, Screen Catalog EN/VI, ScreenInventory, Report3_Task_Assignment, ProblemNote (câu hỏi của mem), Brief 6 luồng (đổi tên "SO LUONG CU" 12/10), Danh_Sach_Luong_Chot (12-10-2026)
01_Idea/           — đề gốc (De_tai_goc_VN.docx)
02_Tasks/          — nộp task theo tuần: Tuần1 (NFR_QA, Role_Matrix, Maintenance_Case_Status, summary…), Tuần2, Tuần3 (UC Tùng, UC Hùng, Leader sections), Tuần 4 (bài vẽ luồng của mem, giữ tên số cũ), tuần 5 (Note Review 1, Ghi chú sau họp = note thầy 08/10)
03_Reports/        — Report 1, Report 2, Report 3 (bản template nhóm đang điền)
04_Design/         — Context.drawio, ERD.drawio, ERD_SmartMaint_v3.drawio (Leader), UC diagram .drawio (Tùng); sơ đồ luồng chốt: Leader upload thư mục Luong_Nghiep_Vu_Chot
05_Refs/           — tham khảo (trống)
06_MeetingNotes/   — note họp (trống)
07_Deliverables/   — bản nộp chính thức / packages (trống)
MaterialOfCapStoneProject/1_SEP490/  — Student Guide + templates
```

**Quy trình nộp task:**  
Upload Drive `02_Tasks/Tuần_XX/` → comment link Trello → Review.

**Bản nộp Report:** lưu `03_Reports/` (và/hoặc `07_Deliverables/`) đúng template trường, **English**.

**Lưu ý AI/Leader:** File local `_LEADER_ONLY_*` không đồng bộ Drive nhóm. Khi viết report: đọc template trong `MaterialOfCapStoneProject/`.

**Cập nhật 2026-09-28:** Session Claude có Google Drive connector (đọc được toàn bộ Drive nhóm).  
**Cập nhật 2026-09-29:** Leader cho AI **sửa trực tiếp** R1, R2, R3 (Google Doc), Catalog VI/EN trên Drive và upload `04_Design/ERD_SmartMaint_v3.drawio` (mục 8g). Không xóa/ghi đè file gốc nào khác.  
**Cập nhật 2026-10-12:** Leader yêu cầu **xóa file luồng cũ** để mem không nhầm — đã chuyển vào thùng rác Drive (khôi phục được 30 ngày): `04_Design/F1_Xu_Ly_Su_Co_Thong_Thuong.drawio`, `02_Tasks/Tuần 4/F1_Trang1.png`, và bản sao cũ ở "Drive của tôi" (F1_Phan_Tich_Chi_Tiet, F1_Trang*.png, Flow_Catalog_v2, 2 bản sao Brief). **Không đụng** bài tuần 4 gốc của mem. Local: xóa `Claude outputs/Tuan4_Dung_Lai/` + file F1 05/10; bản đổi số ở `Claude outputs/Luong_Nghiep_Vu_Chot/`; giữ tạm `Claude outputs/_CU_cho_xoa_sau/` (drawio sự cố 05/10 + nguồn cũ) cho tới khi tìm thấy file F3 bản 12/10.

---

## 14. Agenda họp kickoff T7 (đã chốt)

1. Mở đầu + Trello  
2. Nội quy thầy  
3. Idea overview (~15–20')  
4. Check hiểu (1 hiểu + 1 mơ hồ / người)  
5. Chia việc: cả 4 đọc FULL + tóm tắt 6 mục, deadline trước T3  
6. Setup Drive  
7. Chốt: chưa code  

Họp team: **T2–T7 hằng ngày, nghỉ CN** (từ 12/10/2026; trước đó T3 + T6). Họp ngắn 20 phút (cập nhật Trello trước; xong gì / làm gì / kẹt gì) + buổi dài khi cần chốt (vd. T6 16/10 đóng băng ERD v4, T7 17/10 demo nội bộ).

---

## 15. So sánh nhanh với đề "dễ" khác (lịch sử chọn đề)

Nhóm từng shortlist: InviteMe, Facility Maintenance, GrottoWorks, AI Storytelling.  
InviteMe + Storytelling bị nhóm khác giành → chọn **147 Facility Maintenance**.

Lý do phù hợp Leader:
- Domain gần (ticket bảo trì)  
- Không cần bên thứ ba / hardware  
- AI optional gọi API  
- Pattern CMMS quen thuộc cho SE  

Lưu ý: đề ghi **Flutter mobile là deliverable** — không bỏ qua khi plan dài hạn.

---

## 16. Decisions Log (cập nhật dần)

| Ngày | Quyết định |
|---|---|
| 2026-08/09 | Chọn FA26SE147; đăng ký thành công |
| 2026-09 | Phương châm: hiểu idea → report/diagram → code ~80–90% |
| 2026-09 | Tuần 1: cả team đọc FULL đề, không chia chương |
| 2026-09 | Trello board KLTN + cột workflow chuẩn |
| 2026-09 | Folder Desktop + `07_Tasks` cho nộp bài (giai đoạn setup sớm) |
| **2026-09-08** | **Google Drive = nơi làm việc chính của nhóm;** cấu trúc đưa lên Drive và chỉnh cho mem. Link: https://drive.google.com/drive/folders/10Wsj2DINkE_lM_OpFXHQSrzx07am6J3X?usp=sharing |
| 2026-09 | Card WS2 đã tạo trên Trello Backlog (role / state machine / scope V1 / NFR / Idea Summary Leader); kéo Ready sau T3 align |
| **2026-09-09** | Cấu trúc Drive thực tế: `00_Admin` … `02_Tasks` … `07_Deliverables`. Nộp task tại `02_Tasks/TuầnX`. |
| **2026-09-09** | Lộ trình pre-code nội bộ (sau đó **đã supersede một phần** ngày 10/09 bởi Student Guide). |
| **2026-09-10** | Đọc `MaterialOfCapStoneProject` (SEP490 Student Guide + templates). **Bám milestone tuần 1–15.** Docs nộp = English. |
| **2026-09-14** | **Report 1 hoàn thành** (template SEP490, English) — Group name **SmartMaint**, project code **SE147**. Existing systems benchmark: **UpKeep, Fiix**. |
| **2026-09-14** | **Report 2 hoàn thành sớm** (trước due 1 tuần) — WBS ~111 man-day, risk table, RACI theo phân vai thế mạnh, deliverables schedule tuần 1–15 với ngày cụ thể. |
| **2026-09-14** | Xác nhận: SEP490 Student Guide Fall 2023 dùng được nguyên cho khóa Fall 2026, không cần hỏi lại GVHD. |
| **2026-09-14** | Chốt phân vai công khai: Leader = code Web + report chính; Tùng = Mobile Flutter (từ tuần 7); Hùng = review kỹ thuật/debate; Huy = testing/docs/user guide. *(→ thay bằng sở hữu theo luồng 12/10.)* |
| **2026-09-14** | Thu thập đủ thông tin liên hệ (student ID, email, SĐT) của 4 thành viên để điền báo cáo. |
| **2026-09-14 (tối)** | **Nén timeline nội bộ:** mục tiêu hoàn thành mức Package 3 (Case+Asset+Preventive+Analytics+AI) ở **tuần 10** thay vì tuần 13, để tuần 11-15 là buffer test/ổn định/tập bảo vệ. Lịch nộp report chính thức theo trường không đổi. Mobile trong 10 tuần đầu ưu tiên Employee+Technician+Officer, Manager+Admin dời buffer tuần 11-12. |
| **2026-09-16** | Tạo Figma wireframe đầu tiên cho Web — 4 màn core Case. Leader góp ý đúng: còn sơ sài/thiếu thẩm mỹ, thiếu icon/hình ảnh. Dựng mini design system trong Figma và re-skin được 3/4 màn thì **hết quota Figma MCP (gói Starter, 20 lượt/tháng)**. Leader chọn hướng: chuyển sang làm **UI prototype bằng HTML** (icon SVG thật, avatar, chuyển role bằng tab bấm được) — không phụ thuộc Figma. Dùng bản này làm nguồn chính cho Report 4. Link + chi tiết ở mục 8b. |
| **2026-09-16** | HTML prototype v2: sau góp ý "còn chán, thiếu icon/hình ảnh" — thêm stat tile, workflow stepper (Submitted→Approve→Assigned→In Progress→Verified), banner blueprint-pattern có caption đổi theo role, ảnh evidence dạng tile màu + tên file. Chi tiết mục 8b. |
| **2026-09-16** | HTML prototype v3: sau góp ý "vẫn AI-like, tham khảo web mạnh về UI design" — đổi hẳn hướng thiết kế sang "industrial/asset-tag": nền tối gần đen + 1 màu cam an toàn duy nhất, font IBM Plex Sans/Mono, bỏ gradient hero + đổ bóng, trạng thái chọn dùng vạch màu mỏng thay vì nền màu đặc. Tham khảo Raycast/Stripe/Datadog/Linear/HubSpot. Đã republish cùng URL Artifact. Chi tiết mục 8b. |
| **2026-09-16** | HTML prototype v4: sau góp ý "sidebar còn thừa" — bỏ mục AI Assistant + Profile khỏi sidebar; AI Assistant chuyển thành nút icon nổi góc màn hình (floating chat button); Profile chuyển vào dropdown khi bấm avatar/tên user (kèm Log out). Đã republish cùng URL Artifact. Chi tiết mục 8b. |
| **2026-09-16** | **Thay đổi quy trình quan trọng:** Leader đã xong "code frame" (login/register/home/profile/user CRUD) và **không dùng Cursor nữa** — từ giờ dùng **Claude (phiên này) để code trực tiếp** phần còn lại (Case core, Asset, Preventive Maintenance, Analytics, AI...), không chỉ hỗ trợ report/design như trước. Cần bám sát PROJECT_CONTEXT + kế hoạch nén tuần 5-10 khi code. *(⚠️ 12/10: chưa xác minh được code frame này — repo Desktop rỗng, xem mục 8d.)* |
| **2026-09-16** | HTML prototype v5: mở rộng từ 4 màn Case-core lên **9 màn, đủ 5 role** — thêm Assets & Facilities, Preventive Maintenance, Analytics (module trong sidebar Officer, đã nối điều hướng thật) + 2 tab role mới Manager (portfolio overview, duyệt chi phí vượt hạn mức) và Admin (user/role management, bảng RBAC). Đã kiểm tra bằng Playwright, không lỗi JS. Đã republish cùng URL Artifact. Chi tiết mục 8b. |
| **2026-09-14 (tối, muộn)** | **Chốt tech stack cuối cùng:** Web = React+Vite; Backend = NestJS (Node.js+TS) *(→ đổi .NET ngày 28/09)*; DB+Storage = Supabase; Deployment = Railway (backend) + Vercel (frontend); AI = Replicate (model LLM chat, đã có credit). Lưu ý kiến trúc: Replicate API là async (prediction + poll/webhook), khác OpenAI/Gemini — cần note trong SRS/Design. Đã cập nhật Report 2 Tools & Infrastructure. |
| **2026-09-14 (chiều)** | **Đổi scope quan trọng:** AI-assisted Support từ optional → **BẮT BUỘC**, hiển thị **cả 5 role** (không chỉ Technician/Officer), mỗi role bị giới hạn theo RBAC hiện có (xem mục 4b AI Access Rules). **Mobile app** từ "chỉ Employee+Technician" → **full parity cả 5 role trên cả Web và Mobile**. Đã sửa lại Report 1 (FE-10, FE-11 mới, LI-03) và Report 2 (risk table cập nhật impact) cho khớp. *(Lưu ý 28/09: có một lần thử tăng WBS lên 133 man-day để phản ánh scope AI+Mobile mở rộng, nhưng Leader chốt giữ nguyên tổng effort ở 100 man-day — xem mục 8f/16 ngày 28/09 — nên WBS KHÔNG đổi so với bản gốc 100.)* |
| **2026-09-17** | **Fix kết nối VS Code Claude Code extension:** lỗi "Could not locate Claude CLI on PATH" khi mở repo `SmartMaint`. Kiểm tra thật trên máy: User PATH đã có sẵn cả `.local\bin` và `AppData\Roaming\npm`, không thiếu gì. Nguyên nhân thật là process VS Code cũ bị stale — fix bằng cách **đóng hẳn toàn bộ VS Code rồi mở lại** (không cần restart Windows). Đã verify trực tiếp: mở lại VS Code, mở đúng repo `SmartMaint` qua File → Open Recent, extension load thành công, không lỗi. |
| **2026-09-17** | Đọc lại Trello, xác nhận card `Tùng | Research UX mobile CMMS` không trùng phạm vi với Web UI prototype (mục 8b) — không cần comment gì thêm (Leader quyết định giữ nguyên, không sửa card). |
| **2026-09-17** | **Chốt lập luận so sánh tech stack dùng cho bảo vệ hội đồng** — chi tiết mục 8c (React+Vite vs Next.js/Angular, NestJS vs Express/Spring/.NET, Supabase vs Firebase/MongoDB, Railway+Vercel vs AWS/Azure, Replicate vs OpenAI/Gemini trực tiếp). *(Phần NestJS superseded 28/09.)* |
| **2026-09-17** | **Bắt đầu scaffold repo GitHub `SmartMaint`:** đã ghi `README.md` đầu tiên (giới thiệu đồ án, nhóm, role, module, tech stack, cấu trúc thư mục dự kiến). Repo tại `C:\Users\ADMIN\Documents\GitHub\SmartMaint` trên máy Leader. Chi tiết mục 8d. Code skeleton thật (backend/frontend) chưa làm, sẽ làm tiếp theo. |
| **2026-09-21** | Sinh Screen Catalog EN/VI (82 màn hình) từ `content.json` + `SCREEN_INVENTORY.md` lưu vào project. Bàn cách chia việc Report 3 giữa 4 người, viết `Report3_Task_Assignment.md` (đã đối chiếu lại template thật, sửa "Business Rules" từ 1.2 → đúng 5.1). |
| **2026-09-24** | **Audit + sửa Report3_Leader_Sections.docx theo đúng template gốc**: thêm đoạn Product Overview, sửa header bảng ERD/UC/Business Rules cho khớp mẫu, vẽ lại UC diagram nối đủ 22 use case tới Officer. Phát hiện Context Diagram/ERD vẽ bằng matplotlib sai style bản mẫu trường → **vẽ lại bằng draw.io** đúng style (Context Diagram dạng DFD với external entity hình chữ nhật + nhiều luồng dữ liệu đặt tên cụ thể; ERD dạng Chen notation với hình thoi quan hệ + cardinality). Chi tiết mục 8e. |
| **2026-09-25** | **Audit catalog theo góp ý của Hùng:** xác nhận "Retire" thiết bị và Global Search (COM-06) đúng thiết kế, không cần sửa; phát hiện **OFF-03 và MGR-02 trùng nhau** → gộp MGR-02 vào OFF-03 trong `content.json`, build lại 2 file catalog docx, cập nhật `SCREEN_INVENTORY.md`, còn **81 màn**. Phát hiện thêm entity **Floor** còn thiếu trong ERD (Building→Floor→Room, không phải Building→Room). Chi tiết mục 8e/6b. |
| **2026-09-28** | **Backend đổi sang .NET (C#)** (Leader xác nhận khi hỏi về chỗ lệch trong Report 2). Cần: sửa dòng Deployment + WBS trong Report 2, sửa README repo, viết lại lập luận 8c. |
| **2026-09-28** | **Thêm entity Floor vào ERD** (Building 1—M Floor `having`, Floor 1—M Room `consisting`), sửa trên file `ERD.drawio` của Leader, giữ nguyên bố cục. ERD còn **14 entity**. |
| **2026-09-28** | **Hoàn thiện Report 3 (SRS) bản đầy đủ** — làm trên bản sao template trường, tiếng Anh, 138 trang, đủ mọi mục (kể cả 3.2–3.14 chi tiết 81 màn + 8 non-screen function, NFR, Messages, Case life cycle, Notifications). Gộp phần của Tùng (UC Employee/Technician), Hùng (UC Manager/Admin), phần nhóm đã điền 3.1.2/3.1.3 và phần Leader; tự làm các phần còn thiếu (Screens Flow…). Review sửa lỗi: bảng 3.1.3 cũ sai quyền gần như toàn bộ; UC Officer có 2 UC sai quyền (nhập/thanh lý thiết bị là việc của Admin); UC Hùng thiếu 2 UC Admin (SLA, Roles reference); bảng UC của Tùng lệch cột + điều kiện Cancel sai. Không dùng `Core_Workflows_SmartMaint_Final.doc`. Chi tiết mục 8f. |
| **2026-09-28 (sau, cùng ngày)** | **Rút gọn Report 3 mục 3.2 trở đi theo Feature** sau góp ý review của Leader (3 điểm: UC diagram tổng/actor, mục 3.2 quá chi tiết, tự audit hội đồng khó tính). Xác nhận UC diagram theo actor là đúng chuẩn template (giữ nguyên). Viết lại 3.2+ theo cấu trúc Feature/Function đúng mẫu trường: 81 màn + 23 dialog gom thành **16 Feature, 43 Function**; bỏ 3 bảng con/màn, chỉ giữ mô tả ngắn + 1 bảng gọn cho 4 chỗ thật sự phức tạp. **138 → 77 trang.** Verify đủ 81 màn, validate.py PASS. Chi tiết mục 8f. |
| ✅ 2026-09-28 | Tách Approved và Assigned thành 2 bước (theo catalog; BR-20) |
| ✅ 2026-09-28 | Tên status cuối cùng: 12 status theo catalog (mục 5, Report 3 mục 5.4.1) |
| ✅ 2026-09-14 (tối, muộn) → 2026-09-28 | Stack cuối: React+Vite / **.NET (C#)** / Supabase (DB+Storage) / Railway+Vercel / AI=Replicate |
| ✅ 2026-09-28 | Thêm entity Floor vào ERD (Building→Floor→Room) |
| ✅ 2026-09-28 | Rút gọn Report 3 mục 3.2+ theo Feature (138→77 trang) |
| ✅ 2026-09-28 | Sửa Report 2: Deployment NestJS→.NET; thử tăng WBS lên 133 rồi Leader bác, **chốt cuối giữ nguyên tổng 100 man-day**, chỉ sửa text mục 1.2 khớp lại 100 |
| **2026-09-28 → 29** | **Audit chéo + sửa đồng bộ trên Drive** (mục 8g): R1/R2 khớp scope mới + RoC; R3 sửa 63 chỗ, thêm phạm vi Mobile, MSG23–25, ERD 17 entity (thêm IssueType, RootCause, SystemSetting; PreventivePlan M—M Asset); Admin chỉ xem metadata sự kiện AI; UC-GEN giữ hình sửa chữ; Catalog VI+EN gộp MGR-02 vào OFF-03 (81 màn); upload ERD_SmartMaint_v3.drawio. |
| **2026-09-30** | **Lập Scrum Product Backlog (Excel)** `SmartMaint_Scrum_Backlog.xlsx`: 71 module (43 function R3 mục 3.2–3.17 + 8 NSF 3.18 + hạ tầng + Mobile), tiếng Việt, man-day tổng **100** khớp từng dòng WBS R2, 5 sprint 2 tuần: S1 t4–5 Code Frame (29/09–12/10), S2 t6–7 Package 1 Case core, S3 t8–9 Package 2 PM + asset + Mobile Emp/Tech, S4 t10–11 Package 3 Analytics + AI + Mobile Officer, S5 t12–13 Mobile Manager/Admin + system test. Cột Owner để trống (phân vai sau). File lưu local `00_Admin/` + gửi chat; **chưa lên Drive** (Leader tự upload vào `00_Admin`). |
| **2026-10-01 → 03** | **Sau Review 1 (nhóm chưa trả lời được các câu chất vấn nghiệp vụ):** hội đồng góp ý 7 điểm, đánh số **G1–G7** (note Drive `1T7BG3C_yqMHy_RCYix3-grxcn5di2_Ks`): G1 bối cảnh tòa nhà văn phòng; G2 khẩn cấp / mai cần dùng / mượn; G3 PM theo loại máy, tránh vừa sửa lại kiểm tra; G4 sửa hay thay; G5 lịch sử bảo trì; G6 category + template; G7 thời gian thao tác, đúng lịch, "các lịch". Thầy GVHD: cần **5–6 luồng chính, mỗi luồng > 1 actor**. Chốt **6 luồng** (số theo brief, **đã đổi 12/10 — xem dòng 12/10 (3)**): F1 sự cố thường (Trí), F2 khẩn cấp (Tùng), F3 bảo trì định kỳ (Trí), F4 sửa hay thay (Hùng), F5 vòng đời thiết bị + category (Huy), F6 giám sát hiệu suất & đúng lịch (Hùng). Brief nhóm `Brief_Nhom_Phan_Tich_6_Luong_SmartMaint` (quy tắc vẽ swimlane, 7 câu hỏi/bước, màu xanh lá/xanh dương/cam/đỏ, bảng G1–G7 → luồng/bước) + bộ hình mẫu `Template_Ve_Luong` (4 hình + .drawio). Chưa sửa R3/Catalog cho tới khi họp chốt Decision Log. |
| **2026-10-05** | **Luồng sự cố thông thường (brief F1) vẽ xong v2**: 4 trang, kiểm tra tự động + 2 vòng phản biện. Phát hiện mâu thuẫn R3 M1–M4; 22 OPEN. *(Đã bị thay bởi F3 v3 ngày 12/10; file 05/10 đã xóa.)* |
| **2026-10-12** | **Thầy góp ý scope sau khi xem phân tích sâu (note 12/10, đánh mã N1–N26):** "đi đúng hướng nhưng lố vài chỗ". Leader chốt các chỗ mơ hồ: phạm vi **cả văn phòng cho thuê và chung cư**; "nhóm case" = **gộp các case cùng một lỗi** vào case chính; báo giá bên thứ 3 = **chỉ chuyển cho khách xem**, không duyệt / không thanh toán; **Officer phụ trách theo tòa, bỏ hàng chờ chung**. Research có URL (CSVC văn phòng + chung cư theo Luật Nhà ở 2023 / TT 05/2024; 4 mức khẩn cấp; danh mục dịch vụ; On Hold; bên thứ 3; gộp case; phân công theo tòa) và **vẽ lại luồng sự cố thông thường v3** (nay là **F3**), 5 trang, 11 trạng thái, 17 OPEN (O01–O17). Bảng phải sửa R3 ở mục 7 tài liệu. **Chưa sửa R1/R3.** Chi tiết: `claude/F3_Su_Co_Thong_Thuong.md`. |
| **2026-10-12 (2)** | **Đối chiếu bài nộp tuần 4 và dựng lại 4 luồng** (nay là **F1** thiết bị + danh mục từ bài Huy, **F4** khẩn cấp từ bài Tùng, **F5** sửa hay thay từ bài Hùng, **F6** giám sát từ bài Hùng; không sửa file gốc của mem). Kiểm tra tự động 0 lỗi + 1 vòng phản biện (~60 điểm). OPEN: A01–A07, K01–K08, R01–R04, S01–S05. **Chờ Leader duyệt:** F5 làm nguồn duy nhất cho bên thứ 3 / thay thiết bị (F3 Trang 3 rút thành → F5). BR phải sửa: BR-01, BR-02, BR-08, BR-19 + BR mới. Chi tiết: `claude/Doi_Chieu_Tuan4_F1_F4_F5_F6.md`. |
| **2026-10-12 (3)** | **Chốt số luồng theo đúng thứ tự note thầy** (luồng 1 cấu trúc tòa nhà → 2 bảo trì định kỳ → 3 xử lý sự cố → 4 thống kê): **F1** cấu trúc tòa nhà/thiết bị/danh mục (Huy) · **F2** bảo trì định kỳ (**Trí** — Leader chốt) · **F3** sự cố thông thường (Trí) · **F4** khẩn cấp (Tùng) · **F5** sửa hay thay/bên thứ 3 (Hùng) · **F6** giám sát & thống kê (Hùng). Phát hiện: tài liệu sáng 12/10 hiểu "luồng 2, 3 lồng AI" là sự cố thường + khẩn cấp → sai thứ tự thầy; đúng là **bảo trì định kỳ + xử lý sự cố** → F2 phải có AI. Đã: đổi số trong 4 sơ đồ (sinh lại từ spec, kiểm tra tự động 0 lỗi, render lại PNG) → `Claude outputs/Luong_Nghiep_Vu_Chot/`; đổi tên/viết lại tài liệu project (`Danh_Sach_Luong_Chot.md`, `F3_Su_Co_Thong_Thuong.md`, `Doi_Chieu_Tuan4_F1_F4_F5_F6.md`), xóa bản cũ; xóa file luồng cũ local + chuyển thùng rác Drive (mục 13); tạo Google Doc `00_Admin/Danh_Sach_Luong_Chot (12-10-2026)`; đổi tên Brief trên Drive thành "(SO LUONG CU - xem Danh_Sach_Luong_Chot)". **Chưa tìm thấy** file sơ đồ F3 bản 12/10 (`F2_Xu_Ly_Su_Co_Thong_Thuong.drawio`) — Leader cần tìm và đặt vào folder để đổi số. |
| **2026-10-12 (3)** | **Mô hình làm việc mới:** (1) **họp nhóm T2–T7 hằng ngày, nghỉ CN**; (2) **chia task ngang bằng ~20 giờ/người/tuần**, Huy tự đề nghị nhận việc nhiều và khó hơn → nhận việc phân tích + thiết kế + code thật, cùng DoD; (3) **sở hữu theo luồng**: mỗi mem thiết kế (bảng, API, sequence, test, BR) + code đúng luồng mình; **Trí giữ lõi dùng chung**, làm module mẫu + trang mẫu, review mọi PR; Tùng giữ toàn bộ Mobile. Kế hoạch tuần 6 (bù mốc tuần 5, nộp CN 18/10) + 16 quyết định schema D1–D16 (chờ họp T3 13/10): `claude/Tuan6_Ke_Hoach_DB_CodeFrame.md`. |
| **2026-10-12 (4)** | **Tạo 21 card tuần 6 trên Trello** (cột Ready, theo `claude/Tuan6_Ke_Hoach_DB_CodeFrame.md` v4): Trí 6, Huy 5, Tùng 5, Hùng 5; tên card dạng `[T6] <luồng> \| <người> \| <việc>`, hạn 21h giờ VN theo ngày trong kế hoạch, mỗi card có mô tả + checklist DoD. Connector Trello không cho gán thành viên vào card / checklist → mỗi người tự bấm Join card của mình. Card "Rules & Links → WorkFlow" vẫn ghi họp T3 + T6 (chưa sửa). |
| **2026-10-12 (5)** | **Đủ sơ đồ 6 luồng + chốt F5 nguồn duy nhất.** (1) File `F1_Xu_Ly_Su_Co_Thong_Thuong.drawio` Leader bỏ vào folder Claude outputs là **bản 05/10** (12 trạng thái, Employee, Need more info) → **dựng lại F3 v3.1** từ tài liệu phân tích: 5 trang, 11 trạng thái, kiểm tra tự động 0 lỗi (85 nút, 16 hình thoi). (2) **Leader chọn F5 là nguồn duy nhất** cho bảo hành / bên thứ 3 / báo giá / thay thiết bị: F3 Trang 3 chỉ còn Officer phân loại lại phạm vi sau khảo sát → Hủy "Ngoài phạm vi tòa nhà" / Technician tiếp tục / → F5; O11–O13 chuyển F5 (O12 = R02, O13 = R04). F4 chỉ ghi nhận gọi bên thứ 3 trên case. Sửa chữ ở F4, F5 và chạy lại. (3) **Dựng mới F2 bảo trì định kỳ** (Trí): 4 trang — lập kế hoạch (mẫu checklist theo loại thiết bị G6, lịch cố định / trượt, chặn chu kỳ quá quy chuẩn với thiết bị bắt buộc kiểm định, người làm nội bộ / bên thứ 3 có hợp đồng, lead time, cửa sổ đúng hạn ± 10%), đến kỳ (job ngày; thiết bị thanh lý / đang sửa / lệnh kỳ trước còn mở / vừa sửa ≤ N ngày → Officer giữ / dời / bỏ — G3; không bỏ được kỳ kiểm định bắt buộc), thực hiện (checklist Đạt / Không đạt / N/A; Không đạt → sửa ngay + ghi nhận N21 hoặc tạo case BR-07 → F3 / F4), nghiệm thu, phiếu kiểm tra + cờ đúng hạn / trễ → F6; trạng thái kế hoạch Đang chạy / Tạm dừng / Kết thúc; lệnh không có Pending / Rejected. 3 ô AI; OPEN **P01–P09** (gửi thầy P03, P04). Kiểm tra 0 lỗi (71 nút). Tự phản biện đã sửa: sinh trùng lệnh khi kỳ trước còn mở, đồng hồ trễ chỉ gắn In Progress, thiếu Hủy lệnh. (4) Cập nhật `Danh_Sach_Luong_Chot`, `Doi_Chieu` mục 6 (9/11 mâu thuẫn liên luồng đã xử lý), `F3_Su_Co_Thong_Thuong.md` v3.1, kế hoạch tuần 6 (thêm bảng `PlanOccurrence`). Xóa trên máy Leader: `_CU_cho_xoa_sau/` và file F3 bản 05/10. |
| **2026-10-12 (6)** | **Đối chiếu mẫu R4/R5 (thầy gửi, nhóm SP25SE136) với 21 card tuần 6** (`claude/Doi_Chieu_Mau_R4_R5.md`): phần code frame, data dictionary, R5 khung hợp lý; **thiếu cho R4**: (1) không card nào làm **Package Diagram** web/backend/mobile; (2) card ERD ghi "ký hiệu Chen" trong khi R4 cần **ERD vật lý**; (3) **data dictionary bảng lõi + F2 + F3 của Trí không có card**; (4) kiến trúc thiếu job nền, email, push, storage, logging; (5) unit test (N/A/B) chưa nằm trong DoD card code; DB script cần thêm Data.sql. 11 đề xuất sửa card → **Leader đồng ý, đã sửa Trello 20h45**: 10 card sửa mô tả + DoD, thêm 1 card (Hùng – Package Diagram Backend + Web); tổng card tuần 6 = **22**; giờ Trí ~24, Hùng 22, Huy 20, Tùng 20. 21h50: viết lại card D1–D16 cho dễ hiểu (ví dụ "khu chung cư": Trí giữ bảng hành lang user/case…, mỗi người xây bảng luồng mình và nối bằng cột *_id; danh sách "cửa" ai trỏ vào đâu; 16 câu hỏi chia theo tên người). |
| TBD | RBL topic 1/2/3 |
| TBD | Scope cắt V1 chính thức (mỗi chủ luồng đề xuất Must/Should/Could tuần 7) |
| TBD | Lý do chọn .NET (viết lại mục 8c) |
| TBD | D1–D16 schema ERD v4 (họp T3 13/10) |

---

## 17. Open questions (hỏi thầy / chốt nhóm)

- **(12/10, đợt 2 — 6 câu gửi thầy, xem `claude/Doi_Chieu_Tuan4_F1_F4_F5_F6.md` mục 7):** K02/K05 lịch trực ngoài giờ; F4 đếm 24/7 + mức do loại dịch vụ (sửa BR-01, BR-08); R02 = O12 On Hold hay In Progress; K06 hai mốc khống chế / khắc phục; A06/S05 kênh Officer/Manager → Admin; S03 định nghĩa "đúng lịch".

- AI có bắt buộc demo không?  
- Preventive Maintenance depth tối thiểu?  
- Report 1–2–3 sample FLM map nội dung nào? (folder Samples_FLM hiện đang trống trên local)  
- **(28/09) Hùng hỏi trong ProblemNote: "Trang ADM-07 Cài đặt SLA và quy tắc là sao"** — chưa ai trả lời. Gợi ý trả lời: ADM-07 là nơi Admin chỉnh các con số hệ thống dùng trong rule mà không cần sửa code: thời gian SLA theo priority (BR-08), thời gian tự nhả claim 30 phút + tối đa 5 claim (BR-02), cửa sổ báo "Chưa sửa xong" 7 ngày (BR-03), giới hạn AI 20 câu/giờ (BR-15); mọi thay đổi ghi Audit Log.
- **(28/09) Context Diagram chưa có Email service** trong khi catalog/Report 3 có gửi email (tạo tài khoản, reset mật khẩu, thông báo case). Có thêm external entity "Email Service" vào `Context.drawio` không? (Chưa sửa vì là diagram Leader đã chốt.)
- **(28/09) Record of Changes Report 3:** ai là người điền bản đầu 3.1.2/3.1.3 trong template trên Drive? (đang ghi "SmartMaint team").
- **(28/09) Default SLA/password/giới hạn ảnh…** là số mặc định trong catalog (đánh dấu "cần xác nhận với thầy") — đã đưa vào Business Rules; nếu thầy yêu cầu số khác thì sửa ADM-07 + BR.
- **(12/10) Luồng F3 v3: 17 OPEN O01–O17** ở `claude/F3_Su_Co_Thong_Thuong.md` mục 6 (4 câu gửi thầy trước: O12, O13, O04, O02; O11–O13 nay thuộc F5).
- **(12/10 tối) Luồng F2: 9 OPEN P01–P09** ở `claude/F2_Bao_Tri_Dinh_Ky.md` mục 6 (gửi thầy P04 = G3 "vừa sửa bao nhiêu ngày thì dời kỳ", P03 danh sách thiết bị bắt buộc kiểm định + văn bản PCCC hiện hành).

---

## 18. Hướng dẫn dùng file này với AI

Khi mở chat mới, gửi kèm hoặc nhắc:

> "Đọc `PROJECT_CONTEXT.md` (bản mới nhất trên Google Drive `00_Admin/`, hoặc local nếu vừa sync) rồi tiếp tục theo đúng context đó. Tài liệu nhóm xem trên Drive."

**Lưu ý cho AI:**
- Ưu tiên đọc / đối chiếu file trên **Google Drive** khi cần nội dung mới của mem (tóm tắt, nộp task, note họp). **Trước khi làm bất kỳ phần nào, rà hết các folder Drive** (liệt kê cả trang tiếp theo của kết quả tìm kiếm) để không làm lại thứ đã có.
- Bản local Desktop có thể **cũ hơn Drive** — đừng giả định local luôn đúng.
- Repo GitHub `SmartMaint` là nguồn code riêng (khác Drive) — xem mục 8d trước khi code hoặc scaffold thêm. **Backend = .NET (C#)**.
- **Số luồng: chỉ dùng số chốt 12/10 (mục 3b).** Gặp tài liệu cũ mang số khác thì tra `claude/Danh_Sach_Luong_Chot.md`; khi đổi số không dùng thay thế mù trên tên file gốc của mem.
- Trao đổi với Leader bằng **tiếng Việt có dấu**.
- Khi hỗ trợ **report / milestone / dual-track**: đọc `MaterialOfCapStoneProject/` (Student Guide + templates) và `00_Admin/_LEADER_ONLY_PreCode_Plan.md`. Không upload `_LEADER_ONLY_*` lên Drive nhóm.
- Mọi đề xuất lịch phải **ưu tiên bảng milestone Student Guide** hơn lịch nội bộ cũ.
- Report 1 & Report 2 đã có bản hoàn chỉnh (2026-09-14) — khi làm Report 3 trở đi, giữ nhất quán số liệu/tên gọi đã dùng trong 2 file này (Group name SmartMaint, existing systems UpKeep/Fiix, RACI theo phân vai thế mạnh, WBS effort).
- **Làm report thì copy file template trường rồi điền vào** (không tự thiết kế file mới từ đầu) — Leader yêu cầu 28/09.
- **Diagram cần đúng ký hiệu chuẩn (Context Diagram, ERD, UML...) → dùng draw.io**, không tự vẽ bằng matplotlib/code. Nếu nhóm đã có file .drawio trên Drive thì **dùng/sửa đúng file đó**, không vẽ lại. Render .drawio ra PNG theo cách ghi ở mục 8f. Matplotlib chỉ dùng cho chart số liệu thống kê thật.
- Review bài của mem **đối chiếu với catalog `content.json` + Business Rules** (roles từng màn, rule) — sai thì sửa và ghi rõ đã sửa gì (mục 8f là mẫu).
- **Phần 3.2 trở đi của SRS viết theo Feature** (không phải theo từng màn hình) — đúng cấu trúc mẫu trường; xem cách làm/tỉ lệ rút gọn ở mục 8f trước khi sửa tiếp phần này.
- **Mọi thay đổi thật sự được thực hiện (sửa file, sửa catalog, sửa diagram, quyết định mới...) đều phải cập nhật vào chính file PROJECT_CONTEXT.md này** (Decisions Log + mục liên quan + dòng "Cập nhật lần cuối") — không chỉ cập nhật các file phụ (SCREEN_INVENTORY.md, plan...) mà quên gốc.
- Không thao tác gì trên Trello trừ khi Leader yêu cầu.

Sau mỗi quyết định lớn, cập nhật Decisions Log + dòng "Cập nhật lần cuối". Đồng bộ **PROJECT_CONTEXT** lên Drive (không đồng bộ Leader-only / có thể không đồng bộ cả folder Material nếu nặng — nhưng template cần có trên Drive `03_Reports`). *(Bản `00_Admin/PROJECT_CONTEXT.md` trên Drive hiện còn là bản cũ 09/09 — cần Leader upload bản mới.)*

---

## 19. Tóm tắt 5 dòng (tl;dr)

1. Đồ án FA26SE147 (SmartMaint): CMMS cho **ban quản lý tòa nhà văn phòng cho thuê + chung cư** — lõi Maintenance Case; **AI-assisted BẮT BUỘC (RBAC-scoped, cả 5 role)**; **Web + Mobile full parity cả 5 role**.  
2. **Chuẩn trường SEP490:** 7 reports; milestone tuần 1→15; English docs; tracking + schedule.  
3. R1 + R2 xong; R3 (77 trang) xong 28/09 nhưng **chưa sửa theo note thầy 12/10**. Effort **100 man-day**. Stack: React+Vite / **.NET (C#)** / Supabase / Railway+Vercel / Replicate. **Mốc tuần 5 (R4 + R5 + code frame) đang trễ → bù CN 18/10.**  
4. **6 luồng chốt 12/10:** F1 tòa nhà/thiết bị/danh mục (Huy) · F2 bảo trì định kỳ (Trí) · F3 sự cố thường (Trí) · F4 khẩn cấp (Tùng) · F5 sửa hay thay/bên thứ 3 (Hùng) · F6 giám sát & thống kê (Hùng). Sở hữu theo luồng, Trí giữ lõi; họp T2–T7.  
5. **Việc tiếp theo:** họp T3 13/10 chốt D1–D16 (22 card tuần 6 trên Trello, cột Ready, đã chỉnh theo mẫu R4/R5); ERD v4 vật lý đóng băng T6 16/10; code frame online + R4 nháp (kiến trúc, 3 package diagram, ERD vật lý + Table Descriptions, mục mẫu Login) + R5 khung + Schema.sql/Data.sql nộp CN 18/10; chốt OPEN F2 (P01–P09) tuần 7; sau đó sửa R3 theo note thầy. (F2 đã vẽ, F3 đã dựng lại 12/10.)
