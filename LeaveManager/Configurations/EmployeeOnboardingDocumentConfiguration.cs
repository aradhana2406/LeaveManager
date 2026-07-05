using LeaveManager.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LeaveManager.Configurations;

public class EmployeeOnboardingDocumentConfiguration : IEntityTypeConfiguration<EmployeeOnboardingDocument>
{
    public void Configure(EntityTypeBuilder<EmployeeOnboardingDocument> builder)
    {
        builder.ToTable("EmployeeOnboardingDocuments");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.DocumentType)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(x => x.OriginalFileName)
            .IsRequired()
            .HasMaxLength(260);

        builder.Property(x => x.StoredFileName)
            .IsRequired()
            .HasMaxLength(260);

        builder.Property(x => x.ContentType)
            .IsRequired()
            .HasMaxLength(150);

        builder.Property(x => x.RelativePath)
            .IsRequired()
            .HasMaxLength(400);

        builder.HasOne(x => x.Employee)
            .WithMany()
            .HasForeignKey(x => x.EmployeeId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
