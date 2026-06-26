# FULLSTACK TASK PLAN 2 WEEKS - VERTICAL SLICE REVISED

> **Project:** Hotel Management and Service System  
> **Loại ứng dụng:** C# WPF Desktop Application  
> **Database:** SQL Server  
> **Architecture:** 5 Projects + 3 Layer Architecture + MVVM + Repository Pattern  
> **Team:** 4 thành viên fullstack  
> **Thời gian:** 2 tuần, Day 1 đến Day 14  
> **Phiên bản:** Revised Vertical Slice Plan  
> **Mục tiêu:** Cắt bỏ task trùng, gộp task nhỏ thành task fullstack theo luồng nghiệp vụ, đảm bảo mỗi thành viên đều làm đủ `BusinessObjects → DataAccessObjects → Repositories → Services → WPF` trong module của mình.

---

## 1. Lý do viết lại task plan

File task cũ có phạm vi nghiệp vụ tốt, nhưng số issue quá nhiều và bị tách nhỏ theo tầng hoặc theo thao tác nhỏ. Ví dụ một module có thể bị tách thành DAO, Service, UI, validation, search, update flow. Cách chia này dễ tạo nhiều dependency, dễ chờ nhau, dễ merge conflict và khó xác định ai chịu trách nhiệm khi một luồng demo bị lỗi.

Bản revised này chuyển sang cách chia **vertical slice fullstack**.

Thay vì chia:

```text
Một người làm DAO
Một người làm Service
Một người làm UI
Một người làm validation
```

Bản mới chia:

```text
Một người sở hữu trọn một luồng nghiệp vụ
→ tự làm BusinessObjects
→ tự làm DataAccessObjects
→ tự làm Repositories
→ tự làm Services
→ tự làm WPF
→ tự test flow của mình
```

Kết quả:

- Từ khoảng **84 issues** giảm còn khoảng **32 issues**.
- Mỗi thành viên đều làm fullstack thật sự.
- Giảm chồng chéo code.
- Giảm dependency giữa thành viên.
- Dễ tạo GitHub Issues hơn.
- Dễ demo hơn vì mỗi người chịu trách nhiệm một luồng nghiệp vụ rõ ràng.

---

## 2. Project Overview

Hotel Management and Service System là hệ thống quản lý khách sạn nội bộ chạy trên desktop WPF. Hệ thống phục vụ nhân viên khách sạn trong các nghiệp vụ:

- Đăng nhập.
- Phân quyền theo role.
- Quản lý nhân viên.
- Quản lý khách thuê.
- Quản lý loại phòng.
- Quản lý phòng.
- Xem sơ đồ phòng.
- Đặt phòng.
- Check-in.
- Ghi nhận dịch vụ phát sinh.
- Check-out.
- Tạo hóa đơn.
- Nhận thanh toán.
- Xem dashboard và báo cáo.

Phạm vi project tập trung vào **WPF + SQL Server local**. Không bắt buộc web API, cloud, mobile app hoặc payment gateway thật.

---

## 3. App sau khi hoàn thành sẽ có chức năng gì?

### 3.1. Chức năng chung

- Người dùng nội bộ đăng nhập bằng username/password.
- Hệ thống xác định role sau khi login.
- Lưu thông tin phiên đăng nhập trong `CurrentSession`.
- Điều hướng menu theo role.
- Logout và clear session.
- Service vẫn validate quyền, không chỉ ẩn menu ở UI.

### 3.2. Admin

Admin có thể:

- Quản lý tài khoản nhân viên.
- Quản lý role cơ bản.
- Quản lý loại phòng.
- Quản lý phòng.
- Quản lý dịch vụ.
- Xem dashboard.
- Xem report.

### 3.3. Manager

Manager có thể:

- Xem dashboard.
- Xem tình trạng phòng.
- Xem danh sách booking.
- Xem báo cáo công suất phòng.
- Xem báo cáo doanh thu.
- Xem báo cáo sử dụng dịch vụ.
- Xem báo cáo thanh toán.

### 3.4. Receptionist

Receptionist là người thao tác nghiệp vụ chính:

- Tạo/cập nhật thông tin customer.
- Tìm phòng trống.
- Tạo booking.
- Check-in.
- Ghi nhận service order.
- Check-out.
- Tạo invoice.
- Nhận payment.

### 3.5. Luồng demo cuối kỳ

```text
Login Receptionist
→ Tạo customer
→ Tìm phòng trống theo ngày
→ Tạo booking nhiều phòng
→ Room Map hiển thị Reserved
→ Check-in từng phòng
→ Room Map hiển thị Occupied
→ Gọi dịch vụ cho phòng đang ở
→ Check-out
→ Tạo invoice
→ Thanh toán một phần hoặc toàn bộ
→ Booking Completed
→ Manager/Admin xem dashboard và report
```

---

## 4. Database Scope

Database có 12 bảng chính:

| Nhóm | Bảng | Vai trò |
|---|---|---|
| Phân quyền | `roles`, `users` | Lưu role và tài khoản nhân viên nội bộ. |
| Khách thuê | `customers` | Lưu khách đặt/thuê phòng, không đăng nhập. |
| Phòng | `room_types`, `rooms` | Lưu loại phòng, giá cơ bản, phòng cụ thể và trạng thái vận hành. |
| Đặt phòng | `bookings`, `booking_details` | `bookings` là đơn tổng; `booking_details` là từng phòng trong booking. |
| Lưu trú | `check_records` | Lưu check-in/check-out thực tế theo từng booking detail. |
| Dịch vụ | `services`, `service_orders` | Dịch vụ khách sạn và dịch vụ phát sinh theo phòng đang ở. |
| Thanh toán | `invoices`, `payments` | Hóa đơn tổng theo booking và các lần thanh toán. |

---

## 5. Architecture Standard

### 5.1. Solution structure

```text
HotelManagementSystem.sln
├── BusinessObjects
├── DataAccessObjects
├── Repositories
├── Services
└── WPF
```

### 5.2. Luồng gọi code chuẩn

```text
WPF View
→ ViewModel
→ Service Interface
→ Service Implementation
→ Repository Interface
→ Repository Implementation
→ DAO
→ SQL Server
```

### 5.3. Mapping 3 Layer

| Layer | Project | Trách nhiệm |
|---|---|---|
| Presentation Layer | WPF | View, ViewModel, Command, Navigation, UI state. |
| Business Logic Layer | Services | Business rules, validation nghiệp vụ, transaction orchestration. |
| Data Access Layer | Repositories + DataAccessObjects | Repository trung gian, DAO truy cập SQL Server. |
| Shared Model | BusinessObjects | Entity, DTO, Enum, Constants. |

### 5.4. Project Reference Rules

| Project | Được reference |
|---|---|
| BusinessObjects | Không reference project nào khác |
| DataAccessObjects | BusinessObjects |
| Repositories | BusinessObjects, DataAccessObjects |
| Services | BusinessObjects, Repositories |
| WPF | BusinessObjects, Services |

### 5.5. Luật cấm sai kiến trúc

- WPF không được reference `DataAccessObjects`.
- WPF không được gọi DAO trực tiếp.
- WPF không được gọi Repository trực tiếp nếu đã có Service.
- WPF không được viết SQL query.
- ViewModel không chứa business logic phức tạp.
- ViewModel chỉ xử lý binding, command, UI state, validation nhẹ và gọi Service.
- Service là nơi chứa business rules chính.
- Repository là lớp trung gian giữa Service và DAO.
- DAO là nơi duy nhất làm việc trực tiếp với SQL Server.
- BusinessObjects chỉ chứa Entity, DTO, Enum, Constants.
- Không tạo reference ngược tầng.
- Services không reference WPF.
- Repositories không reference Services hoặc WPF.
- DataAccessObjects không reference Repositories, Services hoặc WPF.

---

## 6. Folder Structure

```text
BusinessObjects
├── Entities
├── DTOs
├── Enums
└── Constants

DataAccessObjects
├── DBContext.cs hoặc DbContextFactory.cs
├── DAOs
├── SQLScripts
└── SeedData

Repositories
├── Interfaces
└── Implements

Services
├── Interfaces
├── Implements
├── Validators
└── Helpers

WPF
├── Views
├── ViewModels
├── Commands
├── Resources
├── Helpers
├── Converters
├── App.xaml
└── appsettings.json
```

---

## 7. Vertical Slice Rule

Mỗi task nghiệp vụ phải có đủ 5 phần:

```text
BusinessObjects
→ DataAccessObjects
→ Repositories
→ Services
→ WPF
```

Ví dụ task booking của Member 2 phải bao gồm:

```text
BusinessObjects:
- Booking
- BookingDetail
- CreateBookingRequestDto
- AvailableRoomDto
- BookingDetailDto

DataAccessObjects:
- BookingDao
- BookingDetailDao

Repositories:
- IBookingRepository
- BookingRepository

Services:
- IBookingService
- BookingService

WPF:
- CreateBookingView
- BookingListView
- CreateBookingViewModel
- BookingListViewModel
```

Không chia một nghiệp vụ thành nhiều người theo tầng.

---

## 8. Team Ownership

## 8.1. Member 1 - Leader / Auth / Admin User / Billing / Integration

### Sở hữu chính

- `roles`
- `users`
- `invoices`
- `payments`
- `CurrentSession`
- Login
- Role navigation
- Admin User Management
- Invoice
- Payment
- Billing UI
- Integration review

### Không làm để tránh chồng chéo

