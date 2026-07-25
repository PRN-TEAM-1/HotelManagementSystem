# Hotel Management System - Folder Structure

This document outlines the directory structure of the `HotelManagementSystem` repository.

## Root Directory (`d:\Semester 5\PRN212\PRN-TEAM-1\HotelManagementSystem`)

```text
HotelManagementSystem/
├── .git/                      # Git repository tracking data
├── .vs/                       # Visual Studio workspace settings (hidden)
├── BusinessObjects/           # Class library containing business entities and models
├── DataAccessObjects/         # Class library for database interaction (DAOs, DbContext)
├── Repositories/              # Class library implementing the Repository pattern
├── Services/                  # Class library containing business logic and service layer
├── WPF/                       # WPF application project (User Interface)
├── docs/                      # Documentation folder
├── .gitignore                 # Specifies intentionally untracked files to ignore by Git
├── Database-Ver2.0.md         # Database schema documentation (Version 2.0)
├── FULLSTACK_TASK_PLAN_2.0.md # Project task planning and progress tracking
├── HotelManagementSystem.slnx # Solution configuration file
└── README.md                  # Main project description and instructions
```

## Description of Layers

The project follows an N-Tier architecture (common for .NET applications):
- **BusinessObjects:** Contains POCO classes representing database entities (e.g., Room, Booking, Customer).
- **DataAccessObjects (DAO):** Handles all database queries, utilizing tools like Entity Framework Core.
- **Repositories:** Acts as an abstraction layer over DAOs, providing a clean interface for data access.
- **Services:** Contains the core business logic, orchestrating calls between Repositories and exposing functionality to the UI.
- **WPF:** The Presentation layer, likely using MVVM (Model-View-ViewModel) pattern for the desktop user interface.
