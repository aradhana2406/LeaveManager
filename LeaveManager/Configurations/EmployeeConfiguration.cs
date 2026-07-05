using LeaveManager.Common.Enums;
using LeaveManager.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LeaveManager.Configurations;

public class EmployeeConfiguration
    : IEntityTypeConfiguration<Employee>
{
    public void Configure(EntityTypeBuilder<Employee> builder)
    {
        builder.ToTable("Employees");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.EmployeeCode)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(x => x.FullName)
            .IsRequired()
            .HasMaxLength(150);

        builder.Property(x => x.Email)
            .IsRequired()
            .HasMaxLength(150);

        builder.Property(x => x.Department)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(x => x.Designation)
            .IsRequired()
            .HasMaxLength(120);

        builder.Property(x => x.JobRole)
            .IsRequired()
            .HasMaxLength(120);

        builder.Property(x => x.EmploymentType)
            .IsRequired()
            .HasMaxLength(60);

        builder.Property(x => x.Location)
            .IsRequired()
            .HasMaxLength(120);

        builder.Property(x => x.SalaryStructureDetails)
            .IsRequired()
            .HasMaxLength(400);

        builder.Property(x => x.JoinDate)
            .HasColumnType("date");

        builder.HasIndex(x => x.EmployeeCode)
            .IsUnique();

        builder.HasIndex(x => x.Email)
            .IsUnique();
        builder.Property(x => x.Role)
    .HasConversion<string>()
    .HasMaxLength(50);
        builder.HasOne(x => x.PrimaryTeam)
            .WithMany()
            .HasForeignKey(x => x.PrimaryTeamId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.OrganizationRole)
            .WithMany()
            .HasForeignKey(x => x.OrganizationRoleId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
