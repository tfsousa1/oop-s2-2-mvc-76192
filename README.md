# Food Safety Inspection Tracker

This project is an ASP.NET Core MVC web application developed for the Object-Oriented Programming (OOP) module.

The system manages food safety inspections, including premises, inspections, follow-ups, and a reporting dashboard.

---

## Features

- Premises management (Create, Read, Update, Delete)
- Inspection management with score and outcome
- Follow-up tracking with due dates and status
- Dashboard with:
  - Inspections this month
  - Failed inspections this month
  - Open follow-ups
  - Overdue follow-ups
  - Filtering by Town and Risk Rating
- Role-based authorization (Admin, Inspector, Viewer)
- Logging using Serilog
- Global error handling
- Seed data
- CI pipeline using GitHub Actions
- Unit tests using xUnit and FluentAssertions

---

## Technologies Used

- ASP.NET Core MVC
- Entity Framework Core (Code First)
- SQL Server LocalDB
- ASP.NET Identity
- Serilog
- xUnit
- GitHub Actions

---

## User Roles

- **Admin**  
  Full access to all features (Premises, Inspections, Follow-Ups)

- **Inspector**  
  Can create and manage inspections and follow-ups

- **Viewer**  
  Read-only access to dashboard and data

---

## Seeded Users

The system creates the following users automatically:

- Admin  
  admin@foodsafety.local / Admin123!

- Inspector  
  inspector@foodsafety.local / Inspector123!

- Viewer  
  viewer@foodsafety.local / Viewer123!

---

## How to Run

1. Clone the repository  
2. Open the solution in Visual Studio  
3. Run the application  
4. The database will be created automatically  

---

## Running Tests

Run the following command:

dotnet test

The tests cover:
- Premises filtering
- Dashboard calculations
- Follow-up logic (open vs overdue and validation rules)

---

## Dashboard Overview

The dashboard provides summary information, including:

- Total premises
- Total inspections
- Inspections this month
- Failed inspections this month
- Open follow-ups
- Overdue follow-ups

Filters available:
- Town
- Risk Rating

---

## Notes

- Entity Framework Core migrations are used
- Logging is configured using Serilog
- Seed data is loaded on application startup
- The application follows MVC architecture