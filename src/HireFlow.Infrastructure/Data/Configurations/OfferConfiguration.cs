using HireFlow.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HireFlow.Infrastructure.Data.Configurations;

// Fluent API rules for the Offers table.
public class OfferConfiguration : IEntityTypeConfiguration<Offer>
{
    public void Configure(EntityTypeBuilder<Offer> builder)
    {
        builder.HasKey(o => o.Id);
        builder.Property(o => o.Salary).HasPrecision(18, 2);
        builder.Property(o => o.Status).HasConversion<string>().HasMaxLength(30);
        builder.Property(o => o.AcceptToken).IsRequired().HasMaxLength(100);
        builder.HasIndex(o => o.AcceptToken).IsUnique();

        builder.HasOne(o => o.Application).WithMany().HasForeignKey(o => o.ApplicationId).OnDelete(DeleteBehavior.Restrict);
    }
}
