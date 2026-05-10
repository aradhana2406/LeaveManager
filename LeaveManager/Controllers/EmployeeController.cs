using LeaveManager.Features.Employee.Commands.UploadLeaveBalanceExcel;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace LeaveManager.Controllers;

[ApiController]
[Route("api/[controller]")]
public class EmployeeController : ControllerBase
{
    private readonly IMediator _mediator;

    public EmployeeController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost("upload-leave-balances")]
    public async Task<IActionResult> UploadLeaveBalances(
        IFormFile file)
    {
        var result =
            await _mediator.Send(
                new UploadLeaveBalanceExcelCommand(file));

        return Ok(result);
    }
}