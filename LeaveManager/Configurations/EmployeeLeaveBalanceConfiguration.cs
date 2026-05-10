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
        builder.ToTable("EmployeeLeaveBalances");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.AllocatedLeaves)
            .IsRequired();

        builder.Property(x => x.UsedLeaves)
            .IsRequired();
        builder.HasOne(x => x.Employee)
            .WithMany()
            .HasForeignKey(x => x.EmployeeId)
            .OnDelete(DeleteBehavior.NoAction);


        builder.HasOne<LeaveType>()
            .WithMany()
            .HasForeignKey(x => x.LeaveTypeId);

        builder.HasIndex(x =>
            new { x.EmployeeId, x.LeaveTypeId })
            .IsUnique();
   
    }
}