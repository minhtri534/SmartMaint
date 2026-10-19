# Đối chiếu bài nộp tuần 4 và bản dựng lại — 4 luồng F1, F4, F5, F6

> **Cập nhật 12/10 (17h):** đổi sang số luồng chốt theo thứ tự note thầy (mục 0). Nội dung phân tích không đổi.

> Người soạn: Claude cho Leader (Quách Minh Trí) · Ngày 12/10/2026 · Căn cứ: note thầy 12/10 (mã N1–N26), góp ý hội đồng R1 (G1–G7), Business Rules của Report 3, luồng F3 sự cố thông thường bản 12/10 (bản chuẩn).
> Không sửa file gốc của thành viên trên Drive. Mỗi luồng có một thư mục riêng gồm: file `.drawio` dựng lại, ảnh từng trang, ảnh render bài gốc (`Ban_goc_*.png`) để đặt cạnh nhau.

## 0. Số luồng chốt (12/10/2026, theo thứ tự note thầy)

> Số luồng đã chốt theo đúng thứ tự thầy đưa (luồng 1 cấu trúc tòa nhà → luồng 2 bảo trì định kỳ → luồng 3 xử lý sự cố → luồng 4 thống kê). Toàn bộ tài liệu này đã dùng số chốt. Bảng dưới để đọc lại các file cũ / bài tuần 4 còn mang số cũ.

| Số chốt | Luồng | Phụ trách | Brief 03/10 gọi là | Bài tuần 4 (file gốc của mem, giữ nguyên tên) |
|---|---|---|---|---|
| **F1** | Cấu trúc tòa nhà, thiết bị và danh mục dịch vụ | Huy | F5 | `F5_Asset Lifecycle.drawio` (Huy) |
| **F2** | Bảo trì định kỳ | Trí | F3 | chưa có |
| **F3** | Xử lý sự cố thông thường | Trí | F1 | (Trí làm, tài liệu `F3_Su_Co_Thong_Thuong.md`) |
| **F4** | Xử lý sự cố khẩn cấp | Tùng | F2 | `F2-Nhánh khẩn cấp.drawio` (Tùng) |
| **F5** | Sửa hay thay / chuyển bên thứ 3 | Hùng | F4 | `DEPRICATED_F4_Replacement_Procedure.drawio` (Hùng) |
| **F6** | Giám sát hiệu suất và thống kê | Hùng | F6 | `F6_Statistic_Monitoring` (Hùng) |

## 1. Tổng quan

| Luồng | Bài gốc | Bản dựng lại | Lỗi nặng ở bài gốc | Kiểm tra tự động | OPEN |
|---|---|---|---|---|---|
| F1 | 1 trang, 21 ô, 2 hình thoi, toàn ô trắng | 5 trang, 12 hình thoi, có bản đồ trạng thái thiết bị | 6 | 0 lỗi | A01–A07 |
| F4 | 1 trang, khoảng 40 ô, 7 hình thoi | 5 trang, 9 hình thoi, bản đồ trạng thái case khẩn cấp | 7 | 0 lỗi | K01–K08 + 1 MÂU THUẪN |
| F5 | 1 trang, 14 ô, 4 hình thoi | 2 trang, 9 hình thoi | 6 | 0 lỗi | R01–R04 |
| F6 | 1 trang, 6 ô, 0 hình thoi | 2 trang, 5 hình thoi | 5 | 0 lỗi | S01–S05 |

Kiểm tra tự động là cùng bộ kiểm tra đã dùng cho F3. Nó xác nhận: hình thoi đủ nhánh và có nhãn, không có ô cụt, đi tới được mọi ô, không kẹt vòng lặp, đổi trạng thái đúng người được phép, bản đồ trạng thái khớp các trang luồng, đường chính đánh số liền mạch.

Sau đó có 1 vòng phản biện độc lập (vai hội đồng) trên chính 4 bản dựng lại. Vòng này tìm thêm khoảng 60 điểm: 3 nặng, khoảng 20 vừa, phần còn lại là lỗi nhỏ và lỗi trình bày (chữ đè, đường cắt qua ô).

