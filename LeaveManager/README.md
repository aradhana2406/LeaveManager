# LeaveManager - Leave Management System

A .NET 8 leave-management application with an ASP.NET Core REST API and a React frontend served from `wwwroot`.

## Project Overview

LeaveManager manages employee administration, leave applications, approvals, leave balances, Excel imports, onboarding, HR policies, and device-support tickets. The backend exposes REST endpoints, and the browser UI is a React app loaded by ASP.NET Core static files.

**Tech Stack:**
- **.NET 8** - Runtime framework
- **ASP.NET Core** - Web API framework
- **Entity Framework Core** - ORM
- **ClosedXML** - Excel file processing
- **SQL Server** - Database
- **React 18** - Browser UI
- **Redux** - Browser-side state management
- **Tabulator** - Interactive employee directory tables

## Frontend

The React frontend lives in `wwwroot`:

| File or folder | Purpose |
|---|---|
| `wwwroot/index.html` | Loads React, ReactDOM, Redux, Tabulator, styles, and the app script |
| `wwwroot/app.js` | React components, API calls, Redux store, screen routing, and UI behavior |
| `wwwroot/styles.css` | Application layout and visual styling |
| `wwwroot/assets/` | Logo, carousel images, and other static assets |

Run the application and open `http://localhost:5054` to use the React UI. Swagger remains available at `http://localhost:5054/swagger` in Development.

```powershell
dotnet run --project LeaveManager.csproj --launch-profile http
```

## Architecture

The project uses a lightweight command-handler style. Controllers receive HTTP requests from the React UI or Swagger, create or accept command objects, and call the matching handler directly through dependency injection.

Example flow for uploading leave balances:

| Step | Component | Work |
|------|-----------|------|
| 1 | HTTP request | POST file to `/api/Excel/upload-leave-balances` |
| 2 | `ExcelController` | Receives the file and creates `UploadLeaveBalanceExcelCommand` |
| 3 | `UploadLeaveBalanceExcelHandler` | Parses and processes the Excel file |
| 4 | `LeaveBalanceService` | Creates or updates employee leave balances |
| 5 | Database | Saves changes |

Handlers are registered in `Program.cs`:

```csharp
builder.Services.AddScoped<ApplyLeaveHandler>();
builder.Services.AddScoped<ApproveLeaveHandler>();
builder.Services.AddScoped<UploadLeaveBalanceExcelHandler>();
```

Static frontend files are enabled in `Program.cs`:

```csharp
app.UseDefaultFiles();
app.UseStaticFiles();
```

## Excel File Format

Expected columns in the Excel file:

| Column 1 | Column 2 | Column 3 | Column 4 |
|----------|----------|----------|----------|
| Employee Code | Leave Type Name | Allocated Leaves | Used Leaves |
| EMP001 | Sick/Casual Leave | 10 | 0 |
| EMP002 | Planned Leave | 12 | 2 |

## Main Flows

### Apply Leave

```plaintext
LeaveController
- Receives leave application request
- Calls ApplyLeaveHandler

ApplyLeaveHandler
- Validates employee
- Validates leave type
- Checks leave balance for paid leave
- Finds project approver
- Creates pending leave application
- Sends approval email
```

### Approve or Reject Leave

```plaintext
LeaveController
- Receives approval token and action
- Decodes token
- Calls ApproveLeaveHandler

ApproveLeaveHandler
- Verifies assigned approver
- Ensures request is still pending
- Approves or rejects the leave
- Updates used leave balance when approved
- Sends employee notification email
```
