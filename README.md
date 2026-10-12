# AcxiomCRM

A modern customer relationship management (CRM) web application built with **ASP.NET Core 8 MVC**, **Entity Framework Core**, **ASP.NET Core Identity**, and **SQL Server**.

---

## Features

- **Authentication & Authorization**:
  - ASP.NET Core Identity with role-based access control (RBAC).
  - Predefined roles: `Admin`, `Manager`, and `SalesExecutive`.
  - Secure password hashing, account lockout after 5 failed attempts, HTTP-only anti-tamper cookies, and automated anti-forgery token verification.
- **Customer Management**:
  - Track customer contacts, unique emails, and phone validation.
  - Soft-delete status toggling and row-level scoping for sales representatives.
- **Lead Tracking**:
  - Full lead lifecycle tracking (`New`, `Contacted`, `Qualified`, `Unqualified`, `Converted`, `Lost`).
  - Expected deal valuation and assignment to sales executives.
- **Opportunity Pipeline**:
  - Stage tracking (`Qualification`, `Proposal`, `Negotiation`, `Won`, `Lost`).
  - Closing probabilities, expected deal values, and close dates.
- **Activities & Follow-Ups**:
  - Schedule calls, meetings, tasks, and follow-up reminders.
- **Audit Logging**:
  - Centralized audit trail recording user actions, IP addresses, entity mutations, and timestamps.
- **RESTful API**:
  - `/api/customers` endpoints for integration with JSON payloads and standard HTTP verbs (`GET`, `POST`, `PUT`, `DELETE`).

---

## Tech Stack

- **Framework**: .NET 8 (C#)
- **Architecture**: ASP.NET Core MVC + Web API
- **Database**: Microsoft SQL Server (LocalDB / SQLEXPRESS)
- **ORM**: Entity Framework Core 8
- **UI & Styling**: Bootstrap 5 + jQuery Validation (Unobtrusive)

---

## Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [Microsoft SQL Server](https://www.microsoft.com/sql-server/) (or SQL Server Express / LocalDB)
- Optional: `dotnet-ef` global CLI tool (`dotnet tool install -g dotnet-ef`)

---

## Getting Started

### 1. Clone & Navigate to Project

```bash
git clone <repository-url>
cd AcxiomCRM/AcxiomCRM
```

### 2. Configure Database Connection

Open `appsettings.json` and adjust the connection string to match your SQL Server instance:

```json
{
  "ConnectionStrings": {
    "Default": "Server=localhost\\SQLEXPRESS;Database=AcxiomCRM;Trusted_Connection=True;TrustServerCertificate=True;"
  },
  "Seed": {
    "AdminEmail": "admin@acxiom.local",
    "AdminPassword": "ChangeMe#2026"
  }
}
```

> **Security Note**: For production or shared environments, store credentials and passwords using .NET User Secrets or environment variables instead of `appsettings.json`.

### 3. Restore & Build

```bash
dotnet restore
dotnet build
```

### 4. Apply Database Migrations (Optional)

The application automatically runs `Database.MigrateAsync()` and seeds initial roles and the default admin user on startup.

If you wish to apply migrations manually via CLI:

```bash
dotnet ef database update
```

### 5. Run the Application

```bash
dotnet run
```

Once started, open your browser and navigate to:
**[http://localhost:5000](http://localhost:5000)** (or the port specified in terminal output).

---

## Default Admin Credentials

On first run, the database automatically seeds an initial administrator:

| Field | Value |
|---|---|
| **Email** | `admin@acxiom.local` |
| **Password** | `ChangeMe#2026` |
| **Role** | `Admin` |

> Remember to change the administrator password immediately after first sign-in.

---

## Project Structure

```text
AcxiomCRM/
├── Controllers/         # MVC Controllers & REST API Controllers
│   ├── AccountController.cs        # Login, registration, lockout, sign out
│   ├── Api.cs                      # RESTful /api/customers endpoints
│   ├── CustomersController.cs      # Customer CRUD operations
│   ├── DashboardController.cs      # CRM overview & metrics
│   └── OpportunitiesController.cs  # Pipeline & opportunity tracking
├── Data/                # DbContext, configuration & database seeder
│   └── AppDbContext.cs
├── Migrations/          # Entity Framework Core migrations
├── Models/              # Domain models, ViewModels & validation rules
│   └── Models.cs
├── Services/            # Core business & audit services
├── Views/               # Razor views (Razor syntax + Bootstrap 5)
│   ├── Account/
│   ├── Customers/
│   ├── Dashboard/
│   ├── Opportunities/
│   └── Shared/
├── appsettings.json     # Connection string & application settings
└── Program.cs           # Web host configuration, DI & middleware pipeline
```

---

## License

This project is licensed under the MIT License.
