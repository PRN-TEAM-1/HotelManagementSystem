USE [HotelManagementSystem];
GO

SET NOCOUNT ON;
SET ANSI_NULLS ON;
SET ANSI_PADDING ON;
SET ANSI_WARNINGS ON;
SET ARITHABORT ON;
SET CONCAT_NULL_YIELDS_NULL ON;
SET QUOTED_IDENTIFIER ON;
SET NUMERIC_ROUNDABORT OFF;

/*
Default demo accounts:
- admin / Admin@123
- manager / Manager@123
- reception / Reception@123

Password hashing:
- SHA2_256
- Stored as uppercase hexadecimal string
*/

IF NOT EXISTS (SELECT 1 FROM dbo.roles WHERE role_name = N'Admin')
BEGIN
    INSERT INTO dbo.roles (role_name) VALUES (N'Admin');
END

IF NOT EXISTS (SELECT 1 FROM dbo.roles WHERE role_name = N'Manager')
BEGIN
    INSERT INTO dbo.roles (role_name) VALUES (N'Manager');
END

IF NOT EXISTS (SELECT 1 FROM dbo.roles WHERE role_name = N'Receptionist')
BEGIN
    INSERT INTO dbo.roles (role_name) VALUES (N'Receptionist');
END

DECLARE @AdminRoleId INT = (SELECT role_id FROM dbo.roles WHERE role_name = N'Admin');
DECLARE @ManagerRoleId INT = (SELECT role_id FROM dbo.roles WHERE role_name = N'Manager');
DECLARE @ReceptionistRoleId INT = (SELECT role_id FROM dbo.roles WHERE role_name = N'Receptionist');

DECLARE @AdminPasswordHash NVARCHAR(64) = CONVERT(VARCHAR(64), HASHBYTES('SHA2_256', 'Admin@123'), 2);
DECLARE @ManagerPasswordHash NVARCHAR(64) = CONVERT(VARCHAR(64), HASHBYTES('SHA2_256', 'Manager@123'), 2);
DECLARE @ReceptionPasswordHash NVARCHAR(64) = CONVERT(VARCHAR(64), HASHBYTES('SHA2_256', 'Reception@123'), 2);

IF NOT EXISTS (SELECT 1 FROM dbo.users WHERE username = N'admin')
BEGIN
    INSERT INTO dbo.users
    (
        role_id,
        username,
        password_hash,
        full_name,
        email,
        phone_number,
        status
    )
    VALUES
    (
        @AdminRoleId,
        N'admin',
        @AdminPasswordHash,
        N'System Administrator',
        N'admin@hotel.local',
        N'0900000001',
        N'Active'
    );
END

IF NOT EXISTS (SELECT 1 FROM dbo.users WHERE username = N'manager')
BEGIN
    INSERT INTO dbo.users
    (
        role_id,
        username,
        password_hash,
        full_name,
        email,
        phone_number,
        status
    )
    VALUES
    (
        @ManagerRoleId,
        N'manager',
        @ManagerPasswordHash,
        N'Hotel Manager',
        N'manager@hotel.local',
        N'0900000002',
        N'Active'
    );
END

IF NOT EXISTS (SELECT 1 FROM dbo.users WHERE username = N'reception')
BEGIN
    INSERT INTO dbo.users
    (
        role_id,
        username,
        password_hash,
        full_name,
        email,
        phone_number,
        status
    )
    VALUES
    (
        @ReceptionistRoleId,
        N'reception',
        @ReceptionPasswordHash,
        N'Receptionist Demo',
        N'reception@hotel.local',
        N'0900000003',
        N'Active'
    );
END

IF NOT EXISTS (SELECT 1 FROM dbo.room_types WHERE type_name = N'Standard')
BEGIN
    INSERT INTO dbo.room_types (type_name, description, base_price, capacity, status)
    VALUES (N'Standard', N'Cozy standard room for short stays.', 450000, 2, N'Active');
END

IF NOT EXISTS (SELECT 1 FROM dbo.room_types WHERE type_name = N'Deluxe')
BEGIN
    INSERT INTO dbo.room_types (type_name, description, base_price, capacity, status)
    VALUES (N'Deluxe', N'Larger room with upgraded interior and amenities.', 700000, 3, N'Active');
END

IF NOT EXISTS (SELECT 1 FROM dbo.room_types WHERE type_name = N'Suite')
BEGIN
    INSERT INTO dbo.room_types (type_name, description, base_price, capacity, status)
    VALUES (N'Suite', N'Premium suite suitable for long stays or VIP guests.', 1200000, 4, N'Active');
END

