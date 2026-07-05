namespace LeaveManager.Entities;

public class UserLogin
{
    public int Id { get; set; }

    public string Username { get; set; } = string.Empty;

    public string Password { get; set; } = string.Empty;

    public bool IsActive { get; set; } = true;

    public int EmployeeId { get; set; }

    public Employee Employee { get; set; } = null!;
}
