Purpose
This user manual provides a concise walkthrough of the main screens and typical user flows in the HotelManagementSystem WPF application.

Audience
Hotel staff: front desk, operations, billing; demo evaluators; QA testers.

Main screens (overview)
1. Login Screen
   - Purpose: Authenticate user. Fields: Username, Password. Buttons: Login.
   - Notes: Application uses authentication roles (Admin, Receptionist, Accountant).

2. Dashboard
   - Purpose: Overview of room status, today operations, bookings summary.
   - Sections: Room summary, Booking summary, Today operations, Quick links.

3. Booking Management
   - Purpose: Create, view, edit, and cancel bookings.
   - Typical actions: New booking, search bookings by customer/date, assign rooms.

4. Check-in
   - Purpose: Process guest arrival, assign rooms, create check records.
   - Inputs: Booking selection or walk-in customer details, payment method, ID.

5. Service Order Management
   - Purpose: Order and manage room services (mini-bar, housekeeping, extra services).
   - Actions: Create service order, attach to room or booking, mark completed.

6. Check-out & Invoice
   - Purpose: Finalize stay, compute charges, print/preview invoice, accept payments.
   - Actions: Calculate room charges, service charges, taxes, apply discounts, save invoice.

7. User Management (Admin)
   - Purpose: Create users, assign roles, enable/disable accounts.

Guided example flows
- Create a Booking:
  1. Login as Receptionist.
  2. Navigate to Booking Management -> New Booking.
  3. Enter customer info, dates, room type, and Save.
  4. Confirm booking appears in Booking list.

- Check-in a Guest:
  1. From Booking list, select booking arriving today.
  2. Click Check-in, verify guest ID and payment details.
  3. Assign room and finalize check-in.

- Create Service Order:
  1. Select occupied room -> Create Service Order.
  2. Choose service item(s) and quantity -> Save.
  3. Service order will appear in service list and affect invoice.

Screenshots
- If time permits for the final deliverable, include annotated screenshots per screen above (placeholders below):
  - [SCREENSHOT] Login
  - [SCREENSHOT] Dashboard
  - [SCREENSHOT] Booking Management
  - [SCREENSHOT] Check-in
  - [SCREENSHOT] Invoice/Check-out

Support & troubleshooting
- If application fails to start: ensure the database exists and connection string in WPF/appsettings.json is correct.
- For login failures: verify user account exists in Users table or use admin account to reset.

Notes for demo
- Use seeded accounts from DataAccessObjects/SeedData/users.csv (or from the seed script) when performing the demo.

SENTINEL: DEEPER_RESEARCH_PHASE