DECLARE @StandardRoomTypeId INT = (SELECT room_type_id FROM dbo.room_types WHERE type_name = N'Standard');
DECLARE @DeluxeRoomTypeId INT = (SELECT room_type_id FROM dbo.room_types WHERE type_name = N'Deluxe');
DECLARE @SuiteRoomTypeId INT = (SELECT room_type_id FROM dbo.room_types WHERE type_name = N'Suite');

IF NOT EXISTS (SELECT 1 FROM dbo.rooms WHERE room_number = N'101')
BEGIN
    INSERT INTO dbo.rooms (room_type_id, room_number, floor, status, note)
    VALUES (@StandardRoomTypeId, N'101', 1, N'Available', N'Near the lobby.');
END

IF NOT EXISTS (SELECT 1 FROM dbo.rooms WHERE room_number = N'102')
BEGIN
    INSERT INTO dbo.rooms (room_type_id, room_number, floor, status, note)
    VALUES (@StandardRoomTypeId, N'102', 1, N'Available', N'Quiet side of the building.');
END

IF NOT EXISTS (SELECT 1 FROM dbo.rooms WHERE room_number = N'103')
BEGIN
    INSERT INTO dbo.rooms (room_type_id, room_number, floor, status, note)
    VALUES (@StandardRoomTypeId, N'103', 1, N'Cleaning', N'Prepared for next arrival.');
END

IF NOT EXISTS (SELECT 1 FROM dbo.rooms WHERE room_number = N'201')
BEGIN
    INSERT INTO dbo.rooms (room_type_id, room_number, floor, status, note)
    VALUES (@DeluxeRoomTypeId, N'201', 2, N'Available', N'City view.');
END

IF NOT EXISTS (SELECT 1 FROM dbo.rooms WHERE room_number = N'202')
BEGIN
    INSERT INTO dbo.rooms (room_type_id, room_number, floor, status, note)
    VALUES (@DeluxeRoomTypeId, N'202', 2, N'Available', N'Extra sofa bed.');
END

IF NOT EXISTS (SELECT 1 FROM dbo.rooms WHERE room_number = N'203')
BEGIN
    INSERT INTO dbo.rooms (room_type_id, room_number, floor, status, note)
    VALUES (@DeluxeRoomTypeId, N'203', 2, N'Maintenance', N'Air conditioner under maintenance.');
END

IF NOT EXISTS (SELECT 1 FROM dbo.rooms WHERE room_number = N'301')
BEGIN
    INSERT INTO dbo.rooms (room_type_id, room_number, floor, status, note)
    VALUES (@SuiteRoomTypeId, N'301', 3, N'Available', N'VIP suite with living area.');
END

IF NOT EXISTS (SELECT 1 FROM dbo.rooms WHERE room_number = N'302')
BEGIN
    INSERT INTO dbo.rooms (room_type_id, room_number, floor, status, note)
    VALUES (@SuiteRoomTypeId, N'302', 3, N'Inactive', N'Reserved for renovation plan.');
END

IF NOT EXISTS (SELECT 1 FROM dbo.services WHERE service_name = N'Breakfast Buffet')
BEGIN
    INSERT INTO dbo.services (service_name, category, price, status)
    VALUES (N'Breakfast Buffet', N'Food', 120000, N'Active');
END

IF NOT EXISTS (SELECT 1 FROM dbo.services WHERE service_name = N'Laundry')
BEGIN
    INSERT INTO dbo.services (service_name, category, price, status)
    VALUES (N'Laundry', N'Laundry', 80000, N'Active');
END

IF NOT EXISTS (SELECT 1 FROM dbo.services WHERE service_name = N'Mineral Water')
BEGIN
    INSERT INTO dbo.services (service_name, category, price, status)
    VALUES (N'Mineral Water', N'Minibar', 15000, N'Active');
END

IF NOT EXISTS (SELECT 1 FROM dbo.services WHERE service_name = N'Airport Transfer')
BEGIN
    INSERT INTO dbo.services (service_name, category, price, status)
    VALUES (N'Airport Transfer', N'Transport', 250000, N'Active');
END

IF NOT EXISTS (SELECT 1 FROM dbo.services WHERE service_name = N'Spa Package')
BEGIN
    INSERT INTO dbo.services (service_name, category, price, status)
    VALUES (N'Spa Package', N'Spa', 500000, N'Active');
END

-- =========================================================
-- Demo operation data
-- Covers the main screens after login:
-- 1. Check-in candidate: Reserved booking detail without check record.
-- 2. Checkout candidate: CheckedIn booking detail with service orders.
-- 3. Invoice candidate: CheckedOut booking without invoice.
-- 4. Payment demo: PartiallyPaid invoice with payment history.
-- 5. Paid history demo: Completed booking with fully paid invoice.
-- =========================================================

