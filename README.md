# Komfy - Library Management System

Komfy is a web-based library management system built by Group 5 for the subject SD3-H2. It allows libraries to manage their book inventory, track borrowing activities, and provide members with a catalog to browse, review, and borrow books.

## Tech Stack

- **Backend:** ASP.NET Core 9.0 (C#), MVC pattern
- **Frontend:** Razor Views, HTML/CSS/JS
- **Database:** SQL Server (LocalDB) via Entity Framework Core 9.0
- **Auth:** JWT Bearer + Cookie-based authentication
- **Other:** AutoMapper, BCrypt, Serilog, Newtonsoft.Json, Gmail SMTP

## Features

### For Members
- Browse and search the book catalog (filter by genre, author, publisher, year, rating)
- Borrow books and track due dates and return status
- Write reviews and rate books (1–5 stars)
- View borrowing history and receive notifications

### For Admins
- Full CRUD for book management (physical books and eBooks)
- User management — create, activate/deactivate accounts, assign roles
- Dashboard with analytics: most borrowed books, top-rated books, top borrowers
- Overdue tracking and email notifications

## Project Structure

```
ASI.Basecode.Data/        # Data access layer — EF Core models and migrations
ASI.Basecode.Services/    # Business logic — service interfaces and implementations
ASI.Basecode.WebApp/      # Web app — controllers, Razor views, static assets
ASI.Basecode.Resources/   # Shared constants and resources
```

## Quick Start

### 1. Add appsettings.development.json

Place `appsettings.development.json` inside `ASI.Basecode.WebApp/`, alongside `appsettings.json`. This file contains connection strings and secrets and is not committed to the repo.

### 2. Set Up the Database

```bash
dotnet ef database update --project ASI.Basecode.Data/ASI.Basecode.Data.csproj --startup-project ASI.Basecode.WebApp/ASI.Basecode.WebApp.csproj
```

### 3. Fix Default Passwords (First-Time Setup Only)

Navigate to: `https://localhost:58014/Account/FixPasswords`

This re-encrypts the seeded passwords after migration.

### 4. Login

| Role   | Username | Password |
|--------|----------|----------|
| Admin  | `admin`  | `admin`  |
| Member | `user`   | `user`   |

## Troubleshooting

**Can't log in after migration?** → Visit `/Account/FixPasswords` to re-encrypt passwords.
