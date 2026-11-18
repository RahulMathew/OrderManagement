using Microsoft.EntityFrameworkCore;
using OrderManagement.Domain.Aggregates;
using OrderManagement.Domain.Repositories.Abstractions;

namespace OrderManagement.Infrastructure.Persistence.Repositories
{
    public sealed class OrderRepository : BaseRepository<Order>, IOrderRepository
    {
        public OrderRepository(AppDbContext context) : base(context)
        {

        }

        // Retrieve an order with its OrderItems
        public async Task<Order?> GetOrderWithItemsAsync(Guid orderId, CancellationToken cancellationToken = default)
        {
            return await Context.Orders
                .Include(order => order.Items)
                .FirstOrDefaultAsync(order => order.Id == orderId, cancellationToken);
        }
    }
}
