using ClosedXML.Excel;

namespace LeaveManager.Features.EmployeeManagement.Commands.UploadExistingEmployeesExcel;

public class ExistingEmployeeExcelParser : IExistingEmployeeExcelParser
{
    public async Task<List<ExistingEmployeeExcelRow>> ParseAsync(IFormFile file)
    {
        var rows = new List<ExistingEmployeeExcelRow>();

        using var stream = new MemoryStream();
        await file.CopyToAsync(stream);
        stream.Position = 0;

        using var workbook = new XLWorkbook(stream);
        var worksheet = workbook.Worksheet(1);

        foreach (var row in worksheet.RowsUsed().Skip(1))
        {
            if (row.Cells(1, 17).All(cell => string.IsNullOrWhiteSpace(cell.GetString())))
            {
                continue;
            }

            rows.Add(new ExistingEmployeeExcelRow
            {
                RowNumber = row.RowNumber(),
                EmployeeCode = GetText(row, 1),
                FullName = GetText(row, 2),
                OfficialEmail = GetText(row, 3),
                Department = GetText(row, 4),
                Designation = GetText(row, 5),
                JobRole = GetText(row, 6),
                JoinDate = GetDate(row, 7),
                EmploymentType = GetText(row, 8),
                Location = GetText(row, 9),
                Role = GetText(row, 10),
                ProjectName = GetText(row, 11),
                PrimaryTeam = GetText(row, 12),
                AdditionalTeams = GetText(row, 13),
                TeamLeadEmpcode = GetText(row, 14),
                SalaryStructureDetails = GetText(row, 15),
                Username = GetText(row, 16),
                TemporaryPassword = GetText(row, 17)
            });
        }

        return rows;
    }

    private static string GetText(IXLRow row, int columnNumber)
    {
        return row.Cell(columnNumber).GetString().Trim();
    }

    private static DateTime GetDate(IXLRow row, int columnNumber)
    {
        var cell = row.Cell(columnNumber);
        if (cell.TryGetValue<DateTime>(out var date))
        {
            return date.Date;
        }

        return DateTime.TryParse(cell.GetString(), out var parsed)
            ? parsed.Date
            : default;
    }
}
