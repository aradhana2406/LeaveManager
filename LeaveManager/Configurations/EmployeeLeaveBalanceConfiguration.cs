using LeaveManager.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LeaveManager.Configurations;

public class EmployeeLeaveBalanceConfiguration
    : IEntityTypeConfiguration<EmployeeLeaveBalance>
{
    public void Configure(
        EntityTypeBuilder<EmployeeLeaveBalance> builder)
    {
        builder.HasKey(e => e.Id);

        // Configure decimal properties with precision and scale
        builder.Property(e => e.AllocatedLeaves)
            .HasPrecision(5, 2)
            .IsRequired();

        builder.Property(e => e.UsedLeaves)
            .HasPrecision(5, 2)
            .IsRequired();

        // Configure foreign key to Employee
        builder.HasOne(e => e.Employee)
            .WithMany(x => x.EmployeeLeaveBalances)
            .HasForeignKey(e => e.EmployeeId)
            .OnDelete(DeleteBehavior.Cascade);

        // Configure foreign key to LeaveType
        builder.HasOne(e => e.LeaveType)
            .WithMany(lt => lt.EmployeeLeaveBalances)
            .HasForeignKey(e => e.LeaveTypeId)
            .OnDelete(DeleteBehavior.Cascade);

        // Create unique constraint
        builder.HasIndex(e => new { e.EmployeeId, e.LeaveTypeId })
            .IsUnique()
            .HasDatabaseName("IX_EmployeeLeaveBalance_EmployeeId_LeaveTypeId");

    }
}