- Hai điểm nặng của F4 đã sửa trực tiếp: hạ mức không còn nhảy thẳng sang giao việc; đã có bước ghi bên chịu chi phí.
- Điểm nặng của F5 (trùng với F3 Trang 3): ✅ đã chốt F5 là nguồn duy nhất (mục 6 dòng 1).
- Phần lớn điểm vừa và nhỏ đã sửa ngay.
- Phần còn lại là việc phải sửa ở F3 hoặc cần thầy chốt (mục 6).
- Để lại 2 lỗi trình bày nhỏ có chủ ý: F1 Trang 4 có một đường quay lại cắt đường "báo Admin"; F5 Trang 1 có vài nét đứt chú thích cắt nét liền.

---

## 2. F1 — Thiết bị, địa điểm và danh mục dịch vụ (bài gốc của Huy)

### 2.1. Bài gốc làm được

- Đúng hướng vòng đời: tạo tòa / tầng / phòng → loại thiết bị → đăng ký (thủ công hoặc nhập file) → kiểm tra hợp lệ → kích hoạt → in QR → khách quét QR báo hỏng → thanh lý.
- Có ý "checklist + thời gian chuẩn" để trả lời G6, G7.

### 2.2. Lỗ hổng (vai hội đồng)

1. **NẶNG — Thiếu danh mục dịch vụ cố định (N24) và danh sách khẩn cấp (N26).** Đây là 2 ý thầy chốt rõ nhất cho luồng dữ liệu nền. F3 và F4 đều dựa vào nó.
2. **NẶNG — Không phân biệt khu chung và khu thuê, không có Officer phụ trách tòa (N11, N23).** Hai câu "lỗi có thuộc tòa nhà không" và "báo cho ai" không có dữ liệu nền để trả lời.
3. **NẶNG — Sai thứ tự thanh lý và sai người quyết định.** Officer quyết "hết vòng đời" → Admin "Retire" → rồi mới sang "F4 quyết định sửa hay thay" (số theo brief, nay là F5). Tức là thanh lý xong mới quyết định có thay hay không. Bài gốc cũng không kiểm tra BR-10 (không thanh lý khi còn case mở) và không có cách nào để Officer báo cho Admin (Admin không xem được case, BR-11).
4. **NẶNG — Sửa dữ liệu xong không kiểm tra lại.** Nhánh "Correct Data" đi thẳng tới "Print QR". Có hai ô "Register Asset" trùng nhau, một nhánh nhập file bị tách rời không nối vào luồng.
5. **NẶNG — Không có trạng thái thiết bị.** R3 có 4 trạng thái (Active, Under repair, Out of service, Retired) nhưng sơ đồ không có, và không có kết thúc.
6. **NẶNG — Checklist do Admin định nghĩa, lệch R3.** R3 để checklist nằm trong từng kế hoạch định kỳ, do Officer lập (PM-02). Nếu muốn làm theo G6 thì phải ghi rõ là đề xuất mới.
7. VỪA — Làn Hệ thống để trống; mọi ô màu trắng, không theo quy ước màu của brief; chữ Anh và Việt lẫn lộn; ô "Continue Asset" bị cụt.

### 2.3. Bản dựng lại xử lý thế nào

