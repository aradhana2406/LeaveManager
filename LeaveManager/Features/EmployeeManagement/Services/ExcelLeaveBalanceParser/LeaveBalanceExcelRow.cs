namespace LeaveManager.Features.EmployeeManagement.Services;

/// <summary>
/// Data Transfer Object representing a single row from the Excel file.
/// </summary>
public class LeaveBalanceExcelRow
{
    /// <summary>
    /// Employee code (unique identifier from Excel column 1)
    /// </summary>
    public string EmployeeCode { get; set; } = string.Empty;

    /// <summary>
    /// Leave type name (from Excel column 2)
    /// </summary>
    public string LeaveTypeName { get; set; } = string.Empty;

    /// <summary>
    /// Number of leaves allocated (from Excel column 3)
    /// </summary>
    public int AllocatedLeaves { get; set; }

    /// <summary>
    /// Number of leaves already used (from Excel column 4)
    /// </summary>
    public decimal UsedLeaves { get; set; }
}