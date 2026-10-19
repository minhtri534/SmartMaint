# F3 — Xử lý sự cố thông thường (v3, theo note thầy 12/10/2026)

> **Cập nhật 12/10 (tối) — v3.1:** (1) **Leader chốt F5 là nguồn duy nhất** cho bảo hành / bên thứ 3 / báo giá / thay thiết bị → **Trang 3 của F3 rút gọn**: chỉ còn Officer phân loại lại phạm vi sau khi Technician khảo sát, rồi Hủy (ngoài phạm vi) · Technician tiếp tục (trong khả năng) · **→ F5**. O11–O13 chuyển sang F5. (2) Sơ đồ **dựng lại từ tài liệu này** (file bạn bỏ vào folder là bản 05/10 cũ), đã đổi số sang F3: `F3_Xu_Ly_Su_Co_Thong_Thuong.drawio`, `F3_Trang1..5.png`. (3) Case sinh từ **F2** (mục Không đạt, BR-07) vào Trang 1 như cách 3.
>
> Cập nhật 12/10 (17h): đổi sang số luồng chốt (xem `Danh_Sach_Luong_Chot.md`). Luồng này trước đó đánh số F2 (bản 12/10 sáng) và F1 (brief 03/10).

**Nhóm:** SmartMaint (FA26SE147) · **Người phụ trách luồng:** Trí · **Ngày:** 12/10/2026
**Thay thế:** bản ngày 05/10 (khi đó gọi là F1 theo brief; đã xóa ngày 12/10)
**File kèm:** sơ đồ 5 trang `F3_Su_Co_Thong_Thuong/F3_Xu_Ly_Su_Co_Thong_Thuong.drawio` + `F3_Trang1..5.png`, `Ket_qua_kiem_tra_tu_dong.txt`

> **Số luồng chốt (12/10, theo thứ tự note thầy):** F1 Cấu trúc tòa nhà, thiết bị và danh mục dịch vụ (Huy) · F2 Bảo trì định kỳ (Trí) · **F3 Sự cố thông thường (tài liệu này, Trí)** · F4 Sự cố khẩn cấp (Tùng) · F5 Sửa hay thay / chuyển bên thứ 3 (Hùng) · F6 Giám sát hiệu suất và thống kê (Hùng).
>
> **Quy ước màu trên sơ đồ:** xanh lá = Report 3 đã có · xanh dương = mới (đề xuất hoặc thầy chốt) · cam = OPEN · xám viền đứt = việc diễn ra ngoài hệ thống. **Thẻ xanh ngọc N1–N26** = ý trong note thầy (bảng mục 1) · thẻ tím G1–G7 = góp ý hội đồng Review 1 · thẻ **AI** = điểm AI hỗ trợ.

---

## 1. Note của thầy được thể hiện ở đâu

| Mã | Ý trong note | Thể hiện trên sơ đồ | Tình trạng |
|---|---|---|---|
| N1 | Technician chỉ xử lý sự cố thông thường; vượt khả năng thì sao | Trang 2: hình thoi "Trong khả năng?" → nhánh "Vượt khả năng / cần thay / lỗi do khách" → Trang 3 (phân loại) → F5 | Đã làm |
| N2 | Bỏ quản lý kho | Trang 2: linh kiện chỉ ghi tay trong báo cáo sửa; ghi chú "mua ngoài hệ thống" | Đã làm |
| N3 | Hỏng nặng cần thay → bên thứ 3, hệ thống chuyển báo giá cho khách | Trang 3 → **F5** (nguồn duy nhất) | Đã làm ở F5; câu hỏi O13 chuyển sang F5 |
| N4 | Luồng thiết bị (brief gọi F5) đẩy lên đầu | Số luồng chốt theo thứ tự note thầy (khung trên) | Đã chốt 12/10 |
| N5 | Officer gộp case trùng | Trang 1: "Gộp vào case chính" | Đã làm; có O03 |
| N6 | Bỏ Need more info, chỉ Reject → khách gửi lại | Trang 1: lý do từ chối "Thiếu thông tin" + nút "Gửi lại"; Trang 5 còn 11 trạng thái | Đã làm; có O05 |
| N7 | Ghi rõ mức độ khẩn cấp | Trang 1: bảng 4 mức (mục 3.2 có nguồn) | Đã làm |
| N8 | BR chỉ làm giờ hành chính | Trang 1–2: đồng hồ chỉ đếm giờ hành chính; F4 không áp dụng | Đã làm; có O04 |
| N9 | O8 cũ → đổi lịch / thuê ngoài | Trang 2: "Không có người phù hợp → dời lịch"; thuê ngoài qua Trang 3 → F5 | Đã làm |
| N10 | Manager xem số lần Technician từ chối + lý do | Trang 2, làn Manager | Đã làm |
| N11 | Thông báo chỉ Officer phụ trách tòa | Trang 1–2: bỏ hàng chờ chung, ghi chú thông báo | Đã làm; có O02 |
| N12 | Technician xem được lịch sử thiết bị | Trang 2, bước ⑧ | Đã làm (sửa mâu thuẫn M2 cũ) |
| N13 | On Hold cần research | Trang 2–3: đề xuất On Hold theo research (mục 3.4) | Đề xuất; chờ chốt O07 |
| N14 | O16 cũ chưa giải quyết (Manager và escalation) | Trang 2: O10 | Giữ OPEN |
| N15 | Thay thiết bị thì case vẫn In Progress (không chắc) | O12 (nay ở F5): nhóm đề xuất On Hold vì research | Chờ thầy chốt |
| N16 | "Đúng hạn" lấy hạn dài hơn | Trang 2: "hạn có hiệu lực = hạn dài hơn", dùng chung cho cờ đỏ, thông báo, KPI | Đã làm; có O09 |
| N17 | Văn phòng cho thuê + chung cư | Bối cảnh toàn bộ; mục 3.1 | Đã làm |
| N18 | Research CSVC từ nguồn thật | Mục 3 (có URL) | Đã làm |
| N19 | Nhân viên CSVC = ban tòa nhà; Employee = khách / người thuê | Tên làn "Khách hàng (người thuê / cư dân)" | Đã làm |
| N20 | Luồng 2, 3 lồng AI; gạch phạm vi AI (theo thứ tự của thầy: luồng 2 = **bảo trì định kỳ F2**, luồng 3 = **xử lý sự cố F3/F4/F5**) | 3 ô AI ở F3; mục 4 | Đã làm (F3); F4 do Tùng; F2 đã vẽ 12/10 với 3 ô AI (gợi ý chu kỳ, giữ / dời kỳ, mục Không đạt) — xem `F2_Bao_Tri_Dinh_Ky.md` |
| N21 | Sau khi sửa phải có ghi nhận nội dung sửa chữa | Trang 2: "Ghi nhận sửa chữa", ô NỘI DUNG ĐÃ LÀM bắt buộc | Đã làm |
| N22 | Không quản lý thuê duyệt, tiền bạc | F5: không lưu số tiền, không nút duyệt, chỉ ghi nhận quyết định và bằng chứng | Đã làm |
| N23 | Phân biệt lỗi thuộc phạm vi tòa nhà hay không | Trang 1 "Trong phạm vi dịch vụ của tòa?" + ghi bên chịu chi phí; Trang 3 Officer phân loại lại sau khi Technician khảo sát | Đã làm |
| N24 | Danh mục dịch vụ cố định theo lỗi thường gặp | Trang 1: khách chọn loại dịch vụ; mục 3.3 | Đã làm |
| N25 | Thợ chỉ sửa sự cố thông thường | Trùng N1 | Đã làm |
| N26 | Khẩn cấp phụ thuộc danh sách dịch vụ và loại thiết bị | Trang 1: loại dịch vụ trong danh sách khẩn cấp → F4 **ngay khi tạo case**, bỏ qua Pending | Đã làm |

