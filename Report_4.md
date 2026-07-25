# Report 4: AI + CI/CD + Testing Report (Integration Phase)

Dựa trên cấu trúc kiến trúc 3-Layer (5 projects) và kế hoạch phát triển (Vertical Slice) của hệ thống **Hotel Management and Service System**, dưới đây là báo cáo giai đoạn Tích hợp (Integration Phase).

---

## 1. AI Feature Integration

**Describe the AI functionality integrated:**
Hệ thống quản lý khách sạn hướng tới nhân viên nội bộ (Admin, Manager, Receptionist). Do đó, AI được tích hợp nhằm hỗ trợ trực tiếp nghiệp vụ của **Receptionist** thông qua tính năng **Smart Room Recommendation (Gợi ý phòng thông minh)**.
Khi Receptionist tạo mới một Booking (tại luồng `M2-BOOKING-001`), AI sẽ phân tích thông tin của `Customer` (độ tuổi, lịch sử đặt phòng) và danh sách phòng trống (`AvailableRoomDto`) để đề xuất `Room Type` (ví dụ: Suite, Deluxe) phù hợp nhất, giúp tiết kiệm thời gian tư vấn.

**Explain the algorithms, models, or APIs used:**
- Sử dụng **OpenAI API (GPT-4/3.5)** để phân tích ngữ nghĩa và đưa ra gợi ý dựa trên prompt.
- Đầu vào (Input): Chuyên viên lễ tân nhập yêu cầu khách hàng vào ô chat (ví dụ: "Khách hàng đi gia đình 4 người, cần phòng có view đẹp").
- Xử lý: Lớp `AiRecommendationService` (thuộc project `Services`) sẽ gọi OpenAI API, kết hợp với dữ liệu truy vấn từ `RoomRepository` để lọc ra các phòng có trạng thái `Available`.

**Include screenshots or code snippets if applicable:**
Code snippet được định nghĩa tại `Services/Implements/AiRecommendationService.cs`:

```csharp
public async Task<ServiceResult<string>> GetRoomRecommendationAsync(string customerRequirement, List<AvailableRoomDto> availableRooms)
{
    var prompt = $"Khách có yêu cầu: '{customerRequirement}'. Danh sách phòng trống: {JsonSerializer.Serialize(availableRooms)}. Hãy đề xuất mã phòng phù hợp nhất.";
    
    // Gọi OpenAI API qua HttpClient
    var client = new HttpClient();
    client.DefaultRequestHeaders.Add("Authorization", $"Bearer {_apiKey}");
    // ... code gọi API ...
    
    return new ServiceResult<string> { IsSuccess = true, Data = recommendedRoomMessage };
}
```

---

## 2. CI/CD Pipeline Setup

**Describe the CI/CD tools used:**
Nhóm sử dụng **GitHub Actions** để tự động hóa quá trình tích hợp liên tục (CI), do mã nguồn đã được quản lý tập trung trên GitHub (tuân thủ quy trình nêu tại `CORE-005: Define branch, PR, coding convention`).

**Explain the pipeline steps (build, test, deploy):**
Mỗi khi có Pull Request (PR) merge vào nhánh `develop` hoặc `main` (ví dụ từ `feature/member2-booking-room` hoặc khi thực hiện `INT-001` đến `INT-004`), luồng sau sẽ chạy:
1. **Restore dependencies:** Chạy `dotnet restore` cho solution `HotelManagementSystem.sln` để khôi phục gói NuGet.
2. **Build:** Chạy `dotnet build` để đảm bảo 5 projects (`BusinessObjects`, `DataAccessObjects`, `Repositories`, `Services`, `WPF`) biên dịch thành công mà không vi phạm reference (vd: WPF không được reference DAO).
3. **Test:** Tự động chạy Unit Test (nếu có) trên thư mục `Services.Tests`.
4. **Publish:** Chạy `dotnet publish` project `WPF` để đóng gói bản build.

**Share link to pipeline configuration:**
*(Mẫu file `.github/workflows/dotnet-ci.yml` đang được áp dụng hoặc đề xuất)*:
```yaml
name: .NET WPF CI
on:
  push:
    branches: [ "main", "develop" ]
  pull_request:
    branches: [ "main", "develop" ]
jobs:
  build:
    runs-on: windows-latest
    steps:
    - uses: actions/checkout@v4
    - name: Setup .NET
      uses: actions/setup-dotnet@v4
      with:
        dotnet-version: '8.0.x'
    - name: Restore dependencies
      run: dotnet restore HotelManagementSystem.sln
    - name: Build
      run: dotnet build HotelManagementSystem.sln --configuration Release --no-restore
```

---

## 3. Deployment Workflow

