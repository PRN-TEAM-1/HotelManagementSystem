
-- =========================================================
-- FILE 003: Tạo các ràng buộc dữ liệu và index hỗ trợ truy vấn
-- Mục tiêu:
-- 1. Đảm bảo dữ liệu không vi phạm quy tắc nghiệp vụ
-- 2. Thiết lập quan hệ khóa ngoại giữa các bảng
-- 3. Tạo index để tăng tốc các truy vấn thường dùng
-- 4. Dùng IF NOT EXISTS / OBJECT_ID để script có thể chạy lại an toàn
-- =========================================================

-- Role name phải là duy nhất để không có 2 role trùng tên.
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'UX_roles_role_name' AND object_id = OBJECT_ID(N'dbo.roles'))
BEGIN
    CREATE UNIQUE INDEX UX_roles_role_name ON dbo.roles(role_name);
END
GO

-- Username là định danh đăng nhập nên không được phép trùng.
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'UX_users_username' AND object_id = OBJECT_ID(N'dbo.users'))
BEGIN
    CREATE UNIQUE INDEX UX_users_username ON dbo.users(username);
END
GO

-- Email user cũng cần duy nhất để tránh nhiều tài khoản dùng chung email.
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'UX_users_email' AND object_id = OBJECT_ID(N'dbo.users'))
BEGIN
    CREATE UNIQUE INDEX UX_users_email ON dbo.users(email);
END
GO

-- CCCD/CMND/Hộ chiếu của customer là duy nhất khi có nhập dữ liệu.
-- Cho phép nhiều bản ghi NULL vì không phải khách nào cũng bắt buộc có identity_card.
IF NOT EXISTS (
    SELECT 1
    FROM sys.indexes
    WHERE name = N'UX_customers_identity_card'
      AND object_id = OBJECT_ID(N'dbo.customers')
)
BEGIN
    CREATE UNIQUE INDEX UX_customers_identity_card
    ON dbo.customers(identity_card)
    WHERE identity_card IS NOT NULL;
END
GO

-- Tên loại phòng phải duy nhất để tránh trùng Standard/Deluxe/Suite.
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'UX_room_types_type_name' AND object_id = OBJECT_ID(N'dbo.room_types'))
BEGIN
    CREATE UNIQUE INDEX UX_room_types_type_name ON dbo.room_types(type_name);
END
GO

-- Mỗi số phòng trong khách sạn là duy nhất.
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'UX_rooms_room_number' AND object_id = OBJECT_ID(N'dbo.rooms'))
BEGIN
    CREATE UNIQUE INDEX UX_rooms_room_number ON dbo.rooms(room_number);
END
GO

-- Tên dịch vụ là duy nhất để tránh tạo trùng danh mục dịch vụ.
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'UX_services_service_name' AND object_id = OBJECT_ID(N'dbo.services'))
BEGIN
    CREATE UNIQUE INDEX UX_services_service_name ON dbo.services(service_name);
END
GO

-- Mỗi booking chỉ được phép có tối đa một invoice.
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'UX_invoices_booking_id' AND object_id = OBJECT_ID(N'dbo.invoices'))
BEGIN
    CREATE UNIQUE INDEX UX_invoices_booking_id ON dbo.invoices(booking_id);
END
GO

-- Mỗi booking_detail chỉ có một check_record để lưu trạng thái check-in/check-out thực tế.
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'UX_check_records_booking_detail_id' AND object_id = OBJECT_ID(N'dbo.check_records'))
BEGIN
    CREATE UNIQUE INDEX UX_check_records_booking_detail_id ON dbo.check_records(booking_detail_id);
END
GO

-- Index hỗ trợ join user với role khi login hoặc quản lý tài khoản.
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_users_role_id' AND object_id = OBJECT_ID(N'dbo.users'))
BEGIN
    CREATE INDEX IX_users_role_id ON dbo.users(role_id);
END
GO

