# LeaveManager - Leave Management System

A robust .NET 8 leave management application built with ASP.NET Core, Entity Framework Core, and MediatR.

##  Project Overview

LeaveManager is a RESTful API for managing employee leave applications, leave types, and leave balance allocations. It implements the CQRS (Command Query Responsibility Segregation) pattern using MediatR for clean, maintainable command handling.

**Tech Stack:**
- **.NET 8** - Runtime Framework
- **ASP.NET Core** - Web Framework
- **Entity Framework Core** - ORM
- **MediatR** - CQRS Pattern
- **ClosedXML** - Excel File Processing
- **SQL Server** - Database

---

## Architecture

### CQRS Pattern with MediatR

This project uses the **Command Query Responsibility Segregation** pattern:


What Happens:
Step	Who	What
1	HTTP Request	POST file to /api/employee/upload-leave-balances
2	EmployeeController	Receives file, creates command object
3	_mediator.Send()	Dispatches the command
4	MediatR Engine	Searches for handler implementing IRequestHandler<UploadLeaveBalanceExcelCommand, string>
5	MediatR Engine	Finds UploadLeaveBalanceExcelHandler (registered in DI)
6	MediatR Engine	EXPLICITLY CALLS: handler.Handle(request, cancellationToken)
7	UploadLeaveBalanceExcelHandler.Handle()	Processes Excel file
8	Database	Saves changes

The Key Line in Program.cs:
builder.Services.AddMediatR(cfg => 
    cfg.RegisterServicesFromAssembly(typeof(Program).Assembly));

This tells MediatR to:
1.	Scan the entire assembly for all classes implementing IRequestHandler<T, U>
2.	Auto-register them in the dependency injection container
3.	When _mediator.Send() is called, MediatR uses reflection to find the matching handler
4.	MediatR instantiates the handler and calls Handle() internally
 
### Excel File Format

Expected columns in the Excel file:

| Column 1 | Column 2 | Column 3 |
|----------|----------|----------|
| Employee Code | Leave Type Name | Allocated Leaves |
| EMP001 | Annual Leave | 20 |
| EMP002 | Sick Leave | 10 |

### How It Works

#### 1. **Request Flow**

```plaintext
Employee Controller:
- Authenticates user
- Validates request
- Maps to command object
- Sends command via _mediator.Send()

MediatR:
- Receives command
- Identifies handler
- Calls handler's Handle method

Handler (e.g., UploadLeaveBalanceExcelHandler):
- Validates data
- Processes each row in the Excel file
- Saves data to the database

Database:
- Updates leave balances
- Commits transaction
```

#### 2. **Response**

```plaintext
Employee Controller:
- Receives response from MediatR
- Returns success or error response to the client

