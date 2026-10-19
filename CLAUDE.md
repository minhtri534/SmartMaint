# CLAUDE.md — SmartMaint (FA26SE147)

Đồ án tốt nghiệp: **Smart Corporate Facility Maintenance Management Platform** (CMMS cho ban quản lý tòa nhà văn phòng cho thuê + chung cư). Leader: Quách Minh Trí. Nhóm 4 người.
Trao đổi với Leader bằng **tiếng Việt có dấu**. Code, tên biến, commit message, comment trong code: **tiếng Anh**.

## ⛔ Quy tắc Git / GitHub — BẮT BUỘC

- **Không bao giờ** thêm `Co-Authored-By: Claude ...`, "Generated with Claude Code" hay bất kỳ dòng ghi công AI nào vào commit, PR, issue, README.
- **Không** đổi `git config user.name` / `user.email`. Commit dưới danh tính git đang cấu hình trên máy (tài khoản GitHub của Leader).
- **Không** dùng GitHub App / tài khoản bot nào, **không** thêm collaborator, không đổi quyền repo, không đổi remote.
- **Không tự commit hay push** trừ khi Leader yêu cầu rõ trong lượt đó. Mặc định: sửa file xong thì để Leader tự review rồi commit.
- Không commit secret: `.env`, connection string Supabase, key Replicate, JWT secret. Chỉ commit `.env.example`.

## Tech stack (đã chốt)

| Tầng | Công nghệ | Thư mục | Người phụ trách chính |
|---|---|---|---|
| Backend API | .NET (C#), ASP.NET Core Web API, EF Core (code-first, Npgsql) | `backend/` | Trí giữ lõi; mỗi chủ luồng code module của mình |
| Web | React + Vite (TypeScript) | `frontend/` | như trên |
| Mobile | Flutter (5 role, full parity với web) | `mobile/` | Tùng |
| DB + file | Supabase (Postgres + Storage). **Không dùng Supabase Auth** — JWT + RBAC tự code trong .NET | — | Trí |
| AI | Replicate (LLM chat, **bất đồng bộ**: tạo prediction → poll/webhook) | `backend` module AI | Trí |
| Deploy | Railway (backend) + Vercel (web) | — | Trí |

Phiên bản cụ thể (.NET, Node, Flutter) chốt khi dựng skeleton và ghi vào `docs/` — đừng tự đoán.

## Nghiệp vụ cốt lõi (đọc trước khi code bất cứ gì)

- **5 role:** Customer (khách thuê / cư dân — tài liệu cũ gọi là Employee), Facility Officer, Maintenance Technician, Facility Manager, Administrator. Không có tự đăng ký — Admin tạo tài khoản.
- **Officer phụ trách theo tòa** (Primary / Backup), **không có hàng chờ chung** (chốt 12/10, khác SRS cũ).
- **Case 11 trạng thái** (đã bỏ "Need more info" — 12/10). Technician báo Complete ≠ Closed; Officer phải Verify mới đóng.
- **6 luồng chốt 12/10:** F1 tòa nhà/thiết bị/danh mục (Huy) · F2 bảo trì định kỳ (Trí) · F3 sự cố thông thường (Trí) · F4 khẩn cấp (Tùng) · F5 sửa hay thay / bên thứ 3 (Hùng) · F6 giám sát & thống kê (Hùng). **Lõi dùng chung** (Auth/RBAC, Case + máy trạng thái, notification, audit, attachment, AI gateway) do Trí giữ — sửa bảng lõi phải qua PR.
- **AI chỉ gợi ý**, không tự đổi dữ liệu; luôn bị giới hạn đúng phạm vi RBAC của role đang hỏi; mọi câu hỏi AI ghi audit (Admin chỉ thấy metadata, không thấy nội dung).

## Thứ tự ưu tiên khi tài liệu mâu thuẫn

1. Leader nói trong phiên hiện tại.
2. `docs/flows/` (sơ đồ + phân tích 6 luồng, 12/10) và `docs/plan/Tuan6_Ke_Hoach_DB_CodeFrame.md` (D1–D16).
3. `docs/00_PROJECT_CONTEXT.md`.
4. `docs/requirements/SRS_Report3.md` và `Screen_Catalog_EN.md` — viết 28/09, **chưa sửa theo note thầy 12/10** (vẫn còn "Need more info", hàng chờ chung, Employee).
Gặp mâu thuẫn thì **dừng lại hỏi Leader**, không tự chọn.

## Database — ⚠️ CHƯA ĐÓNG BĂNG

- ERD v5 Page-2 (logical) **chưa đạt** — xem `docs/database/ERD_v5_REVIEW.md`. **Không tạo migration cho các bảng nghiệp vụ** khi chưa có ERD đóng băng.
- Được làm trước: bảng `users` tối thiểu cho Auth, Building / Floor / Room.
- Quy ước (D-plan): bảng + cột **snake_case tiếng Anh**; PK `bigint identity` + mã hiển thị riêng (vd. `CS-000123`); thời gian `timestamptz` (lưu UTC); enum lưu dạng text + CHECK; có `created_at` / `updated_at`. EF Core code-first → `dotnet ef migrations script` sinh `Schema.sql`; seed → `Data.sql`.

## Quy ước code

- Chia theo **domain / luồng**, không chia theo loại file. Backend: mỗi module có controller / service / DTO / validator riêng; RBAC bằng policy / attribute dùng chung, không check role bằng tay rải rác. Pattern backend (layered hay CQRS) **chưa chốt** — hỏi Leader trước khi dựng module đầu tiên; sau đó mọi module theo đúng **module mẫu**.
- Web: `frontend/src/features/<feature>`, dùng chung `shared/`, `lib/` (API client). Mọi trang theo **trang mẫu**.
- Unit test: mỗi API ≥ 1 case Normal + 1 Abnormal + 1 Boundary (yêu cầu Report 5).
- Mọi biến môi trường liệt kê trong `.env.example`.
- Không thêm thư viện lớn hoặc dịch vụ ngoài mới mà không hỏi.

## Bản đồ tài liệu (`docs/`) — mở khi cần, đừng đọc hết một lúc

| File | Khi nào đọc |
|---|---|
| `00_PROJECT_CONTEXT.md` | Bối cảnh tổng, quyết định, lịch sử (dài) |
| `flows/Danh_Sach_Luong_Chot.md` | Bảng số luồng F1–F6 + đối chiếu số cũ |
| `flows/F2_Bao_Tri_Dinh_Ky.md`, `flows/F3_Su_Co_Thong_Thuong.md`, `flows/Doi_Chieu_Tuan4_F1_F4_F5_F6.md` | Trước khi code luồng tương ứng (có OPEN chưa chốt) |
| `flows/<Fx>/*.drawio, *.png` | Sơ đồ swimlane từng luồng |
| `plan/Tuan6_Ke_Hoach_DB_CodeFrame.md` | Phân công, D1–D16, lộ trình package |
| `plan/Doi_Chieu_Mau_R4_R5.md` | Yêu cầu Report 4/5 (package diagram, ERD vật lý, test N/A/B) |
| `requirements/SRS_Report3.md` | Business Rules (5.1), Messages (5.3), Notifications (5.4.2), NFR (4) |
| `requirements/Screen_Catalog_EN.md`, `SCREEN_INVENTORY.md` | Đặc tả 81 màn hình + quyền theo role |
| `requirements/UC_Master_List.md` | 67 use case |
| `database/` | ERD v4 (Chen), v5 (đang sửa), review |
