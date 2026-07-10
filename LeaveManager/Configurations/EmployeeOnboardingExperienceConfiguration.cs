using LeaveManager.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LeaveManager.Configurations;

public class EmployeeOnboardingExperienceConfiguration : IEntityTypeConfiguration<EmployeeOnboardingExperience>
{
    public void Configure(EntityTypeBuilder<EmployeeOnboardingExperience> builder)
    {
        builder.ToTable("EmployeeOnboardingExperiences");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.CompanyName)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(x => x.JobTitle)
            .HasMaxLength(150);

        builder.Property(x => x.YearsOfExperience)
            .HasPrecision(5, 2);

        builder.HasOne(x => x.EmployeeOnboardingProfile)
            .WithMany(x => x.Experiences)
            .HasForeignKey(x => x.EmployeeOnboardingProfileId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
