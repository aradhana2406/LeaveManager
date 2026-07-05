using ClosedXML.Excel;
using LeaveManager.Data;
using LeaveManager.Features.EmployeeManagement.Commands.UploadExistingEmployeesExcel;
using LeaveManager.Features.EmployeeManagement.Commands.UploadLeaveBalanceExcel;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LeaveManager.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ExcelController : ControllerBase
{
    private readonly UploadExistingEmployeesExcelHandler _uploadExistingEmployeesExcelHandler;
    private readonly UploadLeaveBalanceExcelHandler _uploadLeaveBalanceExcelHandler;
    private readonly AppDbContext _context;

    public ExcelController(
        UploadExistingEmployeesExcelHandler uploadExistingEmployeesExcelHandler,
        UploadLeaveBalanceExcelHandler uploadLeaveBalanceExcelHandler,
        AppDbContext context)
    {
        _uploadExistingEmployeesExcelHandler = uploadExistingEmployeesExcelHandler;
        _uploadLeaveBalanceExcelHandler = uploadLeaveBalanceExcelHandler;
        _context = context;
    }

    [HttpPost("upload-existing-employees")]
    public async Task<IActionResult> UploadExistingEmployees(
        IFormFile file,
        CancellationToken cancellationToken)
    {
        var result = await _uploadExistingEmployeesExcelHandler.Handle(
            new UploadExistingEmployeesExcelCommand(file),
            cancellationToken);

        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpPost("upload-leave-balances")]
    public async Task<IActionResult> UploadLeaveBalances(
        IFormFile file,
        CancellationToken cancellationToken)
    {
        var result = await _uploadLeaveBalanceExcelHandler.Handle(
            new UploadLeaveBalanceExcelCommand(file),
            cancellationToken);

        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpGet("leave-balance-template")]
    public async Task<IActionResult> DownloadLeaveBalanceTemplate(CancellationToken cancellationToken)
    {
        var employees = await _context.Employees
            .AsNoTracking()
            .OrderBy(x => x.EmployeeCode)
            .Take(3)
            .Select(x => new { x.EmployeeCode, x.FullName })
            .ToListAsync(cancellationToken);

        var leaveTypes = await _context.LeaveTypes
            .AsNoTracking()
            .OrderBy(x => x.Name)
            .Select(x => x.Name)
            .ToListAsync(cancellationToken);

        using var workbook = new XLWorkbook();
        var sheet = workbook.Worksheets.Add("LeaveBalances");

        sheet.Cell(1, 1).Value = "EmployeeCode";
        sheet.Cell(1, 2).Value = "LeaveTypeName";
        sheet.Cell(1, 3).Value = "AllocatedLeaves";
        sheet.Cell(1, 4).Value = "UsedLeaves";
        sheet.Cell(1, 5).Value = "EmployeeName (reference)";

        var header = sheet.Range(1, 1, 1, 5);
        header.Style.Font.Bold = true;
        header.Style.Fill.BackgroundColor = XLColor.FromHtml("#DBE7DC");

        var sampleRows = new List<(string EmployeeCode, string LeaveTypeName, int AllocatedLeaves, decimal UsedLeaves, string EmployeeName)>();

        foreach (var employee in employees)
        {
            foreach (var leaveType in leaveTypes)
            {
                sampleRows.Add((employee.EmployeeCode, leaveType, 12, 2, employee.FullName));
            }
        }

        if (sampleRows.Count == 0)
        {
            sampleRows.Add(("EMP001", "Planned Leave", 12, 2, "Sample Employee"));
            sampleRows.Add(("EMP001", "Sick/Casual Leave", 7, 1, "Sample Employee"));
            sampleRows.Add(("EMP001", "Director Special Leave", 0, 0, "Sample Employee"));
        }

        var rowIndex = 2;
        foreach (var row in sampleRows)
        {
            sheet.Cell(rowIndex, 1).Value = row.EmployeeCode;
            sheet.Cell(rowIndex, 2).Value = row.LeaveTypeName;
            sheet.Cell(rowIndex, 3).Value = row.AllocatedLeaves;
            sheet.Cell(rowIndex, 4).Value = row.UsedLeaves;
            sheet.Cell(rowIndex, 5).Value = row.EmployeeName;
            rowIndex++;
        }

        sheet.Columns().AdjustToContents();

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        stream.Position = 0;

        return File(
            stream.ToArray(),
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            "leave-balance-template.xlsx");
    }

    [HttpGet("existing-employees-template")]
    public IActionResult DownloadExistingEmployeesTemplate()
    {
        using var workbook = new XLWorkbook();
        var sheet = workbook.Worksheets.Add("ExistingEmployees");

        var headers = new[]
        {
            "EmployeeCode",
            "FullName",
            "OfficialEmail",
            "Department",
            "Designation",
            "Role",
            "JoinDate",
            "EmploymentType",
            "Location",
            "SystemAccess",
            "ProjectName",
            "PrimaryTeam",
            "AdditionalTeams",
            "TeamLeadEmpcode",
            "SalaryStructureDetails",
            "Username",
            "TemporaryPassword"
        };

        for (var index = 0; index < headers.Length; index++)
        {
            sheet.Cell(1, index + 1).Value = headers[index];
        }

        var rows = new[]
        {
            new[]
            {
                "67",
                "Aradhana Shinde",
                "aradhana.shinde@company.com",
                "Engineering",
                "Software Engineer",
                ".NET Developer",
                "2026-02-10",
                "Full-time",
                "Mumbai",
                "Software Engineer",
                "Data Operations",
                "Data Surfers",
                "",
                "ARIF001",
                "CTC 8 LPA | Fixed 7 LPA | Variable 1 LPA",
                "aradhana",
                "demo123"
            },
            new[]
            {
                "ARIF001",
                "Arif Mirza",
                "arif.mirza@company.com",
                "Data Operations",
                "Delivery",
                "Technical Delivery",
                "2025-06-12",
                "Full-time",
                "Pune",
                "Technical Manager L2",
                "Data Operations",
                "",
                "",
                "",
                "CTC 22 LPA | Fixed 19 LPA | Variable 3 LPA",
                "arif",
                "demo123"
            },
            new[]
            {
                "GIRISH001",
                "Girish Patil",
                "girish.patil@company.com",
                "Data Operations",
                "Delivery",
                "Technical Delivery",
                "2025-08-20",
                "Full-time",
                "Bengaluru",
                "Technical Manager L2",
                "Data Operations",
                "",
                "",
                "",
                "CTC 21 LPA | Fixed 18 LPA | Variable 3 LPA",
                "girish",
                "demo123"
            }
        };

        for (var rowIndex = 0; rowIndex < rows.Length; rowIndex++)
        {
            for (var columnIndex = 0; columnIndex < rows[rowIndex].Length; columnIndex++)
            {
                sheet.Cell(rowIndex + 2, columnIndex + 1).Value = rows[rowIndex][columnIndex];
            }
        }

        var header = sheet.Range(1, 1, 1, headers.Length);
        header.Style.Font.Bold = true;
        header.Style.Fill.BackgroundColor = XLColor.FromHtml("#DBE7DC");
        sheet.Column(7).Style.DateFormat.Format = "yyyy-mm-dd";
        sheet.SheetView.FreezeRows(1);
        sheet.Columns().AdjustToContents();

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        stream.Position = 0;

        return File(
            stream.ToArray(),
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            "existing-employees-template.xlsx");
    }
}