| Lỗ hổng | Trang / ô trong bản dựng lại |
|---|---|
| 1 | Trang 1 bước ⑤ "DANH MỤC DỊCH VỤ cố định 2 cấp" (mở rộng IssueType): mức khẩn cấp mặc định, cờ thuộc danh sách khẩn cấp (→ F4), bên chịu chi phí mặc định, loại thiết bị áp dụng, chuyên môn, thời gian chuẩn. Ghi chú danh sách khẩn cấp đặt ngay bên cạnh. |
| 2 | Trang 1 bước ② "Gắn loại khu", bước ③ "Gán Officer phụ trách tòa (+ dự phòng)", ghi rõ là đề xuất, chờ chốt O02 / S01. |
| 3 | Trang 4 riêng cho thay thế và thanh lý: Officer gửi đề nghị (chỉ mã thiết bị + lý do, không kèm nội dung case) → kiểm tra BR-10 → chặn và chờ case đóng → có thiết bị thay thì đăng ký mới trước → thanh lý → lịch sử giữ nguyên. Quyết định sửa hay thay nằm ở F5, sau đó mới sang đây. |
| 4 | Trang 2: kiểm tra hợp lệ có vòng sửa lại; nhập file đủ 4 bước theo R3 (tải lên → kiểm tra từng dòng → nhập dòng tốt hoặc dừng sửa file → kết quả). Thêm hình thoi đầu vào "thiết bị thuộc tòa nhà?": thiết bị của khách thì không đăng ký. |
| 5 | Trang 5 bản đồ trạng thái thiết bị, khớp Trang 2–4 (kiểm tra tự động). |
| 6 | "Mẫu checklist theo loại thiết bị" tô xanh dương (đề xuất G6), ghi rõ R3 hiện đặt checklist trong từng kế hoạch PM-02. |
| 7 | Đủ làn, màu theo quy ước, tiếng Việt thống nhất. |

### 2.4. Việc cho Huy

- So 2 bản cạnh nhau. Viết bảng "mỗi bước: ai, làm gì, màn hình nào, BR nào" cho Trang 1–4.
- Đi tìm câu trả lời cho A03 (danh mục dùng chung hay riêng từng tòa) và A05 (thiết bị bắt buộc kiểm định) bằng nguồn thật về tòa nhà văn phòng và chung cư (theo yêu cầu của thầy: không tự suy diễn).

---

## 3. F4 — Sự cố khẩn cấp (bài gốc của Tùng)

### 3.1. Bài gốc làm được (bài tốt nhất trong 4 bài)

- Có biện pháp tạm (phòng thay thế, mượn thiết bị), trả lời G2.
- Có leo thang theo thời gian, nhánh Technician từ chối, thiếu linh kiện, gia hạn với ETA mới.
- Tự đặt OPEN-01 → OPEN-05 rất đúng chỗ.

### 3.2. Lỗ hổng (vai hội đồng)

1. **NẶNG — Vi phạm N11.** Bài gốc "Notify ALL Officers" và "Claim case from queue"; thầy đã chốt chỉ báo Officer phụ trách tòa, bỏ hàng chờ chung.
2. **NẶNG — Vi phạm BR-20.** "Approve + Assign in one step" làm mất số đo thời gian chờ giao việc.
3. **NẶNG — Khẩn cấp do khách tự khai** ("Need before" deadline + Impact level). Thầy chốt khẩn cấp phụ thuộc loại dịch vụ / thiết bị (N26).
4. **NẶNG — Hẹn giờ vẽ như bước của luồng.** Các ô "Officer responds within 15 min?", "Still no response after 30 min?", "Approaching SLA 4h?" là đồng hồ chạy nền, nhưng lại được vẽ như bước tuần tự. Ví dụ ô "Approaching SLA" được đặt ngay sau ô "Lack of Component? – No". Nhiều hình thoi chỉ có một nhánh. Nhãn Yes / No là ô chữ rời, không gắn vào mũi tên.
5. **NẶNG — Có ô cụt và đường vô lý.**
   - "5c. Pause (On Hold)" không có đường ra.
   - "5d. Reject" quay về "Approve + Assign", tức là duyệt lại.
   - "3c. Alert Manager" đi thẳng tới "Record workaround" mà chưa có ai nhận case.
   - Ô OPEN-02 lại được dùng như một bước của luồng.
6. **NẶNG — Sai thứ tự đóng case.** Officer "Close Case" trước, rồi khách mới "Confirm operational". Nếu khách nói chưa xong thì case đã đóng.
7. **NẶNG — Thiếu ý thầy.**
   - Bên thứ 3 cho sự cố vượt khả năng (thang máy, PCCC) (N1, N3).
   - Ghi nhận nội dung sửa chữa (N21).
   - Gộp case trùng (N5) — khẩn cấp là lúc nhiều người báo cùng một sự cố nhất.
   - Phân biệt phạm vi tòa nhà (N23).
   - AI cho luồng 3 (N20).