- Không làm Customer CRUD chính.
- Không làm Room CRUD chính.
- Không làm Booking Create chính.
- Không làm Check-in/Service Order chính.
- Không làm Report query chính.

---

## 8.2. Member 2 - Customer / Room / Booking / Room Map

### Sở hữu chính

- `customers`
- `room_types`
- `rooms`
- `bookings`
- `booking_details`
- Customer Management
- Room Type Management
- Room Management
- Room Map
- Available Room Search
- Booking Conflict Validation
- Booking List/Cancel/History

### Không làm để tránh chồng chéo

- Không làm Auth.
- Không làm Admin User Management.
- Không làm Check-in.
- Không làm Service Order.
- Không làm Invoice/Payment.
- Không làm Report/Dashboard chính.

---

## 8.3. Member 3 - Stay Operation / Service / Check-in / Check-out

### Sở hữu chính

- `check_records`
- `services`
- `service_orders`
- Check-in
- Service Catalog
- Service Order
- Check-out
- Operation flow

### File operation được phép tạo thêm

Để tránh sửa trực tiếp file chính của Member 2, Member 3 có thể tạo file operation riêng:

```text
BookingOperationDao.cs
RoomOperationDao.cs
BookingOperationRepository.cs
RoomOperationRepository.cs
BookingOperationService.cs
RoomOperationService.cs
```

Các file này chỉ phục vụ cập nhật trạng thái nghiệp vụ sau check-in/check-out, không thay thế BookingService/RoomService chính của Member 2.

### Không làm để tránh chồng chéo

- Không làm Booking Create.
- Không làm Invoice/Payment.
- Không làm Dashboard/Report chính.
- Không làm Admin User Management.

---

## 8.4. Member 4 - Dashboard / Report / QA / Docs

### Sở hữu chính

- Dashboard
- Occupancy Report
- Revenue Report
- Service Usage Report
- Report filters
- Export CSV optional
- README
- User Manual
- Demo Script
- Manual Test Cases

### Không làm để tránh chồng chéo

- Không làm Auth core.
- Không làm Booking Create.
- Không làm Check-in/Check-out business logic.
- Không làm Invoice/Payment business logic.
- Không sửa DB schema chính nếu chưa thống nhất với Member 2 và Leader.

---

## 9. Revised Issue Summary

| Nhóm | Số issue | Ghi chú |
|---|---:|---|
| Core Shared | 5 | Nền chung cho cả team. |
| Member 1 | 6 | Auth, Admin User, Invoice, Payment, Billing, Integration Review. |
| Member 2 | 6 | Customer, Room, Room Map, Booking. |
| Member 3 | 5 | Check-in, Service, Service Order, Check-out. |
| Member 4 | 5 | Dashboard, Report, QA, Docs. |
| Integration/Test/Release | 5 | Merge, E2E, release. |
| **Tổng** | **32** | Gọn hơn, ít chồng chéo hơn. |

---

## 10. Revised Issue Index

| Issue ID | Title | Owner | Start | Deadline | Depends on |
|---|---|---|---|---|---|
| `CORE-001` | Create solution, 5 projects, references and architecture rules | Member 1 | Day 1 | Day 1 | None |
| `CORE-002` | Create common enums, base DTOs, ServiceResult and constants | Member 1 + All | Day 1 | Day 2 | CORE-001 |
| `CORE-003` | Create SQL Server master schema, seed data and appsettings connection | Member 2 | Day 1 | Day 2 | CORE-001, CORE-002 |
| `CORE-004` | Create BaseViewModel, RelayCommand, Navigation, Dialog and shared WPF styles | Member 1 + Member 4 | Day 2 | Day 3 | CORE-001, CORE-002 |
| `CORE-005` | Define branch, PR, coding convention and integration checklist | Member 1 | Day 1 | Day 1 | CORE-001 |
| `M1-AUTH-001` | Auth, Login, CurrentSession, Role Navigation and Logout Fullstack | Member 1 | Day 2 | Day 4 | CORE-002, CORE-003, CORE-004 |
| `M1-USERMGMT-001` | Admin User Management Fullstack | Member 1 | Day 5 | Day 6 | M1-AUTH-001 |
| `M1-INVOICE-001` | Invoice Fullstack | Member 1 | Day 10 | Day 11 | M3-CHECKOUT-001, M3-SERVICEORDER-001 |
| `M1-PAYMENT-001` | Payment Fullstack | Member 1 | Day 11 | Day 12 | M1-INVOICE-001 |
| `M1-BILLINGUI-001` | Invoice/Payment UI and Billing Flow | Member 1 | Day 12 | Day 13 | M1-PAYMENT-001 |
| `M1-INTEGRATION-REVIEW-001` | Review architecture, PRs and cross-module integration | Member 1 | Day 1 | Day 14 | CORE-005 |
| `M2-CUSTOMER-001` | Customer Management Fullstack | Member 2 | Day 3 | Day 5 | CORE-002, CORE-003, CORE-004, M1-AUTH-001 |
| `M2-ROOM-001` | Room Type and Room Management Fullstack | Member 2 | Day 3 | Day 6 | CORE-002, CORE-003, CORE-004, M1-AUTH-001 |
| `M2-ROOMMAP-001` | Visual Room Map Fullstack | Member 2 | Day 5 | Day 7 | M2-ROOM-001 |
| `M2-BOOKING-001` | Create Booking Fullstack | Member 2 | Day 6 | Day 8 | M2-CUSTOMER-001, M2-ROOM-001, M2-ROOMMAP-001 |
| `M2-BOOKING-002` | Booking List, Cancel and Detail History Fullstack | Member 2 | Day 8 | Day 9 | M2-BOOKING-001 |
| `M2-BOOKING-TEST-001` | Booking flow test and handoff to operation team | Member 2 | Day 9 | Day 9 | M2-BOOKING-001, M2-BOOKING-002 |
| `M3-CHECKIN-001` | Check-in Fullstack | Member 3 | Day 8 | Day 9 | M2-BOOKING-001 |
| `M3-SERVICE-001` | Service Catalog Management Fullstack | Member 3 | Day 4 | Day 6 | CORE-002, CORE-003, CORE-004, M1-AUTH-001 |
| `M3-SERVICEORDER-001` | Service Order Fullstack | Member 3 | Day 9 | Day 10 | M3-CHECKIN-001, M3-SERVICE-001 |
| `M3-CHECKOUT-001` | Check-out Fullstack | Member 3 | Day 10 | Day 11 | M3-CHECKIN-001, M3-SERVICEORDER-001 |
| `M3-OPERATION-TEST-001` | Operation flow test and handoff to billing team | Member 3 | Day 11 | Day 11 | M3-CHECKOUT-001 |
| `M4-DASHBOARD-001` | Dashboard Fullstack | Member 4 | Day 8 | Day 10 | M2-BOOKING-001, M3-CHECKIN-001 |
| `M4-REPORT-001` | Occupancy Report Fullstack | Member 4 | Day 10 | Day 12 | M2-BOOKING-001, M3-CHECKIN-001 |
| `M4-REPORT-002` | Revenue and Service Usage Report Fullstack | Member 4 | Day 12 | Day 13 | M1-PAYMENT-001, M3-SERVICEORDER-001 |
| `M4-REPORT-EXPORT-001` | Report Export CSV Optional | Member 4 | Day 13 | Day 13 | M4-REPORT-001, M4-REPORT-002 |
| `M4-QA-DOCS-001` | README, User Manual, Demo Script and Manual Test Cases | Member 4 | Day 10 | Day 14 | INT-002 |
| `INT-001` | Merge Core + Auth + Admin User | All, Leader owns | Day 6 | Day 6 | M1-AUTH-001, M1-USERMGMT-001 |
| `INT-002` | Merge Customer + Room + Booking + Room Map | All, Leader owns | Day 9 | Day 9 | M2-BOOKING-TEST-001 |
| `INT-003` | Merge Check-in + Service + Checkout | All, Leader owns | Day 11 | Day 11 | M3-OPERATION-TEST-001 |
| `INT-004` | Merge Billing + Dashboard + Reports | All, Leader owns | Day 13 | Day 13 | M1-BILLINGUI-001, M4-REPORT-002 |
| `TEST-RELEASE-001` | Full E2E test, bug fix and final release package | All | Day 14 | Day 14 | INT-004, M4-QA-DOCS-001 |

---

## 11. Detailed Issues - Core Shared

### CORE-001: Create solution, 5 projects, references and architecture rules

- **Assignee:** Member 1 - Leader
- **Labels:** `type:setup`, `area:architecture`, `priority:critical`
- **Branch:** `feature/core-solution-setup`
- **Start:** Day 1
- **Deadline:** Day 1
- **Estimate:** 4h
- **Depends on:** None
- **Blocks:** CORE-002, CORE-003, CORE-004, all feature tasks

#### Objective

Tạo solution chuẩn 5 project và cấu hình reference đúng kiến trúc.

#### Scope

```text
HotelManagementSystem.sln
├── BusinessObjects
├── DataAccessObjects
├── Repositories
├── Services
└── WPF
```

#### Acceptance Criteria

- Solution build thành công.
- Có đủ 5 projects.
- Project reference đúng rule.
- WPF chỉ reference `BusinessObjects` và `Services`.
- Services không reference WPF.
- Repositories không reference Services.
- DataAccessObjects không reference Repositories.

---

### CORE-002: Create common enums, base DTOs, ServiceResult and constants

