namespace LeaveManager.Entities
{
    public class Team
    {
        public int Id { get; set; }

        public string Name { get; set; } = string.Empty;

        public int ProjectId { get; set; }

        public Project Project { get; set; } = null!;

        public int LeadId { get; set; }

        public Employee Lead { get; set; } = null!;

        public ICollection<EmployeeTeam> EmployeeTeams { get; set; } = new List<EmployeeTeam>();
    }
}
