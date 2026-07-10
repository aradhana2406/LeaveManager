using LeaveManager.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LeaveManager.Configurations;

public class EmployeeDeviceTicketTimelineEventConfiguration : IEntityTypeConfiguration<EmployeeDeviceTicketTimelineEvent>
{
    public void Configure(EntityTypeBuilder<EmployeeDeviceTicketTimelineEvent> builder)
    {
        builder.ToTable("EmployeeDeviceTicketTimelineEvents");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Status)
            .IsRequired()
            .HasMaxLength(40);

        builder.Property(x => x.Notes)
            .IsRequired()
            .HasMaxLength(1000);

        builder.HasOne(x => x.EmployeeDeviceTicket)
            .WithMany(x => x.TimelineEvents)
            .HasForeignKey(x => x.EmployeeDeviceTicketId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