- **Assignee:** Member 1 + All members support
- **Labels:** `type:core`, `area:businessobjects`, `priority:critical`
- **Branch:** `feature/core-businessobjects-common`
- **Start:** Day 1
- **Deadline:** Day 2
- **Estimate:** 6h
- **Depends on:** CORE-001
- **Blocks:** All vertical slice tasks

#### Objective

Tạo các thành phần chung trong `BusinessObjects` để toàn team dùng thống nhất.

#### Scope

```text
BusinessObjects
├── DTOs
│   ├── ServiceResult.cs
│   ├── PagedResult.cs
│   ├── CurrentSessionDto.cs
│   └── LookupItemDto.cs
├── Enums
│   ├── RoleName.cs
│   ├── UserStatus.cs
│   ├── RoomOperationalStatus.cs
│   ├── BookingStatus.cs
│   ├── BookingDetailStatus.cs
│   ├── CheckRecordStatus.cs
│   ├── ServiceStatus.cs
│   ├── ServiceOrderStatus.cs
│   ├── InvoiceStatus.cs
│   ├── PaymentStatus.cs
│   └── PaymentMethod.cs
└── Constants
    ├── AppRoles.cs
    ├── ErrorMessages.cs
    └── ValidationRules.cs
```

#### Acceptance Criteria

- Có enum dùng chung cho status.
- Có `ServiceResult<T>` để service trả kết quả an toàn cho ViewModel.
- Không throw exception trực tiếp ra UI với lỗi nghiệp vụ thông thường.
- Các member không tự tạo enum/status string riêng trong module.

---

### CORE-003: Create SQL Server master schema, seed data and appsettings connection

- **Assignee:** Member 2 - DB owner
- **Reviewer:** Member 1 - Leader
- **Labels:** `type:database`, `area:sqlserver`, `priority:critical`
- **Branch:** `feature/core-database-schema`
- **Start:** Day 1
- **Deadline:** Day 2
- **Estimate:** 8h
- **Depends on:** CORE-001, CORE-002
- **Blocks:** DAO tasks, Auth, Customer, Room, Booking, Service

#### Objective

Tạo database schema từ ERD, seed data và cấu hình connection string cho WPF.

#### Scope

```text
DataAccessObjects
├── SQLScripts
│   ├── 001_create_database.sql
│   ├── 002_create_tables.sql
│   ├── 003_constraints.sql
│   └── 004_seed_data.sql
├── DBContext.cs hoặc DbContextFactory.cs
└── SeedData
```

#### Required Tables

- `roles`
- `users`
- `customers`
- `room_types`
- `rooms`
- `bookings`
- `booking_details`
- `check_records`
- `services`
- `service_orders`
- `invoices`
- `payments`

#### Acceptance Criteria

- Script chạy được từ database rỗng.
- Có seed roles: Admin, Manager, Receptionist.
- Có seed admin user.
- Có seed room types, rooms, services.
- Có unique constraints cơ bản.
- Có check constraints cơ bản.
- Có `appsettings.json` với connection string.
- Không hard-code connection string trong DAO.
- Hướng dẫn đổi server name/localdb rõ ràng.

---

### CORE-004: Create BaseViewModel, RelayCommand, Navigation, Dialog and shared WPF styles

- **Assignee:** Member 1 + Member 4 support
- **Labels:** `type:wpf`, `area:ui-core`, `priority:critical`
- **Branch:** `feature/core-wpf-foundation`
- **Start:** Day 2
- **Deadline:** Day 3
- **Estimate:** 8h
- **Depends on:** CORE-001, CORE-002
- **Blocks:** All WPF views

#### Objective

Tạo nền MVVM dùng chung cho toàn bộ WPF app.

#### Scope

```text
WPF
├── ViewModels
│   └── BaseViewModel.cs
├── Commands
│   └── RelayCommand.cs
├── Helpers
│   ├── NavigationService.cs
│   ├── DialogService.cs
│   └── CurrentSession.cs
├── Resources
│   ├── Colors.xaml
│   ├── Buttons.xaml
│   ├── TextBoxes.xaml
│   └── Tables.xaml
└── Views
    └── MainWindow.xaml
```

#### Acceptance Criteria

- `BaseViewModel` có `INotifyPropertyChanged`.
- `RelayCommand` implement `ICommand`.
- Có navigation cơ bản cho MainWindow.
- Có DialogService cho confirm/error/info.
- Có CurrentSession dùng chung.
- Có style cơ bản cho Button/TextBox/DataGrid.
- ViewModel không gọi DAO/Repository.

---

### CORE-005: Define branch, PR, coding convention and integration checklist

- **Assignee:** Member 1 - Leader
- **Labels:** `type:process`, `area:github`, `priority:high`
- **Branch:** `docs/core-github-workflow`
- **Start:** Day 1
- **Deadline:** Day 1
- **Estimate:** 2h
- **Depends on:** CORE-001
- **Blocks:** All PRs

#### Objective

Chuẩn hóa cách làm việc GitHub để giảm conflict và lỗi merge.

#### Branch Strategy

```text
main
├── develop
├── feature/member1-auth-billing
├── feature/member2-booking-room
├── feature/member3-operation-service
├── feature/member4-report-dashboard
├── fix/*
└── release/final-demo
```

#### Commit Convention

```text
feat(auth): implement login service
feat(booking): add available room search
fix(payment): prevent over-payment
docs(readme): add sql setup guide
```

#### Acceptance Criteria

- Có `README_GIT_WORKFLOW.md`.
- PR phải merge vào `develop`.
- PR phải ghi issue ID.
- Không merge nếu solution không build.
- Không merge nếu vi phạm reference tầng.
- Schema thay đổi phải được Member 2 và Leader review.

---

## 12. Detailed Issues - Member 1

### M1-AUTH-001: Auth, Login, CurrentSession, Role Navigation and Logout Fullstack

- **Assignee:** Member 1 - Leader
- **Labels:** `type:feature`, `area:auth`, `priority:critical`
- **Branch:** `feature/member1-auth-login-session`
- **Start:** Day 2
- **Deadline:** Day 4
- **Estimate:** 14h
- **Depends on:** CORE-002, CORE-003, CORE-004
- **Blocks:** All role-based UI tasks

#### Objective

Triển khai đầy đủ login, session, phân quyền điều hướng và logout.

#### BusinessObjects

```text
Entities:
- Role
- User

DTOs:
- LoginRequestDto
- LoginResultDto
- CurrentSessionDto

Enums:
- RoleName
- UserStatus
```

#### DataAccessObjects

```text
- RoleDao
- UserDao
```

#### Repositories

```text
- IRoleRepository
- RoleRepository
- IUserRepository
- UserRepository
```

#### Services

```text
- IAuthService
- AuthService
- ICurrentUserService
- CurrentUserService
```

#### WPF

```text
- LoginWindow.xaml
- LoginViewModel.cs
- MainWindow.xaml
- MainViewModel.cs
- Role-based menu visibility
- LogoutCommand
```

#### Business Rules

- User phải login thành công mới vào MainWindow.
- User `Inactive` hoặc `Locked` không được login.
- Password không lưu plain text.
- Sau login lưu `CurrentSession`.
- Logout phải clear `CurrentSession`.
- Menu hiển thị theo role.
- Service vẫn validate quyền, không chỉ dựa vào UI.

#### Acceptance Criteria

- Login thành công với admin seed.
- Login sai username/password hiển thị lỗi.
- User inactive/locked không login được.
- Sau login hiển thị tên user và role.
- Admin/Manager/Receptionist thấy menu khác nhau.
- Logout quay về LoginWindow.
- Không có password plain text trong database.

---

### M1-USERMGMT-001: Admin User Management Fullstack

- **Assignee:** Member 1 - Leader
- **Labels:** `type:feature`, `area:admin`, `priority:high`
- **Branch:** `feature/member1-admin-user-management`
- **Start:** Day 5
- **Deadline:** Day 6
- **Estimate:** 10h
- **Depends on:** M1-AUTH-001
- **Blocks:** INT-001

#### Objective

Admin quản lý tài khoản nhân viên nội bộ.

#### BusinessObjects

```text
DTOs:
- UserListItemDto
- CreateUserRequestDto
- UpdateUserRequestDto
- ChangeUserStatusRequestDto
```

#### DataAccessObjects

```text
- UserManagementDao
```

#### Repositories

```text
- IUserManagementRepository
- UserManagementRepository
```

#### Services

```text
- IUserManagementService
- UserManagementService
```

#### WPF

```text
- UserManagementView.xaml
- UserDialog.xaml
- UserManagementViewModel.cs
- UserDialogViewModel.cs
```

#### Business Rules

- Chỉ Admin được quản lý user.
- Username không được trùng.
- Email không được trùng nếu có nhập.
- Không cho tự khóa chính tài khoản đang login.
- Không xóa cứng user đã có dữ liệu nghiệp vụ.
- Nên dùng status `Active`, `Inactive`, `Locked`.

#### Acceptance Criteria

- Admin xem danh sách user.
- Admin tạo user mới.
- Admin cập nhật thông tin user.
- Admin đổi role cho user.
- Admin khóa/mở tài khoản.
- Manager/Receptionist không truy cập được màn hình này.

---

### M1-INVOICE-001: Invoice Fullstack

- **Assignee:** Member 1 - Leader
- **Labels:** `type:feature`, `area:invoice`, `priority:critical`
- **Branch:** `feature/member1-invoice`
- **Start:** Day 10
- **Deadline:** Day 11
- **Estimate:** 12h
- **Depends on:** M3-CHECKOUT-001, M3-SERVICEORDER-001
- **Blocks:** M1-PAYMENT-001

