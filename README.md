# Komfy - Library Management System

Repository for Group 5's project Komfy - Library Management System for the requirements of the subject SD3-H2

## Quick Start

### 1. Setup Database
```bash
dotnet ef database update --project ASI.Basecode.Data/ASI.Basecode.Data.csproj --startup-project ASI.Basecode.WebApp/ASI.Basecode.WebApp.csproj
```

### 2. Fix Default Passwords (First Time Setup)
Navigate to: `https://localhost:58014/Account/FixPasswords`

### 3. Login
- **Admin**: username: `admin`, password: `admin`
- **Member**: username: `user`, password: `user`

## Documentation

- [📋 Setup Instructions](SETUP_INSTRUCTIONS.md) - Complete setup guide
- [🔒 Security & Authorization](SECURITY_AND_AUTHORIZATION.md) - Security documentation
- [✅ Feature Summary](FINAL_IMPLEMENTATION_SUMMARY.md) - All implemented features

## Troubleshooting

**Can't login after migration?** → Visit `/Account/FixPasswords` to re-encrypt passwords

See [SETUP_INSTRUCTIONS.md](SETUP_INSTRUCTIONS.md) for detailed troubleshooting.
