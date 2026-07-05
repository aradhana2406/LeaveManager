namespace LeaveManager.Entities
{
    public class EmployeeTeam
    {
        public int EmployeeId { get; set; }

        public Employee Employee { get; set; } = null!;

        public int TeamId { get; set; }

        public Team Team { get; set; } = null!;
    }
}