#### Objective

Tạo hóa đơn tổng cho booking sau khi khách check-out.

#### BusinessObjects

```text
Entities:
- Invoice

DTOs:
- InvoiceSummaryDto
- CreateInvoiceRequestDto
- InvoiceDetailDto

Enums:
- InvoiceStatus
```

#### DataAccessObjects

```text
- InvoiceDao
- InvoiceCalculationDao
```

#### Repositories

```text
- IInvoiceRepository
- InvoiceRepository
```

#### Services

```text
- IInvoiceService
- InvoiceService
```

#### WPF

```text
- InvoiceView.xaml
- InvoiceDetailView.xaml
- InvoiceViewModel.cs
- InvoiceDetailViewModel.cs
```

#### Business Rules

- Invoice gắn với `booking_id`.
- Một booking chỉ có tối đa một invoice.
- Chỉ tạo invoice khi tất cả `booking_details` của booking đã `CheckedOut` hoặc `Cancelled`.
- `room_amount = SUM(booking_details.room_total)`.
- `service_amount = SUM(service_orders.total_price)` với status `Ordered`.
- `total_amount = room_amount + service_amount + tax_amount - discount_amount`.
- Invoice mới tạo có status `Unpaid`.
- `paid_amount = 0` khi mới tạo.
- `remaining_amount = total_amount - paid_amount`.

#### Acceptance Criteria

- Tạo invoice đúng cho booking đã checkout xong.
- Không tạo invoice cho booking còn phòng đang CheckedIn/Reserved.
- Không tạo duplicate invoice cho cùng booking.
- Hiển thị room amount, service amount, tax, discount, total.
- Invoice detail hiển thị đủ booking/customer/room/service/payment summary.

---

### M1-PAYMENT-001: Payment Fullstack

- **Assignee:** Member 1 - Leader
- **Labels:** `type:feature`, `area:payment`, `priority:critical`
- **Branch:** `feature/member1-payment`
- **Start:** Day 11
- **Deadline:** Day 12
- **Estimate:** 12h
- **Depends on:** M1-INVOICE-001
- **Blocks:** M1-BILLINGUI-001, M4-REPORT-002

#### Objective

Triển khai thanh toán một phần hoặc toàn bộ cho invoice.

#### BusinessObjects

```text
Entities:
- Payment

DTOs:
- PaymentRequestDto
- PaymentHistoryDto
- PaymentResultDto

Enums:
- PaymentMethod
- PaymentStatus
```

#### DataAccessObjects

```text
- PaymentDao
```

#### Repositories

```text
- IPaymentRepository
- PaymentRepository
```

#### Services

```text
- IPaymentService
- PaymentService
```

#### WPF

```text
- PaymentView.xaml
- PaymentHistoryView.xaml
- PaymentViewModel.cs
- PaymentHistoryViewModel.cs
```

#### Business Rules

- Payment gắn với `invoice_id`.
- Một invoice có thể có nhiều payments.
- `amount > 0`.
- Tổng payment `Success` không được vượt quá `invoice.total_amount`.
- Chỉ payment `Success` mới cộng vào `paid_amount`.
- Sau payment phải cập nhật `paid_amount`, `remaining_amount`, `invoice.status`.
- Nếu `remaining_amount = 0`, invoice status là `Paid`.
- Nếu invoice `Paid` và tất cả booking details đã checked out hoặc cancelled thì booking status là `Completed`.

#### Acceptance Criteria

- Nhập payment hợp lệ thì lưu thành công.
- Không cho thanh toán vượt quá remaining amount.
- Thanh toán một phần cập nhật invoice `PartiallyPaid`.
- Thanh toán đủ cập nhật invoice `Paid`.
- Booking chuyển `Completed` khi đủ điều kiện.
- Payment history hiển thị đúng.

---

### M1-BILLINGUI-001: Invoice/Payment UI and Billing Flow

- **Assignee:** Member 1 - Leader
- **Labels:** `type:feature`, `area:billing-ui`, `priority:high`
- **Branch:** `feature/member1-billing-ui-flow`
- **Start:** Day 12
- **Deadline:** Day 13
- **Estimate:** 8h
- **Depends on:** M1-PAYMENT-001
- **Blocks:** INT-004

#### Objective

Hoàn thiện màn hình billing để Receptionist demo được từ invoice đến payment.

#### WPF Scope

```text
- BillingView.xaml
- BillingViewModel.cs
- InvoiceSearchPanel
- InvoiceDetailPanel
- PaymentInputPanel
- PaymentHistoryPanel
```

#### Acceptance Criteria

- Receptionist tìm booking đã checkout.
- Tạo invoice từ UI.
- Xem invoice detail.
- Nhập payment từ UI.
- Payment history cập nhật ngay trên màn hình.
- Có thông báo lỗi/thành công rõ ràng.
- Không crash khi dữ liệu rỗng.

---

### M1-INTEGRATION-REVIEW-001: Review architecture, PRs and cross-module integration

- **Assignee:** Member 1 - Leader
- **Labels:** `type:integration`, `area:review`, `priority:critical`
- **Branch:** ongoing
- **Start:** Day 1
- **Deadline:** Day 14
- **Estimate:** 14h
- **Depends on:** CORE-005
- **Blocks:** All integration tasks

#### Objective

Leader kiểm soát architecture, review PR và đảm bảo các module ghép được với nhau.

#### Scope

- Review project reference.
- Review naming convention.
- Review Service không chứa SQL.
- Review ViewModel không gọi DAO/Repository.
- Review business rule quan trọng.
- Review conflict trước khi merge.
- Tạo bug issue nếu phát hiện lỗi.

#### Acceptance Criteria

- Không merge code không build.
- Không merge WPF gọi DAO.
- Không merge code phá schema.
- Mỗi integration milestone có checklist pass/fail.

---

## 13. Detailed Issues - Member 2

### M2-CUSTOMER-001: Customer Management Fullstack

- **Assignee:** Member 2
- **Labels:** `type:feature`, `area:customer`, `priority:high`
- **Branch:** `feature/member2-customer-management`
- **Start:** Day 3
- **Deadline:** Day 5
- **Estimate:** 12h
- **Depends on:** CORE-002, CORE-003, CORE-004, M1-AUTH-001
- **Blocks:** M2-BOOKING-001

#### Objective

Quản lý khách thuê phòng. Customer không đăng nhập hệ thống.

#### BusinessObjects

```text
Entities:
- Customer

DTOs:
- CustomerDto
- CustomerListItemDto
- CreateCustomerRequestDto
- UpdateCustomerRequestDto
```

#### DataAccessObjects

```text
- CustomerDao
```

#### Repositories

```text
- ICustomerRepository
- CustomerRepository
```

#### Services

```text
- ICustomerService
- CustomerService
```

#### WPF

```text
- CustomerManagementView.xaml
- CustomerDialog.xaml
- CustomerManagementViewModel.cs
- CustomerDialogViewModel.cs
```

#### Business Rules

- `identity_card` không được trùng nếu có nhập.
- `full_name` bắt buộc.
- Phone/email validate cơ bản.
- Không xóa cứng customer đã có booking.
- Có search theo tên, số điện thoại, CCCD.

#### Acceptance Criteria

- Tạo customer mới.
- Cập nhật customer.
- Tìm kiếm customer.
- Validate trùng identity card.
- Hiển thị lỗi trên UI thay vì crash.
- Booking UI có thể chọn customer từ service này.

---

### M2-ROOM-001: Room Type and Room Management Fullstack

- **Assignee:** Member 2
- **Labels:** `type:feature`, `area:room`, `priority:critical`
- **Branch:** `feature/member2-room-management`
- **Start:** Day 3
- **Deadline:** Day 6
- **Estimate:** 16h
- **Depends on:** CORE-002, CORE-003, CORE-004, M1-AUTH-001
- **Blocks:** M2-ROOMMAP-001, M2-BOOKING-001

#### Objective

Quản lý loại phòng và phòng cụ thể trong khách sạn.

#### BusinessObjects

```text
Entities:
- RoomType
- Room

DTOs:
- RoomTypeDto
- RoomDto
- RoomListItemDto
- CreateRoomTypeRequestDto
- CreateRoomRequestDto
- UpdateRoomStatusRequestDto

Enums:
- RoomTypeStatus
- RoomOperationalStatus
```

#### DataAccessObjects

```text
- RoomTypeDao
- RoomDao
```

#### Repositories

```text
- IRoomTypeRepository
- RoomTypeRepository
- IRoomRepository
- RoomRepository
```

#### Services

```text
- IRoomTypeService
- RoomTypeService
- IRoomService
- RoomService
```

#### WPF

```text
- RoomTypeManagementView.xaml
- RoomManagementView.xaml
- RoomTypeDialog.xaml
- RoomDialog.xaml
- RoomTypeManagementViewModel.cs
- RoomManagementViewModel.cs
```

#### Business Rules

- Room type name không được trùng.
- Room number không được trùng.
- Room thuộc một room type.
- Không booking phòng `Maintenance` hoặc `Inactive`.
- `rooms.status` là trạng thái vận hành, không phải trạng thái đặt phòng theo ngày.

#### Acceptance Criteria

- Admin tạo/sửa room type.
- Admin tạo/sửa room.
- Admin đổi trạng thái phòng: Available, Cleaning, Maintenance, Inactive.
- Không xóa cứng room đã có booking detail.
- Booking module lấy được danh sách room active.

---

### M2-ROOMMAP-001: Visual Room Map Fullstack

