Objective: End-to-end demo script (approx. 10–15 minutes)

Pre-demo checks
- Ensure SQL Server is running and HotelManagementSystem database is created and seeded.
- Update WPF/appsettings.json connection string if needed.
- Start application on a machine with display sharing.
- Use a seeded demo account (username: demo_reception / password: demo123) or Admin account.

Demo flow
1. Login (0:30)
   - Action: Show login screen, enter demo account credentials, click Login.
   - Expected: Main dashboard loads with accurate summaries.

2. Dashboard overview (1:00)
   - Action: Walk through room summary, today operations, bookings count.
   - Talking points: Real-time counts, quick navigation links to Bookings and Check-in.

3. Create a new booking (2:30)
   - Action: Open Booking Management -> New Booking. Fill customer info, date range (today+1 to today+3), choose room type, save.
   - Expected: Booking appears in Booking list; booking details show in Bookings view.

4. Check-in (2:00)
   - Action: Select the booking created, click Check-in. Assign room number, collect ID/presence info, confirm.
   - Expected: Booking status changes to CheckedIn; CheckRecords updated; Dashboard updates OccupiedRooms.

5. Service order (1:30)
   - Action: From occupied room, create Service Order for minibar and breakfast. Save.
   - Expected: Service orders list shows the new order; invoice will include service charges.

6. Check-out and invoice (2:00)
   - Action: From Check-out screen for the room, generate invoice, show breakdown (room nights, services, taxes). Accept payment and finalize.
   - Expected: Invoice saved; room becomes Available; Dashboard updates.

7. Reports & closing (1:00)
   - Action: Open Reports / Dashboard charts, show how to filter by date and export data if available.
   - Expected: Data reflects recent booking/check-in/check-out actions.

Demo tips
- Keep data simple and predictable (use seed data and consistent demo user).
- Highlight role-based features (Admin vs Receptionist) if time allows.
- If a step fails, have a prepared backup scenario (e.g., use seeded booking instead of creating new one).

SENTINEL: DEEPER_RESEARCH_PHASE