---

## 2. Thay đổi lớn so với bản ngày 05/10

1. **Khách hàng là người thuê / cư dân**, không còn là nhân viên nội bộ. Officer, Technician, Manager là nhân viên ban tòa nhà.
2. **Còn 11 trạng thái**: bỏ Need more info. Thiếu thông tin thì Officer từ chối với lý do "Thiếu thông tin"; khách bấm "Gửi lại" để tạo case mới điền sẵn nội dung cũ.
3. **Officer phụ trách theo tòa**, bỏ hàng chờ chung (BR-19 cũ). Thông báo chỉ gửi người phụ trách.
4. **Sự cố khẩn cấp rẽ sang F4 ngay khi tạo case** nếu loại dịch vụ nằm trong danh sách khẩn cấp; Officer vẫn có thể nâng lên Khẩn cấp khi duyệt.
5. **Gộp case trùng** thay cho từ chối "Trùng".
6. **Trang 3 = Officer phân loại lại sau khảo sát**, thay cho nhánh "→ sửa hay thay" của bản 05/10: ngoài phạm vi → Hủy kèm lý do; trong khả năng → Technician tiếp tục; trong phạm vi nhưng vượt khả năng / cần thay → **F5** (ghi nhận bên thứ 3, báo giá, quyết định của bên chịu chi phí, thi công, nghiệm thu — vẽ một chỗ duy nhất ở F5, chốt 12/10).
7. **Danh mục dịch vụ** thay cho "loại sự cố": mỗi loại có mức khẩn cấp mặc định và bên chịu chi phí mặc định.
8. **Ghi nhận sửa chữa** có thêm ô bắt buộc "Nội dung đã làm"; ảnh trước / sau.
9. **"Hạn có hiệu lực"** = hạn dài hơn giữa hạn SLA gốc và hạn Officer đã sửa, dùng thống nhất cho cờ đỏ, thông báo và KPI.
10. **AI** gắn vào 3 điểm trong quy trình (mục 4), luôn chỉ gợi ý.

---

## 3. Kết quả research (nguồn thật, không suy diễn từ tài liệu của nhóm)

### 3.1. Cơ sở vật chất và ranh giới trách nhiệm

