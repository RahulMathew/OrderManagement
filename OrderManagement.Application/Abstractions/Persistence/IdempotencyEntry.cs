namespace OrderManagement.Application.Abstractions.Persistence
{
    public sealed record IdempotencyEntry(string RequestHash, Guid OrderId);
}
