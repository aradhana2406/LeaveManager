# LeaveManager - Leave Management System

A .NET 8 leave management API built with ASP.NET Core, Entity Framework Core, ClosedXML, and SQL Server.

## Project Overview

LeaveManager is a RESTful API for managing employee leave applications, leave approval, leave types, and leave balance allocations.

**Tech Stack:**
- **.NET 8** - Runtime framework
- **ASP.NET Core** - Web API framework
- **Entity Framework Core** - ORM
- **ClosedXML** - Excel file processing
- **SQL Server** - Database

## Architecture

The project uses a lightweight command-handler style. Controllers receive HTTP requests, create or accept command objects, and call the matching handler directly through dependency injection.

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
