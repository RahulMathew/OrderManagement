using OrderManagement.Domain.Events.Abstractions;

namespace OrderManagement.Application.Abstractions.Events
{
    public interface IDomainEventDispatcher
    {
        Task DispatchAsync(IDomainEvent domainEvent, CancellationToken cancellationToken = default);
    }
}
