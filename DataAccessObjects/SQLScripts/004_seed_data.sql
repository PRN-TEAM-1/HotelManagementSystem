USE [HotelManagementSystem];
GO

SET NOCOUNT ON;

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
