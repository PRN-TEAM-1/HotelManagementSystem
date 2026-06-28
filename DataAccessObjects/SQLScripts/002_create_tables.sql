USE [HotelManagementSystem];
GO

IF OBJECT_ID(N'dbo.roles', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.roles
    (
        role_id INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        role_name NVARCHAR(50) NOT NULL
    );
END
GO

IF OBJECT_ID(N'dbo.users', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.users
    (
        user_id INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        role_id INT NOT NULL,
        username NVARCHAR(50) NOT NULL,
        password_hash NVARCHAR(255) NOT NULL,
        full_name NVARCHAR(150) NOT NULL,
        email NVARCHAR(255) NOT NULL,
        phone_number NVARCHAR(20) NULL,
        status NVARCHAR(20) NOT NULL CONSTRAINT DF_users_status DEFAULT (N'Active'),
        created_at DATETIME2 NOT NULL CONSTRAINT DF_users_created_at DEFAULT SYSUTCDATETIME(),
        updated_at DATETIME2 NOT NULL CONSTRAINT DF_users_updated_at DEFAULT SYSUTCDATETIME()
    );
END
GO

IF OBJECT_ID(N'dbo.customers', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.customers
    (
        customer_id INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        full_name NVARCHAR(150) NOT NULL,
        identity_card NVARCHAR(30) NULL,
        phone_number NVARCHAR(20) NULL,
        email NVARCHAR(255) NULL,
        address NVARCHAR(255) NULL,
        created_at DATETIME2 NOT NULL CONSTRAINT DF_customers_created_at DEFAULT SYSUTCDATETIME(),
        updated_at DATETIME2 NOT NULL CONSTRAINT DF_customers_updated_at DEFAULT SYSUTCDATETIME()
    );
END
GO

IF OBJECT_ID(N'dbo.room_types', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.room_types
    (
        room_type_id INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        type_name NVARCHAR(100) NOT NULL,
        description NVARCHAR(500) NULL,
        base_price DECIMAL(18,2) NOT NULL,
        capacity INT NOT NULL,
        status NVARCHAR(20) NOT NULL CONSTRAINT DF_room_types_status DEFAULT (N'Active'),
        created_at DATETIME2 NOT NULL CONSTRAINT DF_room_types_created_at DEFAULT SYSUTCDATETIME(),
        updated_at DATETIME2 NOT NULL CONSTRAINT DF_room_types_updated_at DEFAULT SYSUTCDATETIME()
    );
END
GO

IF OBJECT_ID(N'dbo.rooms', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.rooms
    (
        room_id INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        room_type_id INT NOT NULL,
        room_number NVARCHAR(20) NOT NULL,
        floor INT NOT NULL,
        status NVARCHAR(20) NOT NULL CONSTRAINT DF_rooms_status DEFAULT (N'Available'),
        note NVARCHAR(500) NULL,
        created_at DATETIME2 NOT NULL CONSTRAINT DF_rooms_created_at DEFAULT SYSUTCDATETIME(),
        updated_at DATETIME2 NOT NULL CONSTRAINT DF_rooms_updated_at DEFAULT SYSUTCDATETIME()
    );
END
GO

IF OBJECT_ID(N'dbo.bookings', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.bookings
    (
        booking_id INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        customer_id INT NOT NULL,
        created_by_user_id INT NOT NULL,
        booking_date DATETIME2 NOT NULL CONSTRAINT DF_bookings_booking_date DEFAULT SYSUTCDATETIME(),
        status NVARCHAR(20) NOT NULL CONSTRAINT DF_bookings_status DEFAULT (N'Pending'),
        note NVARCHAR(500) NULL,
        created_at DATETIME2 NOT NULL CONSTRAINT DF_bookings_created_at DEFAULT SYSUTCDATETIME(),
        updated_at DATETIME2 NOT NULL CONSTRAINT DF_bookings_updated_at DEFAULT SYSUTCDATETIME()
    );
END
GO

IF OBJECT_ID(N'dbo.booking_details', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.booking_details
    (
        booking_detail_id INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        booking_id INT NOT NULL,
        room_id INT NOT NULL,
        check_in_date DATETIME2 NOT NULL,
        check_out_date DATETIME2 NOT NULL,
        room_price DECIMAL(18,2) NOT NULL,
        number_of_nights INT NOT NULL,
        room_total DECIMAL(18,2) NOT NULL,
        status NVARCHAR(20) NOT NULL CONSTRAINT DF_booking_details_status DEFAULT (N'Reserved'),
        note NVARCHAR(500) NULL,
        created_at DATETIME2 NOT NULL CONSTRAINT DF_booking_details_created_at DEFAULT SYSUTCDATETIME(),
        updated_at DATETIME2 NOT NULL CONSTRAINT DF_booking_details_updated_at DEFAULT SYSUTCDATETIME()
    );
END
GO

IF OBJECT_ID(N'dbo.check_records', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.check_records
    (
        check_record_id INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        booking_detail_id INT NOT NULL,
        check_in_by_user_id INT NOT NULL,
        check_out_by_user_id INT NULL,
        actual_check_in_date DATETIME2 NOT NULL CONSTRAINT DF_check_records_actual_check_in_date DEFAULT SYSUTCDATETIME(),
        actual_check_out_date DATETIME2 NULL,
        check_in_note NVARCHAR(500) NULL,
        check_out_note NVARCHAR(500) NULL,
        status NVARCHAR(20) NOT NULL CONSTRAINT DF_check_records_status DEFAULT (N'CheckedIn'),
        created_at DATETIME2 NOT NULL CONSTRAINT DF_check_records_created_at DEFAULT SYSUTCDATETIME(),
        updated_at DATETIME2 NOT NULL CONSTRAINT DF_check_records_updated_at DEFAULT SYSUTCDATETIME()
    );
END
GO

IF OBJECT_ID(N'dbo.services', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.services
    (
        service_id INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        service_name NVARCHAR(100) NOT NULL,
        category NVARCHAR(100) NULL,
        price DECIMAL(18,2) NOT NULL,
        status NVARCHAR(20) NOT NULL CONSTRAINT DF_services_status DEFAULT (N'Active'),
        created_at DATETIME2 NOT NULL CONSTRAINT DF_services_created_at DEFAULT SYSUTCDATETIME(),
        updated_at DATETIME2 NOT NULL CONSTRAINT DF_services_updated_at DEFAULT SYSUTCDATETIME()
    );
END
GO

IF OBJECT_ID(N'dbo.service_orders', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.service_orders
    (
        service_order_id INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        booking_detail_id INT NOT NULL,
        service_id INT NOT NULL,
        created_by_user_id INT NOT NULL,
        quantity INT NOT NULL,
        unit_price DECIMAL(18,2) NOT NULL,
        total_price DECIMAL(18,2) NOT NULL,
        order_date DATETIME2 NOT NULL CONSTRAINT DF_service_orders_order_date DEFAULT SYSUTCDATETIME(),
        status NVARCHAR(20) NOT NULL CONSTRAINT DF_service_orders_status DEFAULT (N'Ordered'),
        note NVARCHAR(500) NULL,
        created_at DATETIME2 NOT NULL CONSTRAINT DF_service_orders_created_at DEFAULT SYSUTCDATETIME(),
        updated_at DATETIME2 NOT NULL CONSTRAINT DF_service_orders_updated_at DEFAULT SYSUTCDATETIME()
    );
END
GO

IF OBJECT_ID(N'dbo.invoices', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.invoices
    (
        invoice_id INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        booking_id INT NOT NULL,
        created_by_user_id INT NOT NULL,
        room_amount DECIMAL(18,2) NOT NULL CONSTRAINT DF_invoices_room_amount DEFAULT (0),
        service_amount DECIMAL(18,2) NOT NULL CONSTRAINT DF_invoices_service_amount DEFAULT (0),
        discount_amount DECIMAL(18,2) NOT NULL CONSTRAINT DF_invoices_discount_amount DEFAULT (0),
        tax_amount DECIMAL(18,2) NOT NULL CONSTRAINT DF_invoices_tax_amount DEFAULT (0),
        total_amount DECIMAL(18,2) NOT NULL CONSTRAINT DF_invoices_total_amount DEFAULT (0),
        paid_amount DECIMAL(18,2) NOT NULL CONSTRAINT DF_invoices_paid_amount DEFAULT (0),
        remaining_amount DECIMAL(18,2) NOT NULL CONSTRAINT DF_invoices_remaining_amount DEFAULT (0),
        create_date DATETIME2 NOT NULL CONSTRAINT DF_invoices_create_date DEFAULT SYSUTCDATETIME(),
        status NVARCHAR(20) NOT NULL CONSTRAINT DF_invoices_status DEFAULT (N'Unpaid'),
        note NVARCHAR(500) NULL,
        created_at DATETIME2 NOT NULL CONSTRAINT DF_invoices_created_at DEFAULT SYSUTCDATETIME(),
        updated_at DATETIME2 NOT NULL CONSTRAINT DF_invoices_updated_at DEFAULT SYSUTCDATETIME()
    );
END
GO

IF OBJECT_ID(N'dbo.payments', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.payments
    (
        payment_id INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        invoice_id INT NOT NULL,
        received_by_user_id INT NOT NULL,
        payment_method NVARCHAR(30) NOT NULL,
        amount DECIMAL(18,2) NOT NULL,
        payment_date DATETIME2 NOT NULL CONSTRAINT DF_payments_payment_date DEFAULT SYSUTCDATETIME(),
        transaction_code NVARCHAR(100) NULL,
        status NVARCHAR(20) NOT NULL CONSTRAINT DF_payments_status DEFAULT (N'Success'),
        note NVARCHAR(500) NULL,
        created_at DATETIME2 NOT NULL CONSTRAINT DF_payments_created_at DEFAULT SYSUTCDATETIME(),
        updated_at DATETIME2 NOT NULL CONSTRAINT DF_payments_updated_at DEFAULT SYSUTCDATETIME()
    );
END
GO
