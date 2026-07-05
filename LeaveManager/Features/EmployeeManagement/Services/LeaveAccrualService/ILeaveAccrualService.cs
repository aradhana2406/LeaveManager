namespace LeaveManager.Features.EmployeeManagement.Services;

public interface ILeaveAccrualService
{
    Task SyncAllAsync(CancellationToken cancellationToken);

    Task SyncEmployeeAsync(int employeeId, CancellationToken cancellationToken);
}
