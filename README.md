# Food Safety Inspection Tracker

This project is an ASP.NET Core MVC application developed for the OOP assignment.

## Features
- Premises management (CRUD)
- Inspection management (CRUD)
- Follow-up management (CRUD)
- Dashboard with summary data and town filter
- Role-based authorization
- Global exception handling
- Logging with Serilog
- Seed data
- CI pipeline using GitHub Actions
- Unit tests with xUnit

## Technologies Used
- ASP.NET Core MVC
- Entity Framework Core (Code First)
- SQL Server LocalDB
- ASP.NET Identity
- Serilog
- xUnit

## Seeded Users
- Admin: admin@foodsafety.local / Admin123!
- Inspector: inspector@foodsafety.local / Inspector123!
- Viewer: viewer@foodsafety.local / Viewer123!

## How to Run
1. Clone the repository
2. Open the solution
3. Run the application
4. The database is created automatically

## Notes
- The application uses EF Core migrations
- Logging is configured with Serilog
- Seed data is loaded on startup