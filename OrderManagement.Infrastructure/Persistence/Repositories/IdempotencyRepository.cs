using Microsoft.EntityFrameworkCore;
using OrderManagement.Application.Abstractions.Persistence;
using OrderManagement.Infrastructure.Idempotency;

namespace OrderManagement.Infrastructure.Persistence.Repositories
{
    public sealed class IdempotencyRepository : IIdempotencyRepository
    {
        private readonly AppDbContext _context;

        public IdempotencyRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IdempotencyEntry?> GetByKeyAsync(string key, CancellationToken cancellationToken = default)
        {
            return await _context.Set<IdempotencyRecord>()
                .AsNoTracking()
                .Where(x => x.Key == key)
                .Select(x => new IdempotencyEntry(
                    x.RequestHash,
                    x.OrderId))
                .FirstOrDefaultAsync(cancellationToken);
        }

        public async Task AddAsync(string key,
            string requestHash,
            Guid orderId,
            CancellationToken cancellationToken = default)
        {
            var record = IdempotencyRecord.Create(key,
                requestHash,
                orderId);

            await _context.Set<IdempotencyRecord>()
                .AddAsync(record, cancellationToken);
        }
    }
}