-- Index hỗ trợ lọc phòng theo loại phòng và trạng thái vận hành.
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_rooms_room_type_status' AND object_id = OBJECT_ID(N'dbo.rooms'))
BEGIN
    CREATE INDEX IX_rooms_room_type_status ON dbo.rooms(room_type_id, status);
END
GO

-- Index hỗ trợ tra cứu booking theo customer và trạng thái booking.
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_bookings_customer_status' AND object_id = OBJECT_ID(N'dbo.bookings'))
BEGIN
    CREATE INDEX IX_bookings_customer_status ON dbo.bookings(customer_id, status);
END
GO

-- Index hỗ trợ tra cứu các booking do một nhân viên tạo.
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_bookings_created_by_user_id' AND object_id = OBJECT_ID(N'dbo.bookings'))
BEGIN
    CREATE INDEX IX_bookings_created_by_user_id ON dbo.bookings(created_by_user_id);
END
GO

-- Index hỗ trợ join booking -> booking_details khi xem chi tiết booking.
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_booking_details_booking_id' AND object_id = OBJECT_ID(N'dbo.booking_details'))
BEGIN
    CREATE INDEX IX_booking_details_booking_id ON dbo.booking_details(booking_id);
END
GO

-- Index quan trọng cho bài toán kiểm tra trùng lịch phòng theo room và khoảng ngày.
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_booking_details_room_schedule' AND object_id = OBJECT_ID(N'dbo.booking_details'))
BEGIN
    CREATE INDEX IX_booking_details_room_schedule
        ON dbo.booking_details(room_id, check_in_date, check_out_date, status);
END
GO

-- Index hỗ trợ lấy service order theo booking_detail và trạng thái còn hiệu lực.
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_service_orders_booking_detail_status' AND object_id = OBJECT_ID(N'dbo.service_orders'))
BEGIN
    CREATE INDEX IX_service_orders_booking_detail_status ON dbo.service_orders(booking_detail_id, status);
END
GO

-- Index hỗ trợ truy vấn payment theo invoice và trạng thái thanh toán.
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_payments_invoice_status' AND object_id = OBJECT_ID(N'dbo.payments'))
BEGIN
    CREATE INDEX IX_payments_invoice_status ON dbo.payments(invoice_id, status);
END
GO

-- Index hỗ trợ lọc invoice theo trạng thái như Unpaid / Paid / PartiallyPaid.
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_invoices_status' AND object_id = OBJECT_ID(N'dbo.invoices'))
BEGIN
    CREATE INDEX IX_invoices_status ON dbo.invoices(status);
END
GO

-- User phải thuộc về một role hợp lệ có trong bảng roles.
IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_users_roles')
BEGIN
    ALTER TABLE dbo.users
        ADD CONSTRAINT FK_users_roles FOREIGN KEY (role_id) REFERENCES dbo.roles(role_id);
END
GO

-- Mỗi room phải gắn với đúng một room_type có tồn tại.
IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_rooms_room_types')
BEGIN
    ALTER TABLE dbo.rooms
        ADD CONSTRAINT FK_rooms_room_types FOREIGN KEY (room_type_id) REFERENCES dbo.room_types(room_type_id);
END
GO

-- Mỗi booking phải thuộc về một customer có tồn tại trong hệ thống.
IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_bookings_customers')
BEGIN
    ALTER TABLE dbo.bookings
        ADD CONSTRAINT FK_bookings_customers FOREIGN KEY (customer_id) REFERENCES dbo.customers(customer_id);
END
GO

-- Mỗi booking phải biết nhân viên nào đã tạo booking đó.
IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_bookings_users')
BEGIN
    ALTER TABLE dbo.bookings
        ADD CONSTRAINT FK_bookings_users FOREIGN KEY (created_by_user_id) REFERENCES dbo.users(user_id);
END
GO