DECLARE @ReceptionistUserId INT = (SELECT user_id FROM dbo.users WHERE username = N'reception');
DECLARE @AdminUserId INT = (SELECT user_id FROM dbo.users WHERE username = N'admin');

DECLARE @Room101Id INT = (SELECT room_id FROM dbo.rooms WHERE room_number = N'101');
DECLARE @Room102Id INT = (SELECT room_id FROM dbo.rooms WHERE room_number = N'102');
DECLARE @Room201Id INT = (SELECT room_id FROM dbo.rooms WHERE room_number = N'201');
DECLARE @Room202Id INT = (SELECT room_id FROM dbo.rooms WHERE room_number = N'202');
DECLARE @Room301Id INT = (SELECT room_id FROM dbo.rooms WHERE room_number = N'301');

DECLARE @StandardRoomPrice DECIMAL(18,2) = (SELECT base_price FROM dbo.room_types WHERE type_name = N'Standard');
DECLARE @DeluxeRoomPrice DECIMAL(18,2) = (SELECT base_price FROM dbo.room_types WHERE type_name = N'Deluxe');
DECLARE @SuiteRoomPrice DECIMAL(18,2) = (SELECT base_price FROM dbo.room_types WHERE type_name = N'Suite');

DECLARE @BreakfastServiceId INT = (SELECT service_id FROM dbo.services WHERE service_name = N'Breakfast Buffet');
DECLARE @BreakfastPrice DECIMAL(18,2) = (SELECT price FROM dbo.services WHERE service_name = N'Breakfast Buffet');
DECLARE @LaundryServiceId INT = (SELECT service_id FROM dbo.services WHERE service_name = N'Laundry');
DECLARE @LaundryPrice DECIMAL(18,2) = (SELECT price FROM dbo.services WHERE service_name = N'Laundry');
DECLARE @WaterServiceId INT = (SELECT service_id FROM dbo.services WHERE service_name = N'Mineral Water');
DECLARE @WaterPrice DECIMAL(18,2) = (SELECT price FROM dbo.services WHERE service_name = N'Mineral Water');
DECLARE @AirportTransferServiceId INT = (SELECT service_id FROM dbo.services WHERE service_name = N'Airport Transfer');
DECLARE @AirportTransferPrice DECIMAL(18,2) = (SELECT price FROM dbo.services WHERE service_name = N'Airport Transfer');
DECLARE @SpaServiceId INT = (SELECT service_id FROM dbo.services WHERE service_name = N'Spa Package');
DECLARE @SpaPrice DECIMAL(18,2) = (SELECT price FROM dbo.services WHERE service_name = N'Spa Package');

DECLARE @DemoToday DATETIME2 = CAST(SYSDATETIME() AS DATE);
DECLARE @DemoNow DATETIME2 = SYSDATETIME();

-- Demo customers
IF NOT EXISTS (SELECT 1 FROM dbo.customers WHERE identity_card = N'DEMO-CHECKIN-001')
BEGIN
    INSERT INTO dbo.customers (full_name, identity_card, phone_number, email, address)
    VALUES (N'Nguyen Van Checkin', N'DEMO-CHECKIN-001', N'0911000001', N'checkin.demo@hotel.local', N'Ho Chi Minh City');
END

IF NOT EXISTS (SELECT 1 FROM dbo.customers WHERE identity_card = N'DEMO-CHECKOUT-001')
BEGIN
    INSERT INTO dbo.customers (full_name, identity_card, phone_number, email, address)
    VALUES (N'Tran Thi Checkout', N'DEMO-CHECKOUT-001', N'0911000002', N'checkout.demo@hotel.local', N'Da Nang');
END

IF NOT EXISTS (SELECT 1 FROM dbo.customers WHERE identity_card = N'DEMO-INVOICE-001')
BEGIN
    INSERT INTO dbo.customers (full_name, identity_card, phone_number, email, address)
    VALUES (N'Le Van Invoice', N'DEMO-INVOICE-001', N'0911000003', N'invoice.demo@hotel.local', N'Ha Noi');
END

IF NOT EXISTS (SELECT 1 FROM dbo.customers WHERE identity_card = N'DEMO-PARTIALPAY-001')
BEGIN
    INSERT INTO dbo.customers (full_name, identity_card, phone_number, email, address)
    VALUES (N'Pham Thi Partial Pay', N'DEMO-PARTIALPAY-001', N'0911000004', N'partialpay.demo@hotel.local', N'Can Tho');
END

