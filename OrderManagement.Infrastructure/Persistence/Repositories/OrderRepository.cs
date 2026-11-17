using Microsoft.EntityFrameworkCore;
using OrderManagement.Domain.Aggregates;
using OrderManagement.Domain.Repositories.Abstractions;

namespace OrderManagement.Infrastructure.Persistence.Repositories
{
    public sealed class OrderRepository : IOrderRepository
    {
        private readonly AppDbContext _context;

        public OrderRepository(AppDbContext context)
        {
            _context = context;
        }

        // Retrieve an order with its OrderItems
        public async Task<Order?> GetByIdAsync(Guid orderId, CancellationToken cancellationToken = default)
        {
            return await _context.Orders
                .Include(order => order.Items)
                .FirstOrDefaultAsync(
                    order => order.Id == orderId,
                    cancellationToken);
        }

        // Add a new order aggregate
        public async Task AddAsync(Order order,CancellationToken cancellationToken = default)
        {
            await _context.Orders.AddAsync(order, cancellationToken);
        }

        // Explicitly mark a detached order as modified
        public void Update(Order order)
        {
            _context.Orders.Update(order);
        }

        // Mark an order for physical deletion
        public void Remove(Order order)
        {
            _context.Orders.Remove(order);
        }
    }
}
