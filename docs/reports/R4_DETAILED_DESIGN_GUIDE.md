# Report 4 — Hướng dẫn vẽ Class Diagram & Sequence Diagram (mục 3 Detailed Design)

> Soạn 28/10/2026 (Claude, cho Leader). Nguồn: template trường `Report4_Software Design Document.docx` (Drive `MaterialOfCapStoneProject/2_SEP490/`, thêm 22/10) + SEP490 Student Guide (mốc tuần 5/7/9/11/13).
> Hình mẫu của template: `samples/sample_class_diagram.png`, `samples/sample_sequence_diagram.png`, `samples/sample_package_diagram.png`.

## 1. Template yêu cầu gì

```
3. Detailed Design
3.1 <Feature/Function Name1>
    3.1.1 Class Diagram               ← 1 class diagram cho cả feature
    3.1.2 <Sequence Diagram Name1>    ← 1 sequence / 1 chức năng (kịch bản)
    3.1.3 <Sequence Diagram Name2>
    3.1.4 …
3.2 <Feature/Function Name2>
```

Câu gốc của template:
- Mỗi feature gồm *"Class Diagram, Class Specifications, and Sequence Diagram(s)"*.
- *"For the features/functions with the same structure of class & sequence diagrams, you need to provide the diagrams once for one feature/function and refer to those diagrams from other features/functions."*

→ **Không** vẽ 1 diagram cho cả luồng F1–F6. Luồng nghiệp vụ đã có swimlane (activity) trong `docs/flows/`. Sequence chỉ mô tả **1 lần tương tác** (1 request / 1 nút bấm) đi qua các class thật.

**Mốc nộp (Student Guide):** class + sequence được bổ sung vào R4 ở mỗi package — P1 tuần 7 (F1 + F3), P2 tuần 9 (F2, F4, F5), P3 tuần 11 (F6, AI), Full tuần 13. Mốc tuần 5 chỉ có System + Database design.

## 2. Quy tắc bắt buộc khi vẽ

| # | Quy tắc |
|---|---|
| R1 | **Class diagram vẽ class có thật trong code** (Controller, I…Service, …Service, AppDbContext, Entity, DTO, Validator, Errors) — giống mẫu (ProductController / ProductDao / ProductBean). Không vẽ class nghiệp vụ tưởng tượng, không vẽ lại ERD. |
| R2 | Quan hệ đúng ký hiệu UML: kế thừa (tam giác rỗng), implement interface (nét đứt + tam giác rỗng), phụ thuộc/gọi (nét đứt mũi tên), association/aggregation khi có field giữ tham chiếu. |
| R3 | **Sequence: lifeline = class thật**, message = **tên method thật** (`LoginAsync`, `SaveChangesAsync`…), có activation bar, có message trả về (nét đứt). Không dùng 1 lifeline "System" hộp đen. |
| R4 | Lifeline đầu tiên là actor (role thật: Customer / Facility Officer / Technician / Facility Manager / Administrator), tiếp theo là màn hình web (Page/Component) rồi mới tới API. |
| R5 | Nhánh lỗi chính vẽ bằng `alt` (vd. sai mật khẩu → `AuthErrors.InvalidCredentials` → 401 ProblemDetails). Không vẽ hết mọi nhánh — chỉ nhánh có trong BR/MSG của R3. |
| R6 | Chiều gọi phải đúng `CONVENTIONS.md`: `Controllers → Modules → Data / Entities`. Diagram mà Controller gọi thẳng AppDbContext = sai so với code. |
| R7 | Các chức năng cùng cấu trúc (CRUD Building / Floor / Room / ServiceType…) **vẽ 1 lần**, chỗ khác ghi "Refer to Figure X". |
| R8 | Sau mỗi class diagram có bảng **Class Specifications** ngắn: Class · Method chính · Mô tả (template có nhắc, không có heading riêng). |
| R9 | Tên feature/function lấy theo R3 mục 3.2+ (16 Feature / 43 Function) để R3 ↔ R4 truy vết được. |

## 3. Khung mục 3 cho SmartMaint (theo 6 luồng, chủ luồng vẽ phần mình)

| Mục R4 | Feature | Chủ | Sequence gợi ý | Trạng thái code |
|---|---|---|---|---|
| 3.1 | Authentication | Trí | Login · Get Current User · Change Password (bắt đổi lần đầu) | ✅ có code — **vẽ trước, làm mẫu** |
| 3.2 | F1 – Building, Asset & Service Catalog | Huy | Manage Building (CRUD, làm mẫu cho Floor/Room/ServiceType) · Create Asset · Import Assets | chưa có |
| 3.3 | F3 – Normal Incident Handling | Trí | Submit Case · Approve/Reject · Assign Technician · Update Work Progress · Verify & Close / Reopen · Merge Duplicate Cases | web mock, backend chưa có |
| 3.4 | F2 – Preventive Maintenance | Trí | Create Plan · Generate Work Orders (job nền) · Record Checklist Result | chưa có |
| 3.5 | F4 – Emergency Incident | Tùng | Report Emergency · Escalate | chưa có |
| 3.6 | F5 – Repair or Replace / Third Party | Hùng | Propose Replacement · Record Outsourcing | chưa có |
| 3.7 | F6 – Monitoring & Statistics | Hùng | View Dashboard KPI · Technician Performance | chưa có |
| 3.8 | AI-assisted Support | Trí | Ask AI (Replicate async: create prediction → poll/webhook) | chưa có |

Danh sách sequence chỉ là gợi ý — chủ luồng chốt theo function R3 và code thật.

## 4. Định dạng file

- Nguồn diagram để trong repo: `docs/design/<mục>-<feature>/` (vd. `docs/design/3.1-auth/class.mmd`, `seq-login.mmd`) + PNG xuất ra cạnh đó để dán vào R4.
- **Định dạng: chờ Leader chốt** (PROJECT_CONTEXT có quy tắc "UML → draw.io"). Đề xuất: Mermaid (text, sửa code xong sinh lại được, render bằng `npx @mermaid-js/mermaid-cli`, không cần Java); nếu cần đúng style mẫu trường hơn thì draw.io.
- Vẽ **sau khi** code của function đó đã chạy và có unit test — diagram phải khớp code tại thời điểm nộp.
