using OrderManagement.Domain.Events.Abstractions;

namespace OrderManagement.Domain.Events.Order
{
    public sealed record OrderCreatedEvent(Guid OrderId, 
        int CustomerId, 
        DateTime OccurredOnUtc
    ) : IDomainEvent;
}
