# Hotel Management System (WPF + SQL Server)

Project: Hotel Management System with room map, booking, check-in/out, service management (restaurant, laundry), and occupancy reporting.

## Tech stack
- C# / .NET (WPF)
- SQL Server

## Features (planned)
- Visual room map with room status
- Booking management and check-in/out
- Guest profiles and history
- Service management (restaurant, laundry)
- Billing and reports (occupancy, revenue)

## Project structure
```
/docs
  requirements.md
  database-design.md
  ui-mockups.md
/sql
  schema.sql
  stored_procedures.sql
/src
  HotelManagement.UI
  HotelManagement.Core
  HotelManagement.Data
  HotelManagement.Tests
```

## Setup (local)
1. Install Visual Studio with .NET Desktop Development workload.
2. Install SQL Server and SQL Server Management Studio (SSMS).
3. Create a database and run scripts in /sql.
4. Open the solution in /src and update the connection string in appsettings.
5. Build and run the WPF app.

## Conventions
- Branch naming: feature/<short-name>, bugfix/<short-name>
- Commit message: short, present tense

## Team
- Team size: 4

## License
- For education use.