-- Mỗi booking_detail phải thuộc về một booking cha.
IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_booking_details_bookings')
BEGIN
    ALTER TABLE dbo.booking_details
        ADD CONSTRAINT FK_booking_details_bookings FOREIGN KEY (booking_id) REFERENCES dbo.bookings(booking_id);
END
GO

-- Mỗi booking_detail phải tham chiếu đến một room có thật.
IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_booking_details_rooms')
BEGIN
    ALTER TABLE dbo.booking_details
        ADD CONSTRAINT FK_booking_details_rooms FOREIGN KEY (room_id) REFERENCES dbo.rooms(room_id);
END
GO

-- Check record luôn phải gắn với đúng một booking_detail.
IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_check_records_booking_details')
BEGIN
    ALTER TABLE dbo.check_records
        ADD CONSTRAINT FK_check_records_booking_details FOREIGN KEY (booking_detail_id) REFERENCES dbo.booking_details(booking_detail_id);
END
GO

-- Lưu dấu nhân viên thực hiện thao tác check-in.
IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_check_records_check_in_users')
BEGIN
    ALTER TABLE dbo.check_records
        ADD CONSTRAINT FK_check_records_check_in_users FOREIGN KEY (check_in_by_user_id) REFERENCES dbo.users(user_id);
END
GO

-- Lưu dấu nhân viên thực hiện thao tác check-out.
IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_check_records_check_out_users')
BEGIN
    ALTER TABLE dbo.check_records
        ADD CONSTRAINT FK_check_records_check_out_users FOREIGN KEY (check_out_by_user_id) REFERENCES dbo.users(user_id);
END
GO

-- Service order phải phát sinh từ một booking_detail cụ thể.
IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_service_orders_booking_details')
BEGIN
    ALTER TABLE dbo.service_orders
        ADD CONSTRAINT FK_service_orders_booking_details FOREIGN KEY (booking_detail_id) REFERENCES dbo.booking_details(booking_detail_id);
END
GO

-- Service order phải tham chiếu tới một dịch vụ có trong danh mục services.
IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_service_orders_services')
BEGIN
    ALTER TABLE dbo.service_orders
        ADD CONSTRAINT FK_service_orders_services FOREIGN KEY (service_id) REFERENCES dbo.services(service_id);
END
GO

-- Lưu dấu user nào đã ghi nhận service order.
IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_service_orders_users')
BEGIN
    ALTER TABLE dbo.service_orders
        ADD CONSTRAINT FK_service_orders_users FOREIGN KEY (created_by_user_id) REFERENCES dbo.users(user_id);
END
GO

-- Invoice phải thuộc về một booking có tồn tại.
IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_invoices_bookings')
BEGIN
    ALTER TABLE dbo.invoices
        ADD CONSTRAINT FK_invoices_bookings FOREIGN KEY (booking_id) REFERENCES dbo.bookings(booking_id);
END
GO

-- Lưu dấu user nào đã tạo invoice.
IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_invoices_users')
BEGIN
    ALTER TABLE dbo.invoices
        ADD CONSTRAINT FK_invoices_users FOREIGN KEY (created_by_user_id) REFERENCES dbo.users(user_id);
END
GO

-- Payment phải gắn với một invoice hợp lệ.
IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_payments_invoices')
BEGIN
    ALTER TABLE dbo.payments
        ADD CONSTRAINT FK_payments_invoices FOREIGN KEY (invoice_id) REFERENCES dbo.invoices(invoice_id);
END
GO

-- Lưu dấu user nào đã nhận thanh toán từ khách.
IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_payments_users')
BEGIN
    ALTER TABLE dbo.payments
        ADD CONSTRAINT FK_payments_users FOREIGN KEY (received_by_user_id) REFERENCES dbo.users(user_id);
END
GO

-- Chỉ cho phép status user nằm trong 3 trạng thái hệ thống định nghĩa sẵn.
IF OBJECT_ID(N'dbo.CK_users_status', N'C') IS NULL
BEGIN
    ALTER TABLE dbo.users
        ADD CONSTRAINT CK_users_status CHECK (status IN (N'Active', N'Inactive', N'Locked'));