8. VỪA — Không có trạng thái nào trên sơ đồ. OPEN-05 (ngoài giờ) thực ra thầy đã trả lời: F4 không áp dụng giờ hành chính.

### 3.3. Bản dựng lại xử lý thế nào

| Lỗ hổng | Trang / ô trong bản dựng lại |
|---|---|
| 1 | Trang 1 "Báo ĐÚNG NGƯỜI": trong giờ báo Officer phụ trách tòa, ngoài giờ báo Officer trực; quá 10 phút báo Officer dự phòng. Thuật ngữ "trực / dự phòng" định nghĩa ngay ở đầu trang. |
| 2 | Trang 2: hệ thống tự duyệt (vào thẳng Approved), Officer giao Technician là bước ⑤ riêng. |
| 3 | Case được tạo ở F3 bước ①; loại dịch vụ thuộc danh sách khẩn cấp thì sang F4. Officer xác nhận ở ④: không đúng khẩn cấp thì hạ mức về F3 (kèm ghi bên chịu chi phí), báo nhầm / ngoài phạm vi thì hủy. Ý "mai cần dùng phòng họp" (G2) chuyển thành OPEN K01: thuộc F3 mức Cao. |
| 4 | Mọi đồng hồ là ô chạy nền (⏱) gắn bằng nét đứt. Hình thoi nào cũng đủ nhánh có nhãn (đã kiểm tra tự động). |
| 5 | Technician từ chối thì về Approved để Officer giao lại; On Hold có đường "Tiếp tục" quay lại; không ô cụt (đã kiểm tra tự động). |
| 6 | Trang 4: kiểm tra (gọi khách xác nhận đã an toàn) → đóng → khách đánh giá / báo "Chưa sửa xong" theo F3 Trang 4; mở lại thì quay về F4 bước ⑦, vẫn tính 24/7. |
| 7 | Bên thứ 3 có hợp đồng thì gọi ngay ở Trang 2; Technician chỉ làm đầu mối. Bước ⑨ ghi nhận sửa chữa có RootCause. Gộp case ở Trang 1. Bên chịu chi phí ghi ở ⑪ khi đóng. Có 2 điểm AI (gợi ý case trùng + an toàn; gợi ý hướng xử lý). Thêm báo cáo sau sự cố ⑫ → F2 / F5. |
| 8 | Trang 5 bản đồ trạng thái case khẩn cấp. Ô đỏ MÂU THUẪN ghi rõ phải sửa BR-01 và BR-08. |

### 3.4. Việc cho Tùng

- K02 + K05 (lịch trực ngoài giờ) là lỗ lớn nhất còn lại. Cần tìm nguồn thật: tòa văn phòng / chung cư ở Việt Nam tổ chức kỹ thuật trực 24/7 ra sao.
- K06 (hai mốc "khống chế xong" và "khắc phục xong") là câu hội đồng rất dễ hỏi khi thang máy kẹt. Chuẩn bị số liệu tham chiếu.

---

## 4. F5 — Quyết định sửa hay thay (bài gốc của Hùng)

### 4.1. Bài gốc làm được

- Có Technician đánh giá và hệ thống hiện lịch sử thiết bị trên form, đúng hướng trả lời G4, G5.

### 4.2. Lỗ hổng (vai hội đồng)

1. **NẶNG — Vi phạm N2 / N5 (không quản lý kho).** Có các ô "Thay thế trong kho hay mua mới", "Có tồn tại trong kho".
2. **NẶNG — Vi phạm BR-12 và N22.** Có ô "Manager xem xét báo cáo thay thế" rồi "Có duyệt yêu cầu?", tức là Manager duyệt mua. Manager chỉ được xem; thầy cũng nói không quản lý duyệt và tiền.
3. **NẶNG — Thiếu bên thứ 3 và báo giá cho khách (N3)**, là ý chính của thầy cho chỗ "hỏng nặng cần thay".
4. **NẶNG — Luồng đứt.**
   - Hình thoi "Có tồn tại trong kho" không có đường vào và đường ra.
   - Nhánh "Kho" chỉ vào khoảng trống.
   - Ô "Xem xét báo cáo" của Officer bị treo.
   - Nhánh "Không duyệt" quay về "F1" (số theo brief, nay là F3) mà không nói case đi đâu.
