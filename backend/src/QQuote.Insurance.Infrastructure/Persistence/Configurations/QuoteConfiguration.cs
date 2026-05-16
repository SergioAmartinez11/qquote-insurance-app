using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QQuote.Insurance.Domain.Entities;
using QQuote.Insurance.Domain.ValueObjects;

namespace QQuote.Insurance.Infrastructure.Persistence.Configurations;

public class QuoteConfiguration : IEntityTypeConfiguration<Quote>
{
    public void Configure(EntityTypeBuilder<Quote> builder)
    {
        builder.HasKey(q => q.Id);

        builder.OwnsOne(q => q.Vehicle, v =>
        {
            v.Property(x => x.Year).HasColumnName("VehicleYear").IsRequired();
            v.Property(x => x.Make).HasColumnName("VehicleMake").HasMaxLength(100).IsRequired();
            v.Property(x => x.Model).HasColumnName("VehicleModel").HasMaxLength(100).IsRequired();
        });

        builder.OwnsOne(q => q.MonthlyPremium, m =>
        {
            m.Property(x => x.Amount).HasColumnName("PremiumAmount")
             .HasPrecision(18, 2).IsRequired();
            m.Property(x => x.Currency).HasColumnName("PremiumCurrency")
             .HasMaxLength(3).IsRequired();
        });

        builder.Property(q => q.CoverageType)
               .HasConversion(c => c.Name, name => CoverageType.From(name))
               .HasMaxLength(50).IsRequired();

        builder.Property(q => q.RiskLevel)
               .HasConversion(r => r.Name, name => RiskLevel.From(name))
               .HasMaxLength(20).IsRequired();

        builder.Property(q => q.RiskExplanation).HasMaxLength(500).IsRequired();
        builder.Property(q => q.Status).HasConversion<string>().HasMaxLength(20);
    }
}
