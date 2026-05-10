using ClosedXML.Excel;
using LeaveManager.Data;
using LeaveManager.Entities;
using LeaveManager.Features.Employee.Commands.UploadLeaveBalanceExcel;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LeaveManager.Features.Employee.Commands;

public class UploadLeaveBalanceExcelHandler
    : IRequestHandler<UploadLeaveBalanceExcelCommand, string>
{
    private readonly AppDbContext _context;

    public UploadLeaveBalanceExcelHandler(
        AppDbContext context)
    {
        _context = context;
    }

    public async Task<string> Handle(
        UploadLeaveBalanceExcelCommand request,
        CancellationToken cancellationToken)
    {
        using var stream = new MemoryStream();

        await request.File.CopyToAsync(stream, cancellationToken);

        using var workbook = new XLWorkbook(stream);

        var worksheet = workbook.Worksheet(1);

        var rows = worksheet.RowsUsed().Skip(1);

        foreach (var row in rows)
        {
            var employeeCode =
                row.Cell(1).GetString();

            var leaveTypeName =
                row.Cell(2).GetString();

            var allocatedLeaves =
                row.Cell(3).GetValue<int>();

            var employee = await _context.Employees
                .FirstOrDefaultAsync(
                    x => x.EmployeeID == employeeCode,
                    cancellationToken);

            var leaveType = await _context.LeaveTypes
                .FirstOrDefaultAsync(
                    x => x.Name == leaveTypeName,
                    cancellationToken);

            if (employee == null || leaveType == null)
                continue;

            var existing =
                await _context.EmployeeLeaveBalances
                    .FirstOrDefaultAsync(x =>
                        x.EmployeeId == employee.Id &&
                        x.LeaveTypeId == leaveType.Id,
                        cancellationToken);

            if (existing == null)
            {
                await _context.EmployeeLeaveBalances.AddAsync(
                    new EmployeeLeaveBalance
                    {
                        EmployeeId= employee.Id,
                        LeaveTypeId = leaveType.Id,
                        AllocatedLeaves = allocatedLeaves,
                        UsedLeaves = 0
                    });
            }
            else
            {
                existing.AllocatedLeaves =
                    allocatedLeaves;
            }
        }

        await _context.SaveChangesAsync(cancellationToken);

        return "Excel uploaded successfully";
    }
}