5. **NẶNG — Không nối với case và với thiết bị.**
   - Không có trạng thái case (On Hold?).
   - Không có ghi nhận sửa chữa.
   - Kết thúc ở "Closed" mà không qua nghiệm thu.
   - Làn Admin để trống: thay thiết bị mà không ai tạo thiết bị mới hay thanh lý thiết bị cũ (BR-09, BR-10).
6. **NẶNG — Bỏ qua bảo hành.** R3 đã có trường "ngày hết bảo hành" (AST-04) mà luồng không dùng.
7. VỪA — Không kiểm tra phạm vi (lỗi do khách hoặc thiết bị của khách thì sao, N23).

### 4.3. Bản dựng lại xử lý thế nào

| Lỗ hổng | Trang / ô trong bản dựng lại |
|---|---|
| 1, 2 | Bỏ kho, bỏ mua, bỏ Manager duyệt. Chữ ghi rõ "ban tòa nhà tự quyết ngoài hệ thống"; tiêu chí sửa hay thay chỉ là thông tin hiển thị. |
| 3 | Trang 1: ghi nhận bên thứ 3 → hợp đồng trọn gói hay báo giá theo lần → ai chịu chi phí → chuyển báo giá cho đại diện hợp đồng → ghi nhận quyết định + bằng chứng. |
| 4, 5 | Có trạng thái On Hold (lý do "Chờ Officer quyết định") → Trang 2: bên thứ 3 thực hiện → Technician kiểm tra (không đạt thì làm lại) → Tiếp tục → ghi nhận sửa chữa → Completed → F3 Trang 4 nghiệm thu. Case đóng xong mới sang F1 Trang 4 để đăng ký thiết bị mới và thanh lý thiết bị cũ. |
| 6 | Hình thoi ④ "Còn bảo hành?" → gửi hãng; hãng từ chối thì quay về ⑤. |
| 7 | Hình thoi ③ phạm vi được kiểm tra ngay khi Officer nhận, trước khi xem bảng thông tin. |

**Đã chốt 12/10 tối:** F5 trùng phần lớn với F3 Trang 3 → chọn **F5 làm nguồn duy nhất** cho "bên thứ 3 / thay thiết bị". F3 Trang 3 rút gọn thành một ô "→ F5". Nếu không, nhóm sẽ có 2 bản khác nhau cho cùng một bước (vòng phản biện đánh dấu NẶNG).

### 4.4. Việc cho Hùng

- Đọc lại note thầy dòng 4–6 và 27. Viết 1 đoạn giải thích vì sao không có kho, không có duyệt, để trả lời khi hội đồng hỏi.
- Tìm nguồn cho R03 (tuổi thọ chuẩn theo loại thiết bị).

---

## 5. F6 — Giám sát hiệu suất và đúng lịch (bài gốc của Hùng)

### 5.1. Bài gốc làm được

- Xác định đúng 4 actor có liên quan (Manager, Hệ thống, Officer, Admin).

### 5.2. Lỗ hổng (vai hội đồng)