IF NOT EXISTS (SELECT 1 FROM dbo.customers WHERE identity_card = N'DEMO-PAID-001')
BEGIN
    INSERT INTO dbo.customers (full_name, identity_card, phone_number, email, address)
    VALUES (N'Hoang Van Paid', N'DEMO-PAID-001', N'0911000005', N'paid.demo@hotel.local', N'Nha Trang');
END

DECLARE @CheckInCustomerId INT = (SELECT customer_id FROM dbo.customers WHERE identity_card = N'DEMO-CHECKIN-001');
DECLARE @CheckoutCustomerId INT = (SELECT customer_id FROM dbo.customers WHERE identity_card = N'DEMO-CHECKOUT-001');
DECLARE @InvoiceCustomerId INT = (SELECT customer_id FROM dbo.customers WHERE identity_card = N'DEMO-INVOICE-001');
DECLARE @PartialPayCustomerId INT = (SELECT customer_id FROM dbo.customers WHERE identity_card = N'DEMO-PARTIALPAY-001');
DECLARE @PaidCustomerId INT = (SELECT customer_id FROM dbo.customers WHERE identity_card = N'DEMO-PAID-001');

-- 1. Check-in candidate
IF NOT EXISTS (SELECT 1 FROM dbo.bookings WHERE note = N'SEED-DEMO-CHECKIN-READY')
BEGIN
    INSERT INTO dbo.bookings (customer_id, created_by_user_id, booking_date, status, note)
    VALUES (@CheckInCustomerId, @ReceptionistUserId, DATEADD(DAY, -1, @DemoToday), N'Confirmed', N'SEED-DEMO-CHECKIN-READY');
END

DECLARE @CheckInBookingId INT = (SELECT booking_id FROM dbo.bookings WHERE note = N'SEED-DEMO-CHECKIN-READY');

IF NOT EXISTS (SELECT 1 FROM dbo.booking_details WHERE note = N'SEED-DEMO-CHECKIN-READY-101')
BEGIN
    INSERT INTO dbo.booking_details
    (
        booking_id,
        room_id,
        check_in_date,
        check_out_date,
        room_price,
        number_of_nights,
        room_total,
        status,
        note
    )
    VALUES
    (
        @CheckInBookingId,
        @Room101Id,
        DATEADD(DAY, 1, @DemoToday),
        DATEADD(DAY, 2, @DemoToday),
        @StandardRoomPrice,
        1,
        @StandardRoomPrice,
        N'Reserved',
        N'SEED-DEMO-CHECKIN-READY-101'
    );
END

-- 2. Checkout candidate with active stay and service orders
IF NOT EXISTS (SELECT 1 FROM dbo.bookings WHERE note = N'SEED-DEMO-CHECKOUT-READY')
BEGIN
    INSERT INTO dbo.bookings (customer_id, created_by_user_id, booking_date, status, note)
    VALUES (@CheckoutCustomerId, @ReceptionistUserId, DATEADD(DAY, -2, @DemoToday), N'Confirmed', N'SEED-DEMO-CHECKOUT-READY');
END

DECLARE @CheckoutBookingId INT = (SELECT booking_id FROM dbo.bookings WHERE note = N'SEED-DEMO-CHECKOUT-READY');

IF NOT EXISTS (SELECT 1 FROM dbo.booking_details WHERE note = N'SEED-DEMO-CHECKOUT-READY-102')
BEGIN
    INSERT INTO dbo.booking_details
    (
        booking_id,
        room_id,
        check_in_date,
        check_out_date,
        room_price,
        number_of_nights,
        room_total,
        status,
        note
    )
    VALUES
    (
        @CheckoutBookingId,
        @Room102Id,
        DATEADD(DAY, -1, @DemoToday),
        DATEADD(DAY, 1, @DemoToday),
        @StandardRoomPrice,
        2,
        @StandardRoomPrice * 2,
        N'CheckedIn',
        N'SEED-DEMO-CHECKOUT-READY-102'
    );
END

DECLARE @CheckoutBookingDetailId INT = (SELECT booking_detail_id FROM dbo.booking_details WHERE note = N'SEED-DEMO-CHECKOUT-READY-102');

IF NOT EXISTS (SELECT 1 FROM dbo.check_records WHERE booking_detail_id = @CheckoutBookingDetailId)
BEGIN
    INSERT INTO dbo.check_records
    (
        booking_detail_id,
        check_in_by_user_id,
        actual_check_in_date,
        check_in_note,
        status
    )
    VALUES
    (
        @CheckoutBookingDetailId,
        @ReceptionistUserId,
        DATEADD(HOUR, 14, DATEADD(DAY, -1, @DemoToday)),
        N'Demo guest checked in and ready for checkout.',
        N'CheckedIn'
    );