- **Assignee:** Member 2
- **Labels:** `type:feature`, `area:room-map`, `priority:high`
- **Branch:** `feature/member2-room-map`
- **Start:** Day 5
- **Deadline:** Day 7
- **Estimate:** 10h
- **Depends on:** M2-ROOM-001
- **Blocks:** M2-BOOKING-001, M3-CHECKIN-001

#### Objective

Hiển thị sơ đồ phòng theo trạng thái vận hành và trạng thái booking.

#### BusinessObjects

```text
DTOs:
- RoomMapItemDto
- RoomMapFilterDto
```

#### DataAccessObjects

```text
- RoomMapDao
```

#### Repositories

```text
- IRoomMapRepository
- RoomMapRepository
```

#### Services

```text
- IRoomMapService
- RoomMapService
```

#### WPF

```text
- RoomMapView.xaml
- RoomMapViewModel.cs
- RoomMapLegendControl.xaml
```

#### Display Status Rule

| Priority | Điều kiện | DisplayStatus |
|---:|---|---|
| 1 | `rooms.status = Maintenance` | `Maintenance` |
| 2 | `rooms.status = Inactive` | `Inactive` |
| 3 | `rooms.status = Cleaning` | `Cleaning` |
| 4 | Có `booking_detail.status = CheckedIn` tại thời điểm hiện tại | `Occupied` |
| 5 | Có `booking_detail.status = Reserved` trong khoảng ngày liên quan | `Reserved` |
| 6 | Không có booking active | `Available` |

#### Acceptance Criteria

- Room Map hiển thị theo tầng/phòng.
- Có legend rõ ràng.
- Phòng Maintenance/Inactive/Cleaning ưu tiên hiển thị trước trạng thái booking.
- Phòng Reserved/Occupied được tính từ booking_details.
- Có refresh sau booking/check-in/check-out.

---

### M2-BOOKING-001: Create Booking Fullstack

- **Assignee:** Member 2
- **Labels:** `type:feature`, `area:booking`, `priority:critical`
- **Branch:** `feature/member2-create-booking`
- **Start:** Day 6
- **Deadline:** Day 8
- **Estimate:** 18h
- **Depends on:** M2-CUSTOMER-001, M2-ROOM-001, M2-ROOMMAP-001
- **Blocks:** M3-CHECKIN-001, M4-DASHBOARD-001

#### Objective

Tạo booking mới, hỗ trợ một booking có nhiều phòng và chống trùng lịch.

#### BusinessObjects

```text
Entities:
- Booking
- BookingDetail

DTOs:
- CreateBookingRequestDto
- AvailableRoomDto
- BookingDetailDto
- BookingSummaryDto
```

#### DataAccessObjects

```text
- BookingDao
- BookingDetailDao
```

#### Repositories

```text
- IBookingRepository
- BookingRepository
```

#### Services

```text
- IBookingService
- BookingService
```

#### WPF

```text
- CreateBookingView.xaml
- CreateBookingViewModel.cs
- AvailableRoomPickerView.xaml
- SelectedRoomListControl.xaml
```

#### Business Rules

- Receptionist mới được tạo booking.
- Customer phải tồn tại hoặc được tạo mới trước khi booking.
- `CheckOutDate > CheckInDate`.
- `number_of_nights > 0`.
- `room_price` lấy từ `room_types.base_price` tại thời điểm booking.
- `room_total = room_price * number_of_nights`.
- Một booking có thể có nhiều booking details.
- Không booking phòng Maintenance/Inactive.
- Không booking trùng lịch.
- Chỉ kiểm tra trùng với booking details có status `Reserved` hoặc `CheckedIn`.

#### Conflict Formula

```sql
new_check_in_date < existing_check_out_date
AND new_check_out_date > existing_check_in_date
```

#### Acceptance Criteria

- Tạo booking cho một phòng.
- Tạo booking cho nhiều phòng.
- Tìm phòng trống theo khoảng ngày.
- Chặn phòng bị trùng lịch.
- Lưu đúng room price tại thời điểm booking.
- Tính đúng number of nights.
- Tính đúng room total.
- Sau booking, Room Map hiển thị Reserved.

---

### M2-BOOKING-002: Booking List, Cancel and Detail History Fullstack

- **Assignee:** Member 2
- **Labels:** `type:feature`, `area:booking`, `priority:medium`
- **Branch:** `feature/member2-booking-list-history`
- **Start:** Day 8
- **Deadline:** Day 9
- **Estimate:** 8h
- **Depends on:** M2-BOOKING-001
- **Blocks:** M2-BOOKING-TEST-001

#### Objective

Hiển thị danh sách booking, chi tiết booking và hỗ trợ cancel khi hợp lệ.

#### BusinessObjects

```text
DTOs:
- BookingListItemDto
- BookingDetailHistoryDto
- CancelBookingRequestDto
```

#### DataAccessObjects

```text
- BookingQueryDao
```

#### Repositories

```text
- IBookingQueryRepository
- BookingQueryRepository
```

#### Services

```text
- IBookingQueryService
- BookingQueryService
```

#### WPF

```text
- BookingListView.xaml
- BookingDetailView.xaml
- BookingListViewModel.cs
- BookingDetailViewModel.cs
```

#### Business Rules

- Chỉ cancel booking/detail khi chưa check-in.
- Cancel booking phải cập nhật booking details tương ứng.
- Không xóa cứng booking.
- Có search/filter theo customer, date, status.

#### Acceptance Criteria

- Xem danh sách booking.
- Xem chi tiết từng booking.
- Xem các phòng trong booking.
- Cancel booking hợp lệ.
- Không cancel booking đã CheckedIn/CheckedOut.

---

### M2-BOOKING-TEST-001: Booking flow test and handoff to operation team

- **Assignee:** Member 2
- **Labels:** `type:test`, `area:booking`, `priority:high`
- **Branch:** `test/member2-booking-flow`
- **Start:** Day 9
- **Deadline:** Day 9
- **Estimate:** 4h
- **Depends on:** M2-BOOKING-001, M2-BOOKING-002
- **Blocks:** INT-002, M3-CHECKIN-001

#### Objective

Test và bàn giao flow booking cho Member 3 dùng để làm check-in.

#### Acceptance Criteria

- Có ít nhất 3 booking mẫu.
- Có booking một phòng.
- Có booking nhiều phòng.
- Có booking Reserved để check-in.
- Có test case chống trùng lịch.
- Có ghi chú cách lấy `booking_detail_id` cho check-in.

---

## 14. Detailed Issues - Member 3

### M3-CHECKIN-001: Check-in Fullstack

- **Assignee:** Member 3
- **Labels:** `type:feature`, `area:checkin`, `priority:critical`
- **Branch:** `feature/member3-checkin`
- **Start:** Day 8
- **Deadline:** Day 9
- **Estimate:** 12h
- **Depends on:** M2-BOOKING-001
- **Blocks:** M3-SERVICEORDER-001, M3-CHECKOUT-001

#### Objective

Check-in theo từng booking detail/phòng.

#### BusinessObjects

```text
Entities:
- CheckRecord

DTOs:
- CheckInRequestDto
- CheckInCandidateDto
- CheckRecordDto

Enums:
- CheckRecordStatus
```

#### DataAccessObjects

```text
- CheckRecordDao
- CheckInQueryDao
- BookingOperationDao
```

#### Repositories

```text
- ICheckRecordRepository
- CheckRecordRepository
- ICheckInQueryRepository
- CheckInQueryRepository
- IBookingOperationRepository
- BookingOperationRepository
```

#### Services

```text
- ICheckInService
- CheckInService
```

#### WPF

```text
- CheckInView.xaml
- CheckInViewModel.cs
- CheckInCandidateListView.xaml
- CheckRecordDetailView.xaml
```

#### Business Rules

- Check-in xử lý theo `booking_detail_id`.
- Chỉ `booking_details.status = Reserved` mới được check-in.
- Không check-in nếu `rooms.status = Maintenance`, `Cleaning`, `Inactive`.
- Tạo `check_records` nếu chưa tồn tại.
- `actual_check_in_date = now`.
- `check_in_by_user_id = CurrentSession.UserId`.
- `check_records.status = CheckedIn`.
- `booking_details.status = CheckedIn`.

#### Acceptance Criteria

- Tìm được danh sách booking details sẵn sàng check-in.
- Check-in thành công một phòng.
- Check-in từng phòng trong booking nhiều phòng.
- Không check-in lại phòng đã CheckedIn.
- Room Map cập nhật Occupied sau check-in.
- Có lịch sử check-in cơ bản.

---

### M3-SERVICE-001: Service Catalog Management Fullstack

- **Assignee:** Member 3
- **Labels:** `type:feature`, `area:service`, `priority:high`
- **Branch:** `feature/member3-service-catalog`
- **Start:** Day 4
- **Deadline:** Day 6
- **Estimate:** 10h
- **Depends on:** CORE-002, CORE-003, CORE-004, M1-AUTH-001
- **Blocks:** M3-SERVICEORDER-001

#### Objective

Quản lý danh mục dịch vụ khách sạn.

#### BusinessObjects

```text
Entities:
- Service

DTOs:
- ServiceDto
- ServiceListItemDto
- CreateServiceRequestDto
- UpdateServiceRequestDto

Enums:
- ServiceStatus
```

#### DataAccessObjects

```text
- ServiceDao
```

#### Repositories

```text
- IServiceRepository
- ServiceRepository
```

#### Services

```text
- IServiceCatalogService
- ServiceCatalogService
```

#### WPF

