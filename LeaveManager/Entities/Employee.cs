using LeaveManager.Common.Enums;

namespace LeaveManager.Entities
{
    public class Employee
    {
        public int Id { get; set; }

        public string EmployeeCode { get; set; } = string.Empty;

        public string FullName { get; set; } = string.Empty;

        public string Email { get; set; } = string.Empty;

        public string Department { get; set; } = string.Empty;

        public string Designation { get; set; } = string.Empty;

        public string JobRole { get; set; } = string.Empty;

        public string EmploymentType { get; set; } = string.Empty;

        public string Location { get; set; } = string.Empty;

        public string SalaryStructureDetails { get; set; } = string.Empty;

        public DateTime JoinDate { get; set; }

        public int? PrimaryTeamId { get; set; }

        public Team? PrimaryTeam { get; set; }

        public Role Role { get; set; }

        public int? OrganizationRoleId { get; set; }

        public OrganizationRole? OrganizationRole { get; set; }

        // Instead of deleting employees
        public bool IsActive { get; set; } = true;

        public ICollection<EmployeeLeaveBalance> EmployeeLeaveBalances { get; set; } = new List<EmployeeLeaveBalance>();
        public ICollection<EmployeeTeam> EmployeeTeams { get; set; } = new List<EmployeeTeam>();
    }
}
