using LeaveManager.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LeaveManager.Configurations;

public class UserLoginConfiguration : IEntityTypeConfiguration<UserLogin>
{
    public void Configure(EntityTypeBuilder<UserLogin> builder)
    {
        builder.ToTable("UserLogins");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Username)
            .IsRequired()
            .HasMaxLength(80);

        builder.Property(x => x.Password)
            .IsRequired()
            .HasMaxLength(120);

        builder.HasIndex(x => x.Username)
            .IsUnique();

        builder.HasOne(x => x.Employee)
            .WithMany()
            .HasForeignKey(x => x.EmployeeId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
