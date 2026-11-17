using OrderManagement.Domain.Aggregates;

namespace OrderManagement.Domain.Repositories.Abstractions
{
    public interface IOrderRepository
    {
        Task<Order?> GetByIdAsync(Guid orderId, CancellationToken cancellationToken = default);
        Task AddAsync(Order order, CancellationToken cancellationToken = default);
        void Update(Order order);
        void Remove(Order order);
    }
}
