namespace OrderManagement.Application.Abstractions.Persistence
{
    public interface IIdempotencyRepository
    {
        Task<IdempotencyEntry?> GetByKeyAsync(string key, CancellationToken cancellationToken = default);
        Task AddAsync(string key, string requestHash, Guid orderId, CancellationToken cancellationToken = default);
    }
}
