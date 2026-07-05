using ClosedXML.Excel;

namespace LeaveManager.Features.EmployeeManagement.Services;

/// <summary>
/// Implementation of Excel parser using ClosedXML library.
/// Extracts leave balance data from .xlsx files.
/// </summary>
public class ExcelLeaveBalanceParser : IExcelLeaveBalanceParser
{
    public async Task<List<LeaveBalanceExcelRow>> ParseAsync(IFormFile file)
    {
        var rows = new List<LeaveBalanceExcelRow>();

        using var stream = new MemoryStream();
        await file.CopyToAsync(stream);
        stream.Position = 0;

        using var workbook = new XLWorkbook(stream);
        var worksheet = workbook.Worksheet(1);

        foreach (var row in worksheet.RowsUsed().Skip(1))
        {
            try
            {
                rows.Add(new LeaveBalanceExcelRow
                {
                    EmployeeCode = row.Cell(1).GetString().Trim(),
                    LeaveTypeName = row.Cell(2).GetString().Trim(),
                    AllocatedLeaves = row.Cell(3).GetValue<int>(),
                    UsedLeaves = row.Cell(4).GetValue<decimal>()
                });
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException(
                    $"Error parsing row {row.RowNumber}: {ex.Message}");
            }
        }

        return rows;
    }
}
