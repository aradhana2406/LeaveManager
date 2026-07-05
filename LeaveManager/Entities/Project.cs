namespace LeaveManager.Entities
{
    public class Project
    {
        public int Id { get; set; }

        public string Name { get; set; } = string.Empty;

        public ICollection<Team> Teams { get; set; } = new List<Team>();
    }
}

