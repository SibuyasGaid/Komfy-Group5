# Komfy Library Management System - Setup Instructions

## First Time Setup on New Device

### 1. Clone the Repository
```bash
git clone <repository-url>
cd Komfy-Group5
```

### 2. Restore Dependencies
```bash
dotnet restore
```

### 3. Run Database Migrations
```bash
cd ASI.Basecode.Data
dotnet ef database update --startup-project ../ASI.Basecode.WebApp/ASI.Basecode.WebApp.csproj
```

This will:
- Create the `AsiBasecodeDB` database
- Create all tables (Users, Books, Borrowings, Reviews, Notifications, UserSettings)
- Seed default admin and user accounts

### 4. Fix Default Account Passwords (IMPORTANT!)

⚠️ **If you cannot login after running migrations**, the encrypted passwords may not match your environment's encryption key.

**Solution**: Navigate to this URL in your browser:
```
https://localhost:58014/Account/FixPasswords
```

Or if running on HTTP:
```
http://localhost:58015/Account/FixPasswords
```

This will re-encrypt the default passwords using your environment's secret key.

You should see:
```
✅ Passwords fixed successfully!

You can now login with:
- Username: admin, Password: admin
- Username: user, Password: user

Go to /Account/Login to sign in.
```

### 5. Login

Navigate to `/Account/Login` and use:

**Admin Account:**
- Username: `admin`
- Password: `admin`

**Member Account:**
- Username: `user`
- Password: `user`

---

## Default User Accounts

After running migrations, these accounts are created:

| Username | Password | Role   | Access                           |
|----------|----------|--------|----------------------------------|
| admin    | admin    | Admin  | Full access including User Mgmt  |
| user     | user     | Member | Standard user access             |

---

## Troubleshooting

### Cannot Login After Migration

**Problem**: Login fails with "Incorrect UserId or Password"

**Cause**: The encrypted passwords in the database don't match your environment's encryption key (from `appsettings.json`)

**Solution**: Visit `/Account/FixPasswords` to re-encrypt the passwords

### Database Connection Issues

**Problem**: Cannot connect to database

**Solution**: Check the connection string in `appsettings.json`:
```json
"ConnectionStrings": {
  "DefaultConnection": "Server=(localdb)\\mssqllocaldb; database=AsiBasecodeDB; Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True"
}
```

Make sure SQL Server LocalDB is installed.

### Migration Errors

**Problem**: Migration fails

**Solution**: Drop and recreate the database:
```bash
dotnet ef database drop --force --startup-project ../ASI.Basecode.WebApp/ASI.Basecode.WebApp.csproj
dotnet ef database update --startup-project ../ASI.Basecode.WebApp/ASI.Basecode.WebApp.csproj
```

Then visit `/Account/FixPasswords` to fix the default passwords.

---

## Running the Application

```bash
cd ASI.Basecode.WebApp
dotnet run
```

The application will start on:
- HTTPS: `https://localhost:58014`
- HTTP: `http://localhost:58015`

---

## Security Notes

- **Default Passwords**: Change the default admin password after first login!
- **Secret Key**: The encryption key is in `appsettings.json` under `TokenAuthentication.SecretKey`
- **Production**: Use environment variables or Azure Key Vault for secrets in production
- **Password Encryption**: All passwords are encrypted using AES with the secret key

---

## Additional Resources

- [SECURITY_AND_AUTHORIZATION.md](SECURITY_AND_AUTHORIZATION.md) - Security and authorization documentation
- [FINAL_IMPLEMENTATION_SUMMARY.md](FINAL_IMPLEMENTATION_SUMMARY.md) - Complete feature list
- [DATABASE_MIGRATION_PASSWORD_RESET.md](DATABASE_MIGRATION_PASSWORD_RESET.md) - Password reset migration guide