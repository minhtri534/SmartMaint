# F2 — Bảo trì định kỳ (v1, dựng mới 12/10/2026)

**Nhóm:** SmartMaint (FA26SE147) · **Người phụ trách luồng:** Trí · **Ngày:** 12/10/2026
**Ứng với:** luồng 2 trong note thầy ("luồng 2, 3 lồng AI") → F2 **bắt buộc có AI**.
**File kèm:** `F2_Bao_Tri_Dinh_Ky/F2_Bao_Tri_Dinh_Ky.drawio` (4 trang) + `F2_Trang1..4.png`, `Ket_qua_kiem_tra_tu_dong.txt`

> **Quy ước màu trên sơ đồ** (giống F1–F6): xanh lá = Report 3 đã có · xanh dương = mới (đề xuất) · cam = OPEN · xám viền đứt = việc ngoài hệ thống · thẻ Nx = ý trong note thầy · thẻ Gx = góp ý hội đồng Review 1 · thẻ **AI** = điểm AI hỗ trợ.

---

## 1. F2 làm gì, trong một câu

Officer biến **kế hoạch bảo trì năm** (đã được duyệt ngoài hệ thống) thành **lịch chạy cho từng thiết bị**. Đến kỳ, hệ thống tự sinh **lệnh bảo trì**. Technician làm checklist; mục nào **không đạt** thì sửa ngay hoặc sinh case sửa chữa sang F3 / F4. Kết quả được ghi thành **phiếu kiểm tra** và tính **tỷ lệ đúng lịch** cho F6.

| Trang | Nội dung | Ai làm chính |
|---|---|---|
| 1 | Lập kế hoạch (bước ①–⑥) + tạm dừng / kết thúc kế hoạch | Officer phụ trách tòa |
| 2 | Đến kỳ: kiểm tra thiết bị, giữ / dời / bỏ kỳ, sinh lệnh (⑦–⑧) | Hệ thống, Officer |
| 3 | Thực hiện checklist, xử lý mục Không đạt, nghiệm thu, ghi phiếu kiểm tra, tính kỳ sau (⑨–⑬) | Technician, Officer, Hệ thống |
| 4 | Bản đồ trạng thái: (A) kế hoạch · (B) lệnh bảo trì | — |

---

## 2. Research (nguồn thật)

