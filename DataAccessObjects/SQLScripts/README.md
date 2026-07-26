# Cách tạo database:

Ae bắt buộc phải chạy các file này lần lượt theo thứ tự nha:

1. `001_create_database.sql`
2. `002_create_tables.sql`
3. `003_constraints.sql`
4. `004_seed_data.sql`
5. `005_audit_login_activity.sql`

## Default database name

- `HotelManagementSystem`

## Seeded demo accounts

- `admin / Admin@123`
- `manager / Manager@123`
- `reception / Reception@123`

Password hashes are stored with `SHA2_256` in uppercase hexadecimal form.
