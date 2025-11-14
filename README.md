# Komfy - Library Management System

Repository for Group 5's project Komfy - Library Management System for the requirements of the subject SD3-H2

## Quick Start

### 0.5. Paste appsettings.development.json

- Paste appsettings.development.json in the ASI.Basecode.WebApp folder, should be alongside the appsettings.json file.

### 1. Setup Database

```bash
dotnet ef database update --project ASI.Basecode.Data/ASI.Basecode.Data.csproj --startup-project ASI.Basecode.WebApp/ASI.Basecode.WebApp.csproj
```

### 2. Fix Default Passwords (First Time Setup)

Navigate to: `https://localhost:58014/Account/FixPasswords`

### 3. Login

- **Admin**: username: `admin`, password: `admin`
- **Member**: username: `user`, password: `user`

## Troubleshooting

**Can't login after migration?** → Visit `/Account/FixPasswords` to re-encrypt passwords
