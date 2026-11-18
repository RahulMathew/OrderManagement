using OrderManagement.Domain.Aggregates;

namespace OrderManagement.Domain.Repositories.Abstractions
{
    public interface IOrderRepository : IBaseRepository<Order>
    {
        Task<Order?> GetOrderWithItemsAsync(Guid orderId, CancellationToken cancellationToken = default);
    }
}