1. **NẶNG — Không phải luồng.** Bài gốc chỉ có 6 ô nối thẳng, không có hình thoi, không có bắt đầu / kết thúc, không có điều kiện nào. Thầy yêu cầu mỗi luồng có nhiều actor và có rẽ nhánh.
2. **NẶNG — Sai quyền.** Manager "xuất báo cáo" → Officer "điều chỉnh phân công" → Admin "chỉnh SLA". Như vậy Manager đang ra lệnh trong hệ thống (BR-12 nói chỉ xem), còn Admin chỉnh theo case mà Admin không xem được (BR-11). Chuỗi này chỉ đúng nếu ghi rõ phần chỉ đạo diễn ra ngoài hệ thống.
3. **NẶNG — Không định nghĩa chỉ số.** G7 hỏi "đúng lịch", "thời gian thao tác"; N16 chốt "hạn dài hơn"; N10 yêu cầu Manager xem số lần Technician từ chối. Bài gốc không có chỉ số nào trong số này.
4. **NẶNG — Không có ngưỡng cảnh báo** và không nói ai nhận cảnh báo (N11, N14).
5. VỪA — Không loại trừ case gộp / hủy khỏi KPI (N5); không tách F4 tính 24/7.

### 5.3. Bản dựng lại xử lý thế nào

| Lỗ hổng | Trang / ô trong bản dựng lại |
|---|---|
| 1 | Hai trang, có đường chính ①–⑨, có nhánh. |
| 2 | Trang 2: làn "Ngoài hệ thống" cho phần Manager chỉ đạo. Officer làm thay đổi thật (F3 / F4 / F2 / F5), Admin chỉnh ADM-07 / ADM-06 theo đề nghị bằng văn bản. |
| 3 | Trang 1 ô ③ liệt kê đủ chỉ số: % đúng hạn theo HẠN CÓ HIỆU LỰC và theo SLA GỐC, số lần dời hạn, thời gian On Hold theo lý do, thời gian pha bên thứ 3, thời gian khống chế / khắc phục của khẩn cấp, số case hạ mức, % kế hoạch định kỳ đúng lịch, thời gian thao tác thực tế so với giờ công nhập, số lần Technician từ chối + lý do. |
| 4 | Hình thoi ④ + ghi chú ngưỡng (R3 đã có + đề xuất thêm). Cảnh báo báo Officer phụ trách tòa; quá hạn thì không gửi Manager lần 2 vì F3 / F4 đã gửi. |
| 5 | Ô loại trừ: case con đã gộp, Rejected, Cancelled, case "Khảo sát thiết bị". |

### 5.4. Việc cho Hùng

- S03 (định nghĩa "đúng lịch") là câu G7 của hội đồng: phải có định nghĩa chính xác, có nguồn.
- Soạn sẵn bảng công thức cho từng chỉ số (tử số, mẫu số, loại trừ) để dùng cho Report 3, mục ANA.

---

## 6. Mâu thuẫn giữa các luồng — quyết định và việc còn lại

