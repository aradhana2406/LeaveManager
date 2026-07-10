# LeaveManager

LeaveManager is a .NET 8 employee leave-management application. It provides a browser UI and REST API for employee administration, leave balances and approvals, Excel imports, onboarding, HR policies, and device-support tickets.

## Technology stack

- ASP.NET Core 8 Web API
- Entity Framework Core 8
- SQL Server (LocalDB is the development default)
- EF Core migrations and seed data
- ClosedXML for Excel import/export
- Swagger/OpenAPI
- HTML, CSS, and JavaScript served from `LeaveManager/wwwroot`
- Gmail SMTP notifications

The front end is part of the ASP.NET Core project, so there is no separate Node.js build or package installation.

## Repository structure

```text
LeaveManager/
|-- LeaveManager.sln                  # Visual Studio/.NET solution
|-- README.md
`-- LeaveManager/
    |-- LeaveManager.csproj           # Web application project
    |-- Program.cs                    # Services and HTTP pipeline
    |-- Controllers/                  # REST endpoints
    |-- Data/                         # EF Core DbContext
    |-- Entities/                     # Database entities
    |-- Configurations/               # EF mappings and seed logic
    |-- Features/                     # Application handlers/services
    |-- Infrastructure/               # Email and token services
    |-- Migrations/                   # SQL Server schema migrations
    `-- wwwroot/                      # Browser application
```

## Prerequisites

Install:

