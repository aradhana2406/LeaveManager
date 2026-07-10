using LeaveManager.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LeaveManager.Configurations;

public class EmployeeDeviceTicketConfiguration : IEntityTypeConfiguration<EmployeeDeviceTicket>
{
    public void Configure(EntityTypeBuilder<EmployeeDeviceTicket> builder)
    {
        builder.ToTable("EmployeeDeviceTickets");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.RequestType)
            .IsRequired()
            .HasMaxLength(80);

        builder.Property(x => x.DeviceType)
            .IsRequired()
            .HasMaxLength(80);

        builder.Property(x => x.NotificationTo)
            .IsRequired()
            .HasMaxLength(150);

        builder.Property(x => x.NotificationCc)
            .IsRequired()
            .HasMaxLength(150);

        builder.Property(x => x.Subject)
            .IsRequired()
            .HasMaxLength(160);

        builder.Property(x => x.Description)
            .IsRequired()
            .HasMaxLength(1000);

        builder.Property(x => x.Status)
            .IsRequired()
            .HasMaxLength(40);

        builder.HasOne(x => x.Employee)
            .WithMany()
            .HasForeignKey(x => x.EmployeeId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.AssignedHr)
            .WithMany()
            .HasForeignKey(x => x.AssignedHrId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