| # | Mâu thuẫn | Đề xuất | Trạng thái |
|---|---|---|---|
| 1 | Bên thứ 3 / thay thiết bị có ở 3 chỗ: F3 Trang 3, F5, F4 | F5 là nguồn duy nhất. F4 chỉ giữ "gọi ngay bên thứ 3 có hợp đồng" vì cần nhanh. | ✅ **Leader chốt 12/10 tối**; F3 Trang 3 đã rút gọn → F5 |
| 2 | Thẻ "Hỏng thường xuyên": F3 Trang 4 chỉ dẫn sang F2, F1 Trang 3 dẫn sang F2 hoặc F5 | F1 Trang 3 là nơi xử lý; F3 Trang 4 đổi thành "→ F1 Trang 3" | ✅ Đã sửa F3 (12/10 tối) |
| 3 | Officer theo tòa: F1 vẽ như đã có; F3 (O02), F6 (S01) còn OPEN; BR-02 (claim, trả về hàng chờ chung) chưa sửa | F1 đã ghi "đề xuất, chờ chốt O02 / S01". Khi chốt thì sửa BR-02 và BR-19 cùng lúc. | Chờ thầy |
| 4 | Danh sách khẩn cấp: F1 có thêm "mất nước cả tòa, an ninh cửa từ", ghi chú ở F3 không có | Lấy danh sách của F1 làm chuẩn, cập nhật ghi chú ở F3 | ✅ F3 Trang 1 trỏ "danh sách khẩn cấp (F1)" |
| 5 | F3 cách 2 "Duyệt ngay" bỏ qua bước ④ nên chưa ghi bên chịu chi phí | F5 đã xử lý ("chưa có thì chọn ngay"); F3 nên bắt buộc chọn ngay ở OFF-05 | ✅ Đã sửa F3 Trang 1 |
| 6 | F3 Trang 4 không có lối ra khi case đã thay thiết bị | Thêm "đã thay thiết bị → nhắc Officer → F1 Trang 4" | ✅ Đã sửa (F3 Trang 4: → F5 Trang 2 → F1 Trang 4) |
| 7 | Thuật ngữ: "Ngoài trách nhiệm tòa nhà" (Rejected) và "Ngoài phạm vi tòa nhà" (Cancelled) | Thống nhất một tên lý do "Ngoài phạm vi tòa nhà". Trước khi duyệt thì Rejected, sau khi duyệt thì Cancelled. | ✅ Đã sửa F3 Trang 1 + 3 |
| 8 | Đại diện hợp đồng: F3 để ở làn khách, F5 có làn riêng | Dùng làn riêng như F5 (vì đây không phải người báo hỏng) | ✅ Hết mâu thuẫn: phần báo giá chỉ còn ở F5 |
| 9 | "Phản hồi đầu" dừng ở đâu: F4 dừng khi Officer bấm xác nhận; R3 định nghĩa là "quyết định đầu tiên của Officer" | Khẩn cấp: xác nhận. Thường: duyệt / từ chối. Ghi rõ ở ANA. | Ghi vào Report 3 |
| 10 | Gộp case khẩn cấp vào một case chính đang ở mức thường | Case chính tự nâng lên mức cao nhất trong nhóm (đề xuất); cần thêm vào F3 bước gộp | ✅ Đã thêm vào F3 Trang 1 |
| 11 | Mẫu checklist: F1 đặt theo loại thiết bị, tài liệu F3 mục 3.3 đặt theo loại dịch vụ | Theo loại thiết bị (hợp với bảo trì định kỳ); sửa F3 mục 3.3 | ✅ Đã sửa F3 mục 3.3; F2 dùng mẫu theo loại thiết bị |

**BR phải sửa (gom từ 4 luồng):**

- BR-01: mức khẩn cấp do loại dịch vụ đặt sẵn, Officer hạ / nâng có lý do.
- BR-02: claim và hàng chờ chung.
- BR-08: khẩn cấp đếm 24/7; On Hold của case khẩn cấp không dừng đồng hồ.
- BR-19: Officer phụ trách theo tòa.
- BR mới: RootCause do Technician chọn khi ghi nhận sửa chữa.
- BR mới: lịch trực ngoài giờ (nếu thầy đồng ý K02).

## 7. Sáu câu nên hỏi thầy (xếp theo mức ảnh hưởng)

1. **K02 / K05** — Ngoài giờ ai nhận case khẩn cấp: hệ thống có lịch trực, hay chỉ có hotline ngoài hệ thống?
2. **MÂU THUẪN F4** — Khẩn cấp đếm 24/7 và mức do loại dịch vụ đặt sẵn, tức là sửa BR-01, BR-08. Thầy có đồng ý không?
3. **R02 = O12 của F3** — Khi chờ bên thứ 3 / thay thiết bị thì case để On Hold hay In Progress?
4. **K06** — Case khẩn cấp đo 2 mốc ("khống chế xong" và "khắc phục xong") hay chỉ 1?
5. **A06 / S05** — Có cần kênh trong hệ thống để Officer / Manager gửi đề nghị cho Admin không (Admin không xem case)?
6. **S03** — Định nghĩa "đúng lịch" của bảo trì định kỳ.

## 8. Danh sách OPEN theo luồng

**F1**