```text
- ServiceManagementView.xaml
- ServiceDialog.xaml
- ServiceManagementViewModel.cs
- ServiceDialogViewModel.cs
```

#### Business Rules

- Service name không được trùng.
- Price phải >= 0.
- Không xóa cứng service đã có service order.
- Service `Inactive` không được order.

#### Acceptance Criteria

- Admin tạo/sửa service.
- Có search/filter theo name/category/status.
- Có inactive service.
- Service order chỉ chọn được service Active.

---

### M3-SERVICEORDER-001: Service Order Fullstack

- **Assignee:** Member 3
- **Labels:** `type:feature`, `area:service-order`, `priority:critical`
- **Branch:** `feature/member3-service-order`
- **Start:** Day 9
- **Deadline:** Day 10
- **Estimate:** 12h
- **Depends on:** M3-CHECKIN-001, M3-SERVICE-001
- **Blocks:** M3-CHECKOUT-001, M1-INVOICE-001, M4-REPORT-002

#### Objective

Ghi nhận dịch vụ phát sinh cho phòng đang ở.

#### BusinessObjects

```text
Entities:
- ServiceOrder

DTOs:
- ServiceOrderRequestDto
- ServiceOrderListItemDto
- ServiceOrderSummaryDto

Enums:
- ServiceOrderStatus
```

#### DataAccessObjects

```text
- ServiceOrderDao
- ServiceOrderSummaryDao
```

#### Repositories

```text
- IServiceOrderRepository
- ServiceOrderRepository
```

#### Services

```text
- IServiceOrderService
- ServiceOrderService
```

#### WPF

```text
- ServiceOrderView.xaml
- ServiceOrderListView.xaml
- ServiceOrderViewModel.cs
- ServiceOrderListViewModel.cs
```

#### Business Rules

- Service order xử lý theo `booking_detail_id`.
- Chỉ `booking_details.status = CheckedIn` mới được gọi dịch vụ.
- Không order service `Inactive`.
- `quantity > 0`.
- `unit_price` lấy từ `services.price` tại thời điểm order.
- `total_price = quantity * unit_price`.
- Lưu `created_by_user_id = CurrentSession.UserId`.
- Không tính service order `Cancelled` vào invoice.

#### Acceptance Criteria

- Chọn phòng đang CheckedIn.
- Chọn service Active.
- Nhập quantity.
- Tự tính total price.
- Lưu service order thành công.
- Cancel service order khi hợp lệ.
- Invoice module lấy được service order summary.

---

### M3-CHECKOUT-001: Check-out Fullstack

- **Assignee:** Member 3
- **Labels:** `type:feature`, `area:checkout`, `priority:critical`
- **Branch:** `feature/member3-checkout`
- **Start:** Day 10
- **Deadline:** Day 11
- **Estimate:** 12h
- **Depends on:** M3-CHECKIN-001, M3-SERVICEORDER-001
- **Blocks:** M1-INVOICE-001

#### Objective

Check-out theo từng booking detail/phòng bằng một transaction thống nhất.

#### BusinessObjects

```text
DTOs:
- CheckoutRequestDto
- CheckoutResultDto
- CheckoutCandidateDto
```

#### DataAccessObjects

```text
- CheckoutDao
- CheckRecordDao
- BookingOperationDao
- RoomOperationDao
```

#### Repositories

```text
- ICheckoutRepository
- CheckoutRepository
- IRoomOperationRepository
- RoomOperationRepository
```

#### Services

```text
- ICheckoutService
- CheckoutService
```

#### WPF

```text
- CheckoutView.xaml
- CheckoutViewModel.cs
- CheckoutCandidateListView.xaml
```

#### Business Rules

- Check-out xử lý theo `booking_detail_id`.
- Chỉ `booking_details.status = CheckedIn` mới được check-out.
- Khi check-out phải cập nhật trong cùng transaction:
  - `check_records.actual_check_out_date`.
  - `check_records.check_out_by_user_id = CurrentSession.UserId`.
  - `check_records.status = CheckedOut`.
  - `booking_details.status = CheckedOut`.
  - `rooms.status = Cleaning`.
- Không tạo invoice trong checkout service.
- Invoice là bước sau checkout.

#### Acceptance Criteria

- Tìm được danh sách phòng đang CheckedIn.
- Check-out thành công một phòng.
- Room status chuyển Cleaning.
- Booking detail chuyển CheckedOut.
- Check record có actual checkout date.
- Không check-out lại phòng đã CheckedOut.
- Invoice module nhận biết booking đã đủ điều kiện tạo invoice.

---

### M3-OPERATION-TEST-001: Operation flow test and handoff to billing team

- **Assignee:** Member 3
- **Labels:** `type:test`, `area:operation`, `priority:high`
- **Branch:** `test/member3-operation-flow`
- **Start:** Day 11
- **Deadline:** Day 11
- **Estimate:** 4h
- **Depends on:** M3-CHECKOUT-001
- **Blocks:** INT-003, M1-INVOICE-001

#### Objective

Test luồng vận hành và bàn giao dữ liệu cho billing.

#### Acceptance Criteria

- Có booking detail CheckedIn.
- Có service order phát sinh.
- Có booking detail CheckedOut.
- Có room Cleaning sau checkout.
- Có ghi chú booking nào dùng để tạo invoice.
- Test case operation pass trước khi merge INT-003.

---

## 15. Detailed Issues - Member 4

### M4-DASHBOARD-001: Dashboard Fullstack

- **Assignee:** Member 4
- **Labels:** `type:feature`, `area:dashboard`, `priority:medium`
- **Branch:** `feature/member4-dashboard`
- **Start:** Day 8
- **Deadline:** Day 10
- **Estimate:** 10h
- **Depends on:** M2-BOOKING-001, M3-CHECKIN-001
- **Blocks:** INT-004

#### Objective

Xây dựng dashboard tổng quan cho Admin/Manager.

#### BusinessObjects

```text
DTOs:
- DashboardSummaryDto
- RoomStatusSummaryDto
- BookingStatusSummaryDto
- TodayOperationSummaryDto
```

#### DataAccessObjects

```text
- DashboardDao
```

#### Repositories

```text
- IDashboardRepository
- DashboardRepository
```

#### Services

```text
- IDashboardService
- DashboardService
```

#### WPF

```text
- DashboardView.xaml
- DashboardViewModel.cs
- DashboardCardControl.xaml
```

#### Dashboard Metrics

- Tổng số phòng.
- Số phòng Available.
- Số phòng Occupied.
- Số phòng Reserved.
- Số phòng Cleaning/Maintenance.
- Số booking hôm nay.
- Số check-in hôm nay.
- Số check-out hôm nay.

#### Acceptance Criteria

- Admin/Manager xem được dashboard.
- Receptionist có thể bị ẩn menu nếu không cấp quyền.
- Dashboard không crash khi chưa có nhiều dữ liệu.
- Có refresh button.
- Có empty state rõ ràng.

---

### M4-REPORT-001: Occupancy Report Fullstack

- **Assignee:** Member 4
- **Labels:** `type:feature`, `area:report`, `priority:high`
- **Branch:** `feature/member4-occupancy-report`
- **Start:** Day 10
- **Deadline:** Day 12
- **Estimate:** 10h
- **Depends on:** M2-BOOKING-001, M3-CHECKIN-001
- **Blocks:** INT-004

#### Objective

Báo cáo công suất phòng dựa trên booking details.

#### BusinessObjects

```text
DTOs:
- OccupancyReportDto
- ReportFilterDto
```

#### DataAccessObjects

```text
- OccupancyReportDao
```

#### Repositories

```text
- IOccupancyReportRepository
- OccupancyReportRepository
```

#### Services

```text
- IOccupancyReportService
- OccupancyReportService
```

#### WPF

```text
- OccupancyReportView.xaml
- OccupancyReportViewModel.cs
```

#### Report Logic

- Dựa trên `booking_details`.
- Có filter theo date range.
- Có filter theo room type nếu kịp.
- Tính số phòng đã sử dụng/được đặt trong khoảng ngày.

#### Acceptance Criteria

- Admin/Manager xem được report.
- Có chọn from date/to date.
- Hiển thị tổng room nights.
- Hiển thị occupancy theo ngày hoặc theo room type.
- Không crash khi không có dữ liệu.

---

### M4-REPORT-002: Revenue and Service Usage Report Fullstack

- **Assignee:** Member 4
- **Labels:** `type:feature`, `area:report`, `priority:high`
- **Branch:** `feature/member4-revenue-service-report`
- **Start:** Day 12
- **Deadline:** Day 13
- **Estimate:** 12h
- **Depends on:** M1-PAYMENT-001, M3-SERVICEORDER-001
- **Blocks:** INT-004

#### Objective

Báo cáo doanh thu và sử dụng dịch vụ.

#### BusinessObjects

```text
DTOs:
- RevenueReportDto
- ServiceUsageReportDto
- PaymentRevenueDto
- ReportFilterDto
```

#### DataAccessObjects

```text
- RevenueReportDao
- ServiceUsageReportDao
```

#### Repositories

```text
- IRevenueReportRepository
- RevenueReportRepository
- IServiceUsageReportRepository
- ServiceUsageReportRepository
```

#### Services

```text
- IRevenueReportService
- RevenueReportService
- IServiceUsageReportService
- ServiceUsageReportService
```

#### WPF

```text
- RevenueReportView.xaml
- ServiceUsageReportView.xaml
- RevenueReportViewModel.cs
- ServiceUsageReportViewModel.cs
```

