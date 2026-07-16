Scope
Manual test cases grouped by module. Each test case includes: ID, Title, Preconditions, Input, Steps, Expected Result, Actual Result, Status.

Auth
- AUTH-001: Valid login
  Preconditions: Application and DB running. User 'reception' exists.
  Input: Username=reception, Password=reception123
  Steps:
	1. Open app
	2. Enter credentials
	3. Click Login
  Expected Result: Login successful, Dashboard loads.
  Actual Result:
  Status:

- AUTH-002: Invalid password
  Preconditions: Application running.
  Input: Username=reception, Password=wrongpass
  Steps: Attempt login
  Expected Result: Login fails with 'Invalid credentials' message.
  Actual Result:
  Status:

- AUTH-003: Disabled user
  Preconditions: User 'testuser' disabled in Users table.
  Input: Username=testuser, Password=whatever
  Steps: Attempt login
  Expected Result: Login blocked, message that account is disabled.
  Actual Result:
  Status:

Customer
- CUST-001: Create new customer
  Preconditions: Logged in as Receptionist
  Input: Customer details (name, phone, email)
  Steps: Open customer form, fill details, save.
  Expected Result: Customer saved and appears in customer list.
  Actual Result:
  Status:

- CUST-002: Search customer by phone
  Preconditions: Customer exists
  Input: Phone number
  Steps: Use search box, enter phone, search
  Expected Result: Matching customer appears
  Actual Result:
  Status:

- CUST-003: Update customer info
  Preconditions: Customer exists
  Input: New email
  Steps: Open customer, change email, save
  Expected Result: Changes persisted
  Actual Result:
  Status:

Room
- ROOM-001: View room list
  Preconditions: DB seeded with rooms
  Input: N/A
  Steps: Navigate to Rooms screen
  Expected Result: List of rooms with statuses displayed
  Actual Result:
  Status:

- ROOM-002: Change room status to Maintenance
  Preconditions: Room is Available
  Input: Select room, set status=Maintenance
  Steps: Update status and save
  Expected Result: Room status updated and visible in Dashboard
  Actual Result:
  Status:

- ROOM-003: Filter rooms by status
  Preconditions: Rooms with mixed statuses exist
  Input: Filter=Cleaning
  Steps: Apply filter
  Expected Result: Only rooms with Cleaning status shown
  Actual Result:
  Status:

Booking
- BOOK-001: Create booking
  Preconditions: Room types defined
  Input: Customer, dates, room type
  Steps: New Booking -> Fill details -> Save
  Expected Result: Booking created with Reserved status
  Actual Result:
  Status:

- BOOK-002: Prevent double booking same room
  Preconditions: Room already reserved for dates
  Input: Same room/date
  Steps: Attempt to create booking
  Expected Result: Validation error preventing double-booking
  Actual Result:
  Status:

- BOOK-003: Search bookings by date
  Preconditions: Bookings exist
  Input: Date range
  Steps: Search bookings
  Expected Result: Matching bookings returned
  Actual Result:
  Status:

Check-in
- CHECKIN-001: Check-in from booking
  Preconditions: Booking status Reserved and arrival date is today
  Input: Booking reference
  Steps: Select booking -> Check-in -> Assign room -> Confirm
  Expected Result: Booking status becomes CheckedIn; CheckRecord created
  Actual Result:
  Status:

- CHECKIN-002: Walk-in check-in (no booking)
  Preconditions: Room available
  Input: Customer info, dates
  Steps: New Check-in -> Create or select customer -> Assign room -> Confirm
  Expected Result: New CheckRecord created and room becomes Occupied
  Actual Result:
  Status:

- CHECKIN-003: Fail check-in if no available room
  Preconditions: No rooms available for requested type
  Input: Customer, dates
  Steps: Attempt check-in
  Expected Result: System prevents check-in with suitable message
  Actual Result:
  Status:

Service order
- SERVICE-001: Create service order for occupied room
  Preconditions: Room occupied
  Input: Service items and quantities
  Steps: New Service Order -> Pick room -> Add services -> Save
  Expected Result: Service order saved and linked to room/booking
  Actual Result:
  Status:

- SERVICE-002: Mark service order completed
  Preconditions: Service order exists
  Input: Order ID
  Steps: Open order -> Mark complete -> Save
  Expected Result: Order status updated to Completed
  Actual Result:
  Status:

- SERVICE-003: Service charges reflected in invoice
  Preconditions: Service orders exist for stay
  Input: Checkout for room
  Steps: Generate invoice
  Expected Result: Service items appear in invoice total
  Actual Result:
  Status:

Check-out
- CHECKOUT-001: Full checkout and invoice generation
  Preconditions: Guest checked in and has service orders
  Input: Booking/room
  Steps: Start checkout -> Review charges -> Finalize -> Save
  Expected Result: Invoice persisted, payment state updated, room released
  Actual Result:
  Status:

- CHECKOUT-002: Partial payment allowed
  Preconditions: Payment module supports partial payments
  Input: Partial payment amount
  Steps: During checkout, enter partial payment -> Save
  Expected Result: Invoice shows outstanding balance
  Actual Result:
  Status:

- CHECKOUT-003: Prevent checkout with unresolved issues
  Preconditions: Business rules require clearance (e.g., minibar not cleared)
  Input: Attempt to checkout
  Steps: Try to finalize checkout
  Expected Result: System blocks and shows reason
  Actual Result:
  Status:

Invoice
- INVOICE-001: View invoice details
  Preconditions: Invoice exists
  Input: Invoice ID
  Steps: Open invoice viewer
  Expected Result: Itemized invoice displayed
  Actual Result:
  Status:

- INVOICE-002: Reprint invoice
  Preconditions: Invoice exists
  Input: Invoice ID
  Steps: Click Print/Export
  Expected Result: PDF/Print dialog or export file available
  Actual Result:
  Status:

- INVOICE-003: Apply discount to invoice
  Preconditions: Business rule allows discounts
  Input: Discount amount/percent
  Steps: Apply discount -> Recalculate invoice
  Expected Result: Invoice total reflects discount correctly
  Actual Result:
  Status:

Payment
- PAYMENT-001: Record cash payment
  Preconditions: Invoice pending
  Input: Amount = invoice total
  Steps: Choose cash payment -> Save
  Expected Result: Invoice marked paid, payment record created
  Actual Result:
  Status:

- PAYMENT-002: Record card payment
  Preconditions: Card processing simulated or integrated
  Input: Card details (test)
  Steps: Process card payment -> Save
  Expected Result: Payment recorded, status Paid
  Actual Result:
  Status:

- PAYMENT-003: Payment refund
  Preconditions: Payment exists
  Input: Refund amount
  Steps: Initiate refund -> Confirm
  Expected Result: Payment reversed and invoice updated
  Actual Result:
  Status:

Dashboard/Report
- DASH-001: Dashboard reflects changed room counts after check-in
  Preconditions: Known counts before action
  Input: Perform a check-in
  Steps: Check dashboard counts
  Expected Result: OccupiedRooms increases, AvailableRooms decreases
  Actual Result:
  Status:

- DASH-002: Generate booking report for date range
  Preconditions: Bookings present in range
  Input: Date range
  Steps: Generate report
  Expected Result: Report contains correct bookings and totals
  Actual Result:
  Status:

- DASH-003: Export report to CSV/PDF
  Preconditions: Reporting feature present
  Input: Export command
  Steps: Export
  Expected Result: File created with expected contents
  Actual Result:
  Status:

Notes
- Actual Result and Status columns to be filled by QA during execution.
- Add attachments/screenshots for failed cases.

SENTINEL: DEEPER_RESEARCH_PHASE
