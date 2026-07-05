using LeaveManager.Features.EmployeeManagement.Services;

namespace LeaveManager.Features.EmployeeManagement.Commands.UploadLeaveBalanceExcel;

public class UploadLeaveBalanceExcelHandler
{
    private readonly IExcelLeaveBalanceParser _parser;
    private readonly ILeaveBalanceService _leaveBalanceService;

    public UploadLeaveBalanceExcelHandler(
        IExcelLeaveBalanceParser parser,
        ILeaveBalanceService leaveBalanceService)
    {
        _parser = parser;
        _leaveBalanceService = leaveBalanceService;
    }

    public async Task<UploadLeaveBalanceExcelResponse> Handle(
        UploadLeaveBalanceExcelCommand request,
        CancellationToken cancellationToken)
    {
        try
        {
            var rows = await _parser.ParseAsync(request.File);

            if (!rows.Any())
                return new UploadLeaveBalanceExcelResponse
                {
                    Success = false,
                    Message = "No data found in Excel file"
                };

            var (processed, failed, errors) =
                await _leaveBalanceService.UpdateLeaveBalancesAsync(rows, cancellationToken);

            return new UploadLeaveBalanceExcelResponse
            {
                Success = failed == 0,
                Message = $"Processed: {processed}, Failed: {failed}",
                RecordsProcessed = processed,
                RecordsFailed = failed,
                Errors = errors
            };
        }
        catch (Exception ex)
        {
            return new UploadLeaveBalanceExcelResponse
            {
                Success = false,
                Message = $"Upload failed: {ex.Message}",
                Errors = new() { ex.Message }
            };
        }
    }
}
