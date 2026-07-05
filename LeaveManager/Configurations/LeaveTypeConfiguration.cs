using LeaveManager.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LeaveManager.Configurations;

public class LeaveTypeConfiguration
    : IEntityTypeConfiguration<LeaveType>
{
    public void Configure(EntityTypeBuilder<LeaveType> builder)
    {
        builder.ToTable("LeaveTypes");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Name)
            .IsRequired()
            .HasMaxLength(100);
        builder.Property(x => x.AccrualPerMonth)
    .HasPrecision(5, 2);


        builder.HasData(
            new LeaveType
            {
                Id = 1,
                Name = "Sick/Casual Leave",
                IsPaid = true,
                RequiresAdvanceNotice = false,
                AdvanceNoticeDays = 0,
            },


            new LeaveType
            {
                Id = 2,
                Name = "Planned Leave",
                IsPaid = true,
                RequiresAdvanceNotice = true,
                AdvanceNoticeDays = 15,
                IsAccrued = true,
                AccrualPerMonth = 1m
            },

            new LeaveType
            {
                Id = 3,
                Name = "Unpaid Leave",
                IsPaid = false,
                RequiresAdvanceNotice = false,
                AdvanceNoticeDays = 0,
            },

            new LeaveType
            {
                Id = 4,
                Name = "Maternity Leave",
                IsPaid = true,
                RequiresAdvanceNotice = true,
                AdvanceNoticeDays = 30,
            },

            new LeaveType
            {
                Id = 5,
                Name = "Director Special Leave",
                IsPaid = true,
                RequiresAdvanceNotice = false,
                AdvanceNoticeDays = 0,
            });
    }
}
