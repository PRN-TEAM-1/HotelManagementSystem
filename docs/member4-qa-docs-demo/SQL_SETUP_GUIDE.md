Purpose
This guide explains how to create the HotelManagementSystem database and import seed data using the SQL scripts and CSV files included in the repository.

Prerequisites
- Microsoft SQL Server (Developer/Express) or LocalDB installed and running.
- SQL Server Management Studio (SSMS) or sqlcmd utility or PowerShell with SqlServer module.
- Project repository checked out locally.

Files to use
- DataAccessObjects/SQLScripts/001_create_database.sql
- DataAccessObjects/SQLScripts/002_create_tables.sql
- DataAccessObjects/SQLScripts/003_constraints.sql
- DataAccessObjects/SQLScripts/004_seed_data.sql
- DataAccessObjects/SeedData/* (CSV files for seed data)

Recommended approach (SSMS)
1. Open SSMS and connect to your server instance (example Server name: localhost, .\SQLEXPRESS, or (localdb)\MSSQLLocalDB).
2. Right-click "New Query" and open 001_create_database.sql. Execute.
3. Open and execute 002_create_tables.sql.
4. Execute 003_constraints.sql.
5. Execute 004_seed_data.sql. If the seed script references CSV files, import them first using "Import Data" or modify the seed script to use bulk insert with correct paths.

Recommended approach (sqlcmd)
Adjust server name and authentication as needed. Examples below assume Windows Authentication.

-- Create database and schema (Windows auth)
sqlcmd -S "(localdb)\MSSQLLocalDB" -E -i DataAccessObjects/SQLScripts/001_create_database.sql
sqlcmd -S "(localdb)\MSSQLLocalDB" -E -i DataAccessObjects/SQLScripts/002_create_tables.sql
sqlcmd -S "(localdb)\MSSQLLocalDB" -E -i DataAccessObjects/SQLScripts/003_constraints.sql
sqlcmd -S "(localdb)\MSSQLLocalDB" -E -i DataAccessObjects/SQLScripts/004_seed_data.sql

-- SQL auth example (replace -U and -P values)
sqlcmd -S "localhost" -U sa -P "YourStrongPassword" -i DataAccessObjects/SQLScripts/001_create_database.sql

Important notes
- If 004_seed_data.sql expects CSV files, update file paths inside the script to absolute or correct relative paths from the server machine.
- If you prefer to use a GUI bulk import: In SSMS, Database -> Tasks -> Import Data and follow the wizard to import CSVs into target tables.

Connection string and application
- Default WPF connection string: WPF/appsettings.json -> ConnectionStrings.DefaultConnection
- If your SQL Server instance is a named instance, update the Server parameter (e.g., Server=.\SQLEXPRESS).
- To use Windows Integrated Authentication, replace User ID/Password with Trusted_Connection=True (or Integrated Security=True).

Troubleshooting
- "Login failed for user 'sa'": confirm SA is enabled and password is correct; or switch to Windows auth.
- "File not found" in seed step: ensure CSV files are accessible to the SQL Server service account or adjust the script to use BULK INSERT with a UNC path accessible to the server.
- Permission errors during import: run SSMS with an administrative account or grant appropriate permissions.

Validation
- After running scripts, in SSMS expand Databases -> HotelManagementSystem -> Tables and verify core tables exist (Rooms, Bookings, BookingDetails, CheckRecords, Users, Services).
- Run a quick query to validate data: SELECT TOP 5 * FROM HotelManagementSystem.dbo.Rooms;

If you want, I can generate an example PowerShell script to run these steps automatically on your machine.