END
GO

-- Status của room_type chỉ được là Active hoặc Inactive.
IF OBJECT_ID(N'dbo.CK_room_types_status', N'C') IS NULL
BEGIN
    ALTER TABLE dbo.room_types
        ADD CONSTRAINT CK_room_types_status CHECK (status IN (N'Active', N'Inactive'));
END
GO

-- Giá loại phòng không âm và sức chứa phải lớn hơn 0.
IF OBJECT_ID(N'dbo.CK_room_types_values', N'C') IS NULL
BEGIN
    ALTER TABLE dbo.room_types
        ADD CONSTRAINT CK_room_types_values CHECK (base_price >= 0 AND capacity > 0);
END
GO

-- Trạng thái vận hành của phòng chỉ được thuộc bộ status đã thiết kế.
IF OBJECT_ID(N'dbo.CK_rooms_status', N'C') IS NULL
BEGIN
    ALTER TABLE dbo.rooms
        ADD CONSTRAINT CK_rooms_status CHECK (status IN (N'Available', N'Cleaning', N'Maintenance', N'Inactive'));
END
GO

-- Số tầng không được âm.
IF OBJECT_ID(N'dbo.CK_rooms_floor', N'C') IS NULL
BEGIN
    ALTER TABLE dbo.rooms
        ADD CONSTRAINT CK_rooms_floor CHECK (floor >= 0);
END
GO

-- Booking tổng chỉ được dùng các trạng thái nghiệp vụ hợp lệ.
IF OBJECT_ID(N'dbo.CK_bookings_status', N'C') IS NULL
BEGIN
    ALTER TABLE dbo.bookings
        ADD CONSTRAINT CK_bookings_status CHECK (status IN (N'Pending', N'Confirmed', N'Cancelled', N'Completed', N'NoShow'));
END
GO

-- Booking detail chỉ được đi qua các trạng thái đã định nghĩa trong flow đặt phòng/lưu trú.
IF OBJECT_ID(N'dbo.CK_booking_details_status', N'C') IS NULL
BEGIN
    ALTER TABLE dbo.booking_details
        ADD CONSTRAINT CK_booking_details_status CHECK (status IN (N'Reserved', N'CheckedIn', N'CheckedOut', N'Cancelled', N'NoShow'));
END
GO

-- Check nghiệp vụ cốt lõi của booking_detail:
-- 1. Ngày check-out phải lớn hơn check-in
-- 2. Số đêm phải > 0
-- 3. Giá và tổng tiền không âm
-- 4. room_total phải đúng bằng room_price * number_of_nights
IF OBJECT_ID(N'dbo.CK_booking_details_values', N'C') IS NULL
BEGIN
    ALTER TABLE dbo.booking_details
        ADD CONSTRAINT CK_booking_details_values CHECK
        (
            check_out_date > check_in_date
            AND number_of_nights > 0
            AND room_price >= 0
            AND room_total >= 0
            AND room_total = room_price * number_of_nights
        );
END
GO

-- Check record chỉ được lưu các trạng thái hợp lệ của quá trình check-in/check-out.
IF OBJECT_ID(N'dbo.CK_check_records_status', N'C') IS NULL
BEGIN
    ALTER TABLE dbo.check_records
        ADD CONSTRAINT CK_check_records_status CHECK (status IN (N'CheckedIn', N'CheckedOut', N'Cancelled'));
END
GO

-- Nếu đã có actual_check_out_date thì thời điểm đó không được nhỏ hơn actual_check_in_date.
IF OBJECT_ID(N'dbo.CK_check_records_dates', N'C') IS NULL
BEGIN
    ALTER TABLE dbo.check_records
        ADD CONSTRAINT CK_check_records_dates CHECK
        (
            actual_check_out_date IS NULL
            OR actual_check_out_date >= actual_check_in_date
        );
END
GO

