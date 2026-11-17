using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OrderManagement.Application.Abstractions.Events;
using OrderManagement.Application.Abstractions.Messaging;
using OrderManagement.Application.Abstractions.Persistence;
using OrderManagement.Domain.Repositories.Abstractions;
using OrderManagement.Infrastructure.Events;
using OrderManagement.Infrastructure.Messaging.Kafka;
using OrderManagement.Infrastructure.Messaging.Kafka.Abstractions;
using OrderManagement.Infrastructure.Outbox;
using OrderManagement.Infrastructure.Persistence;
using OrderManagement.Infrastructure.Persistence.Repositories;
using OrderManagement.Infrastructure.Persistence.Repositories.Abstractions;

namespace OrderManagement.Infrastructure.DIRegistrations
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services,
            IConfiguration configuration)
        {
            //// 1. Entity Framework Core - SQL Server
            //var connectionString = configuration.GetConnectionString("DefaultConnection");

            //if (string.IsNullOrWhiteSpace(connectionString))
            //{
            //    throw new InvalidOperationException(
            //        "DefaultConnection is missing from configuration.");
            //}

            //services.AddDbContext<AppDbContext>(options => options.UseSqlServer(connectionString));

            // 1. EF Core InMemory Database (Demonstration purposes)
            services.AddDbContext<AppDbContext>(options =>
                options.UseInMemoryDatabase("OrderManagementDb"));

            // 2. Domain repositories
            services.AddScoped<IOrderRepository, OrderRepository>();

            // 3. Idempotency repository
            services.AddScoped<IIdempotencyRepository, IdempotencyRepository>();

            // 4. Outbox repository
            services.AddScoped<IOutboxRepository, OutboxRepository>();

            // 5. Unit of Work
            services.AddScoped<IUnitOfWork, UnitOfWork>();

            // 6. Domain event dispatcher
            services.AddScoped<IDomainEventDispatcher, MediatRDomainEventDispatcher>();

            // 7. Outbox writer
            services.AddScoped<IOutboxWriter, OutboxWriter>();

            // 8. Kafka publisher
            services.AddSingleton<IKafkaPublisher, KafkaPublisher>();

            // 9. Background outbox processor
            services.AddHostedService<OutboxProcessor>();

            return services;
        }
    }
}
