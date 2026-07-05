namespace LeaveManager.Features.EmployeeManagement.Services;

/// <summary>
/// Interface for parsing Excel files containing leave balance data.
/// Handles file parsing and validation.
/// </summary>
public interface IExcelLeaveBalanceParser
{
    /// <summary>
    /// Parses an Excel file and extracts leave balance rows.
    /// </summary>
    /// <param name="file">The Excel file to parse (.xlsx format)</param>
    /// <returns>List of parsed leave balance rows</returns>
    /// <exception cref="InvalidOperationException">Thrown if file format is invalid</exception>
    Task<List<LeaveBalanceExcelRow>> ParseAsync(IFormFile file);
}