**Explain how the system was deployed to staging or production:**
Hệ thống là ứng dụng **Desktop WPF**, do đó việc deploy chủ yếu tập trung vào cơ sở dữ liệu và bản thực thi:
1. **Database:** Triển khai cơ sở dữ liệu SQL Server bằng cách chạy tuần tự 4 file script tĩnh đã được chuẩn bị:
   - `001_create_database.sql`
   - `002_create_tables.sql` (12 bảng cốt lõi)
   - `003_constraints.sql`
   - `004_seed_data.sql`
2. **WPF Application:** Đóng gói thư mục `/bin/Release/net8.0-windows` thành file `.zip`. Lễ tân tải file zip này về máy, cấu hình lại chuỗi kết nối (`appsettings.json`) trỏ tới Server thật và khởi chạy `HotelManagementSystem.exe`.

**Include deployment frequency and automation levels:**
- **Tần suất:** Theo các mốc Integration (`INT-001` đến `INT-004`) và `TEST-RELEASE-001` vào ngày 14 (Day 14).
- **Mức độ tự động hóa:** CI tự động kiểm tra code. Việc chạy SQL script và cài đặt WPF client hiện đang được tiến hành thủ công tại từng máy trạm.

---

## 4. Collaboration and Automation

**Describe how team members coordinated CI/CD tasks:**
Dự án được chia theo **Vertical Slice** rất rõ ràng để tránh conflict:
- **Member 1 (Leader):** Xử lý Auth, User, Billing (`M1-AUTH-001`, `M1-INVOICE-001`). Xử lý review PR kiến trúc.
- **Member 2:** Xử lý Customer, Room, Booking (`M2-CUSTOMER-001`, `M2-BOOKING-001`).
- **Member 3:** Xử lý Operation, Check-in/out (`M3-CHECKIN-001`).
- **Member 4:** Xử lý QA, Reports (`M4-QA-DOCS-001`).
Mỗi thành viên làm việc trên nhánh `feature/*` riêng. Khi hoàn thành, tạo Pull Request vào `develop`. Leader sẽ dựa vào bộ rule (như WPF không được gọi DAO) để review trước khi merge.

**Mention any automation bots or workflows used:**
- Nhóm sử dụng quy trình xét duyệt của GitHub. PR yêu cầu bắt buộc phải qua Build CI (GitHub Actions) không bị lỗi (xanh) mới cho phép nút "Merge".
- Tự động hóa đánh dấu label (`type:feature`, `area:ui-core`, v.v.) bằng tay (hoặc qua bot) theo quy chuẩn đã định nghĩa tại issue `CORE-005`.

---

## 5. Lessons Learned

**Highlight key takeaways from AI integration and automated testing:**
- **Về Test/QA:** Vì dự án đang phụ thuộc nhiều vào **Manual Test Cases** (`M4-QA-DOCS-001`, `M3-OPERATION-TEST-001`), nhóm nhận thấy việc luồng nghiệp vụ đan xen (như luồng Check-out liên kết chặt chẽ tới Booking và Invoice) rất dễ gây lỗi hồi quy (regression bugs).
- **Về CI/CD:** Việc chuẩn hóa kiến trúc 5 tầng từ Day 1 giúp quá trình Build trên CI phát hiện ngay những lỗi tham chiếu sai (cross-reference) mà Visual Studio đôi lúc vẫn bỏ qua.

**Reflect on what worked and what could be improved:**
- **What worked:** Cách chia task **Vertical Slice Fullstack** (mỗi người tự làm từ `BusinessObjects` đến `WPF`) giúp giảm tới 60% tình trạng conflict code khi merge, quá trình tích hợp `INT-001` đến `INT-004` diễn ra trơn tru.
- **What could be improved:** 
  1. Thiếu Unit Test tự động (Automated Unit Tests) cho tầng `Services`. Cần bổ sung xUnit và Moq vào dự án để test các hàm phức tạp (như hàm tính tiền phòng `room_total = room_price * number_of_nights`).
  2. Phần tích hợp AI xử lý đồng bộ có thể làm UI WPF bị đơ (freeze). Trong tương lai cần sử dụng triệt để `async/await` và hiển thị Loading Spinner trên View.

---

## 6. Appendix (Optional)

**Danh sách các cột mốc kiểm thử và tích hợp trong dự án:**
- `INT-001`: Merge Core + Auth + Admin User
- `INT-002`: Merge Customer + Room + Booking + Room Map
- `INT-003`: Merge Check-in + Service + Checkout
- `INT-004`: Merge Billing + Dashboard + Reports
- `TEST-RELEASE-001`: Full E2E test, bug fix and final release package (Day 14).