#### Report Logic

- Revenue report ưu tiên dựa trên `payments.status = Success` để phản ánh tiền thực thu.
- Service usage report dựa trên `service_orders.status = Ordered`.
- Có filter theo date range.

#### Acceptance Criteria

- Xem tổng doanh thu theo khoảng ngày.
- Xem doanh thu theo payment method nếu kịp.
- Xem số lượng service đã dùng.
- Xem doanh thu theo service.
- Không tính payment failed/refunded vào revenue.
- Không tính service order cancelled vào report.

---

### M4-REPORT-EXPORT-001: Report Export CSV Optional

- **Assignee:** Member 4
- **Labels:** `type:optional`, `area:report`, `priority:low`
- **Branch:** `feature/member4-report-export-csv`
- **Start:** Day 13
- **Deadline:** Day 13
- **Estimate:** 4h
- **Depends on:** M4-REPORT-001, M4-REPORT-002
- **Blocks:** None

#### Objective

Export report ra CSV nếu còn thời gian.

#### Scope

- Export Occupancy Report.
- Export Revenue Report.
- Export Service Usage Report.

#### Acceptance Criteria

- File CSV mở được bằng Excel.
- Có header rõ ràng.
- Không lỗi tiếng Việt nếu dùng UTF-8 BOM.

#### Note

Task này là optional. Nếu trễ deadline, có thể bỏ mà không ảnh hưởng core demo.

---

### M4-QA-DOCS-001: README, User Manual, Demo Script and Manual Test Cases

- **Assignee:** Member 4
- **Labels:** `type:docs`, `area:qa`, `priority:high`
- **Branch:** `docs/member4-qa-docs-demo`
- **Start:** Day 10
- **Deadline:** Day 14
- **Estimate:** 14h
- **Depends on:** INT-002
- **Blocks:** TEST-RELEASE-001

#### Objective

Chuẩn bị tài liệu setup, hướng dẫn dùng app, test case và demo script cuối kỳ.

#### Scope

```text
Documentation:
- README.md
- SQL_SETUP_GUIDE.md
- USER_MANUAL.md
- DEMO_SCRIPT.md
- MANUAL_TEST_CASES.md
- RELEASE_CHECKLIST.md
```

#### Manual Test Case Groups

- Auth.
- Customer.
- Room.
- Booking.
- Check-in.
- Service order.
- Check-out.
- Invoice.
- Payment.
- Dashboard/Report.

#### Acceptance Criteria

- README có hướng dẫn chạy project.
- SQL setup guide có hướng dẫn tạo database.
- User manual có ảnh hoặc mô tả từng màn hình chính nếu kịp.
- Demo script có luồng demo rõ ràng.
- Mỗi module có ít nhất 3 test cases.
- Test cases có input, steps, expected result, actual result, status.

---

## 16. Integration / Test / Release Issues

### INT-001: Merge Core + Auth + Admin User

- **Owner:** Member 1 - Leader
- **Participants:** All
- **Labels:** `type:integration`, `priority:critical`
- **Branch:** `integration/core-auth-admin`
- **Start:** Day 6
- **Deadline:** Day 6
- **Depends on:** M1-AUTH-001, M1-USERMGMT-001
- **Blocks:** INT-002

#### Acceptance Criteria

- Solution build thành công.
- Login chạy được.
- Role navigation chạy được.
- Admin user management chạy được.
- Không có reference sai tầng.

---

### INT-002: Merge Customer + Room + Booking + Room Map

- **Owner:** Member 1 - Leader
- **Main contributor:** Member 2
- **Labels:** `type:integration`, `priority:critical`
- **Branch:** `integration/customer-room-booking`
- **Start:** Day 9
- **Deadline:** Day 9
- **Depends on:** M2-BOOKING-TEST-001
- **Blocks:** INT-003

#### Acceptance Criteria

- Customer CRUD chạy được.
- Room/RoomType CRUD chạy được.
- Room Map chạy được.
- Create Booking chạy được.
- Booking list chạy được.
- Room Map hiển thị Reserved sau booking.

---

### INT-003: Merge Check-in + Service + Checkout

- **Owner:** Member 1 - Leader
- **Main contributor:** Member 3
- **Labels:** `type:integration`, `priority:critical`
- **Branch:** `integration/operation-service-checkout`
- **Start:** Day 11
- **Deadline:** Day 11
- **Depends on:** M3-OPERATION-TEST-001
- **Blocks:** INT-004

#### Acceptance Criteria

- Check-in chạy từ booking Reserved.
- Room Map hiển thị Occupied sau check-in.
- Service order chạy cho phòng CheckedIn.
- Check-out chạy được.
- Room chuyển Cleaning sau checkout.
- Data sẵn sàng để tạo invoice.

---

### INT-004: Merge Billing + Dashboard + Reports

- **Owner:** Member 1 - Leader
- **Participants:** Member 1, Member 4
- **Labels:** `type:integration`, `priority:critical`
- **Branch:** `integration/billing-report-dashboard`
- **Start:** Day 13
- **Deadline:** Day 13
- **Depends on:** M1-BILLINGUI-001, M4-REPORT-002
- **Blocks:** TEST-RELEASE-001

#### Acceptance Criteria

- Invoice tạo được sau checkout.
- Payment chạy được.
- Booking Completed sau paid đủ điều kiện.
- Dashboard hiển thị dữ liệu.
- Occupancy report chạy được.
- Revenue/service usage report chạy được.

---

### TEST-RELEASE-001: Full E2E test, bug fix and final release package

- **Owner:** All members
- **Lead:** Member 1
- **Labels:** `type:test`, `type:release`, `priority:critical`
- **Branch:** `release/final-demo`
- **Start:** Day 14
- **Deadline:** Day 14
- **Depends on:** INT-004, M4-QA-DOCS-001

#### Objective

Test full luồng demo, fix bug critical và đóng gói bản demo cuối kỳ.

#### E2E Flow

```text
1. Login Admin
2. Tạo Receptionist account nếu cần
3. Login Receptionist
4. Tạo customer
5. Tạo room type/room nếu cần
6. Tạo booking
7. Xem Room Map Reserved
8. Check-in
9. Xem Room Map Occupied
10. Gọi service
11. Check-out
12. Tạo invoice
13. Thanh toán
14. Booking Completed
15. Login Manager/Admin
16. Xem dashboard/report
```

#### Acceptance Criteria

- App build được trên máy demo.
- Database script chạy được từ đầu.
- Full E2E flow pass.
- Không còn crash critical.
- README và demo script hoàn chỉnh.
- Có release folder chứa source, sql script, docs, demo evidence nếu cần.

---

## 17. 2-Week Timeline Revised

| Day | Goal | Member 1 | Member 2 | Member 3 | Member 4 | Output cuối ngày |
|---|---|---|---|---|---|---|
| Day 1 | Khóa kiến trúc | CORE-001, CORE-002, CORE-005 | CORE-003 bắt đầu | Đọc ERD, chuẩn bị service module | Chuẩn bị UI/docs structure | Solution + convention |
| Day 2 | Database + common objects | CORE-002, M1-AUTH bắt đầu | CORE-003 hoàn tất | Chuẩn bị Service Catalog | CORE-004 support | DB schema + seed |
| Day 3 | WPF base + Auth | M1-AUTH, CORE-004 | M2-CUSTOMER, M2-ROOM bắt đầu | M3-SERVICE bắt đầu | CORE-004 support | Login backend draft |
| Day 4 | Login + CRUD nền | M1-AUTH | M2-CUSTOMER, M2-ROOM | M3-SERVICE | UI support/docs | Login chạy cơ bản |
| Day 5 | Customer/Room/Service | M1-USERMGMT bắt đầu | M2-CUSTOMER hoàn tất, M2-ROOM | M3-SERVICE | Docs/test template | CRUD nền |
| Day 6 | Admin + Room | INT-001 | M2-ROOM hoàn tất, M2-ROOMMAP | M3-SERVICE hoàn tất | Dashboard chuẩn bị | Merge auth/admin |
| Day 7 | Room Map + Booking | Review PR | M2-ROOMMAP, M2-BOOKING | Chuẩn bị check-in | Dashboard draft | Room Map chạy |
| Day 8 | Booking + Check-in | Support integration | M2-BOOKING | M3-CHECKIN | M4-DASHBOARD | Booking tạo được |
| Day 9 | Merge Booking | Review INT-002 | M2-BOOKING-TEST | M3-CHECKIN, M3-SERVICEORDER | M4-DASHBOARD | Booking → Check-in |
| Day 10 | Service Order + Checkout | M1-INVOICE chuẩn bị | Fix booking bugs | M3-SERVICEORDER, M3-CHECKOUT | M4-REPORT-001 | Service order chạy |
| Day 11 | Checkout + Invoice | M1-INVOICE | Support bugs | M3-CHECKOUT, TEST | M4-REPORT-001 | Checkout → Invoice |
| Day 12 | Payment + Reports | M1-PAYMENT | Regression booking | Support service data | M4-REPORT-002, docs | Payment chạy |
| Day 13 | Billing UI + Report | M1-BILLINGUI, INT-004 | Regression | Regression | Report polish/export optional | Full flow gần hoàn chỉnh |
| Day 14 | Release | TEST-RELEASE | TEST-RELEASE | TEST-RELEASE | QA/docs/release | Final demo package |

---

## 18. Dependency Map Revised

