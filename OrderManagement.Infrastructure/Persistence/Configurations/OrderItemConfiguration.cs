using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OrderManagement.Domain.Entities;

namespace OrderManagement.Infrastructure.Persistence.Configurations
{
    public sealed class OrderItemConfiguration : IEntityTypeConfiguration<OrderItem>
    {
        public void Configure(EntityTypeBuilder<OrderItem> builder)
        {
            // Table name
            builder.ToTable("OrderItems");

            // Primary key
            builder.HasKey(x => x.Id);

            builder.Property(x => x.Id)
                .ValueGeneratedNever();

            // Foreign key to Orders
            builder.Property(x => x.OrderId)
                .IsRequired();

            // Product ID
            builder.Property(x => x.ProductId)
                .IsRequired();

            // Quantity
            builder.Property(x => x.Quantity)
                .IsRequired();

            // Unit price
            builder.Property(x => x.UnitPrice)
                .HasPrecision(18, 2)
                .IsRequired();

            // Calculated property - not stored in database
            builder.Ignore(x => x.TotalPrice);

            // Index for retrieving items by OrderId
            builder.HasIndex(x => x.OrderId);
        }
    }
}
