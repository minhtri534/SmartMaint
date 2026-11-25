# Sơ đồ tổng quan 4 luồng (25/11/2026)

Bản rút gọn để trình bày với hội đồng, vẽ theo logic web **sau** các buổi họp 07/11 và 18/11 (commit `be25e0b`).
Không thay các sơ đồ chi tiết cũ trong `F1_…` – `F6_…` (bản 12/10, nhiều chỗ đã khác code).

| File | Luồng | Nhánh phụ đã gộp vào |
|---|---|---|
| `F1_Tong_Quan.png` | F1 · Tòa nhà, thiết bị và danh mục | Danh sách thay đổi asset / địa điểm (Staff lập trên cây, Manager duyệt) |
| `F2_Tong_Quan.png` | F2 · Bảo trì định kỳ | Task Fail → case sửa chữa sang F3; Pause / gia hạn / Archive plan |
| `F3_Tong_Quan.png` | F3 · Xử lý sự cố | F4 khẩn cấp, F5 bên thứ 3, đổi KTV, “chưa sửa xong” trong 7 ngày |
| `F6_Tong_Quan.png` | F6 · Giám sát hiệu suất và thống kê | Needs attention, trợ lý AI, xuất Excel |

`SmartMaint_4_Luong.drawio` chứa cả 4 trang, mở bằng draw.io (app.diagrams.net hoặc extension VS Code) để sửa; sửa xong
xuất lại PNG bằng File → Export as → PNG.