END

IF NOT EXISTS (SELECT 1 FROM dbo.service_orders WHERE note = N'SEED-DEMO-CHECKOUT-BREAKFAST')
BEGIN
    INSERT INTO dbo.service_orders (booking_detail_id, service_id, created_by_user_id, quantity, unit_price, total_price, order_date, status, note)
    VALUES (@CheckoutBookingDetailId, @BreakfastServiceId, @ReceptionistUserId, 2, @BreakfastPrice, @BreakfastPrice * 2, DATEADD(HOUR, 8, @DemoToday), N'Ordered', N'SEED-DEMO-CHECKOUT-BREAKFAST');
END

IF NOT EXISTS (SELECT 1 FROM dbo.service_orders WHERE note = N'SEED-DEMO-CHECKOUT-WATER')
BEGIN
    INSERT INTO dbo.service_orders (booking_detail_id, service_id, created_by_user_id, quantity, unit_price, total_price, order_date, status, note)
    VALUES (@CheckoutBookingDetailId, @WaterServiceId, @ReceptionistUserId, 3, @WaterPrice, @WaterPrice * 3, DATEADD(HOUR, 9, @DemoToday), N'Ordered', N'SEED-DEMO-CHECKOUT-WATER');
END

-- 3. Invoice candidate: checked-out booking without invoice
IF NOT EXISTS (SELECT 1 FROM dbo.bookings WHERE note = N'SEED-DEMO-INVOICE-READY')
BEGIN
    INSERT INTO dbo.bookings (customer_id, created_by_user_id, booking_date, status, note)
    VALUES (@InvoiceCustomerId, @ReceptionistUserId, DATEADD(DAY, -3, @DemoToday), N'Confirmed', N'SEED-DEMO-INVOICE-READY');
END

DECLARE @InvoiceReadyBookingId INT = (SELECT booking_id FROM dbo.bookings WHERE note = N'SEED-DEMO-INVOICE-READY');

IF NOT EXISTS (SELECT 1 FROM dbo.booking_details WHERE note = N'SEED-DEMO-INVOICE-READY-201')
BEGIN
    INSERT INTO dbo.booking_details
    (
        booking_id,
        room_id,
        check_in_date,
        check_out_date,
        room_price,
        number_of_nights,
        room_total,
        status,
        note
    )
    VALUES
    (
        @InvoiceReadyBookingId,
        @Room201Id,
        DATEADD(DAY, -2, @DemoToday),
        DATEADD(DAY, -1, @DemoToday),
        @DeluxeRoomPrice,
        1,
        @DeluxeRoomPrice,
        N'CheckedOut',
        N'SEED-DEMO-INVOICE-READY-201'
    );
END

DECLARE @InvoiceReadyBookingDetailId INT = (SELECT booking_detail_id FROM dbo.booking_details WHERE note = N'SEED-DEMO-INVOICE-READY-201');

IF NOT EXISTS (SELECT 1 FROM dbo.check_records WHERE booking_detail_id = @InvoiceReadyBookingDetailId)
BEGIN
    INSERT INTO dbo.check_records
    (
        booking_detail_id,
        check_in_by_user_id,
        check_out_by_user_id,
        actual_check_in_date,
        actual_check_out_date,
        check_in_note,
        check_out_note,
        status
    )
    VALUES
    (
        @InvoiceReadyBookingDetailId,
        @ReceptionistUserId,
        @ReceptionistUserId,
        DATEADD(HOUR, 14, DATEADD(DAY, -2, @DemoToday)),
        DATEADD(HOUR, 11, DATEADD(DAY, -1, @DemoToday)),
        N'Demo invoice-ready check-in.',
        N'Demo invoice-ready checkout.',
        N'CheckedOut'
    );
END

IF NOT EXISTS (SELECT 1 FROM dbo.service_orders WHERE note = N'SEED-DEMO-INVOICE-LAUNDRY')
BEGIN
    INSERT INTO dbo.service_orders (booking_detail_id, service_id, created_by_user_id, quantity, unit_price, total_price, order_date, status, note)
    VALUES (@InvoiceReadyBookingDetailId, @LaundryServiceId, @ReceptionistUserId, 1, @LaundryPrice, @LaundryPrice, DATEADD(HOUR, 16, DATEADD(DAY, -2, @DemoToday)), N'Ordered', N'SEED-DEMO-INVOICE-LAUNDRY');
END

