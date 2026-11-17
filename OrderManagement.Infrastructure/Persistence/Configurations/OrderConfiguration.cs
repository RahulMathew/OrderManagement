using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OrderManagement.Domain.Aggregates;

namespace OrderManagement.Infrastructure.Persistence.Configurations
{
    public sealed class OrderConfiguration : IEntityTypeConfiguration<Order>
    {
        public void Configure(EntityTypeBuilder<Order> builder)
        {
            // Table name
            builder.ToTable("Orders");

            // Primary key
            builder.HasKey(x => x.Id);

            builder.Property(x => x.Id)
                .ValueGeneratedNever();

            // CustomerId
            builder.Property(x => x.CustomerId)
                .IsRequired();

            // Order -> OrderItems (One-to-Many)
            builder.HasMany(x => x.Items)
                .WithOne()
                .HasForeignKey(x => x.OrderId)
                .OnDelete(DeleteBehavior.Cascade);

            // Use private _items backing field
            builder.Navigation(x => x.Items)
                .HasField("_items")
                .UsePropertyAccessMode(
                    PropertyAccessMode.Field);

            // Index for searching orders by customer
            builder.HasIndex(x => x.CustomerId);

            // Domain events are not database columns
            builder.Ignore(x => x.DomainEvents);
        }
    }
}
