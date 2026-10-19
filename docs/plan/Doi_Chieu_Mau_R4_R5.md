# Đối chiếu mẫu Report 4 / Report 5 (thầy gửi) với card Trello tuần 6

> Soạn: Claude cho Leader · 12/10/2026 (tối).
> Mẫu: Final Report nhóm **SP25SE136 – FreeDriver** (GVHD Tôn Thất Hoàng Minh, 04/2025), phần IV = R4, phần V = R5. File PDF 209 trang, Leader giữ.
> Dùng file này làm **khung R4/R5** trong lúc chưa có template chính thức (card T1 của Tùng).

---

## 1. Mẫu R4 và R5 gồm những gì

### Report 4 — Software Design Description (mẫu dài khoảng 26 trang)

| Mục | Nội dung trong mẫu | Ghi chú |
|---|---|---|
| **1.1 System Architecture** | 1 sơ đồ triển khai, gồm: 3 app mobile (Expo) · reverse proxy (Traefik) · .NET backend · PostgreSQL · logging (Serilog + Seq) · Admin web (Next.js) · khung "Third parties" (Hangfire, Cloudinary, payOS, VietQR, MailKit, FPT.AI). Mũi tên request / response. | Có cả **job nền (Hangfire)**, **gửi mail**, **lưu file**, **log** |
| **1.2 Package Diagram** | **3 sơ đồ package**: Admin Web App · Backend Server (Endpoints → Commands / Queries → Domains / Persistence / Infrastructures) · Mobile App. Kèm **bảng Package Descriptions** (tên package, mô tả). | Sơ đồ khớp đúng cấu trúc thư mục code |
| **2. Database Design** | **ERD vật lý** cho toàn hệ thống (36 bảng; mỗi bảng có cột, kiểu, PK / FK). Kèm bảng **Table Descriptions** (# · Entity · mô tả · PK · FK). | Không dùng ký hiệu Chen |
| **3. Detailed Design** | Nhóm theo **chức năng lớn** (3.1 Auth, 3.3 Car, 3.4 Booking). Mỗi chức năng có **Class Diagram + Sequence Diagram**. Class diagram theo mẫu CQRS: Command / Response / Handler / Validator. | Hình kiểu Mermaid, vẽ đều tay vì cùng một pattern code |

### Report 5 — Software Testing Documentation (mẫu dài khoảng 3 trang + 2 file Excel)

| Mục | Nội dung trong mẫu |
|---|---|
| **1. Scope of Testing** | Liệt kê 6 **luồng chính** sẽ test |
| **2. Test Strategy** | 2.1 Loại test (Functional black-box, Usability) — mỗi loại có mục tiêu, kỹ thuật, tiêu chí hoàn thành · 2.2 Ma trận loại test × cấp test (Unit / Integration / System / Acceptance) · 2.3 Công cụ (Swagger, Postman) |
| **3. Test Plan** | 3.1 Nhân lực · 3.2 Môi trường (trình duyệt, mobile; VSCode + TestContainers) · 3.3 Mốc |
| **4. Test Cases** | Link file `Report5_Unit Test.xlsx`, `Report5_Test Report.xlsx` |
| **5. Test Reports** | **5.1 Unit test report**: 24 chức năng (mã UT01…), 137 test case; cột Passed / Failed / Untested, phân loại **N / A / B** (Normal / Abnormal / Boundary); tỷ lệ coverage. **5.2 UAT**: thống kê theo module / actor, 362 test case |

**Mốc tuần 5 của trường** (StudentGuide): R4 = **System design + DB design**, tức mục 1 và 2 ở trên; R5 = **mọi thứ trừ Test Cases / Test Reports**, tức mục 1–3.

---

## 2. Đối chiếu với 21 card `[T6]`

| Phần trong mẫu | Card đang có | Đánh giá |
|---|---|---|
| R4 1.1 System Architecture | Trí "R4 kiến trúc hệ thống + ghép R4 nháp" | ⚠️ **Thiếu thành phần.** Card chỉ ghi Web / Mobile / API / DB / Replicate. Còn thiếu: **job nền** (F2 sinh lệnh mỗi ngày, các đồng hồ nhắc ở F3 / F4 — mẫu có Hangfire), **email** (Context Diagram vẫn chưa có, câu hỏi treo từ 28/09), **push notification** cho mobile, **Supabase Storage** (ảnh, biên bản), **logging** |
| R4 1.2 Package Diagram (web, backend, mobile) + bảng mô tả | **Không có card nào** | ❌ **Lỗ.** Đây là phần bắt buộc của "System design" ở mốc tuần 5 |
| R4 2. ERD | Trí "Ghép ERD v4" — ghi **"ký hiệu Chen như v3"** | ❌ **Sai loại.** Chen là mô hình khái niệm (hợp với R3). R4 cần **ERD vật lý** (bảng, cột, kiểu, PK / FK) như mẫu |
| R4 2. Table Descriptions | Huy (F1), Tùng (F4), Hùng (F5) có data dictionary | ⚠️ **Bảng của Trí không có card.** Lõi + F2 + F3 khoảng 16 bảng (User, Case, Assignment, PreventivePlan…) — nhiều nhất nhóm — nhưng không card nào giao viết |
| R4 2. Bảng F2 | Card ERD ghi "bảng F2 lấy theo ERD v3, chỉnh ở tuần 7–8" | ⚠️ **Lỗi thời** — F2 đã vẽ 12/10, bảng theo `F2_Bao_Tri_Dinh_Ky.md` mục 7 |
| R4 3. Detailed Design (class + sequence) | Chưa có (đúng — mốc tuần 7) | ✅ Đúng lịch. Nhưng cần **1 mục mẫu** (Login: class + sequence) đi kèm module mẫu, để từ tuần 7 bốn người vẽ cùng một kiểu |
| R5 1–3 (scope, strategy, plan) | Huy "Khung Report 5 (test plan)…" | ✅ Đủ. Bổ sung: scope = 6 luồng F1–F6; ma trận cấp test nên có **Integration** (mẫu bỏ trống) |
| R5 4–5 (test case, report) | Huy, Hùng "≥ 8 test case chạy thật" | ⚠️ **Chưa rõ định dạng.** Mẫu tách **Unit test** (theo hàm, có N / A / B) và **UAT** (theo module). Card đang mô tả kiểu test tay trên bản online (= UAT). Unit test chưa ở DoD card code nào |
| Deliverable: Database script | Trí "Migration đợt 1 + database script" | ⚠️ Mẫu nộp **2 file**: `Schema.sql` + `Data.sql`. Seed CSV của Huy phải thành `Data.sql` |
| Code frame (login, home, profile, user CRUD) | Trí (Auth), Tùng (mobile), Hùng (User, Home) | ✅ Đủ. Không có Register — đúng R3 (Admin tạo tài khoản); ghi lý do vào R4 / R5 để thầy không hỏi |
| Template | Tùng "Lấy template R4 + R5" | ✅ Giữ. Template chính thức vẫn cần (bìa, định dạng); trong lúc chờ, dùng cấu trúc mục 1 ở trên |

**Kết luận:**
- Phân công **hợp lý ở phần code frame, data dictionary và R5 khung**.
- **Chưa đủ cho R4**, thiếu 3 thứ:
  1. Package diagram.
  2. ERD vật lý (đang ghi Chen).
  3. Data dictionary cho bảng của Trí.
- Kiến trúc hệ thống còn thiếu thành phần.

---

## 3. Đề xuất sửa — ✅ ĐÃ SỬA TRÊN TRELLO 12/10 20h45 (Leader đồng ý)

Đã sửa mô tả + DoD của 10 card và thêm card mới `[T6] Code frame | Hùng | R4 Package Diagram Backend + Web + bảng mô tả` (hạn CN 18/10).

| # | Sửa | Card | Giờ |
|---|---|---|---|
| 1 | Đổi "ký hiệu Chen" → **ERD vật lý**. Trước khi đóng băng (T6): vẽ tay từ đề xuất bảng. Sau migration (T7): **xuất lại từ DB thật** (DBeaver / EF Core Power Tools) để R4 khớp code. Bảng F2 theo `F2_Bao_Tri_Dinh_Ky.md` mục 7 | Trí – Ghép ERD v4 | 0 (đổi cách vẽ) |
| 2 | **Data dictionary bảng lõi + F2 + F3** (cùng định dạng 3 bạn) | Trí – thêm vào card ERD | +3 |
| 3 | **Package diagram Backend + Web + bảng mô tả**, vẽ theo đúng thư mục skeleton Trí dựng | Hùng (đang có Home 2 giờ; vẽ từ code thật giúp Hùng nắm skeleton) | +2 |
| 4 | Card "kiến trúc Mobile" ghi rõ: **package diagram Mobile + bảng mô tả** | Tùng | 0 |
| 5 | Kiến trúc hệ thống thêm: **job nền** (.NET HostedService / Hangfire — chạy trên instance Railway luôn bật), **email** (SMTP / dịch vụ mail), **push** (FCM), **Supabase Storage**, **logging**. Sửa luôn Context Diagram (thêm Email) | Trí – R4 kiến trúc | 0 (đã có giờ) |
| 6 | **Chốt pattern backend** (layered controller–service hay CQRS như mẫu) trước T5. Package diagram và class diagram 4 người phải theo cùng 1 pattern | Trí – Backend skeleton | 0 |
| 7 | Thêm **1 mục Detailed Design mẫu: Login (class + sequence)** | Trí – Backend skeleton | +1 |
| 8 | DoD mọi card code thêm: **unit test trong project test** — mỗi API ≥ 1 Normal + 1 Abnormal + 1 **Boundary** (mẫu Boundary = 0% → dễ bị hỏi) | Huy, Hùng, Trí | 0 (thay một phần "≥ 8 test case") |
| 9 | Test case có **2 file Excel** như mẫu: Unit Test (mã UTxx, N / A / B) và Test Report (UAT theo **luồng F1–F6**, có cột actor). Huy dựng khung, Hùng dùng chung | Huy, Hùng | 0 |
| 10 | DB script = `Schema.sql` + **`Data.sql`** (từ seed của Huy) | Trí – Migration | 0 |
| 11 | Dời **BR F3 v2** của Trí sang tuần 7 (BR là cập nhật R3, mốc Package 1 cũng yêu cầu) để bù giờ cho #2, #7 | Trí – R4 kiến trúc | −2 |

**Giờ sau khi sửa:**

| Người | Trước | Sau |
|---|---|---|
| Trí | 22 | ~24 |
| Hùng | 20 | 22 |
| Huy | 20 | 20 |
| Tùng | 20 | 20 |

Nếu muốn ngang hơn, chuyển #7 (Login mẫu) sang tuần 7.

---

## 4. Mẫu cũng có lỗi — đừng chép nguyên

| Lỗi trong mẫu | Bài học cho SmartMaint |
|---|---|
| Test Milestones: start 20/03/**2025**, end 25/04/**2024** (kết thúc trước khi bắt đầu) | Rà ngày tháng trước khi nộp |
| Detailed Design nhảy 3.1 → 3.3 (không có 3.2) | Đánh số tự động trong Word |
| Bảng viết tắt có "PWM – Psychology website" (chép từ dự án khác) | Không chép bảng của người khác |
| Unit test **Boundary 0%**; ma trận chỉ có Unit + Acceptance | Có Boundary và Integration — điểm cộng khi hội đồng hỏi |
| Table Descriptions chỉ có PK / FK | Data dictionary của nhóm (kiểu, ràng buộc, BR nguồn) **tốt hơn mẫu** → giữ |
| Detailed Design chỉ phủ 3 nhóm chức năng (Auth, Car, Booking) | SmartMaint chia theo **6 luồng**, mỗi chủ luồng vẽ class + sequence cho luồng mình → phủ đều hơn |
