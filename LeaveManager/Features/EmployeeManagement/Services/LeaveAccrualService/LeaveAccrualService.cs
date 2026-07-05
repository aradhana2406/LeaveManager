using LeaveManager.Data;
using Microsoft.EntityFrameworkCore;

namespace LeaveManager.Features.EmployeeManagement.Services;

public class LeaveAccrualService : ILeaveAccrualService
{
    private readonly AppDbContext _context;

    public LeaveAccrualService(AppDbContext context)
    {
        _context = context;
    }

    public async Task SyncAllAsync(CancellationToken cancellationToken)
    {
        var accruedLeaveTypes = await _context.LeaveTypes
            .Where(x => x.IsAccrued && x.AccrualPerMonth > 0)
            .ToListAsync(cancellationToken);

        if (accruedLeaveTypes.Count == 0)
        {
            return;
        }

        var employees = await _context.Employees
            .Include(x => x.EmployeeLeaveBalances)
            .Where(x => x.IsActive)
            .ToListAsync(cancellationToken);

        foreach (var employee in employees)
        {
            ApplyAccruals(employee.JoinDate, employee.EmployeeLeaveBalances, accruedLeaveTypes);
        }

        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task SyncEmployeeAsync(int employeeId, CancellationToken cancellationToken)
    {
        var accruedLeaveTypes = await _context.LeaveTypes
            .Where(x => x.IsAccrued && x.AccrualPerMonth > 0)
            .ToListAsync(cancellationToken);

        if (accruedLeaveTypes.Count == 0)
        {
            return;
        }

        var employee = await _context.Employees
            .Include(x => x.EmployeeLeaveBalances)
            .FirstOrDefaultAsync(x => x.Id == employeeId, cancellationToken);

        if (employee == null)
        {
            return;
        }

        ApplyAccruals(employee.JoinDate, employee.EmployeeLeaveBalances, accruedLeaveTypes);
        await _context.SaveChangesAsync(cancellationToken);
    }

    private static void ApplyAccruals(
        DateTime joinDate,
        IEnumerable<EmployeeLeaveBalance> balances,
        IEnumerable<LeaveManager.Entities.LeaveType> accruedLeaveTypes)
    {
        var effectiveJoinDate = joinDate.Date;
        var today = DateTime.UtcNow.Date;

        foreach (var leaveType in accruedLeaveTypes)
        {
            var balance = balances.FirstOrDefault(x => x.LeaveTypeId == leaveType.Id);
            if (balance == null)
            {
                continue;
            }

            balance.AllocatedLeaves = CalculateAccruedLeaves(effectiveJoinDate, today, leaveType.AccrualPerMonth);
        }
    }

    private static decimal CalculateAccruedLeaves(DateTime joinDate, DateTime today, decimal accrualPerMonth)
    {
        if (joinDate == default)
        {
            return accrualPerMonth;
        }

        if (joinDate > today)
        {
            return 0m;
        }

        var accrualStart = new DateTime(today.Year, 1, 1);
        if (joinDate > accrualStart)
        {
            accrualStart = joinDate;
        }

        var elapsedMonths = ((today.Year - accrualStart.Year) * 12) + today.Month - accrualStart.Month + 1;
        return elapsedMonths * accrualPerMonth;
    }
}