-- Status dịch vụ chỉ có thể là Active hoặc Inactive.
IF OBJECT_ID(N'dbo.CK_services_status', N'C') IS NULL
BEGIN
    ALTER TABLE dbo.services
        ADD CONSTRAINT CK_services_status CHECK (status IN (N'Active', N'Inactive'));
END
GO

-- Giá dịch vụ không được âm.
IF OBJECT_ID(N'dbo.CK_services_price', N'C') IS NULL
BEGIN
    ALTER TABLE dbo.services
        ADD CONSTRAINT CK_services_price CHECK (price >= 0);
END
GO

-- Service order chỉ được ở trạng thái Ordered hoặc Cancelled.
IF OBJECT_ID(N'dbo.CK_service_orders_status', N'C') IS NULL
BEGIN
    ALTER TABLE dbo.service_orders
        ADD CONSTRAINT CK_service_orders_status CHECK (status IN (N'Ordered', N'Cancelled'));
END
GO

-- Check nghiệp vụ cho service order:
-- 1. quantity phải > 0
-- 2. unit_price và total_price không âm
-- 3. total_price phải đúng bằng quantity * unit_price
IF OBJECT_ID(N'dbo.CK_service_orders_values', N'C') IS NULL
BEGIN
    ALTER TABLE dbo.service_orders
        ADD CONSTRAINT CK_service_orders_values CHECK
        (
            quantity > 0
            AND unit_price >= 0
            AND total_price >= 0
            AND total_price = quantity * unit_price
        );
END
GO

-- Invoice chỉ được ở các trạng thái thanh toán hợp lệ.
IF OBJECT_ID(N'dbo.CK_invoices_status', N'C') IS NULL
BEGIN
    ALTER TABLE dbo.invoices
        ADD CONSTRAINT CK_invoices_status CHECK (status IN (N'Unpaid', N'PartiallyPaid', N'Paid', N'Cancelled'));
END
GO

-- Check công thức tính tiền của invoice:
-- 1. Các số tiền thành phần không được âm
-- 2. total_amount = room_amount + service_amount + tax_amount - discount_amount
-- 3. remaining_amount = total_amount - paid_amount
-- 4. paid_amount không được vượt total_amount
IF OBJECT_ID(N'dbo.CK_invoices_values', N'C') IS NULL
BEGIN
    ALTER TABLE dbo.invoices
        ADD CONSTRAINT CK_invoices_values CHECK
        (
            room_amount >= 0
            AND service_amount >= 0
            AND discount_amount >= 0
            AND tax_amount >= 0
            AND total_amount >= 0
            AND paid_amount >= 0
            AND remaining_amount >= 0
            AND total_amount = room_amount + service_amount + tax_amount - discount_amount
            AND remaining_amount = total_amount - paid_amount
            AND paid_amount <= total_amount
        );
END
GO

-- Payment chỉ được có 3 trạng thái kết quả đã định nghĩa.
IF OBJECT_ID(N'dbo.CK_payments_status', N'C') IS NULL
BEGIN
    ALTER TABLE dbo.payments
        ADD CONSTRAINT CK_payments_status CHECK (status IN (N'Success', N'Failed', N'Refunded'));
END
GO

-- Payment method bị giới hạn trong các phương thức thanh toán hệ thống hỗ trợ.
IF OBJECT_ID(N'dbo.CK_payments_method', N'C') IS NULL
BEGIN
    ALTER TABLE dbo.payments
        ADD CONSTRAINT CK_payments_method CHECK (payment_method IN (N'Cash', N'BankTransfer', N'CreditCard', N'EWallet'));
END
GO

-- Mỗi lần thanh toán phải có số tiền lớn hơn 0.
IF OBJECT_ID(N'dbo.CK_payments_amount', N'C') IS NULL
BEGIN
    ALTER TABLE dbo.payments
        ADD CONSTRAINT CK_payments_amount CHECK (amount > 0);
END
GO
