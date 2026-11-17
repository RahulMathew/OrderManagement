namespace OrderManagement.Application.IntegrationEvents.Order
{
    public sealed record OrderCancelledIntegrationEvent(Guid OrderId,
        int CustomerId,
        DateTime OccurredOnUtc,
        string Reason
    );
}
