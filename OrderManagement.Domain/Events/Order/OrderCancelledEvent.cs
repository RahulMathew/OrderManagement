using OrderManagement.Domain.Events.Abstractions;

namespace OrderManagement.Domain.Events.Order
{
    public sealed record OrderCancelledEvent(Guid OrderId,
        int CustomerId,
        DateTime OccurredOnUtc,
        string Reason
    ) : IDomainEvent;
}
