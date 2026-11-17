namespace OrderManagement.Application.IntegrationEvents.Order
{
    public sealed record OrderCreatedIntegrationEvent(Guid OrderId,
        int CustomerId,
        DateTime OccurredOnUtc
    );
}