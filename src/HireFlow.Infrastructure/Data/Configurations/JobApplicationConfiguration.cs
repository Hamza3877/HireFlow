using HireFlow.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HireFlow.Infrastructure.Data.Configurations;

// Fluent API rules for the Applications table.
// Restrict (no cascade delete) avoids SQL Server "multiple cascade paths" errors.
public class JobApplicationConfiguration : IEntityTypeConfiguration<JobApplication>
{
    public void Configure(EntityTypeBuilder<JobApplication> builder)
    {
        builder.ToTable("Applications");
        builder.HasKey(a => a.Id);
        builder.Property(a => a.Stage).HasConversion<string>().HasMaxLength(30);
        builder.Property(a => a.CvFilePath).IsRequired().HasMaxLength(300);
        builder.Property(a => a.ApplicationCode).IsRequired().HasMaxLength(20);
        builder.Property(a => a.RejectionReason).HasMaxLength(500);

        builder.HasIndex(a => a.ApplicationCode).IsUnique();
        builder.HasIndex(a => new { a.JobId, a.CandidateId }).IsUnique();
        builder.HasIndex(a => a.Stage);

        builder.HasOne(a => a.Job).WithMany().HasForeignKey(a => a.JobId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(a => a.Candidate).WithMany().HasForeignKey(a => a.CandidateId).OnDelete(DeleteBehavior.Restrict);
    }
}
