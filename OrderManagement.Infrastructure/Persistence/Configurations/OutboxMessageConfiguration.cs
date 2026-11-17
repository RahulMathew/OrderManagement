using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OrderManagement.Infrastructure.Outbox;

namespace OrderManagement.Infrastructure.Persistence.Configurations
{
    public sealed class OutboxMessageConfiguration : IEntityTypeConfiguration<OutboxMessage>
    {
        public void Configure(EntityTypeBuilder<OutboxMessage> builder)
        {
            // Table name
            builder.ToTable("OutboxMessages");

            // Primary key
            builder.HasKey(x => x.Id);

            builder.Property(x => x.Id)
                .ValueGeneratedNever();

            // Integration event type
            builder.Property(x => x.EventType)
                .HasMaxLength(200)
                .IsRequired();

            // Serialized integration event JSON
            builder.Property(x => x.Payload)
                .HasColumnType("nvarchar(max)")
                .IsRequired();

            // Message creation timestamp
            builder.Property(x => x.CreatedOnUtc)
                .HasColumnType("datetime2")
                .IsRequired();

            // Null means the message is pending
            builder.Property(x => x.ProcessedOnUtc)
                .HasColumnType("datetime2")
                .IsRequired(false);

            // Optimize pending message retrieval
            builder.HasIndex(x => new
            {
                x.ProcessedOnUtc,
                x.CreatedOnUtc
            })
            .HasDatabaseName(
                "IX_OutboxMessages_ProcessedOnUtc_CreatedOnUtc");
        }
    }
}
