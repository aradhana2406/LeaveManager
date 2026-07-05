using LeaveManager.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LeaveManager.Configurations;

public class HrPolicyConfiguration : IEntityTypeConfiguration<HrPolicy>
{
    public void Configure(EntityTypeBuilder<HrPolicy> builder)
    {
        builder.ToTable("HrPolicies");

        builder.HasKey(x => x.Id);
    }
}
