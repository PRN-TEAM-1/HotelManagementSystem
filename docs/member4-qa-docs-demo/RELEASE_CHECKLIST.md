Release checklist for QA hand-off and demo

Pre-release (Day -1)
- [ ] Verify SQL scripts executed and DB seeded.
- [ ] Run smoke tests for Login, Dashboard, Booking, Check-in, Check-out.
- [ ] Verify connection string in WPF/appsettings.json points to the demo database.
- [ ] Build project in Release configuration and confirm no build errors.
- [ ] Create demo user account and capture credentials for demo.

Release day tasks
- [ ] Start SQL Server service and ensure DB is online.
- [ ] Start application and login with demo account.
- [ ] Execute demo script end-to-end once and note timings.
- [ ] Ensure screen recording or screenshot capture is ready.

Post-demo
- [ ] Collect feedback and log defects in tracking system.
- [ ] Update SQL seed or scripts if data issues found.
- [ ] Tag release commit and merge docs branch into main (follow repo workflow).

Hand-off artifacts
- [ ] docs/member4-qa-docs-demo/* (this folder)
- [ ] Link to recording and any supplementary files
- [ ] List of known issues and workarounds

Sign-off
- QA lead: __________
- Product owner: __________

SENTINEL: DEEPER_RESEARCH_PHASE