IF NOT EXISTS (SELECT 1 FROM dbo.service_orders WHERE note = N'SEED-DEMO-INVOICE-WATER')
BEGIN
    INSERT INTO dbo.service_orders (booking_detail_id, service_id, created_by_user_id, quantity, unit_price, total_price, order_date, status, note)
    VALUES (@InvoiceReadyBookingDetailId, @WaterServiceId, @ReceptionistUserId, 2, @WaterPrice, @WaterPrice * 2, DATEADD(HOUR, 20, DATEADD(DAY, -2, @DemoToday)), N'Ordered', N'SEED-DEMO-INVOICE-WATER');
END

-- 4. Partially paid invoice: use this to test recording the remaining payment
IF NOT EXISTS (SELECT 1 FROM dbo.bookings WHERE note = N'SEED-DEMO-PARTIALPAY')
BEGIN
    INSERT INTO dbo.bookings (customer_id, created_by_user_id, booking_date, status, note)
    VALUES (@PartialPayCustomerId, @ReceptionistUserId, DATEADD(DAY, -5, @DemoToday), N'Confirmed', N'SEED-DEMO-PARTIALPAY');
END

DECLARE @PartialPayBookingId INT = (SELECT booking_id FROM dbo.bookings WHERE note = N'SEED-DEMO-PARTIALPAY');

IF NOT EXISTS (SELECT 1 FROM dbo.booking_details WHERE note = N'SEED-DEMO-PARTIALPAY-202')
BEGIN
    INSERT INTO dbo.booking_details
    (
        booking_id,
        room_id,
        check_in_date,
        check_out_date,
        room_price,
        number_of_nights,
        room_total,
        status,
        note
    )
    VALUES
    (
        @PartialPayBookingId,
        @Room202Id,
        DATEADD(DAY, -4, @DemoToday),
        DATEADD(DAY, -2, @DemoToday),
        @DeluxeRoomPrice,
        2,
        @DeluxeRoomPrice * 2,
        N'CheckedOut',
        N'SEED-DEMO-PARTIALPAY-202'
    );
END

DECLARE @PartialPayBookingDetailId INT = (SELECT booking_detail_id FROM dbo.booking_details WHERE note = N'SEED-DEMO-PARTIALPAY-202');

IF NOT EXISTS (SELECT 1 FROM dbo.check_records WHERE booking_detail_id = @PartialPayBookingDetailId)
BEGIN
    INSERT INTO dbo.check_records
    (
        booking_detail_id,
        check_in_by_user_id,
        check_out_by_user_id,
        actual_check_in_date,
        actual_check_out_date,
        check_in_note,
        check_out_note,
        status
    )
    VALUES
    (
        @PartialPayBookingDetailId,
        @ReceptionistUserId,
        @ReceptionistUserId,
        DATEADD(HOUR, 15, DATEADD(DAY, -4, @DemoToday)),
        DATEADD(HOUR, 10, DATEADD(DAY, -2, @DemoToday)),
        N'Demo partial-pay check-in.',
        N'Demo partial-pay checkout.',
        N'CheckedOut'
    );
END

IF NOT EXISTS (SELECT 1 FROM dbo.service_orders WHERE note = N'SEED-DEMO-PARTIALPAY-AIRPORT')
BEGIN
    INSERT INTO dbo.service_orders (booking_detail_id, service_id, created_by_user_id, quantity, unit_price, total_price, order_date, status, note)
    VALUES (@PartialPayBookingDetailId, @AirportTransferServiceId, @ReceptionistUserId, 1, @AirportTransferPrice, @AirportTransferPrice, DATEADD(HOUR, 9, DATEADD(DAY, -3, @DemoToday)), N'Ordered', N'SEED-DEMO-PARTIALPAY-AIRPORT');
END

IF NOT EXISTS (SELECT 1 FROM dbo.service_orders WHERE note = N'SEED-DEMO-PARTIALPAY-BREAKFAST')
BEGIN
    INSERT INTO dbo.service_orders (booking_detail_id, service_id, created_by_user_id, quantity, unit_price, total_price, order_date, status, note)
    VALUES (@PartialPayBookingDetailId, @BreakfastServiceId, @ReceptionistUserId, 2, @BreakfastPrice, @BreakfastPrice * 2, DATEADD(HOUR, 8, DATEADD(DAY, -3, @DemoToday)), N'Ordered', N'SEED-DEMO-PARTIALPAY-BREAKFAST');
END

DECLARE @PartialRoomAmount DECIMAL(18,2) = @DeluxeRoomPrice * 2;
DECLARE @PartialServiceAmount DECIMAL(18,2) = @AirportTransferPrice + (@BreakfastPrice * 2);
DECLARE @PartialDiscount DECIMAL(18,2) = 90000;
DECLARE @PartialTotal DECIMAL(18,2) = @PartialRoomAmount + @PartialServiceAmount - @PartialDiscount;
DECLARE @PartialPaid DECIMAL(18,2) = 800000;

