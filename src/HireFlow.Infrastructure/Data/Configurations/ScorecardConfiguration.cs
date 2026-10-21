using HireFlow.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HireFlow.Infrastructure.Data.Configurations;

// Fluent API rules for the Scorecards table. One scorecard per interview (unique InterviewId).
public class ScorecardConfiguration : IEntityTypeConfiguration<Scorecard>
{
    public void Configure(EntityTypeBuilder<Scorecard> builder)
    {
        builder.HasKey(s => s.Id);
        builder.Property(s => s.Comments).IsRequired().HasMaxLength(2000);
        builder.Property(s => s.Recommendation).IsRequired().HasMaxLength(50);
        builder.HasIndex(s => s.InterviewId).IsUnique();
    }
}
