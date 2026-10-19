# Phân công Report 3 (SRS) — tuần 3

Nguồn tham khảo: `claude/SCREEN_INVENTORY.md` + `SmartMaint_Web_Screen_Catalog_VI.docx` (catalog 82 màn hình, để riêng — không phải Report 3). Report 3 là file `.docx` riêng theo đúng template trường `Report3_Software Requirement Specification.docx`.

Phạm vi tuần 3 (theo mốc trong `PROJECT_CONTEXT.md`): Part I + II.1 Product Overview + II.2 User Requirements + II.3.1 System Functional Overview. Chưa cần 3.2 (chi tiết từng function) và NFR đầy đủ.

## Bước 0 — Ai cũng đọc trước (bắt buộc)

Trong catalog VI: mục 2 (Quyết định & giả định), mục 4 (5 vai trò), mục 5 (vòng đời Case), mục 9 (Rules chung). Đây là nguyên liệu cho Business Rules (1.2) và Actors (2.1) của Report 3.

## Bước 1 — Use Case Diagram + mô tả (mục 2.2 template), chia theo actor

| Người | Actor phụ trách | Đọc phần nào trong catalog |
|---|---|---|
| Tùng | Employee + Technician | mục 10.4 (EMP), 10.6 (TEC) + cột EMP/TEC trong bảng Permission mục 7 |
| Leader | Officer | mục 10.5 (OFF) + 10.9–10.11 (AST/PM/ANA phần Officer chạm tới) |
| Hùng | Manager + Admin | mục 10.7 (MGR), 10.8 (ADM), 10.9 (AST phần Admin) |
| Huy | — (không vẽ UC, xem bước 2) | |

Cách làm: đọc → liệt kê use case (tên = động từ + tân ngữ, lấy từ cột "Actions" của từng màn hình) → vẽ 1 UC diagram cho actor phụ trách → viết bảng mô tả UC (ID, tên, actor, mô tả ngắn).

## Bước 2 — Các mục còn lại (không chia theo role, cần nhìn toàn hệ thống)

| Mục template | Nguồn lấy | Ai làm |
|---|---|---|
| 3.1.1 Screens Flow | Chuẩn hoá lại 7 sơ đồ mục 8 catalog theo ký hiệu UML (khung thường = page, oval = dialog) | Hùng |
| 3.1.2 Screen Descriptions | Copy/định dạng từ mục 6 (Screen inventory) | Huy |
| 3.1.3 Screen Authorization | Copy/định dạng từ mục 7 (Permission table) | Huy |
| 3.1.4 Non-Screen Functions | Không có sẵn trong catalog — lấy từ `PROJECT_CONTEXT.md` phần kiến trúc AI (Replicate async: tạo prediction, poll/webhook, audit log) | Leader |
| 1.1 Context Diagram | Không có sẵn — vẽ mới: hệ thống ở giữa, 5 actor + Replicate AI xung quanh | Leader hoặc Hùng, làm sau khi Screens Flow xong |
| 3.1.5 ERD | Không có sẵn, catalog không mô tả data model — Building/Room/Asset/Case/WorkOrder/InspectionRecord/User | Leader (bắt buộc, vì cần hiểu schema đang code) |

## Thứ tự làm

1. Bước 0 (mọi người, song song).
2. Bước 1 (UC theo actor) và cột Huy ở bước 2 chạy song song — không phụ thuộc nhau.
3. Context Diagram và ERD làm cuối cùng (Leader), sau khi các phần trên đã có để gộp thông tin.

Chốt ngày: 2026-09-22.