IF NOT EXISTS (SELECT 1 FROM dbo.invoices WHERE booking_id = @PartialPayBookingId)
BEGIN
    INSERT INTO dbo.invoices
    (
        booking_id,
        created_by_user_id,
        room_amount,
        service_amount,
        discount_amount,
        tax_amount,
        total_amount,
        paid_amount,
        remaining_amount,
        create_date,
        status,
        note
    )
    VALUES
    (
        @PartialPayBookingId,
        @ReceptionistUserId,
        @PartialRoomAmount,
        @PartialServiceAmount,
        @PartialDiscount,
        0,
        @PartialTotal,
        @PartialPaid,
        @PartialTotal - @PartialPaid,
        DATEADD(HOUR, 12, DATEADD(DAY, -2, @DemoToday)),
        N'PartiallyPaid',
        N'SEED-DEMO-PARTIALPAY-INVOICE'
    );
END

DECLARE @PartialInvoiceId INT = (SELECT invoice_id FROM dbo.invoices WHERE booking_id = @PartialPayBookingId);

IF NOT EXISTS (SELECT 1 FROM dbo.payments WHERE transaction_code = N'DEMO-PARTIAL-CASH-001')
BEGIN
    INSERT INTO dbo.payments (invoice_id, received_by_user_id, payment_method, amount, payment_date, transaction_code, status, note)
    VALUES (@PartialInvoiceId, @ReceptionistUserId, N'Cash', @PartialPaid, DATEADD(HOUR, 12, DATEADD(DAY, -2, @DemoToday)), N'DEMO-PARTIAL-CASH-001', N'Success', N'SEED-DEMO-PARTIALPAY-FIRST-PAYMENT');
END

IF NOT EXISTS (SELECT 1 FROM dbo.payments WHERE transaction_code = N'DEMO-PARTIAL-CARD-FAILED-002')
BEGIN
    INSERT INTO dbo.payments (invoice_id, received_by_user_id, payment_method, amount, payment_date, transaction_code, status, note)
    VALUES (@PartialInvoiceId, @ReceptionistUserId, N'CreditCard', @PartialTotal - @PartialPaid, DATEADD(MINUTE, 20, DATEADD(HOUR, 12, DATEADD(DAY, -2, @DemoToday))), N'DEMO-PARTIAL-CARD-FAILED-002', N'Failed', N'SEED-DEMO-PARTIALPAY-FAILED-CARD');
END

-- 5. Fully paid invoice with two successful payments
IF NOT EXISTS (SELECT 1 FROM dbo.bookings WHERE note = N'SEED-DEMO-PAID')
BEGIN
    INSERT INTO dbo.bookings (customer_id, created_by_user_id, booking_date, status, note)
    VALUES (@PaidCustomerId, @ReceptionistUserId, DATEADD(DAY, -8, @DemoToday), N'Completed', N'SEED-DEMO-PAID');
END

DECLARE @PaidBookingId INT = (SELECT booking_id FROM dbo.bookings WHERE note = N'SEED-DEMO-PAID');

IF NOT EXISTS (SELECT 1 FROM dbo.booking_details WHERE note = N'SEED-DEMO-PAID-301')
BEGIN
    INSERT INTO dbo.booking_details
    (
        booking_id,
        room_id,
        check_in_date,
        check_out_date,
        room_price,
        number_of_nights,
        room_total,
        status,
        note
    )
    VALUES
    (
        @PaidBookingId,
        @Room301Id,
        DATEADD(DAY, -7, @DemoToday),
        DATEADD(DAY, -6, @DemoToday),
        @SuiteRoomPrice,
        1,
        @SuiteRoomPrice,
        N'CheckedOut',
        N'SEED-DEMO-PAID-301'
    );
END

DECLARE @PaidBookingDetailId INT = (SELECT booking_detail_id FROM dbo.booking_details WHERE note = N'SEED-DEMO-PAID-301');

IF NOT EXISTS (SELECT 1 FROM dbo.check_records WHERE booking_detail_id = @PaidBookingDetailId)
BEGIN
    INSERT INTO dbo.check_records
    (
        booking_detail_id,
        check_in_by_user_id,
        check_out_by_user_id,
        actual_check_in_date,
        actual_check_out_date,
        check_in_note,
        check_out_note,
        status
    )
    VALUES
    (
        @PaidBookingDetailId,
        @ReceptionistUserId,
        @AdminUserId,
        DATEADD(HOUR, 14, DATEADD(DAY, -7, @DemoToday)),
        DATEADD(HOUR, 10, DATEADD(DAY, -6, @DemoToday)),
        N'Demo paid check-in.',
        N'Demo paid checkout.',
        N'CheckedOut'
    );
