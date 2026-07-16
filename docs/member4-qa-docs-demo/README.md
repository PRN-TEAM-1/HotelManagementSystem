Assignee: Member 4
Labels: type:docs, area:qa, priority:high
Branch: docs/member4-qa-docs-demo
Start: Day 10
Deadline: Day 14
Estimate: 14h
Depends on: INT-002
Blocks: TEST-RELEASE-001

Project: HotelManagementSystem — QA & Documentation deliverables

Objective
- Provide setup instructions, user guide, manual test cases, demo script, and release checklist for the semester-end demo.

What is included in this folder
- README.md (this file)
- SQL_SETUP_GUIDE.md — instructions to create the database and import seed data
- USER_MANUAL.md — short user manual and main screens description
- DEMO_SCRIPT.md — demo flow to show app features during evaluation
- MANUAL_TEST_CASES.md — grouped manual test cases, each with input, steps, expected/actual/result fields
- RELEASE_CHECKLIST.md — release/hand-off checklist

Quick start
1. Ensure you have a running SQL Server instance (SQL Server, SQLEXPRESS, or LocalDB).
2. Run SQL scripts in DataAccessObjects/SQLScripts in order to create and seed the HotelManagementSystem database.
3. Update WPF/appsettings.json connection string if necessary (server name or authentication).
4. Build and run the WPF project.

Contact
- Assignee: Member 4
- For technical support, see DataAccessObjects/SQLScripts and DataAccessObjects/SeedData folders.

Acceptance criteria checklist (QA lead checklist)
- README contains run instructions (yes)
- SQL guide includes exact commands for SSMS/sqlcmd/PowerShell (yes)
- User manual covers main screens (login, dashboard, booking, check-in/out) (yes)
- Demo script shows end-to-end flow (yes)
- Each module has at least 3 test cases (yes)
- Test cases include Input, Steps, Expected Result, Actual Result, and Status (yes)

SENTINEL: DEEPER_RESEARCH_PHASE
