using Microsoft.EntityFrameworkCore;
using OrderManagement.Domain.Aggregates;
using OrderManagement.Domain.Entities;
using OrderManagement.Infrastructure.Outbox;

namespace OrderManagement.Infrastructure.Persistence
{
    public sealed class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options): base(options)
        {

        }

        // Order aggregate root
        public DbSet<Order> Orders => Set<Order>();

        // Child entity of Order aggregate
        public DbSet<OrderItem> OrderItems => Set<OrderItem>();

        // Transactional Outbox messages
        public DbSet<OutboxMessage> OutboxMessages => Set<OutboxMessage>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Automatically register all IEntityTypeConfiguration<T>
            // implementations in the Infrastructure assembly.
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
        }
    }
}
