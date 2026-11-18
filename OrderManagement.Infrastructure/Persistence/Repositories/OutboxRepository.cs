using Microsoft.EntityFrameworkCore;
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

        public async Task MarkProcessedAsync(Guid messageId, CancellationToken cancellationToken = default)
        {
            var message = await Context.OutboxMessages.FirstOrDefaultAsync(x => x.Id == messageId, cancellationToken);

            if (message is null)
            {
                throw new InvalidOperationException($"Outbox message {messageId} not found.");
            }

            message.MarkProcessed();

            // No SaveChangesAsync here.
            // UnitOfWork will commit the change.
        }
    }
}
