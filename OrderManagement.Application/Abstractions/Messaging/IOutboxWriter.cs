namespace OrderManagement.Application.Abstractions.Messaging
{
    public interface IOutboxWriter
    {
        Task AddAsync(Guid messageId, string eventType, string payload, CancellationToken cancellationToken = default);
    }
}
