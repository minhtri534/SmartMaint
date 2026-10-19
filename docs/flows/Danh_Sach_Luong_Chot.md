# Danh sách luồng nghiệp vụ — BẢN CHỐT (12/10/2026, cập nhật tối 12/10: đủ 6 luồng)

> Chốt bởi Leader ngày 12/10/2026, theo đúng thứ tự trong note thầy ("Ghi chú sau họp", Drive `02_Tasks/tuần 5`): luồng 1 cấu trúc tòa nhà → luồng 2 bảo trì định kỳ → luồng 3 xử lý sự cố → luồng 4 thống kê.
> **Từ nay mọi tài liệu, sơ đồ, card Trello, Report 3/4/5 dùng số trong bảng này.**

## Bảng chốt

| Số | Tên luồng | Phụ trách (phân tích + thiết kế + code) | Ứng với luồng nào của thầy | Package code |
|---|---|---|---|---|
| **F1** | Cấu trúc tòa nhà, thiết bị và danh mục dịch vụ | **Huy** | Luồng 1 | P1 (tuần 6–7) |
| **F2** | Bảo trì định kỳ | **Trí** | Luồng 2 (có AI) | P2 (tuần 8–9) |
| **F3** | Xử lý sự cố thông thường | **Trí** | Luồng 3 — phần trong phạm vi / khả năng (có AI) | P1 (tuần 6–7) |
| **F4** | Xử lý sự cố khẩn cấp | **Tùng** | Luồng 3 — sự cố khẩn cấp (có AI) | P2 (tuần 8–9) |
| **F5** | Sửa hay thay / chuyển bên thứ 3 | **Hùng** | Luồng 3 — phần ngoài phạm vi / vượt khả năng | P2 (tuần 8–9) |
| **F6** | Giám sát hiệu suất và thống kê | **Hùng** | Luồng 4 | P3 (tuần 10–11) |

Cách nhớ: **dựng tòa nhà (F1) → lên lịch bảo trì (F2) → có sự cố thì xử lý (F3 thường, F4 khẩn, F5 vượt khả năng) → nhìn lại số liệu (F6)**.

Lõi dùng chung (Auth/RBAC, Case + máy trạng thái, thông báo, audit, file đính kèm, AI gateway): **Trí** giữ. Mobile Flutter toàn bộ: **Tùng**.

## Đối chiếu số cũ (để đọc file cũ)

| Số chốt | Brief 03/10 (`Brief_Nhom_Phan_Tich_6_Luong`) | Tài liệu sáng 12/10 (đã đổi hết) | File bài tuần 4 của mem (giữ nguyên tên gốc) |
|---|---|---|---|
| F1 | F5 | F1 | `F5_Asset Lifecycle.drawio` (Huy) |
| F2 | F3 | F4 | — (không có bài tuần 4; Trí vẽ 12/10) |
| F3 | F1 | F2 | — |
| F4 | F2 | F3 | `F2-Nhánh khẩn cấp.drawio` (Tùng) |
| F5 | F4 | F5 | `DEPRICATED_F4_Replacement_Procedure.drawio` (Hùng) |
| F6 | F6 | F6 | `F6_Statistic_Monitoring` (Hùng) |

## Tình trạng từng luồng

| Số | Sơ đồ | Tài liệu phân tích | OPEN |
|---|---|---|---|
| F1 | `F1_Thiet_Bi_Va_Danh_Muc.drawio` (5 trang) | `Doi_Chieu_Tuan4_F1_F4_F5_F6.md` mục 2 | A01–A07 |
| F2 | `F2_Bao_Tri_Dinh_Ky.drawio` (4 trang, dựng 12/10 tối, 3 ô AI) | `F2_Bao_Tri_Dinh_Ky.md` | P01–P09 |
| F3 | `F3_Xu_Ly_Su_Co_Thong_Thuong.drawio` (5 trang, dựng lại 12/10 tối; Trang 3 rút gọn → F5) | `F3_Su_Co_Thong_Thuong.md` | O01–O17 (O11–O13 chuyển F5) |
| F4 | `F4_Xu_Ly_Khan_Cap.drawio` (5 trang) | `Doi_Chieu_Tuan4_F1_F4_F5_F6.md` mục 3 | K01–K08 + 1 mâu thuẫn |
| F5 | `F5_Sua_Hay_Thay.drawio` (2 trang) — **nguồn duy nhất** cho bảo hành / bên thứ 3 / báo giá / thay thiết bị | `Doi_Chieu_Tuan4_F1_F4_F5_F6.md` mục 4 | R01–R04 (+ O11 từ F3) |
| F6 | `F6_Giam_Sat_Hieu_Suat.drawio` (2 trang) | `Doi_Chieu_Tuan4_F1_F4_F5_F6.md` mục 5 | S01–S05 |

## Quyết định liên luồng đã chốt

- **12/10 tối — F5 là nguồn duy nhất** cho "bảo hành / chuyển bên thứ 3 / báo giá / thay thiết bị". F3 Trang 3 chỉ còn Officer phân loại lại phạm vi sau khảo sát rồi → F5. F2 (bên thứ 3 có hợp đồng bảo trì định kỳ) và F4 (gọi bên thứ 3 khẩn) chỉ ghi nhận trên lệnh / case, không vẽ lại quy trình báo giá.
- Case sinh từ **F2** (mục checklist Không đạt, BR-07) vào **F3** ở Approved (thường) hoặc **F4** (nguy hiểm).

Sơ đồ nằm ở folder `Claude outputs/Luong_Nghiep_Vu_Chot/` trên máy Leader; Leader up lên Drive `04_Design/`.
