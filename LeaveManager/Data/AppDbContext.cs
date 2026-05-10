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
    public DbSet<EmployeeLeaveBalance> EmployeeLeaveBalances
    => Set<EmployeeLeaveBalance>();


    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfigurationsFromAssembly(
            typeof(AppDbContext).Assembly);
    }
}