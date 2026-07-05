namespace LeaveManager.Features.EmployeeManagement.Services;

/// <summary>
/// Interface for updating employee leave balances.
/// Handles business logic for creating or updating leave balance records.
/// </summary>
public interface ILeaveBalanceService
{
    /// <summary>
    /// Updates leave balances for multiple employees asynchronously.
    /// </summary>
    /// <param name="rows">List of leave balance rows from Excel</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Tuple containing processed count, failed count, and error messages</returns>
    Task<(int Processed, int Failed, List<string> Errors)> UpdateLeaveBalancesAsync(
        List<LeaveBalanceExcelRow> rows,
        CancellationToken cancellationToken);
}