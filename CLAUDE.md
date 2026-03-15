# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

**PilatesGestionale** is an ASP.NET Core 10.0 MVC web application for managing a Pilates studio — customers, subscriptions, and admin dashboard.

- **Framework:** ASP.NET Core MVC (.NET 10.0)
- **Database:** PostgreSQL via Entity Framework Core + Npgsql
- **Auth:** ASP.NET Core Identity (cookie-based, role-based: Admin/Staff)
- **Namespace/Assembly:** `PilatesStudio`

## Common Commands

```bash
# Restore dependencies
dotnet restore

# Build
dotnet build

# Run (HTTP :5078)
dotnet run

# EF Core migrations
dotnet ef migrations add <MigrationName>
dotnet ef database update

# Docker
docker build -t pilates-studio .
docker run -p 8080:8080 pilates-studio
```

There are no automated tests in this project currently.

## Architecture

### Request Flow

`Program.cs` bootstraps the app: registers services (MVC, EF Core, Identity, Sessions), runs `DbSeeder` on startup to create roles and a default admin account, then starts the server.

Default route: `{controller=Home}/{action=Index}/{id?}`

### Domain Models (`Models/`)

- **`ApplicationUser`** — extends `IdentityUser` with `NomeCompleto` and `DataCreazione`
- **`Cliente`** — customer with personal/contact/address info and status tracking
- **`Abbonamento`** — subscription linked to a `Cliente`; types: `Singola`, `Cinque`, `Dieci`, `Mensile`, `Trimestrale`, `Annuale`; statuses: `Attivo`, `Scaduto`, `Sospeso`, `Annullato`

### Data Layer (`Data/`)

- **`ApplicationDbContext`** — inherits from `IdentityDbContext<ApplicationUser>`; one `Cliente` has many `Abbonamenti` (cascade delete)
- **`DbSeeder`** — seeds Admin/Staff roles and default admin: `admin@pilatesstudio.it` / `Admin123!`

### Controllers

| Controller | Auth | Responsibility |
|---|---|---|
| `HomeController` | Public | Index, Privacy, Error |
| `AccountController` | Public | Login, Logout, AccessDenied |
| `ClientiController` | Admin, Staff | CRUD for customers, pagination, search, status toggle |
| `AdminController` | Admin, Staff | Dashboard statistics (client counts, revenue, expiring subscriptions) |

### Configuration

- `appsettings.json` — production DB connection (PostgreSQL `localhost:5432`, db `pilates_studio_gestionale`)
- `appsettings.Development.json` — development overrides
- For local dev, use [.NET User Secrets](https://learn.microsoft.com/en-us/aspnet/core/security/app-secrets) to override DB credentials instead of editing `appsettings.json`
