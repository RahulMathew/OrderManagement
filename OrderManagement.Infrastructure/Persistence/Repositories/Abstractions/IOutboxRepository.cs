using OrderManagement.Infrastructure.Outbox;

namespace OrderManagement.Infrastructure.Persistence.Repositories.Abstractions
{
    public interface IOutboxRepository
    {
        Task AddAsync(OutboxMessage message, CancellationToken cancellationToken = default);
        Task<List<OutboxMessage>> GetPendingAsync(int batchSize, CancellationToken cancellationToken = default);
        Task MarkProcessedAsync(Guid messageId, CancellationToken cancellationToken = default);
    }
}