```text
CORE-001
  ├─ CORE-002
  ├─ CORE-003
  ├─ CORE-004
  └─ CORE-005

CORE-002 + CORE-003 + CORE-004
  ├─ M1-AUTH-001
  │   ├─ M1-USERMGMT-001
  │   └─ M1-INVOICE-001 → M1-PAYMENT-001 → M1-BILLINGUI-001
  │
  ├─ M2-CUSTOMER-001
  ├─ M2-ROOM-001
  │   └─ M2-ROOMMAP-001
  │       └─ M2-BOOKING-001 → M2-BOOKING-002 → M2-BOOKING-TEST-001
  │
  ├─ M3-SERVICE-001
  │   └─ M3-SERVICEORDER-001
  │
  └─ M4-DASHBOARD-001

M2-BOOKING-001
  └─ M3-CHECKIN-001
      ├─ M3-SERVICEORDER-001
      └─ M3-CHECKOUT-001
          └─ M1-INVOICE-001
              └─ M1-PAYMENT-001
                  └─ M4-REPORT-002

Integration:
INT-001 → INT-002 → INT-003 → INT-004 → TEST-RELEASE-001
```

---

## 19. Business Rules Final

### 19.1. Auth rules

- User phải login thành công trước khi vào MainWindow.
- User `Inactive` hoặc `Locked` không được login.
- Password không lưu plain text.
- Sau login phải lưu `CurrentSession`.
- `CurrentSession` phải có `UserId`, `Username`, `FullName`, `RoleName`.
- UI điều hướng theo role.
- Logout phải clear `CurrentSession`.

### 19.2. Booking rules

- Receptionist mới được tạo booking.
- Customer phải tồn tại hoặc được tạo mới trước khi tạo booking.
- `CheckOutDate > CheckInDate`.
- `number_of_nights > 0`.
- `room_price` lấy từ `room_types.base_price` tại thời điểm booking.
- `room_total = room_price * number_of_nights`.
- Một booking có thể có nhiều booking details.
- Không booking phòng `Maintenance` hoặc `Inactive`.
- Không booking trùng lịch.

```sql
new_check_in_date < existing_check_out_date
AND new_check_out_date > existing_check_in_date
```

- Chỉ kiểm tra trùng với status `Reserved`, `CheckedIn`.
- Không chặn với `Cancelled`, `CheckedOut`, `NoShow`.

### 19.3. Check-in rules

- Check-in xử lý theo `booking_detail_id`.
- Chỉ `Reserved` mới được check-in.
- Tạo `check_records` nếu chưa tồn tại.
- Cập nhật `booking_details.status = CheckedIn`.

### 19.4. Service order rules

- Service order xử lý theo `booking_detail_id`.
- Chỉ `CheckedIn` mới được gọi dịch vụ.
- Không order service `Inactive`.
- `quantity > 0`.
- Lưu `unit_price` tại thời điểm order.
- `total_price = quantity * unit_price`.

### 19.5. Check-out rules

- Check-out xử lý theo `booking_detail_id`.
- Chỉ `CheckedIn` mới được check-out.
- Cập nhật check record, booking detail và room trong một transaction.
- Sau check-out, room status chuyển `Cleaning`.

### 19.6. Invoice rules

- Invoice gắn với `booking_id`.
- Một booking chỉ có tối đa một invoice.
- Chỉ tạo invoice khi tất cả booking details của booking đã `CheckedOut` hoặc `Cancelled`.
- `room_amount = SUM(booking_details.room_total)`.
- `service_amount = SUM(service_orders.total_price)` với status `Ordered`.
- Invoice mới tạo có status `Unpaid`.

### 19.7. Payment rules

- Payment gắn với `invoice_id`.
- Một invoice có thể có nhiều payment.
- `amount > 0`.
- Không cho tổng payment Success vượt quá invoice total.
- Sau payment cập nhật `paid_amount`, `remaining_amount`, `invoice.status`.
- Invoice Paid và booking đã checkout xong thì booking Completed.

### 19.8. Report rules

- Occupancy Report dựa trên `booking_details`.
- Revenue Report ưu tiên dựa trên `payments.status = Success`.
- Service Usage Report dựa trên `service_orders.status = Ordered`.
- Manager/Admin được xem report.

---

## 20. Removed / Merged Task Mapping

Các task cũ không bị bỏ chức năng, mà được gộp vào vertical slice mới.

| Nhóm task cũ | Gộp vào task mới |
|---|---|
| SETUP-001, SETUP-002 | CORE-001 |
| BO-001 phần common | CORE-002 |
| DB-001, DB-002, DAO-001 phần connection | CORE-003 |
| WPF-001, UI-001, UI-003 | CORE-004 |
| AUTH-001 đến AUTH-005 | M1-AUTH-001 |
| USERMGMT-001 đến USERMGMT-003 | M1-USERMGMT-001 |
| CUSTOMER-001 đến CUSTOMER-003 | M2-CUSTOMER-001 |
| ROOMTYPE-001, ROOMTYPE-002, ROOM-001, ROOM-002, ROOM-003 | M2-ROOM-001 |
| ROOMMAP-001, ROOMMAP-002, UI-004 | M2-ROOMMAP-001 |
| BOOKING-001 đến BOOKING-005 | M2-BOOKING-001 |
| BOOKING-006, BOOKING-007 | M2-BOOKING-002 |
| CHECKIN-001 đến CHECKIN-006 | M3-CHECKIN-001 |
| SERVICE-001 đến SERVICE-003 | M3-SERVICE-001 |
| SERVICEORDER-001 đến SERVICEORDER-005 | M3-SERVICEORDER-001 |
| CHECKOUT-001 đến CHECKOUT-003 | M3-CHECKOUT-001 |
| INVOICE-001 đến INVOICE-003 | M1-INVOICE-001 |
| PAYMENT-001 đến PAYMENT-003 | M1-PAYMENT-001 |
| CHECKOUTPAYMENT-001, CHECKOUTPAYMENT-002 | M1-BILLINGUI-001 |
| DASHBOARD-001, DASHBOARD-002 | M4-DASHBOARD-001 |
| REPORT-001, REPORT-004, REPORT-005, REPORT-006 phần occupancy | M4-REPORT-001 |
| REPORT-002, REPORT-003, REPORT-004, REPORT-005, REPORT-006 phần revenue/service | M4-REPORT-002 |
| REPORT-007, REPORT-008 | M4-REPORT-EXPORT-001 optional |
| DOCS-001, DOCS-002, TESTDOC-001, QA-001, QA-002 | M4-QA-DOCS-001 |
| INT-001 đến INT-005, TEST-001, TEST-002, RELEASE-001 | INT-001 đến TEST-RELEASE-001 |

---

## 21. Optional Scope

Nếu team bị trễ, các phần sau có thể cắt mà không ảnh hưởng core demo:

- Export CSV report.
- Print preview.
- Room history view nâng cao.
- Check record history/detail nâng cao.
- Service order cancel nâng cao.
- Dashboard chart nâng cao.
- UI animation.

Không được cắt các phần sau:

- Login.
- Customer.
- Room.
- Booking.
- Check-in.
- Service order.
- Check-out.
- Invoice.
- Payment.
- Basic dashboard/report.

---

## 22. Definition of Done

Một issue chỉ được coi là done khi:

- Code build thành công.
- Có đủ layer theo vertical slice.
- Không vi phạm project reference.
- Không gọi DAO từ WPF.
- Không viết SQL trong ViewModel.
- Service có validate nghiệp vụ chính.
- UI có error message rõ ràng.
- Có test data hoặc test case cơ bản.
- PR được review.
- Merge vào `develop` không làm hỏng flow cũ.

---

## 23. Final Demo Checklist

```text
[ ] SQL script chạy được từ database rỗng
[ ] App build được
[ ] Login Admin thành công
[ ] Login Receptionist thành công
[ ] Role menu đúng
[ ] Tạo customer thành công
[ ] Tạo room type/room thành công
[ ] Tạo booking thành công
[ ] Chặn booking trùng lịch
[ ] Room Map hiển thị Reserved
[ ] Check-in thành công
[ ] Room Map hiển thị Occupied
[ ] Tạo service order thành công
[ ] Check-out thành công
[ ] Room chuyển Cleaning
[ ] Tạo invoice thành công
[ ] Chặn duplicate invoice
[ ] Thanh toán một phần thành công
[ ] Thanh toán đủ thành công
[ ] Booking Completed đúng điều kiện
[ ] Dashboard có dữ liệu
[ ] Report có dữ liệu
[ ] README đầy đủ
[ ] Demo script đầy đủ
```

---

## 24. Kết luận

Bản task revised này giữ nguyên nghiệp vụ chính của hệ thống khách sạn, nhưng chuyển cách chia việc sang **vertical slice fullstack** để phù hợp hơn với team 4 người làm trong 2 tuần.

Phân công cuối cùng:

```text
Member 1: Auth + Admin User + Invoice + Payment + Integration
Member 2: Customer + Room + Room Map + Booking
Member 3: Check-in + Service + Service Order + Check-out
Member 4: Dashboard + Report + QA + Docs
```

Mỗi người đều làm đủ:

```text
BusinessObjects
→ DataAccessObjects
→ Repositories
→ Services
→ WPF
```

Luồng demo chính sau khi hoàn thành:

```text
Login
→ Customer
→ Room
→ Booking
→ Check-in
→ Service order
→ Check-out
→ Invoice
→ Payment
→ Dashboard/Report
```

Đây là cách chia gọn hơn, ít chồng chéo hơn, dễ làm GitHub Issues hơn và thực tế hơn cho đồ án 2 tuần.
