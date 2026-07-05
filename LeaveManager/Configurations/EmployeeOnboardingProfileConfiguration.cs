using LeaveManager.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LeaveManager.Configurations;

public class EmployeeOnboardingProfileConfiguration : IEntityTypeConfiguration<EmployeeOnboardingProfile>
{
    public void Configure(EntityTypeBuilder<EmployeeOnboardingProfile> builder)
    {
        builder.ToTable("EmployeeOnboardingProfiles");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.PanNumber)
            .IsRequired()
            .HasMaxLength(20);

        builder.Property(x => x.AadhaarNumber)
            .IsRequired()
            .HasMaxLength(20);

        builder.Property(x => x.PreviousEmployerName)
            .HasMaxLength(200);

        builder.Property(x => x.YearsOfExperience)
            .HasPrecision(5, 2);

        builder.HasIndex(x => x.EmployeeId)
            .IsUnique();

        builder.HasOne(x => x.Employee)
            .WithMany()
            .HasForeignKey(x => x.EmployeeId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
