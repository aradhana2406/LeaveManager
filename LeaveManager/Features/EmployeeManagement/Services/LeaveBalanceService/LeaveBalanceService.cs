using LeaveManager.Data;
using LeaveManager.Entities;
using Microsoft.EntityFrameworkCore;
using System.Text;

namespace LeaveManager.Features.EmployeeManagement.Services;

/// <summary>
/// Implementation of leave balance service.
/// Handles creation and updates of employee leave balance records.
/// </summary>
public class LeaveBalanceService : ILeaveBalanceService
{
    private static readonly Dictionary<string, string[]> LeaveTypeAliases = new(StringComparer.OrdinalIgnoreCase)
    {
        ["Sick/Casual Leave"] = ["sick leave", "sick leaves", "casual leave", "casual leaves", "medical leave", "medical leaves", "medical", "sick casual leave", "sick/casual leave", "sickcasualleave"],
        ["Planned Leave"] = ["planned leave", "planned leaves", "plan leave", "plannedleave", "planned"],
        ["Director Special Leave"] = ["director special leave", "director special leaves", "special leave", "special leaves", "directors special leave", "directorspecialleave"],
        ["Unpaid Leave"] = ["unpaid leave", "unpaid leaves", "loss of pay", "lop"],
        ["Maternity Leave"] = ["maternity leave", "maternity leaves", "maternity"]
    };

    private readonly AppDbContext _context;

    public LeaveBalanceService(AppDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Processes each row and updates or creates leave balance records.
    /// </summary>
    public async Task<(int Processed, int Failed, List<string> Errors)> UpdateLeaveBalancesAsync(
        List<LeaveBalanceExcelRow> rows,
        CancellationToken cancellationToken)
    {
        int processed = 0;
        int failed = 0;
        var errors = new List<string>();

        foreach (var row in rows)
        {
            try
            {
                await ProcessRowAsync(row, cancellationToken);
                processed++;
            }
            catch (Exception ex)
            {
                failed++;
                errors.Add($"Employee {row.EmployeeCode}: {ex.Message}");
            }
        }

        await _context.SaveChangesAsync(cancellationToken);
        return (processed, failed, errors);
    }

    /// <summary>
    /// Processes a single row: validates, finds employee and leave type, then creates or updates balance.
    /// </summary>
    private async Task ProcessRowAsync(LeaveBalanceExcelRow row, CancellationToken cancellationToken)
    {
        // Validation
        if (string.IsNullOrWhiteSpace(row.EmployeeCode))
            throw new InvalidOperationException("Employee code is required");

        if (string.IsNullOrWhiteSpace(row.LeaveTypeName))
            throw new InvalidOperationException("Leave type name is required");

        if (row.AllocatedLeaves < 0)
            throw new InvalidOperationException("Allocated leaves cannot be negative");

        if (row.UsedLeaves < 0)
            throw new InvalidOperationException("Used leaves cannot be negative");

        // Find entities
        var employee = await _context.Employees
            .FirstOrDefaultAsync(x => x.EmployeeCode == row.EmployeeCode, cancellationToken)
            ?? throw new InvalidOperationException($"Employee {row.EmployeeCode} not found");

        var leaveType = await ResolveLeaveTypeAsync(row.LeaveTypeName, cancellationToken)
            ?? throw new InvalidOperationException($"Leave type {row.LeaveTypeName} not found");

        // Create or update balance
        var existing = await _context.EmployeeLeaveBalances
            .FirstOrDefaultAsync(x =>
                x.EmployeeId == employee.Id &&
                x.LeaveTypeId == leaveType.Id,
                cancellationToken);

        if (existing == null)
        {
            await _context.EmployeeLeaveBalances.AddAsync(
                new EmployeeLeaveBalance
                {
                    EmployeeId = employee.Id,
                    LeaveTypeId = leaveType.Id,
                    AllocatedLeaves = row.AllocatedLeaves,
                    UsedLeaves = row.UsedLeaves
                },
                cancellationToken);
        }
        else
        {
            existing.AllocatedLeaves = row.AllocatedLeaves;
            existing.UsedLeaves = row.UsedLeaves;
        }
    }

    private async Task<LeaveType?> ResolveLeaveTypeAsync(string leaveTypeName, CancellationToken cancellationToken)
    {
        var leaveTypes = await _context.LeaveTypes
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        var normalizedInput = NormalizeLeaveTypeName(leaveTypeName);

        var exactMatch = leaveTypes.FirstOrDefault(x => NormalizeLeaveTypeName(x.Name) == normalizedInput);
        if (exactMatch != null)
        {
            return exactMatch;
        }

        foreach (var leaveType in leaveTypes)
        {
            if (!LeaveTypeAliases.TryGetValue(leaveType.Name, out var aliases))
            {
                continue;
            }

            if (aliases.Any(alias => NormalizeLeaveTypeName(alias) == normalizedInput))
            {
                return leaveType;
            }
        }

        return null;
    }

    private static string NormalizeLeaveTypeName(string value)
    {
        var builder = new StringBuilder(value.Length);
        var previousWasSpace = false;

        foreach (var character in value.Trim().ToLowerInvariant())
        {
            if (char.IsLetterOrDigit(character))
            {
                builder.Append(character);
                previousWasSpace = false;
                continue;
            }

            if (!previousWasSpace)
            {
                builder.Append(' ');
                previousWasSpace = true;
            }
        }

        return builder.ToString().Trim();
    }
}