### 2.1. Ai duyệt kế hoạch bảo trì — nằm NGOÀI hệ thống
- Chung cư: kế hoạch bảo trì phần sở hữu chung do Ban quản trị / đơn vị quản lý vận hành lập và **Hội nghị nhà chung cư thông qua** (TT 05/2024/TT-BXD) ([Thư viện Pháp luật – ai thông qua](https://thuvienphapluat.vn/phap-luat-nha-dat/ke-hoach-bao-tri-phan-so-huu-chung-cua-nha-chung-cu-do-ai-thong-qua-8815.html), [lập trên cơ sở nào](https://thuvienphapluat.vn/phap-luat-nha-dat/ke-hoach-bao-tri-phan-so-huu-chung-cua-nha-chung-cu-duoc-lap-va-thong-qua-tren-co-so-nao-8818.html), [ai có trách nhiệm lập](https://thuvienphapluat.vn/phap-luat-nha-dat/chu-the-co-trach-nhiem-lap-ke-hoach-bao-tri-phan-so-huu-chung-cua-nha-chung-cu-la-ai-8817.html)).
- **Áp dụng:** hệ thống **không có bước duyệt kế hoạch** (khớp N22 "không quản lý duyệt, tiền bạc"). Officer chỉ nhập lịch đã được duyệt.

### 2.2. Thiết bị có chu kỳ bắt buộc
- **Thang máy:**
  - Kiểm định định kỳ **≤ 3 năm/lần**, **≤ 2 năm** nếu đã dùng **> 10 năm**.
  - Do tổ chức kiểm định được cấp phép thực hiện ([Thư viện Pháp luật – kiểm định thang máy](https://thuvienphapluat.vn/hoi-dap-phap-luat/kiem-dinh-ky-thuat-an-toan-thang-may-theo-quy-dinh-hien-hanh-215555.html), [TT 12/2021/TT-BLĐTBXH quy trình kiểm định](https://luatvietnam.vn/lao-dong/thong-tu-12-2021-tt-bldtbxh-quy-trinh-kiem-dinh-ky-thuat-an-toan-thang-may-210813-d1.html)).
- **PCCC:**
  - Phương tiện PCCC phải được kiểm tra, bảo dưỡng định kỳ (thực hành phổ biến: hệ thống ≥ 1 lần/năm, bình chữa cháy 6 tháng/lần).
  - Văn bản PCCC thay đổi nhiều trong 2024–2025 (Luật PCCC và CNCH 2024, TCVN 3890:2023), nên **con số cụ thể để OPEN P03** ([TCVN 3890:2023](https://lawnet.vn/tcvn/TCVN-3890-2023-Fire-protection-Fire-protection-equipments-building-construction-E09BE.html), [tổng hợp tiêu chuẩn PCCC](https://luatminhkhue.vn/tieu-chuan-phong-chay-chua-chay-cho-nha-va-cong-trinh.aspx)).
- **Máy phát điện dự phòng:**
  - Chạy thử **hằng tháng ≥ 30 phút** theo NFPA 110. Đây là tiêu chuẩn Mỹ, chỉ dùng làm tham chiếu quốc tế ([Minnesota Dept. of Health – NFPA 110 gensets](https://health.state.mn.us/facilities/regulation/engineering/lscgensets24.pdf), [W Power – NFPA 110 testing](https://www.wpowerproducts.com/engine-and-generator-service/nfpa-110-routine-maintenance-and-operational-testing/)).
- **Áp dụng:**
  - Thiết bị bắt buộc kiểm định có **giới hạn chu kỳ tối đa**: hệ thống chặn chu kỳ dài hơn.
  - Người làm phải là bên có chứng chỉ.
  - **Không được bỏ kỳ**, chỉ được dời trong cửa sổ đúng hạn (Trang 1 `dMand`, Trang 2 `o_skip`, `o_move`).

### 2.3. Lịch CỐ ĐỊNH và lịch TRƯỢT
- Phần mềm CMMS thật đều có 2 kiểu lịch ([Archibus – fixed / floating scheduling](https://help.archibus.com/user_en/Subsystems/webc/Content/web_user/pm/maint_manager/sched_options.htm), [Accruent Maintenance Connection – schedule configurations](https://help.accruent.com/mc/Content/MCUserGuide/Preventive%20Maintenance/Overview/Preventive%20Maintenance%20Schedule%20Configurations.htm)):
  - **Cố định:** kỳ sau = **ngày đến hạn** + chu kỳ. Làm trễ cũng không đẩy lịch. Hợp với kiểm định và PCCC.
  - **Trượt:** kỳ sau = **ngày làm xong** + chu kỳ. Hợp với vệ sinh điều hòa, thay lọc.
- **Áp dụng:** Officer chọn kiểu lịch khi lập (bước ②). Thiết bị bắt buộc kiểm định chỉ được dùng lịch **cố định** (P02).

### 2.4. Đo "đúng lịch" — PM compliance và quy tắc 10%
- PM compliance = số lệnh định kỳ làm xong **trong cửa sổ cho phép** / số lệnh đến hạn.
- Cửa sổ thường dùng là **± 10% chu kỳ**: chu kỳ 30 ngày thì trễ tối đa 3 ngày.
- Mục tiêu ngành thường ≥ 80–90% ([MaintainX – PM compliance](https://www.getmaintainx.com/blog/preventive-maintenance-pm-compliance), [Fiix – PM compliance](https://fiixsoftware.com/blog/advanced-metrics-pm-compliance/), [Plant Services – 30 days' grace? not with a PM](https://www.plantservices.com/home/blog/11300684/30-days-grace-sorry-not-so-with-a-pm)).
- **Áp dụng:**
  - Mỗi kế hoạch có **cửa sổ đúng hạn ± 10%** (bước ⑤).
  - Phiếu kiểm tra được đánh dấu ĐÚNG HẠN hoặc TRỄ (⑬).
  - **Kỳ bị BỎ vẫn nằm ở mẫu số**, nên không thể "làm đẹp" KPI bằng cách bỏ kỳ.
  - Kỳ bị DỜI được đếm riêng.
  - Định nghĩa này chốt ở **S03** của F6 (Hùng).

---

## 3. Góp ý hội đồng / note thầy được thể hiện ở đâu

| Mã | Nội dung | Ở đâu trong F2 |
|---|---|---|
| **G3** | "Vừa sửa hôm qua, hôm nay lại kiểm tra?" — bảo trì phải biết lịch sử sửa chữa | Trang 2: hình thoi "Đang sửa / có case hoặc lệnh kỳ trước còn mở / vừa sửa ≤ N ngày?" → Officer **giữ / dời / bỏ** kỳ (có AI gợi ý) |
| **G5** | Lịch sử thiết bị | Trang 2 hiện case gần nhất; Trang 3 bước ⑬ ghi lịch sử từ phiếu kiểm tra |
| **G6** | Checklist theo loại thiết bị | Trang 1: mẫu checklist theo loại thiết bị ở F1 (Admin), kế hoạch **sao chép** từ mẫu |
| **G7** | Đo thời gian và đúng lịch | Thời gian chuẩn ở mẫu; ⑪ so thực tế với chuẩn; ⑬ cờ đúng hạn / trễ → F6 |
| **N1** | Technician chỉ làm việc thông thường | Trang 1 ④: thiết bị chuyên ngành (thang máy, PCCC, kiểm định) → bên thứ 3 có hợp đồng, Technician làm đầu mối |
| **N11** | Chỉ Officer phụ trách tòa | Officer chỉ lập kế hoạch cho tòa mình; thông báo chỉ gửi người phụ trách |
| **N20** | Luồng 2 lồng AI | 3 ô AI (mục 4) |
| **N21** | Sửa xong phải ghi nhận | Trang 3: "Sửa ngay + ghi nhận sửa chữa trong lệnh" |
| **N22** | Không duyệt, không tiền | Không có bước duyệt kế hoạch; không lưu chi phí hợp đồng |
| **N23** | Phạm vi / khu thuê | Trang 1 ③ đánh dấu thiết bị ở khu thuê; Trang 2 báo trước và hẹn giờ vào |
| **BR-06** | Tự sinh lệnh trước lead time, bỏ qua duyệt | Trang 2 ⑦–⑧: lệnh vào thẳng Approved / Assigned, không có Pending / Rejected |
| **BR-07** | Mục Fail → case sửa chữa | Trang 3: "Tick tạo case sửa chữa" → F3 (thường) / F4 (nguy hiểm) |

---

## 4. Phạm vi AI trong F2 (N20) — AI chỉ gợi ý, người quyết định

| Ô AI | Ở đâu | AI dùng dữ liệu gì | Ra gì | Ai quyết định |
|---|---|---|---|---|
| Gợi ý chu kỳ + mục checklist | Trang 1 ② | Loại thiết bị, tuổi, lịch sử hỏng (F3), quy chuẩn tham chiếu trong mẫu | Chu kỳ đề xuất + mục kiểm tra nên thêm, kèm lý do | Officer |
| Gợi ý GIỮ hay DỜI kỳ | Trang 2 (G3) | Case sửa gần nhất, ngày sửa, kết quả, tuổi thiết bị, mức ảnh hưởng | "Giữ" / "Dời đến ngày X" kèm lý do | Officer |
| Gợi ý nguyên nhân / hướng xử lý mục Không đạt | Trang 3 ⑩ | Mục checklist không đạt, ảnh, lịch sử thiết bị | Nguyên nhân có thể + hướng xử lý (định vị sự cố) | Technician |

**Không cho AI:**
- Tự đổi chu kỳ.
- Tự bỏ hoặc dời kỳ.
- Tự đánh dấu Đạt / Không đạt.
- Tự chuyển thiết bị sang Out of service.

Lý do: những việc này có trách nhiệm pháp lý (kiểm định) hoặc ảnh hưởng an toàn.

**Gợi ý code:** AI gateway (kernel, Trí giữ) chạy **bất đồng bộ** (Replicate). Nếu AI lỗi, màn hình vẫn dùng được như không có AI.

---

## 5. Đường chính 13 bước

| Bước | Ai | Làm gì | Trạng thái sau |
|---|---|---|---|
| ① | Officer | Chọn tòa (chỉ tòa mình) + loại thiết bị; có mẫu thì hệ thống điền sẵn (sao chép) | — |
| ② | Officer (+AI) | Đặt chu kỳ + kiểu lịch cố định / trượt; thiết bị bắt buộc kiểm định → hệ thống chặn chu kỳ quá quy chuẩn | — |
| ③ | Officer | Chọn thiết bị áp dụng (nhiều thiết bị / 1 kế hoạch), bỏ thiết bị Retired, đánh dấu khu thuê | — |
| ④ | Officer | Người thực hiện: Technician nội bộ mặc định hoặc bên thứ 3 có hợp đồng + Technician đầu mối | — |
| ⑤ | Officer | Ngày bắt đầu, lead time (mặc định 3 ngày), cửa sổ ± 10%, thời gian chuẩn; hệ thống xem trước 5 kỳ, dời kỳ rơi vào ngày nghỉ, cảnh báo trùng | — |
| ⑥ | Officer | Kích hoạt | **KH Đang chạy** |
| ⑦ | Hệ thống (job mỗi ngày) | Kỳ nào tới (ngày đến hạn − lead time)? Thiết bị thanh lý → bỏ; đang sửa / lệnh kỳ trước còn mở / vừa sửa ≤ N ngày → Officer giữ / dời / bỏ (+AI) | — |
| ⑧ | Hệ thống | Sinh lệnh (1 thiết bị = 1 lệnh), checklist sao chép, báo trước khách nếu ảnh hưởng | **Assigned** (có người mặc định) / **Approved** → Officer giao |
| ⑨ | Technician | Nhận việc (từ chối → về Approved, Officer giao lại) → bắt đầu | Accepted → In Progress |
| ⑩ | Technician (+AI) | Làm checklist Đạt / Không đạt / N/A; Không đạt bắt buộc ghi chú + ảnh; bên thứ 3 làm thì điền theo biên bản + đính kèm | In Progress |
| ⑪ | Technician | Mục Không đạt: sửa ngay (ghi nhận) hoặc tick tạo case → F3 / F4; báo hoàn thành | **Completed** |
| ⑫ | Officer | Xác nhận: đạt → đóng; không đạt → mở lại | **Closed** / Reopened |
| ⑬ | Hệ thống | Tạo phiếu kiểm tra (PM-05/06), ghi lịch sử thiết bị, tính kỳ sau theo kiểu lịch, đánh dấu đúng hạn / trễ → F6 | — |

**Trạng thái:**
- **Lệnh định kỳ** dùng chung 11 trạng thái của case nhưng **không có Pending / Rejected**.
- **Kế hoạch** có 3 trạng thái riêng:
  - KH Đang chạy
  - KH Tạm dừng: không sinh lệnh mới; tiếp tục thì kỳ kế tính lại từ ngày tiếp tục.
  - KH Kết thúc: lệnh đã sinh vẫn làm tiếp. Kế hoạch tự kết thúc khi thiết bị cuối cùng bị thanh lý.

---

## 6. Danh sách OPEN (mã khớp với sơ đồ)

| Mã | Trang | Câu hỏi | Đề xuất mở đầu | Ai chốt |
|---|---|---|---|---|
| P01 | 1 | Mọi loại thiết bị bắt buộc có mẫu checklist (G6) hay cho nhập tay? | Bắt buộc với thiết bị bắt buộc kiểm định; còn lại cho nhập tay | Nhóm (với Huy – F1) |
| P02 | 1 | Lịch cố định hay trượt? | Officer chọn; thiết bị bắt buộc kiểm định chỉ được cố định | Nhóm |
| P03 | 1 | Danh sách thiết bị bắt buộc + chu kỳ tối đa: ai nhập, theo văn bản nào (PCCC đổi 2024–2025)? | Admin nhập ở mẫu F1, ghi số hiệu văn bản | **Thầy** |
| P04 | 2 | "Vừa sửa" là bao nhiêu ngày (N)? Case sửa đã kiểm tra đủ hạng mục có được tính thay kỳ này? | N = cửa sổ đúng hạn (10% chu kỳ); tính thay chỉ khi checklist đủ | **Thầy** (G3) |
| P05 | 2 | Báo trước khách bao lâu? | = lead time (3 ngày); cắt điện / nước toàn tòa theo nội quy | Nhóm |
| P06 | 3 | Mục Không đạt nguy hiểm: hệ thống tự chuyển thiết bị Out of service hay Officer chuyển? | Hệ thống đề xuất, Officer xác nhận (tránh khóa nhầm thang máy) | Nhóm (với Tùng – F4) |
| P07 | 3 | Bên thứ 3 làm hằng tháng có cần mỗi lần một lệnh? | Có, để đo đúng lịch và lưu biên bản | Nhóm |
| P08 | 3 | Officer xác nhận mọi lệnh hay chỉ lệnh có mục Không đạt / bên thứ 3? | Mọi lệnh, cho xác nhận hàng loạt | Nhóm |
| P09 | 3 | Lệnh định kỳ có đánh giá của khách? | Không (như O01 của F3) | Nhóm |

**Câu nên gửi thầy trước:**
- **P04:** chính là G3 của hội đồng.
- **P03:** có yếu tố pháp lý.

---

## 7. Phải sửa gì trong Report 3 (chờ họp chốt)

| Chỗ trong R3 | Hiện tại | Đề xuất | Lý do |
|---|---|---|---|
| PM-02 Create / Edit Plan | Nhập checklist tay, 1 chu kỳ | Thêm: chọn **mẫu** theo loại thiết bị, **kiểu lịch** cố định / trượt, **cửa sổ ± %**, thời gian chuẩn, người thực hiện (nội bộ / bên thứ 3), xem trước kỳ | G6, G7, mục 2.3–2.4 |
| PM-01 / PM-03 | — | Trạng thái kế hoạch Đang chạy / Tạm dừng / Kết thúc + nút tương ứng (bắt buộc lý do) | Trang 1 |
| BR-06 | Sinh lệnh trước lead time | Thêm: kiểm tra thiết bị thanh lý / đang sửa / vừa sửa → Officer giữ / dời / bỏ; không bỏ được kỳ kiểm định bắt buộc; Officer không chọn thì vẫn sinh | G3 |
| BR-07 | Fail → case | Thêm lựa chọn **sửa ngay + ghi nhận** cho lỗi nhỏ; case sinh ra có mức thường / nguy hiểm (→ F4) | N21, Trang 3 |
| TEC-05 | Checklist | Đạt / Không đạt / **N/A**; Không đạt bắt buộc ghi chú + ảnh; đính kèm biên bản bên thứ 3 + hạn kiểm định kế | Trang 3 ⑩ |
| PM-05 / PM-06 | Bản ghi kiểm tra | Thêm cờ **đúng hạn / trễ**, kỳ bỏ / dời + lý do | G7 |
| ERD | PreventivePlan, InspectionRecord | PreventivePlan thêm `schedule_type`, `window_pct`, `lead_days`, `std_minutes`, `status`, `vendor_id`; thêm **ChecklistTemplate** (D16) và **PlanOccurrence** (kỳ: due_date, kết quả giữ / dời / bỏ, lý do) | Trang 1–3 |

---

## 8. Đã kiểm tra thế nào

**Kiểm tra tự động** (cùng bộ kiểm tra với F1–F6):
- Hình thoi đủ nhánh có nhãn.
- Không nút cụt, không vòng kẹt.
- Mọi đổi trạng thái đúng danh sách được phép và đúng người làm.
- Trang 4 khớp Trang 1–3.
- Bước ①–⑬ liền mạch.

Kết quả: **0 lỗi, 0 cảnh báo** (71 nút luồng, 16 hình thoi, 9 OPEN).

**Tự phản biện kiểu hội đồng (đã sửa trên sơ đồ):**
1. **Lệnh kỳ trước chưa xong mà kỳ mới tới** (chu kỳ ngắn: tuần) thì sinh trùng 2 lệnh. Đã sửa: hình thoi G3 thêm "lệnh kỳ trước còn mở" → Officer quyết.
2. **Đồng hồ trễ chỉ gắn In Progress.** Lệnh nằm ở Approved / Assigned quá hạn thì không ai bị nhắc. Đã sửa: "đến hạn mà chưa Completed, dù ở trạng thái nào".
3. **Thiếu Hủy lệnh.** Đã thêm vào bản đồ trạng thái: Officer hủy ở mọi trạng thái trước Closed, bắt buộc lý do; kỳ hủy tính như kỳ BỎ ở F6, để không thể dùng Hủy làm đẹp KPI.
4. **Officer không chọn giữ / dời / bỏ thì mất kỳ.** Đã có đồng hồ: chưa chọn trước ngày đến hạn thì vẫn sinh lệnh.

**Còn yếu, nói thẳng:**
- Con số pháp lý PCCC chưa xác minh với văn bản 2024–2025 (P03).
- "N ngày vừa sửa" là đề xuất của nhóm, chưa có nguồn (P04).

---

## 9. Câu hội đồng dễ hỏi nhất

1. **"Thang máy vừa sửa hôm qua, hôm nay lịch kiểm tra thì sao?"** (G3)
   - Job ngày phát hiện case vừa đóng ≤ N ngày và báo Officer kèm case gần nhất; AI gợi ý giữ hay dời.
   - Officer quyết định. Riêng kiểm định bắt buộc thì không được bỏ, chỉ dời trong cửa sổ.
2. **"Bỏ kỳ thì KPI đúng lịch có đẹp lên không?"**
   - Không. Kỳ bỏ hoặc hủy vẫn ở mẫu số; kỳ dời được đếm riêng.
3. **"Ai duyệt kế hoạch?"**
   - Không ai duyệt trong hệ thống. Kế hoạch năm được thông qua ngoài hệ thống (chung cư: Hội nghị nhà chung cư), hệ thống chỉ chạy lịch (N22).
4. **"Thang máy do công ty bảo trì làm thì Technician làm gì?"**
   - Làm đầu mối: có mặt, nhận biên bản / tem, điền checklist theo biên bản. Lệnh vẫn có trong hệ thống để đo đúng lịch và lưu lịch sử (P07).
5. **"Checklist phát hiện phanh thang máy yếu?"**
   - Tick "Nguy hiểm" thì hệ thống tạo case sang F4 và thiết bị Out of service (ai bấm: P06).
6. **"Khác gì F3?"**
   - F3 bắt đầu từ **khách báo**: có Pending, Officer duyệt.
   - F2 bắt đầu từ **lịch**: hệ thống tự sinh, không có Pending / Rejected.
   - Cả hai dùng chung máy trạng thái case từ Assigned trở đi, nên code dùng chung kernel.

---

## 10. Việc tiếp theo

1. Họp chốt P01–P09 (gửi thầy P03, P04).
2. Huy (F1) thêm mẫu checklist theo loại thiết bị + cờ "bắt buộc kiểm định" + chu kỳ tối đa (nối A05).
3. Hùng (F6) chốt S03 theo định nghĩa mục 2.4.
4. F2 thuộc package P2 (tuần 8–9). Tuần 6 chỉ cần đưa các bảng `PreventivePlan`, `PlanOccurrence`, `InspectionRecord`, `ChecklistTemplate` vào ERD.
