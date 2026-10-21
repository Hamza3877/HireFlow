using HireFlow.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HireFlow.Infrastructure.Data.Configurations;

// Fluent API rules for the Jobs table.
public class JobConfiguration : IEntityTypeConfiguration<Job>
{
    public void Configure(EntityTypeBuilder<Job> builder)
    {
        builder.HasKey(j => j.Id);
        builder.Property(j => j.Title).IsRequired().HasMaxLength(150);
        builder.Property(j => j.Department).IsRequired().HasMaxLength(100);
        builder.Property(j => j.Location).IsRequired().HasMaxLength(100);
        builder.Property(j => j.Description).IsRequired().HasMaxLength(4000);
        builder.Property(j => j.RequiredSkills).IsRequired().HasMaxLength(500);
        builder.Property(j => j.Status).HasConversion<string>().HasMaxLength(30);
        builder.HasIndex(j => j.Status);
    }
}