**Tòa nhà văn phòng cho thuê.** Các hệ thống thường có: điện (trạm biến áp, máy phát dự phòng, ATS, tủ phân phối), điều hòa trung tâm, thang máy, cấp thoát nước và bơm, PCCC (báo cháy, sprinkler, bơm, họng nước vách tường, đèn Exit), an ninh / camera / kiểm soát ra vào, chiếu sáng khu chung, BMS, bãi xe, khu vệ sinh / sảnh, tiện ích chung. Tòa nhà chịu trách nhiệm hệ thống chung đến điểm vào khu thuê. Khách thuê chịu phần fit-out và thiết bị lắp thêm cho riêng mình (vách ngăn, trần, thiết bị văn phòng, điều hòa bổ sung).
Nguồn: [Colliers qua VnExpress](https://vnexpress.net/loay-hoay-tim-chuan-xep-hang-van-phong-2706789.html), [BOMA Canada](https://bomacanada.ca/wp-content/uploads/2023/04/building_classification14ang-3.pdf), [Queensland Gov – fitout maintenance](https://www.forgov.qld.gov.au/property-land-and-infrastructure/manage-government-buildings-and-assets/office-accommodation-management-framework/guideline-3-fitout/10.-fitout-maintenance), [Landager – commercial maintenance obligations](https://landager.com/en/property-compliance/australia/new-south-wales/commercial-maintenance-obligations), [Thư viện nhà đất – chi phí thuê văn phòng](https://thuviennhadat.vn/cam-nang-nha-dat/cac-chi-phi-khi-thue-van-phong-o-ho-chi-minh-643529.html). Việt Nam chưa có tiêu chuẩn nhà nước phân hạng cao ốc văn phòng.

**Chung cư** (căn cứ hiện hành: Luật Nhà ở 2023 và Thông tư 05/2024/TT-BXD; **Thông tư 02/2016 đã bị bãi bỏ**, không được dẫn).
- Phần sở hữu chung, do đơn vị quản lý vận hành bảo trì: kết cấu chịu lực, hành lang, cầu thang, thang máy, hệ thống điện / nước / thoát nước / PCCC, hạ tầng kết nối, sân vườn chung (Điều 142 Luật Nhà ở 2023).
- Phần sở hữu riêng (trong căn hộ, thiết bị riêng): chủ sở hữu tự bảo trì (Điều 130).
- Ví dụ ranh giới: rò nước xuyên tầng do căn hộ thì chủ căn hộ chịu; do ống đứng / trục kỹ thuật thì đơn vị quản lý chịu.
Nguồn: [LSVN – sở hữu riêng / chung](https://lsvn.vn/tu-01-01-2025-phan-so-huu-rieng-va-so-huu-chung-trong-nha-chung-cu-duoc-quy-dinh-ra-sao-1722678788-a145953.html), [Thư viện pháp luật – bảo trì phần riêng](https://thuvienphapluat.vn/phap-luat-nha-dat/ai-co-trach-nhiem-bao-tri-phan-so-huu-rieng-trong-nha-chung-cu-7399.html), [Văn bản hợp nhất TT 05/2024 (Bộ Xây dựng)](https://moc.gov.vn/Images/FileVanBan/BXD_17-VBHN-BXD_24032026(1).pdf), [Thư viện nhà đất – hoạt động quản lý vận hành](https://thuviennhadat.vn/phap-ly-nha-dat/cac-hoat-dong-quan-ly-van-hanh-nha-chung-cu-moi-nhat-634603.html), [Luật Long Phan – rò rỉ xuyên tầng](https://luatlongphan.vn/yeu-cau-boi-thuong-khi-ro-ri-nuoc-xuyen-tang-gay-hu-hong-tai-san).

**Bằng chứng gộp hai loại tòa nhà là thực tế:** Savills Phú Mỹ Hưng tuyển một vị trí quản lý nhận yêu cầu của cả khách thuê văn phòng lẫn cư dân ([HotelJob](https://www.hoteljob.vn/viec-lam/238937-savills-pmh-quan-ly-toa-nha)).

→ **Áp dụng vào F3:** câu hỏi "Trong phạm vi dịch vụ của tòa?" và "bên chịu chi phí" dựa trên đúng ranh giới này. Danh mục dịch vụ ghi sẵn ranh giới mặc định cho từng loại.

### 3.2. Mức khẩn cấp

Không tìm được chính sách phân mức công khai của công ty quản lý tòa nhà Việt Nam, nên bảng dưới tham chiếu quốc tế. Gần như mọi nguồn đều dùng 3–4 mức, và mức cao nhất luôn gắn với nguy hiểm tính mạng, an ninh hoặc thiệt hại lan rộng.

| Mức | Định nghĩa đề xuất | Ví dụ | Xử lý |
|---|---|---|---|
| **Khẩn cấp** | Nguy hiểm người / an ninh / thiệt hại lan rộng | Rò gas, cháy, vỡ ống, ngập, kẹt thang máy, mất điện cả tòa | → F4, kể cả ngoài giờ (hotline / người trực) |
| **Cao** | Mất chức năng chính hoặc nhiều người bị ảnh hưởng | Mất điện / nước một khu, tắc cống hoàn toàn, điều hòa hỏng cả tầng | F3, ưu tiên trong giờ hành chính |
| **Bình thường** | Một người / khu nhỏ, vẫn dùng được | Ổ cắm, đèn lẻ, vòi rỉ, bồn cầu chảy | F3 |
| **Thấp** | Thẩm mỹ, có thể hẹn lịch | Sơn, trần ố, tấm trần lệch | F3, hẹn lịch |

Nguồn: [Queensland Housing – response times](https://www.qld.gov.au/housing/public-community-housing/public-housing-tenants/looking-after-your-home/maintenance/property-maintenance/response-times), [SDSU Facilities – prioritization](https://facilities.sdsu.edu/services/prioritization), [Prudential Center – maintenance requests](https://www.prudentialcenteroffice.com/handbook/policies-and-procedures/maintenance-requests), [100 Federal St – maintenance requests](https://www.100federaloffice.com/customer-handbook/building-services/maintenance-requests), [Strata Choice – after-hours emergency](https://www.stratachoice.com.au/after-hours-emergency-service/), [Toronto RentSafeTO – response times](https://www.toronto.ca/community-people/housing-shelter/rental-housing-rights-information/housing-property-standards/apartment-building-standards/rentsafeto-for-building-owners/rentsafeto-for-building-owners-service-requests-response-times/).
Giờ làm việc: văn phòng thường 8:00–17:30 hoặc 8:00–18:00 thứ Hai đến thứ Sáu, ngoài giờ phải đăng ký ([Prudential Center – HVAC](https://www.prudentialcenteroffice.com/handbook/policies-and-procedures/hvac)). Các nguồn chung cư đều có hotline sau giờ cho sự cố khẩn cấp, việc không gấp chờ giờ hành chính ([Strata Choice](https://www.stratachoice.com.au/after-hours-emergency-service/)). Đây là cơ sở cho quyết định "F3 chỉ giờ hành chính, F4 không áp dụng".

### 3.3. Danh mục dịch vụ cố định (N24)

Danh mục đề xuất 2 cấp (nhóm → loại), tổng hợp từ các nguồn trên và thống kê yêu cầu sửa chữa phổ biến ([Hemlane](https://hemlane.com/resources/rental-property-maintenance-requests/): nước 25,4%, điều hòa 12,3%, điện 8,4%…):
**Điện** (mất điện, ổ cắm / công tắc, CB nhảy, đèn hỏng) · **Nước** (rò rỉ, tắc, bồn cầu, bình nóng lạnh) · **Điều hòa** (không mát, chảy nước) · **Thang máy** (kẹt, hỏng) · **PCCC** (báo cháy giả, thiết bị hỏng) · **Cửa và khóa** · **Thấm dột** · **Vệ sinh** · **Côn trùng** · **An ninh** (cửa từ, camera).

Mỗi loại dịch vụ có: mức khẩn cấp mặc định · có thuộc danh sách khẩn cấp không (→ F4) · bên chịu chi phí mặc định (tòa / khách / ngoài phạm vi) · áp dụng cho văn phòng / chung cư / cả hai · trường bắt buộc (ví dụ bắt buộc ảnh với "rò rỉ"). **Mẫu checklist (G6) KHÔNG đặt theo loại dịch vụ mà theo LOẠI THIẾT BỊ** ở F1, dùng cho bảo trì định kỳ F2 (chốt 12/10, mục 6 dòng 11 của `Doi_Chieu`). Khi tạo case, hệ thống **sao chép** các giá trị mặc định vào case, để sửa danh mục sau này không làm đổi case cũ ([Limble – advanced work request settings](https://help.limblecmms.com/en/articles/8677811-advanced-work-request-settings), [Atlassian – service catalog](https://www.atlassian.com/itsm/service-request-management/service-catalog)).

### 3.4. On Hold (N13)

- Sản phẩm thật đều có On Hold kèm lý do: chờ vật tư, chờ bên thứ 3, chờ người yêu cầu… ([Xenia](https://help.xenia.team/hc/en-us/articles/49715444987931-Work-Order-Statuses-Explained), [Brightly / Siemens](https://help.assetmanagement.siemens.com/help/Content/Documentation/Maintenance/Work%20Order/Daily%20Functions/Work%20Order%20Status%20Definitions.htm), [ServiceNow](https://www.servicenow.com/community/incident-management-forum/on-hold-sla-best-practices/td-p/2877776)).
- Lý do phải là **trường có cấu trúc**, không để trong bình luận. MaintainX bị người dùng phàn nàn vì thiếu trường này ([MaintainX community](https://community.getmaintainx.com/product-q-a-6/not-enough-status-372)).
- Đồng hồ SLA: thực hành phổ biến là **chỉ dừng khi chờ chính người yêu cầu** ([Jira SM – ví dụ SLA](https://confluence.atlassian.com/servicemanagementserver/example-sla-configurations-1318391612.html), [ServiceNow – pause conditions](https://www.servicenow.com/community/incident-management-forum/incident-on-hold-and-sla-pause-conditions/m-p/2871817)).

**Đề xuất cho SmartMaint** (khác BR-08 hiện tại vốn dừng SLA với mọi lý do, chờ nhóm duyệt O07):
- Lý do On Hold là danh sách cố định: chờ linh kiện / chờ khách cho vào / chờ bên thứ 3 / chờ Officer quyết định.
- Bắt buộc ngày dự kiến tiếp tục; "chờ khách cho vào" phải có lịch hẹn.
- SLA chỉ dừng khi chờ phía khách; các lý do còn lại SLA vẫn chạy.
- Quá ngày dự kiến thì nhắc Technician và Officer; báo cáo số lần và tổng thời gian On Hold theo từng người.

### 3.5. Chuyển bên thứ 3 và báo giá (N3, N22)

- UpKeep và MaintainX cho chia sẻ work order với vendor qua link; vendor upload báo giá hoặc hóa đơn dưới dạng file, không có bước duyệt trong tài liệu ([UpKeep – external vendors](https://help.onupkeep.com/en/articles/11962183-sharing-work-orders-with-external-vendors-or-contractors), [MaintainX – external work orders](https://help.getmaintainx.com/external-work-orders)).
- Facilio: người quản lý xem lại báo giá của vendor rồi mới gửi khách thuê ([Facilio – quote](https://facilio.com/help/docs/user-guide/facilio-modules/quote/overview/)).
- ServiceNow và Xenia coi "chờ vendor" là một lý do On Hold.

**Áp dụng (nay nằm ở F5, F3 Trang 3 chỉ chuyển sang):**
- Hệ thống chỉ ghi nhận: tên / liên hệ bên thứ 3, lý do, ngày chuyển, file báo giá, quyết định của bên chịu chi phí kèm bằng chứng.
- Không lưu số tiền, không có luồng duyệt, không thanh toán.
- Thiết bị có hợp đồng bảo trì trọn gói (thang máy, PCCC) thì bỏ qua báo giá.

### 3.6. Gộp case trùng (N5)

- Zendesk merge: phiếu nguồn bị đóng, **không hoàn tác được** ([Zendesk – merging tickets](https://support.zendesk.com/hc/en-us/articles/4408882445594-Merging-tickets)).
- ServiceNow parent / child: phiếu cha resolved thì phiếu con tự resolved ([ServiceNow – sync](https://www.servicenow.com/docs/r/QykF2iRSIs6VXztArCSo3w/b7gpurjh65V7AqxYs9Y5Sw)).
- Bẫy cần tránh: đếm SLA hai lần, người báo phiếu con không được thông báo, gộp nhầm khác vị trí.

**Áp dụng:** dùng liên kết case chính – case con, không xóa. Case con bị khóa, hiển thị theo case chính, không tính SLA / KPI; vị trí của case con được thêm vào case chính; khách của case con được báo khi case chính xong.

### 3.7. Phân công theo tòa nhà (N11)

- UpKeep: "auto-assign team by location", có người trực ngoài ca ([UpKeep – FM workflows](https://help.onupkeep.com/en/articles/4733841-facilities-management-workflows)).
- Zendesk: agent nghỉ thì phiếu được chuyển về nhóm cha ([Zendesk – out of office](https://support.zendesk.com/hc/en-us/articles/4408881839642-When-an-agent-is-out-of-office-how-can-I-reassign-their-open-tickets)).

**Đề xuất (O02):**
- Mỗi tòa có 1 Officer chính và 1 Officer dự phòng, do Admin gán.
- Officer tự bật "vắng". Khi người chính vắng, người dự phòng nhận.
- Cả hai vắng thì cần một quy tắc dự phòng thứ ba. Đây là chỗ còn hở, vì Manager chỉ xem (BR-12) và Admin không xem được case (BR-11).

---

## 4. Phạm vi AI trong F3 (N20)

Report 3 hiện chỉ có AI dạng chat nổi (COM-07): trả lời theo phạm vi RBAC, chỉ tư vấn, không có nút duyệt / giao / đóng trong chat (BR-15), gọi Replicate bất đồng bộ, AI lỗi thì luồng chính vẫn chạy (4.1). Note thầy yêu cầu **lồng AI vào quy trình**. Nhóm đề xuất 3 điểm cho F3, giữ nguyên các nguyên tắc trên:

| Điểm | Ai dùng | AI làm gì | Ai quyết | Nếu AI lỗi / chậm |
|---|---|---|---|---|
| Khi gửi yêu cầu (Trang 1, bước ①) | Khách | Gợi ý loại dịch vụ và mức khẩn cấp từ mô tả; nhắc nếu thiếu thông tin trước khi gửi (giảm số case bị từ chối vì "Thiếu thông tin") | Khách chọn; Officer chốt mức khẩn cấp | Vẫn gửi bình thường bằng danh mục |
| Khi Officer xem case (bước ③) | Officer | Gợi ý các case có thể trùng; gợi ý "khu chung hay khu riêng" | Officer | Officer tự xem như cũ |
| Khi Technician chẩn đoán (bước ⑧) | Technician | Gợi ý hướng xử lý theo loại dịch vụ và lịch sử thiết bị | Technician | Không ảnh hưởng |

**AI không được:** tự đổi trạng thái, tự duyệt / từ chối / gộp / giao việc, tự quyết bên chịu chi phí, đọc dữ liệu ngoài phạm vi role (BR-15), hay trở thành điều kiện bắt buộc để luồng chạy tiếp. Mọi câu hỏi và gợi ý ghi Audit Log (BR-16).
Cần nhóm cân nhắc: phân tích **ảnh** cần model có thị giác trên Replicate. Bản 1.0 đề xuất chỉ dùng chữ (mô tả). Luồng F4 khẩn cấp (Tùng) nên làm bảng tương tự. Luồng F2 bảo trì định kỳ (Trí) **bắt buộc** có bảng AI riêng: note thầy ghi "AI hỗ trợ lập kế hoạch bảo trì, quy trình bảo trì, đề xuất bảo trì, định vị sự cố" (ví dụ: gợi ý chu kỳ theo loại thiết bị, gợi ý dời kỳ khi thiết bị vừa sửa — G3).

---

## 5. Đường chính 12 bước

| Bước | Ai | Làm gì | Trạng thái sau |
|---|---|---|---|
| ① | Khách | Gửi yêu cầu: vị trí, **loại dịch vụ**, mô tả, ảnh (AI gợi ý) | Pending (loại khẩn cấp → F4 ngay) |
| ② | Officer phụ trách tòa | Nhận thông báo, mở case (chỉ case của tòa mình) | Pending |
| ③ | Officer | Xem chi tiết + lịch sử thiết bị (AI gợi ý trùng / phạm vi) | Pending |
| ④ | Officer | Trùng? (gộp) → Đủ thông tin? → Trong phạm vi? → **Duyệt**: chốt mức khẩn cấp + bên chịu chi phí | Approved (Khẩn cấp → F4) |
| ⑤ | Officer | Giao Technician (không có người → dời lịch) | Assigned |
| ⑥ | Technician | Nhận việc (từ chối thì lý do được lưu cho Manager) | Accepted |
| ⑦ | Technician | Bắt đầu tại hiện trường | In Progress |
| ⑧ | Technician | Chẩn đoán, xem lịch sử thiết bị; trong khả năng thì sửa; chờ linh kiện / chờ khách → On Hold; vượt khả năng → On Hold "chờ Officer quyết định" → Trang 3 → F5 | In Progress / On Hold |
| ⑨ | Technician | Ghi nhận sửa chữa (nội dung đã làm, nguyên nhân, linh kiện ghi tay, giờ công, ảnh trước / sau) → Báo hoàn thành | Completed |
| ⑩ | Officer | Kiểm tra kết quả | Completed |
| ⑪ | Officer | Xác nhận và đóng; ghi lịch sử thiết bị; báo khách (cả khách của case đã gộp) | Closed |
| ⑫ | Khách | Đánh giá (tùy chọn) hoặc báo "Chưa sửa xong" trong 7 ngày | Closed / Reopened |

---

## 6. Danh sách OPEN (đem ra họp; mã khớp với sơ đồ)

| Mã | Trang | Câu hỏi | Đề xuất mở đầu | Ai chốt |
|---|---|---|---|---|
| O01 | 1 | Case tạo từ F2 không có khách: ai nhận thông báo, ai đánh giá? | Người yêu cầu = Officer phụ trách tòa; bỏ đánh giá với loại case này | Nhóm |
| O02 | 1 | Gán Officer chính / dự phòng, ai bật "vắng", cả hai vắng, giữ hay bỏ claim, đổi người phụ trách | Admin gán chính + dự phòng; Officer tự bật vắng; bỏ claim nếu mỗi tòa chỉ 1 người xử lý; đổi người thì case mở chuyển cho người mới, ghi lịch sử | Thầy + Nhóm |
| O03 | 1 | Case chính bị hủy / từ chối khi còn case con; case con có được đánh giá; cờ liên kết hay trạng thái "Merged" | Chặn khách tự hủy case chính khi có case con; dùng cờ liên kết (giữ 11 trạng thái); case con không đánh giá riêng | Nhóm |
| O04 | 1 | Văn phòng và chung cư dùng chung một lịch làm việc? | Mỗi tòa một lịch làm việc (ADM-07 thêm theo tòa); ngoài giờ chỉ F4 | Thầy |
| O05 | 1 | Bỏ Need more info thì giảm từ chối thế nào | Trường bắt buộc theo loại dịch vụ + AI nhắc thiếu; thống kê riêng lý do "Thiếu thông tin"; không thêm bước hỏi | Nhóm |
| O06 | 2 | Technician im lặng | Assigned quá hạn phản hồi đầu → nhắc Technician + báo Officer | Nhóm |
| O07 | 2 | Duyệt đề xuất On Hold (mục 3.4)? | Theo research: lý do có cấu trúc, SLA chỉ dừng khi chờ khách | Nhóm |
| O08 | 2 | Sự cố nặng lên giữa chừng | Officer đổi mức kèm lý do, hạn tính lại theo mức mới; lên Khẩn cấp → F4 | Nhóm |
| O09 | 2 | "Hạn dài hơn" bị lạm dụng | Chỉ dời trước khi quá hạn, lý do chọn từ danh sách; ANA hiện cả tỷ lệ theo SLA gốc và số lần dời | Nhóm |
| O10 | 2 | Manager nhận cảnh báo quá hạn để làm gì (thầy: chưa giải quyết) | Giữ BR-12; Manager giám sát qua KPI, can thiệp ngoài hệ thống | Thầy |
| O11 | 3 → **F5** | Danh bạ bên thứ 3 do Admin quản lý hay ghi tay | Danh bạ đơn giản do Admin quản lý (tên, lĩnh vực, liên hệ, có hợp đồng trọn gói không) | Nhóm |
| O12 | 3 → **F5** | Khi chuyển bên thứ 3: In Progress (ý thầy, chưa chắc) hay On Hold (research)? | On Hold + lý do "chờ bên thứ 3" | **Thầy** |
| O13 | 3 → **F5** | Có thật cần nhánh "khách chịu chi phí"? Gửi báo giá cho ai? | Nếu có: gửi **đại diện hợp đồng** (công ty thuê / chủ hộ), không gửi người báo | **Thầy** |
| O14 | 4 | Hạn cho bước nghiệm thu | Cảnh báo nếu Completed quá 1 ngày làm việc chưa kiểm tra | Nhóm |
| O15 | 4 | "SLA met" tính theo lần Completed đầu hay cuối | Lần đầu; mở lại có KPI riêng | Nhóm |
| O16 | 4 | Nghiệm thu chỉ bằng ảnh có đủ? | Ảnh trước / sau bắt buộc + 7 ngày khách báo lại | Thầy |
| O17 | 4 | Mở lại ≥ 3 lần | Bắt buộc xem xét chuyển bên thứ 3 / thay thiết bị | Nhóm |

**4 câu nên gửi thầy trước:** O12, O13 (Hùng trình bày cùng F5), O04, O02.

Trên sơ đồ F3 Trang 3, O11–O13 gộp thành 1 ô ghi "chuyển sang F5"; Mã tương ứng bên F5: O12 = R02, O13 = R04; O11 (danh bạ bên thứ 3) do Hùng đưa vào F5 khi họp.

---

## 7. Phải sửa gì trong Report 3 (chưa sửa, chờ họp chốt)

| Chỗ trong R3 | Hiện tại | Sửa thành | Nguồn |
|---|---|---|---|
| 1, 2.1 Actors, toàn bộ | Employee = nhân viên doanh nghiệp | Khách hàng = người thuê / cư dân; bối cảnh ban quản lý tòa nhà văn phòng cho thuê + chung cư; dẫn Luật Nhà ở 2023, TT 05/2024 | N17, N19 |
| 5.4.1, toàn bộ | 12 trạng thái | **11 trạng thái**, bỏ Need more info | N6 |
| UC-OFF-05, UC-GEN-03, EMP-04, DLG-03, OFF-02 (tab "Waiting for employee"), EMP-01 ("Needs your attention"), 5.4.2, các dòng "Who moves each status" | Có Need more info | Bỏ; thêm lý do từ chối "Thiếu thông tin" + nút "Gửi lại" | N6 |
| BR-19, 3.7.3, OFF-02, 3.7.1 ("Officers see all cases") | Hàng chờ chung, Officer thấy mọi tòa | Officer phụ trách theo tòa (chính + dự phòng); chỉ thấy và nhận thông báo case của tòa mình | N11 |
| BR-02, DLG-22, MSG13/14, CR-06, 3.18.3 | Claim 30 phút, tối đa 5 case | Bỏ nếu chốt mỗi tòa 1 người xử lý (O02) | N11 |
| 5.4.2 Notifications | Nhiều dòng "All Officers" | "Officer phụ trách tòa" | N11 |
| BR-01, IssueType, EMP-02 | Khách gợi ý mức khẩn cấp; loại sự cố | **Danh mục dịch vụ** 2 cấp, có mức mặc định, cờ khẩn cấp, bên chịu chi phí, trường bắt buộc, áp dụng loại tòa; Officer chốt | N7, N24, N26 |
| BR-08 | SLA giờ hành chính, dừng khi On Hold với mọi lý do | Giờ hành chính (F4 không áp dụng); On Hold theo mục 3.4; "hạn có hiệu lực" = hạn dài hơn | N8, N13, N16 |
| ADM-07 | Một lịch làm việc chung | Theo O04 | N8 |
| 3.1.3 Screen Authorization (AST-03) | Technician không xem được | Technician xem tab Lịch sử của AST-03 (hoặc tab trong TEC-03) | N12 |
| ANA-02 | Không có số lần từ chối | Số lần Technician từ chối + lý do, kể cả xin giao lại vì sai chuyên môn | N10 |
| DLG-02, UC-OFF-04 | Lý do từ chối "Duplicate" | Bỏ; thay bằng chức năng **Gộp case** (mới) | N5 |
| TEC-03, UC-TEC-07 | Nguyên nhân, linh kiện, giờ công, ảnh | Thêm "Nội dung đã làm" (bắt buộc), ảnh trước / sau; linh kiện ghi tay | N21, N2 |
| Chức năng mới | — | Chuyển bên thứ 3 (ghi nhận, đính kèm báo giá, ghi nhận quyết định + bằng chứng); danh bạ bên thứ 3 (O11) | N1, N3, N22 |
| ERD | 17 entity | Thêm (đề xuất): ServiceType (thay IssueType), BuildingAssignment, VendorHandoff, Vendor (nếu O11), liên kết gộp case | — |
| COM-07, 3.18 | AI chỉ dạng chat | Thêm 3 điểm AI trong quy trình (mục 4) | N20 |

---

## 8. Đã kiểm tra thế nào

**Kiểm tra tự động** (chạy trên chính file nguồn sinh ra sơ đồ): mỗi hình thoi đủ nhánh có nhãn; không nút cụt; không vòng kẹt; mọi lần đổi trạng thái khớp danh sách được phép (đã bỏ Need more info) và đúng người làm; Trang 5 khớp Trang 1–4; bước ①–⑫ liền mạch. Kết quả bản v3.1 dựng lại 12/10 tối: **0 lỗi, 0 cảnh báo** (85 nút luồng, 16 hình thoi, 15 ô OPEN — O11–O13 gộp 1 ô trỏ F5). Bản 12/10 sáng có 104 nút / 22 hình thoi vì Trang 3 còn vẽ toàn bộ bên thứ 3.
Lưu ý trung thực: script chỉ kiểm tra sơ đồ có nhất quán với chính nó và với danh sách được phép. Script **không** chứng minh nghiệp vụ đúng. Nó cũng không bắt được mâu thuẫn với phần còn lại của Report 3, nên các mâu thuẫn đó được liệt kê bằng tay ở mục 7.

**Phản biện kiểu hội đồng** (một người đọc độc lập đối chiếu note thầy, Report 3 và sơ đồ) nêu 12 phát hiện. Đã sửa trên sơ đồ:
1. Sự cố khẩn cấp phải chờ duyệt trong giờ hành chính → nay rẽ sang F4 ngay khi tạo case.
2. Ô "Khách đồng ý?" thực chất là cổng duyệt → đổi thành "Ghi nhận quyết định của bên chịu chi phí + bằng chứng"; tách tòa chịu / khách chịu; báo giá gửi đại diện hợp đồng; bỏ qua báo giá với hợp đồng trọn gói; hủy case thì trả trạng thái thiết bị.
3. Phạm vi chỉ có Có / Không trước khi khảo sát → ghi bên chịu chi phí khi duyệt + Officer phân loại lại sau khi Technician khảo sát (Trang 3).
4. Case con sau khi gộp bị "mồ côi" → định nghĩa rõ cách hiển thị, không tính KPI, gộp ở mọi trạng thái mở, giữ vị trí; phần còn lại thành O03.
5. "Hạn dài hơn" mâu thuẫn với cờ đỏ → một định nghĩa "hạn có hiệu lực" dùng chung.
6. Cơ chế vắng mặt viết như đã chốt → đưa về O02, thêm câu hỏi claim và đổi người phụ trách.
7. Ô chẩn đoán tô xanh lá sai (R3 chưa cho Technician xem AST-03) → đổi xanh dương; lập bảng mục 7.
8. Nhắc khách khi khách trả lời ngoài hệ thống → đổi thành nhắc Officer theo dõi.
9. Nhánh thuê ngoài trước khi chẩn đoán gây giờ Start giả → bỏ, thuê ngoài chỉ sau khảo sát.
10. Đề xuất "hỏi qua bình luận" là Need more info trá hình → đổi thành giảm từ chối từ đầu vào (O05).
11. Thông báo khách khi gửi ngoài giờ; On Hold "chờ khách cho vào" phải có lịch hẹn.
12. Technician né bộ đếm từ chối bằng cách xin giao lại → Manager đếm cả trường hợp này.

Một điểm phản biện **không làm theo**: đề nghị vẽ In Progress theo ý thầy khi chuyển bên thứ 3. Lý do: chính thầy ghi "không chắc", còn research nghiêng về On Hold. Sơ đồ giữ On Hold và nêu rõ ở O12 (nay ở F5) để xin thầy chốt.

---

## 9. Câu hỏi hội đồng dễ hỏi nhất

1. **"Chung cư rò nước 22h thứ Bảy thì sao?"** — "Rò rỉ / vỡ ống" nằm trong danh sách khẩn cấp nên chuyển F4 ngay, không chờ duyệt, báo người trực / hotline. Giờ hành chính chỉ áp dụng F3. Lịch riêng từng tòa: O04.
2. **"Máy bơm là tài sản chung, sao gửi báo giá cho cư dân?"** — Không gửi. Tòa chịu thì ban tòa nhà tự quyết ngoài hệ thống, hệ thống chỉ ghi nhận. Chỉ khi khách chịu (lỗi do khách hoặc khu thuê theo hợp đồng) mới gửi, và gửi đại diện hợp đồng. Có giữ nhánh này không: O13.
3. **"Nói không quản lý duyệt, vậy ô 'Thực hiện?' là gì?"** — Hệ thống chỉ ghi lại quyết định đã có ở ngoài và lưu bằng chứng, không có số tiền, không có ai bấm duyệt trong hệ thống.
4. **"Bỏ Need more info thì khách thiếu một tấm ảnh cũng bị từ chối?"** — Giảm từ đầu vào: trường bắt buộc theo loại dịch vụ và AI nhắc trước khi gửi. Nếu vẫn thiếu thì từ chối kèm lý do, khách bấm "Gửi lại" với nội dung điền sẵn.
5. **"Officer tòa A nghỉ, ai xử lý?"** — Người dự phòng do Admin gán. Cả hai vắng: O02.
6. **"Mười người báo mất điện một tầng?"** — Officer gộp vào một case chính, vị trí các case con được giữ, tất cả được báo khi xong, KPI chỉ tính một case.
7. **"Technician đến nơi mới thấy hỏng do khách làm?"** — Ghi đánh giá → Officer phân loại lại: ngoài phạm vi thì hủy kèm lý do; trong phạm vi nhưng vượt khả năng thì chuyển bên thứ 3.
8. **"Lấy hạn dài hơn thì cứ dời hạn là luôn đúng hạn?"** — Chỉ được dời trước khi quá hạn, có lý do; báo cáo hiện cả tỷ lệ theo SLA gốc và số lần dời (O09).

---

## 10. Việc tiếp theo

1. ✅ Đã chốt số luồng (12/10). ✅ Đã chốt F5 là nguồn duy nhất cho "bên thứ 3 / thay thiết bị"; Trang 3 của F3 đã rút gọn (12/10 tối).
2. Gửi thầy 4 câu O12, O13, O04, O02.
3. Họp nhóm chốt các OPEN "Nhóm", rồi mới sửa Report 3 theo bảng mục 7.
4. Luồng F4 khẩn cấp (Tùng) cần: danh sách loại dịch vụ khẩn cấp, người trực / hotline ngoài giờ, bảng điểm AI tương tự mục 4.
