using LeaveManager.Common.Enums;

namespace LeaveManager.Entities
{
    public class Employee
    {
        public int Id { get; set; }

        public string EmployeeID { get; set; } = string.Empty;

        public string FullName { get; set; } = string.Empty;

        public string Email { get; set; } = string.Empty;
        public string? TeamLeadId { get; set; }

        public string? AlternateTeamLeadId { get; set; }

        public Role Role { get; set; } 
    }
}
