using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OrderManagement.Infrastructure.Idempotency;

namespace OrderManagement.Infrastructure.Persistence.Configurations
{
    public sealed class IdempotencyRecordConfiguration : IEntityTypeConfiguration<IdempotencyRecord>
    {
        public void Configure(
            EntityTypeBuilder<IdempotencyRecord> builder)
        {
            builder.ToTable("IdempotencyRecords");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.Key)
                .HasMaxLength(200)
                .IsRequired();

            builder.HasIndex(x => x.Key)
                .IsUnique();

            builder.Property(x => x.RequestHash)
                .HasMaxLength(64)
                .IsRequired();

            builder.Property(x => x.OrderId)
                .IsRequired();

            builder.Property(x => x.CreatedOnUtc)
                .IsRequired();
        }
    }
}