- **A01** (Trang 1): lưu ĐẠI DIỆN HỢP ĐỒNG của từng công ty thuê / căn hộ (người nhận báo giá, O13 của F3)? Liên kết tài khoản khách ↔ phòng thuê thế nào khi một công ty thuê nhiều phòng / tầng?
- **A02** (Trang 1): đổi / nghỉ việc Officer phụ trách → case đang mở của tòa chuyển cho ai? Ai bật "vắng mặt"?
- **A03** (Trang 1): danh mục dịch vụ dùng chung mọi tòa, hay mỗi tòa bật / tắt loại dịch vụ riêng?
- **A04** (Trang 1): "thời gian chuẩn" của mỗi loại dịch vụ lấy từ đâu?
- **A05** (Trang 2): thiết bị bắt buộc kiểm định (thang máy, PCCC, máy phát) → nhắc Officer lập kế hoạch F2 ngay khi tạo?
- **A06** (Trang 4): kênh Officer → Admin cho đề nghị thanh lý / thay thiết bị.
- **A07** (Trang 4): liên kết "thiết bị mới thay cho thiết bị cũ" (ERD cần thêm khóa tự tham chiếu).

**F4**

- **K01** (Trang 1): "mai cần dùng phòng họp" (G2) → F3 mức Cao + trường "cần dùng trước"?
- **K02** (Trang 1): ngoài giờ ai nhận, có lịch trực không?
- **K03** (Trang 1): Manager nhận cảnh báo để làm gì?
- **K04** (Trang 1): chống lạm dụng hạ mức.
- **K05** (Trang 2): lịch ca / trực của Technician.
- **K06** (Trang 3): hai mốc "khống chế xong" / "khắc phục xong".
- **K07** (Trang 4): có cần trạng thái "chờ khách xác nhận" không (đề xuất không)?
- **K08** (Trang 4): báo cáo sau sự cố bắt buộc khi nào?
- **MÂU THUẪN** (Trang 1): BR-01, BR-08.

**F5**

- **R01**: Manager có xem danh sách "thiết bị đang chờ thay" không?
- **R02**: On Hold hay In Progress khi chờ bên thứ 3.
- **R03**: tuổi thọ chuẩn theo loại thiết bị.
- **R04**: nhánh "khách chịu chi phí" (= O13 của F3).

**F6**

- **S01**: Officer chỉ xem chỉ số tòa mình?
- **S02**: hiện cả 2 tỷ lệ đúng hạn để tránh KPI ảo (= O09 của F3).
- **S03**: định nghĩa "đúng lịch".
- **S04**: Manager có "ghi chú chỉ đạo" trên case không?
- **S05**: kênh Manager → Admin.

## 9. File trong thư mục `Luong_Nghiep_Vu_Chot/`

| Thư mục | Nội dung |
|---|---|
| `F1_Thiet_Bi_Danh_Muc/` | `F1_Thiet_Bi_Va_Danh_Muc.drawio` (5 trang), `F1_Trang1..5.png`, `Ban_goc_Huy.png` |
| `F4_Khan_Cap/` | `F4_Xu_Ly_Khan_Cap.drawio` (5 trang), `F4_Trang1..5.png`, `Ban_goc_Tung.png` |
| `F5_Sua_Hay_Thay/` | `F5_Sua_Hay_Thay.drawio` (2 trang), `F5_Trang1..2.png`, `Ban_goc_Hung_F5.png` |
| `F6_Giam_Sat/` | `F6_Giam_Sat_Hieu_Suat.drawio` (2 trang), `F6_Trang1..2.png`, `Ban_goc_Hung_F6.png` |
| `Ket_qua_kiem_tra_tu_dong.txt` | Kết quả kiểm tra tự động của 4 luồng (chạy lại sau khi đổi số: 0 lỗi) |
| `nguon_sinh_so_do.zip` | Spec Python + bộ kiểm tra, để sửa và sinh lại |

Luồng F3 (sự cố thông thường) có tài liệu riêng `F3_Su_Co_Thong_Thuong.md` + thư mục `F3_Su_Co_Thong_Thuong/`; luồng F2 (bảo trì định kỳ) có `F2_Bao_Tri_Dinh_Ky.md` + thư mục `F2_Bao_Tri_Dinh_Ky/` (cả hai dựng 12/10 tối).
