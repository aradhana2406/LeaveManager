using LeaveManager.Common.Enums;

namespace LeaveManager.Entities;

public class OrganizationRole
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string Label { get; set; } = string.Empty;

    public Role BaseRole { get; set; } = Role.Employee;

    public bool IsActive { get; set; } = true;
}