END

IF NOT EXISTS (SELECT 1 FROM dbo.service_orders WHERE note = N'SEED-DEMO-PAID-SPA')
BEGIN
    INSERT INTO dbo.service_orders (booking_detail_id, service_id, created_by_user_id, quantity, unit_price, total_price, order_date, status, note)
    VALUES (@PaidBookingDetailId, @SpaServiceId, @ReceptionistUserId, 1, @SpaPrice, @SpaPrice, DATEADD(HOUR, 17, DATEADD(DAY, -7, @DemoToday)), N'Ordered', N'SEED-DEMO-PAID-SPA');
END

IF NOT EXISTS (SELECT 1 FROM dbo.service_orders WHERE note = N'SEED-DEMO-PAID-WATER')
BEGIN
    INSERT INTO dbo.service_orders (booking_detail_id, service_id, created_by_user_id, quantity, unit_price, total_price, order_date, status, note)
    VALUES (@PaidBookingDetailId, @WaterServiceId, @ReceptionistUserId, 2, @WaterPrice, @WaterPrice * 2, DATEADD(HOUR, 19, DATEADD(DAY, -7, @DemoToday)), N'Ordered', N'SEED-DEMO-PAID-WATER');
END

DECLARE @PaidRoomAmount DECIMAL(18,2) = @SuiteRoomPrice;
DECLARE @PaidServiceAmount DECIMAL(18,2) = @SpaPrice + (@WaterPrice * 2);
DECLARE @PaidTotal DECIMAL(18,2) = @PaidRoomAmount + @PaidServiceAmount;

IF NOT EXISTS (SELECT 1 FROM dbo.invoices WHERE booking_id = @PaidBookingId)
BEGIN
    INSERT INTO dbo.invoices
    (
        booking_id,
        created_by_user_id,
        room_amount,
        service_amount,
        discount_amount,
        tax_amount,
        total_amount,
        paid_amount,
        remaining_amount,
        create_date,
        status,
        note
    )
    VALUES
    (
        @PaidBookingId,
        @AdminUserId,
        @PaidRoomAmount,
        @PaidServiceAmount,
        0,
        0,
        @PaidTotal,
        @PaidTotal,
        0,
        DATEADD(HOUR, 12, DATEADD(DAY, -6, @DemoToday)),
        N'Paid',
        N'SEED-DEMO-PAID-INVOICE'
    );
END

DECLARE @PaidInvoiceId INT = (SELECT invoice_id FROM dbo.invoices WHERE booking_id = @PaidBookingId);

IF NOT EXISTS (SELECT 1 FROM dbo.payments WHERE transaction_code = N'DEMO-PAID-BANK-001')
BEGIN
    INSERT INTO dbo.payments (invoice_id, received_by_user_id, payment_method, amount, payment_date, transaction_code, status, note)
    VALUES (@PaidInvoiceId, @ReceptionistUserId, N'BankTransfer', 1000000, DATEADD(HOUR, 12, DATEADD(DAY, -6, @DemoToday)), N'DEMO-PAID-BANK-001', N'Success', N'SEED-DEMO-PAID-FIRST-PAYMENT');
END

IF NOT EXISTS (SELECT 1 FROM dbo.payments WHERE transaction_code = N'DEMO-PAID-EWALLET-002')
BEGIN
    INSERT INTO dbo.payments (invoice_id, received_by_user_id, payment_method, amount, payment_date, transaction_code, status, note)
    VALUES (@PaidInvoiceId, @AdminUserId, N'EWallet', @PaidTotal - 1000000, DATEADD(HOUR, 13, DATEADD(DAY, -6, @DemoToday)), N'DEMO-PAID-EWALLET-002', N'Success', N'SEED-DEMO-PAID-FINAL-PAYMENT');
END

IF NOT EXISTS (SELECT 1 FROM dbo.payments WHERE transaction_code = N'DEMO-PAID-CARD-REFUND-003')
BEGIN
    INSERT INTO dbo.payments (invoice_id, received_by_user_id, payment_method, amount, payment_date, transaction_code, status, note)
    VALUES (@PaidInvoiceId, @AdminUserId, N'CreditCard', 100000, DATEADD(HOUR, 15, DATEADD(DAY, -6, @DemoToday)), N'DEMO-PAID-CARD-REFUND-003', N'Refunded', N'SEED-DEMO-PAID-REFUNDED-CARD');
END
