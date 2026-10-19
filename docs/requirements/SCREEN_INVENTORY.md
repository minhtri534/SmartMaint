# SCREEN_INVENTORY — SmartMaint Web (81 màn hình / 81 screens)

Nguồn: SmartMaint_Web_Screen_Catalog_EN/VI.docx (v1.0 draft, cập nhật 25/09/2026 — gộp MGR-02 vào OFF-03, xem Decisions Log). Chỉ Web. Ký hiệu quyền / Access: F đầy đủ, S một phần, R chỉ xem, P công khai, - không có. Thứ tự role: EMP OFF TEC MGR ADM.

| Mã | Tên (EN) | Tên (VI) | Loại | Quyền |
|---|---|---|---|---|
| AUTH-01 | Login | Đăng nhập | Page | PPPPP |
| AUTH-02 | Forgot Password | Quên mật khẩu | Page | PPPPP |
| AUTH-03 | Reset Password | Đặt lại mật khẩu | Page | PPPPP |
| AUTH-04 | Force Change Password (first login) | Bắt buộc đổi mật khẩu (lần đầu) | Page | FFFFF |
| AUTH-05 | Session Expired | Phiên đăng nhập hết hạn | Page | FFFFF |
| AUTH-06 | Account Locked or Deactivated | Tài khoản bị khóa hoặc vô hiệu hóa | Page | PPPPP |
| ERR-01 | No Permission (403) | Không có quyền (403) | Page | FFFFF |
| ERR-02 | Page Not Found (404) | Không tìm thấy trang (404) | Page | FFFFF |
| ERR-03 | Server Error (500) | Lỗi máy chủ (500) | Page | FFFFF |
| ERR-04 | No Connection (Offline) | Mất kết nối mạng | Page | FFFFF |
| COM-01 | App Shell (common layout) | Khung ứng dụng (bố cục chung) | Page | FFFFF |
| COM-02 | Profile | Hồ sơ cá nhân | Page | FFFFF |
| COM-03 | Change Password | Đổi mật khẩu | Page | FFFFF |
| COM-04 | Personal Settings | Cài đặt cá nhân | Page | FFFFF |
| COM-05 | Notification Center | Trung tâm thông báo | Page | FFFFF |
| COM-06 | Global Search and Advanced Filters | Tìm kiếm toàn cục và bộ lọc nâng cao | Page | SSSSS |
| COM-07 | AI Assistant (floating chat) | Trợ lý AI (khung chat nổi) | Page | FFFFF |
| COM-08 | Case Detail | Chi tiết Case | Page | SF-R- |
| EMP-01 | Employee Home | Trang chủ Employee | Page | F---- |
| EMP-02 | New Request | Tạo yêu cầu bảo trì | Page | F---- |
| EMP-03 | My Requests | Yêu cầu của tôi | Page | F---- |
| EMP-04 | Edit Request / Provide More Information | Sửa yêu cầu / Bổ sung thông tin | Page | F---- |
| OFF-01 | Officer Home | Trang chủ Officer | Page | -F--- |
| OFF-02 | Case Queue | Hàng chờ Case | Page | -F--- |
| OFF-03 | Case Tracking (+ All Cases cho Manager, chỉ xem) | Theo dõi Case (+ Tất cả Case cho Manager, chỉ xem) | Page | -F-R- |
| OFF-04 | Verification Queue | Hàng chờ xác nhận hoàn thành | Page | -F--- |
| OFF-05 | Create Case for an Employee | Tạo Case hộ Employee | Page | -F--- |
| TEC-01 | Technician Home | Trang chủ Kỹ thuật viên | Page | --F-- |
| TEC-02 | My Work Orders | Việc của tôi | Page | --F-- |
| TEC-03 | Work Order: Update and Complete | Phiếu việc: Cập nhật và Hoàn thành | Page | --F-- |
| TEC-04 | Work History | Lịch sử công việc | Page | --F-- |
| TEC-05 | Inspection Checklist (preventive) | Bản kiểm tra định kỳ (checklist) | Page | --F-- |
| MGR-01 | Manager Home | Trang chủ Manager | Page | ---F- |
| ADM-01 | Administrator Home | Trang chủ Quản trị viên | Page | ----F |
| ADM-02 | User List | Danh sách người dùng | Page | ----F |
| ADM-03 | Create / Edit User | Tạo / Sửa người dùng | Page | ----F |
| ADM-04 | User Detail | Chi tiết người dùng | Page | ----F |
| ADM-05 | Roles and Permissions (reference) | Role và Phân quyền (tham khảo) | Page | ----F |
| ADM-06 | Maintenance Categories | Danh mục bảo trì | Page | ----F |
| ADM-07 | SLA and Rules Settings | Cài đặt SLA và quy tắc | Page | ----F |
| ADM-08 | Audit Log | Nhật ký hoạt động | Page | ----F |
| AST-01 | Facility Tree (Building, Floor, Room) | Sơ đồ cây địa điểm (Tòa nhà, Tầng, Phòng) | Page | -R-RF |
| AST-02 | Asset List | Danh sách thiết bị | Page | -S-RF |
| AST-03 | Asset Detail | Chi tiết thiết bị | Page | -S-RS |
| AST-04 | Create / Edit Asset | Tạo / Sửa thiết bị | Page | ----F |
| AST-05 | Import Assets from File | Nhập thiết bị từ tệp | Page | ----F |
| AST-06 | QR Labels (print) | Nhãn QR (in) | Page | -F--F |
| AST-07 | QR Landing (scan to report) | Đích của mã QR (quét để báo sự cố) | Page | FFFFF |
| PM-01 | Preventive Plan List | Danh sách kế hoạch bảo trì định kỳ | Page | -F-R- |
| PM-02 | Create / Edit Preventive Plan | Tạo / Sửa kế hoạch định kỳ | Page | -F--- |
| PM-03 | Preventive Plan Detail | Chi tiết kế hoạch định kỳ | Page | -F-R- |
| PM-04 | Preventive Calendar | Lịch bảo trì định kỳ | Page | -F-R- |
| PM-05 | Inspection Records | Bản ghi kiểm tra | Page | -F-R- |
| PM-06 | Inspection Record Detail | Chi tiết bản ghi kiểm tra | Page | -F-R- |
| ANA-01 | KPI Overview Dashboard | Dashboard KPI tổng quan | Page | -S-F- |
| ANA-02 | Technician Performance | Hiệu suất Kỹ thuật viên | Page | ---F- |
| ANA-03 | Failure Trends and Problem Assets | Xu hướng hỏng và thiết bị hay hỏng | Page | -S-F- |
| ANA-04 | Reports and Export | Báo cáo và Xuất tệp | Page | -S-F- |
| DLG-01 | Approve Case | Duyệt Case | Dialog | -F--- |
| DLG-02 | Reject Case | Từ chối Case | Dialog | -F--- |
| DLG-03 | Request More Information | Yêu cầu bổ sung thông tin | Dialog | -F--- |
| DLG-04 | Assign Technician | Giao Kỹ thuật viên | Dialog | -F--- |
| DLG-05 | Reassign Technician | Đổi Kỹ thuật viên | Dialog | -F--- |
| DLG-06 | Verify and Close | Xác nhận và Đóng | Dialog | -F--- |
| DLG-07 | Reopen Case | Mở lại Case | Dialog | -F--- |
| DLG-08 | Decline Reopen Request | Từ chối yêu cầu mở lại | Dialog | -F--- |
| DLG-09 | Reject Assignment | Từ chối việc được giao | Dialog | --F-- |
| DLG-10 | Put Work on Hold | Tạm dừng công việc | Dialog | --F-- |
| DLG-11 | Confirm Completion | Xác nhận hoàn thành | Dialog | --F-- |
| DLG-12 | Cancel Request or Case | Hủy yêu cầu hoặc Case | Dialog | SF--- |
| DLG-13 | Report "Not Fixed" | Báo "Chưa sửa xong" | Dialog | F---- |
| DLG-14 | Rate the Service | Đánh giá dịch vụ | Dialog | F---- |
| DLG-15 | Building, Floor or Room Form | Biểu mẫu Tòa nhà, Tầng hoặc Phòng | Dialog | ----F |
| DLG-16 | User Account Action | Thao tác với tài khoản | Dialog | ----F |
| DLG-17 | Confirm Retire or Delete | Xác nhận Thanh lý hoặc Xóa | Dialog | ----F |
| DLG-18 | Change Asset Status | Đổi trạng thái thiết bị | Dialog | -F--- |
| DLG-19 | Unsaved Changes | Thay đổi chưa lưu | Dialog | FFFFF |
| DLG-20 | Photo Viewer and Upload Problems | Trình xem ảnh và lỗi tải ảnh | Dialog | FFFFF |
| DLG-21 | Generate Preventive Work Order Now | Tạo phiếu định kỳ ngay | Dialog | -F--- |
| DLG-22 | Claim Conflict and Release | Xung đột nhận case và Nhả case | Dialog | -F--- |
| DLG-23 | Category Item Form | Biểu mẫu mục danh mục | Dialog | ----F |

## Ghi chú thay đổi (25/09/2026)

Gộp **MGR-02 (All Cases)** vào **OFF-03 (Case Tracking)** — 2 màn này trùng gần như 100%: cùng tabs, cùng filter, cùng cột bảng, cùng phạm vi dữ liệu (mọi case ở mọi trạng thái). Khác biệt duy nhất là Officer có thêm row-menu hành động (Reassign, Comment, Cancel), Manager chỉ xem (Open). Đây chính là việc bảng Screen Authorization (mục 3.1.3 Report 3) vốn đã được thiết kế để xử lý (ẩn/hiện hành động theo role), nên không cần 2 route/screen riêng. Tổng số màn giảm từ 82 xuống **81** (58 page + 23 dialog).
