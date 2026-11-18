using OrderManagement.Domain.Repositories.Abstractions;
using OrderManagement.Infrastructure.Outbox;

namespace OrderManagement.Infrastructure.Persistence.Repositories.Abstractions
{
    public interface IOutboxRepository : IBaseRepository<OutboxMessage>
    {
        Task<List<OutboxMessage>> GetPendingAsync(int batchSize, CancellationToken cancellationToken = default);
        Task MarkProcessedAsync(Guid messageId, CancellationToken cancellationToken = default);
    }
}
