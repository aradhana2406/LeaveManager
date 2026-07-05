using LeaveManager.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LeaveManager.Configurations;

public class EmployeeTeamConfiguration : IEntityTypeConfiguration<EmployeeTeam>
{
    public void Configure(EntityTypeBuilder<EmployeeTeam> builder)
    {
        builder.ToTable("EmployeeTeams");

        builder.HasKey(x => new { x.EmployeeId, x.TeamId });

        builder.HasOne(x => x.Employee)
            .WithMany(x => x.EmployeeTeams)
            .HasForeignKey(x => x.EmployeeId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.Team)
            .WithMany(x => x.EmployeeTeams)
            .HasForeignKey(x => x.TeamId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
