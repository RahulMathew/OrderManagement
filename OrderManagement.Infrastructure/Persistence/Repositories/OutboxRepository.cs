using Microsoft.EntityFrameworkCore;
using OrderManagement.Domain.Common;
using OrderManagement.Infrastructure.Outbox;
using OrderManagement.Infrastructure.Persistence.Repositories.Abstractions;

namespace OrderManagement.Infrastructure.Persistence.Repositories
{
    public sealed class OutboxRepository : BaseRepository<OutboxMessage>, IOutboxRepository
    {
        public OutboxRepository(AppDbContext context) : base(context)
        {
            
        }

        public async Task<List<OutboxMessage>> GetPendingAsync(int batchSize, CancellationToken cancellationToken = default)
        {
            return await Context.OutboxMessages
                .Where(x => x.ProcessedOnUtc == null)
                .OrderBy(x => x.CreatedOnUtc)
                .ThenBy(x => x.Id)
                .Take(batchSize)
                .ToListAsync(cancellationToken);
        }

        public async Task<Result> MarkProcessedAsync(Guid messageId, CancellationToken cancellationToken = default)
        {
            var message = await GetByIdAsync(messageId, cancellationToken);

            if (message is null)
                return Result.Failure(new Error("Outbox.NotFound", $"Outbox message {messageId} not found."));

            if (message.ProcessedOnUtc is not null)
                return Result.Success();

            message.MarkProcessed();

            // EF Core tracks the entity.
            // UnitOfWork commits the change.

            return Result.Success();
        }
    }
}
