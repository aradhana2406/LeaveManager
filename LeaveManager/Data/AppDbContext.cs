using LeaveManager.Entities;
using Microsoft.EntityFrameworkCore;

namespace LeaveManager.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public DbSet<Employee> Employees => Set<Employee>();

    public DbSet<LeaveType> LeaveTypes => Set<LeaveType>();

    public DbSet<LeaveApplication> LeaveApplications => Set<LeaveApplication>();

    public DbSet<Project> Projects => Set<Project>();

    public DbSet<Team> Teams => Set<Team>();

    public DbSet<EmployeeTeam> EmployeeTeams => Set<EmployeeTeam>();

    public DbSet<EmployeeLeaveBalance> EmployeeLeaveBalances
    => Set<EmployeeLeaveBalance>();

    public DbSet<EmployeeOnboardingProfile> EmployeeOnboardingProfiles
        => Set<EmployeeOnboardingProfile>();

    public DbSet<EmployeeOnboardingDocument> EmployeeOnboardingDocuments
        => Set<EmployeeOnboardingDocument>();

    public DbSet<UserLogin> UserLogins => Set<UserLogin>();

    public DbSet<HrPolicy> HrPolicies => Set<HrPolicy>();

    public DbSet<OrganizationRole> OrganizationRoles => Set<OrganizationRole>();


    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfigurationsFromAssembly(
            typeof(AppDbContext).Assembly);
    }
}