1. [Git](https://git-scm.com/)
2. [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
3. One of the following SQL Server options:
   - SQL Server Express LocalDB (simplest option on Windows)
   - SQL Server Express or Developer Edition
   - A reachable SQL Server/Azure SQL instance
4. A modern browser
5. Visual Studio 2022 with the **ASP.NET and web development** workload (optional)

Verify the command-line tools:

```powershell
git --version
dotnet --version
dotnet --list-sdks
```

The installed SDK list must include an `8.0.x` SDK or a compatible newer SDK capable of building `net8.0`.

## Visual Studio setup (recommended for Windows)

Use this workflow if you want to clone, configure, migrate, and run the project entirely from Visual Studio 2022.

### 1. Install the required Visual Studio components

1. Open **Visual Studio Installer**.
2. Find Visual Studio 2022 and select **Modify**.
3. Select the **ASP.NET and web development** workload.
4. In **Individual components**, confirm that these are selected:
   - .NET 8 SDK
   - SQL Server Express LocalDB
   - SQL Server Data Tools
5. Apply the changes and restart Visual Studio.

### 2. Clone the repository in Visual Studio

1. Start Visual Studio 2022.
2. Select **Clone a repository**.
3. Enter this repository URL:

   ```text
   https://github.com/aradhana2406/LeaveManager.git
   ```

4. Choose a local folder and select **Clone**.
5. If Visual Studio does not open it automatically, select **File > Open > Project/Solution** and open `LeaveManager.sln` from the cloned repository root.
6. Wait for Visual Studio to restore the NuGet packages. The status bar and **Output** window show restore progress.

### 3. Confirm the correct startup project

1. In **Solution Explorer**, right-click the `LeaveManager` project, not the solution.
2. Select **Set as Startup Project**.
3. In the run-profile list near the green Run button, select `http` for the simplest local setup.

The `http` profile starts the application at `http://localhost:5054` with the Development environment.

### 4. Configure SQL Server LocalDB

The project is already configured to use the default LocalDB instance and a database named `LeaveManagerDb`:

```text
Server=(localdb)\MSSQLLocalDB;Database=LeaveManagerDb;Integrated Security=true;TrustServerCertificate=True
```

To confirm LocalDB from Visual Studio:

1. Select **View > SQL Server Object Explorer**.
2. Expand **SQL Server**.
3. Look for `(localdb)\MSSQLLocalDB`.
4. If it is not connected, select **Add SQL Server** and enter `(localdb)\MSSQLLocalDB` as the server name.
5. Use **Windows Authentication** and select **Connect**.

No database needs to be created manually. EF Core creates `LeaveManagerDb` when migrations are applied or when the application first starts.

If you use a different SQL Server instance, do not put its password in `appsettings.json`. Configure it with User Secrets as described next.

### 5. Configure local secrets in Visual Studio

1. In **Solution Explorer**, right-click the `LeaveManager` project.
2. Select **Manage User Secrets**.
3. If Visual Studio asks to initialize User Secrets, allow it. This adds a non-secret `UserSecretsId` to `LeaveManager.csproj`.
4. Put your local settings in the opened `secrets.json` file:

   ```json
   {
     "ConnectionStrings": {
       "DefaultConnection": "Server=(localdb)\\MSSQLLocalDB;Database=LeaveManagerDb;Integrated Security=true;TrustServerCertificate=True"
     },
     "Smtp": {
       "Host": "smtp.gmail.com",
       "Port": "587",
       "Username": "your-address@gmail.com",
       "Password": "your-google-app-password",
       "From": "your-address@gmail.com"
     },
     "AppSettings": {
       "BaseUrl": "http://localhost:5054"
     }
   }
   ```

5. Save the file.

User Secrets are stored outside the repository and are not committed to Git. For Gmail, use a Google App Password, not the normal account password. SMTP is required when testing leave notifications, onboarding emails, and device-ticket emails.

If you only want to explore the UI without SQL Server, add this instead:

```json
{
  "UseInMemoryDatabase": true
}
```

In-memory data is deleted whenever the application stops.

### 6. Apply database migrations from Visual Studio

The application automatically applies pending migrations when it starts. To apply them manually first:

1. Select **Tools > NuGet Package Manager > Package Manager Console**.
2. Set **Default project** in the console to `LeaveManager`.
3. Run:

   ```powershell
   Update-Database
   ```

4. Wait for the command to complete without errors.
5. In **SQL Server Object Explorer**, right-click `(localdb)\MSSQLLocalDB` and select **Refresh**.
6. Expand **Databases > LeaveManagerDb > Tables** to confirm the schema exists.

If `Update-Database` is unavailable, use **View > Terminal**, change to the inner project directory, and run:

```powershell
cd LeaveManager
dotnet tool install --global dotnet-ef --version 8.*
dotnet ef database update
cd ..
```

Do not run SQL migrations when `UseInMemoryDatabase` is enabled.

### 7. Build and run in Visual Studio

1. Select **Build > Build Solution**, or press `Ctrl+Shift+B`.
2. Confirm the **Error List** has no build errors.
3. Select the `http` launch profile.
4. Press `F5` to run with the debugger, or `Ctrl+F5` to run without it.
5. Visual Studio should open Swagger automatically at `http://localhost:5054/swagger`.
6. Open `http://localhost:5054` to use the browser application.

On the first run, EF Core applies migrations and seeds demo accounts. Sign in with `preeti` / `demo123` to verify the HR workspace.

To stop the application, click the red stop button in Visual Studio or press `Shift+F5`.

### 8. Common Visual Studio fixes

- **NuGet packages are missing:** right-click the solution and select **Restore NuGet Packages**.
- **The wrong page opens:** confirm the `http` profile is selected and browse directly to `http://localhost:5054`.
- **LocalDB cannot be found:** add SQL Server Express LocalDB through Visual Studio Installer.
- **Port 5054 is busy:** stop the other process or edit `LeaveManager/Properties/launchSettings.json` and update `AppSettings:BaseUrl` to the same port.
- **HTTPS certificate error:** open **View > Terminal** and run `dotnet dev-certs https --trust`.
- **Database changes do not appear:** refresh the database in SQL Server Object Explorer and confirm `Update-Database` targeted `LeaveManagerDb`.
- **Old CSS or JavaScript is displayed:** stop debugging, restart the app, and hard-refresh the browser with `Ctrl+F5`.

## Command-line setup after cloning

### 1. Clone the repository

```powershell
git clone https://github.com/aradhana2406/LeaveManager.git
cd LeaveManager
```

All commands below assume the terminal is in this repository root unless stated otherwise.

### 2. Restore NuGet packages

```powershell
dotnet restore LeaveManager.sln
```

### 3. Configure the database

The application reads its connection string from `ConnectionStrings:DefaultConnection`. The checked-in development default is:

```text
Server=(localdb)\MSSQLLocalDB;Database=LeaveManagerDb;Integrated Security=true;TrustServerCertificate=True
```

Choose one setup below.

#### Option A: SQL Server LocalDB on Windows

Check that LocalDB is installed and start its default instance:

```powershell
sqllocaldb info
sqllocaldb start MSSQLLocalDB
```

No connection-string change is required. The `LeaveManagerDb` database will be created when migrations run.

If `sqllocaldb` is not recognized, install SQL Server Express LocalDB or use Option B.

#### Option B: another SQL Server instance

Set the connection string for the current PowerShell session. Examples:

Windows authentication:

```powershell
$env:ConnectionStrings__DefaultConnection = "Server=localhost;Database=LeaveManagerDb;Trusted_Connection=True;TrustServerCertificate=True"
```

SQL Server authentication:

```powershell
$env:ConnectionStrings__DefaultConnection = "Server=localhost;Database=LeaveManagerDb;User Id=YOUR_USER;Password=YOUR_PASSWORD;TrustServerCertificate=True"
```

The SQL login needs permission to connect and create/update the development database. Keep this terminal open: session environment variables disappear when it closes.

For persistent local configuration, use .NET User Secrets from the project directory:

```powershell
cd LeaveManager
dotnet user-secrets init
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "YOUR_CONNECTION_STRING"
cd ..
```

`dotnet user-secrets init` adds a `UserSecretsId` to the project file; commit that ID if the team wants to use User Secrets, but never commit actual secret values.

#### Option C: temporary in-memory database

For a quick demonstration without SQL Server:

```powershell
$env:UseInMemoryDatabase = "true"
```

This database is erased every time the application stops. EF migration commands still target SQL Server, so do not use them in this mode.

### 4. Configure email

The leave approval, onboarding, and device-ticket workflows send SMTP email. Configure these settings before testing email-dependent actions:

```powershell
$env:Smtp__Host = "smtp.gmail.com"
$env:Smtp__Port = "587"
$env:Smtp__Username = "your-address@gmail.com"
$env:Smtp__Password = "your-google-app-password"
$env:Smtp__From = "your-address@gmail.com"
$env:AppSettings__BaseUrl = "http://localhost:5054"
```

For Gmail, enable two-step verification and create a Google App Password. Do not use your normal Gmail password. Never commit SMTP passwords, connection-string passwords, tokens, or employee data.

For a deployed application, change `AppSettings__BaseUrl` to its public HTTPS URL so email links point to the correct site.

### 5. Create and update the database

The application automatically runs `Database.Migrate()` and seeds data at startup. Therefore, the simplest setup is to continue to the run step.

To apply or inspect migrations manually, install the EF Core CLI once:

```powershell
dotnet tool install --global dotnet-ef --version 8.*
```

Then run EF commands from the web-project directory so its configuration files are found:

```powershell
cd LeaveManager
dotnet ef database update
dotnet ef migrations list
cd ..
```

Useful SQL Server checks after migration:

```sql
SELECT name FROM sys.databases WHERE name = 'LeaveManagerDb';
USE LeaveManagerDb;
SELECT MigrationId FROM __EFMigrationsHistory ORDER BY MigrationId;
SELECT EmployeeCode, FullName, Role FROM Employees;
```

Do not edit a migration that has already been applied to a shared database. For a schema change, create a new migration:

```powershell
cd LeaveManager
dotnet ef migrations add DescriptiveMigrationName
dotnet ef database update
cd ..
```

### 6. Build the solution

```powershell
dotnet build LeaveManager.sln
```

The build should complete with zero errors.

### 7. Run the application

```powershell
dotnet run --project LeaveManager/LeaveManager.csproj --launch-profile http
```

Open:

- Application: <http://localhost:5054>
- Swagger UI: <http://localhost:5054/swagger>

The `http` profile sets the environment to `Development`, which enables Swagger. Press `Ctrl+C` in the terminal to stop the application.

To use HTTPS instead:

```powershell
dotnet dev-certs https --trust
dotnet run --project LeaveManager/LeaveManager.csproj --launch-profile https
```

The HTTPS profile uses `https://localhost:7087` and also listens on `http://localhost:5054`.

## Database initialization and seed accounts

At startup, the application:

1. Connects to SQL Server, or creates an in-memory database when enabled.
2. Applies all pending EF Core migrations.
3. Seeds leave types and initial application data.
4. Ensures the demo employees, logins, and leave balances exist.

On a fresh database, use these local demo accounts:

| Username | Password | Role |
|---|---|---|
| `preeti` | `demo123` | HR L2 |
| `rakesh` | `demo123` | Organization Head |
| `aradhana` | `demo123` | Employee |

These credentials are development data only. The current application stores login passwords as plain text; do not expose it to production users until password hashing and proper authentication/authorization are implemented.

## First-run verification

After the site starts:

1. Open the application and sign in as `preeti`.
2. Confirm the HR workspace loads employees, teams, projects, roles, policies, leave types, and balances.
3. Sign out and sign in as `aradhana`.
4. Confirm the employee leave dashboard shows available balances.
5. If SMTP is configured, submit a test leave request.
6. Sign in as the appropriate reviewer and approve or reject the request.
7. Return to the employee account and confirm the status/balance update.
8. Use Swagger to inspect and try API request/response schemas.

The front end is static JavaScript. If Node.js is installed, its syntax can be checked with:

```powershell
node --check LeaveManager/wwwroot/app.js
```

## Excel imports

The HR workspace supports employee and leave-balance imports. Templates can be downloaded from the UI or API:

- `GET /api/Excel/existing-employees-template`
- `GET /api/Excel/leave-balance-template`

The leave-balance template uses these columns:

| EmployeeCode | LeaveTypeName | AllocatedLeaves | UsedLeaves |
|---|---|---:|---:|
| `EMP001` | `Sick/Casual Leave` | `7` | `0` |
| `EMP001` | `Planned Leave` | `12` | `2` |

The employee code and leave type must match records in the database. Validate an import in a development database before using real employee data.

## Main API routes

| Area | Method and route |
|---|---|
| Authentication | `POST /api/auth/login` |
| Demo users | `GET /api/auth/demo-users` |
| Workspace | `GET /api/workspace` |
| Apply for leave | `POST /api/leave/apply-leave` |
| Employee leave requests | `GET /api/leave/employee/{employeeId}/requests` |
| Reviewer queue | `GET /api/leave/reviewer/{reviewerId}/requests` |
| Reviewer decision | `POST /api/leave/reviewer/decision` |
| Cancel leave | `POST /api/leave/{leaveApplicationId}/cancel` |
| Upload employees | `POST /api/Excel/upload-existing-employees` |
| Upload balances | `POST /api/Excel/upload-leave-balances` |
| Onboarding | `GET /api/onboarding/{employeeId}` |
| Device tickets | `GET /api/device-tickets/employee/{employeeId}` |
| HR device tickets | `GET /api/device-tickets/hr` |

Swagger is the authoritative interactive reference while running in Development.

## Uploaded files

Onboarding documents are stored under:

```text
LeaveManager/App_Data/OnboardingFiles/{employeeId}/
```

Treat that directory as sensitive employee data. For production, use protected persistent storage, access controls, malware scanning, backups, retention rules, and encryption as appropriate.

## Troubleshooting

### `sqllocaldb` is not recognized

Install SQL Server Express LocalDB, or set `ConnectionStrings__DefaultConnection` for another SQL Server instance.

### Database login or network error

- Confirm the SQL Server service/LocalDB instance is running.
- Confirm the server and instance names in the connection string.
- Confirm the login has access to the target database.
- For local encrypted connections, keep `TrustServerCertificate=True` unless a trusted certificate is configured.
- Print only the server/database portion when debugging; do not paste passwords into logs or issues.

### Migration fails on startup

Run `dotnet ef database update` from the `LeaveManager` project directory and inspect the first error. Check that the connection string targets the intended database and that the login can alter its schema.

### Port 5054 is already in use

```powershell
Get-NetTCPConnection -LocalPort 5054 -ErrorAction SilentlyContinue
dotnet run --project LeaveManager/LeaveManager.csproj --urls http://localhost:5055
```

When changing the port, also update `AppSettings__BaseUrl` so generated links are correct.

### HTTPS certificate warning

```powershell
dotnet dev-certs https --clean
dotnet dev-certs https --trust
```

Restart the browser and application afterward.

### Build files are locked

Stop the running application with `Ctrl+C`. If necessary:

```powershell
Get-Process LeaveManager -ErrorAction SilentlyContinue | Stop-Process
dotnet build LeaveManager.sln
```

### Email sending fails

Verify `Smtp__Host`, `Smtp__Port`, `Smtp__Username`, `Smtp__Password`, and `Smtp__From`. For Gmail, confirm the value is a valid App Password and the account permits SMTP access.

### Browser displays old CSS or JavaScript

Hard-refresh with `Ctrl+F5`, then check the browser developer console and the application terminal.

## Production checklist

Before a production deployment:

- Replace demo/plain-text authentication with ASP.NET Core Identity or another secure provider.
- Hash passwords and remove or disable demo accounts.
- Move all credentials to a secret manager.
- Use a restricted database identity and a controlled migration process.
- Set `AppSettings__BaseUrl` to the public HTTPS origin.
- Restrict or disable public Swagger access.
- Add authentication and authorization policies to protected endpoints.
- Configure HTTPS, CORS, rate limiting, monitoring, backups, and recovery.
- Move uploaded documents to protected persistent storage.
- Review logging so credentials and employee personal data are never recorded.

## Before committing changes

```powershell
dotnet build LeaveManager.sln
git status
git diff --check
```

Review the diff carefully and make sure no passwords, tokens, connection strings containing credentials, database files, uploads, or employee records are included.
