using HireFlow.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HireFlow.Infrastructure.Data.Configurations;

// Fluent API rules for the Interviews table.
public class InterviewConfiguration : IEntityTypeConfiguration<Interview>
{
    public void Configure(EntityTypeBuilder<Interview> builder)
    {
        builder.HasKey(i => i.Id);
        builder.Property(i => i.Status).HasConversion<string>().HasMaxLength(30);
        builder.HasIndex(i => new { i.InterviewerId, i.StartTime });

        builder.HasOne(i => i.Application).WithMany().HasForeignKey(i => i.ApplicationId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(i => i.Interviewer).WithMany().HasForeignKey(i => i.InterviewerId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(i => i.Scorecard).WithOne(s => s.Interview).HasForeignKey<Scorecard>(s => s.InterviewId).OnDelete(DeleteBehavior.Restrict);
    }
}
