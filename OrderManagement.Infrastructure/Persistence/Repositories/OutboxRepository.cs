using Microsoft.EntityFrameworkCore;
using OrderManagement.Infrastructure.Outbox;
using OrderManagement.Infrastructure.Persistence.Repositories.Abstractions;

namespace OrderManagement.Infrastructure.Persistence.Repositories
{
    public sealed class OutboxRepository : IOutboxRepository
    {
        private readonly AppDbContext _context;

        public OutboxRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task AddAsync(OutboxMessage message, CancellationToken cancellationToken = default)
        {
            await _context.OutboxMessages.AddAsync(
                message,
                cancellationToken);
        }

        public async Task<List<OutboxMessage>> GetPendingAsync(
            int batchSize,
            CancellationToken cancellationToken = default)
        {
            return await _context.OutboxMessages
                .Where(x => x.ProcessedOnUtc == null)
                .OrderBy(x => x.CreatedOnUtc)
                .ThenBy(x => x.Id)
                .Take(batchSize)
                .ToListAsync(cancellationToken);
        }

        public async Task MarkProcessedAsync(Guid messageId,CancellationToken cancellationToken = default)
        {
            var message = await _context.OutboxMessages
                .FirstOrDefaultAsync(
                    x => x.Id == messageId,
                    cancellationToken);

            if (message is null)
            {
                throw new InvalidOperationException(
                    $"Outbox message {messageId} not found.");
            }

            message.MarkProcessed();

            // No SaveChangesAsync here.
            // UnitOfWork will commit the change.
        }
    }
}
