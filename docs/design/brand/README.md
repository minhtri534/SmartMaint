# Logo SmartMaint — "Tầng lệch" (chốt 16/11/2026)

Mặt cắt tòa nhà 4 sàn (sàn mái lùi vào). Sàn đang bảo trì trượt ra 1/3 chiều dài, tô màu accent. Chỉ một mảnh có màu, còn lại màu chữ.

| File | Dùng cho |
|---|---|
| `smartmaint-mark.svg` | Bản gốc, lưới 32. Tài liệu, báo cáo, slide |
| `smartmaint-mark-mono.svg` | In 1 màu, con dấu |
| `smartmaint-app-icon.svg` / `-1024.png` | Icon app Flutter (`flutter_launcher_icons`): hình vuông tràn viền, để iOS / Android tự bo góc |
| `frontend/public/favicon.svg` | Favicon web, vẽ lại trên lưới 16px cho nét rõ, tự đổi màu theo dark mode của tab |
| `frontend/public/apple-touch-icon.png` | Icon khi lưu web ra màn hình iPhone (180px) |
| `ProductMark` trong `frontend/src/shared/ui/Icon.tsx` | Logo trong sidebar / trang đăng nhập, vẽ lại trên lưới 20px |

## Màu

| Vai trò | Nền sáng | Nền tối |
|---|---|---|
| Sàn (mực) | `#131416` | `#eef1f3` / trắng |
| Sàn đang bảo trì (accent) | `#1b6c80` | `#6cc0d4` |

## Chữ đi kèm

"SmartMaint" viết IBM Plex Sans 600, letter-spacing −0.02em, đặt bên phải mark, cách 10px khi mark cao 20px.

## Không được

- Đổi màu sàn accent sang màu khác, hoặc tô màu nhiều hơn 1 sàn.
- Thêm gradient, bóng đổ, viền, hay đặt mark trong ô màu (trừ icon app).
- Kéo giãn méo tỷ lệ, xoay, hoặc đổi sàn trượt ra sang tầng khác.
