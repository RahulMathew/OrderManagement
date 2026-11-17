using OrderManagement.Application.DTO;
using OrderManagement.Domain.Aggregates;

namespace OrderManagement.Application.Mappings
{
    public static class OrderMapping
    {
        public static OrderDto ToDto(Order order)
        {
            return new OrderDto(
                order.Id,
                order.CustomerId,
                order.Items.Select(item =>
                    new OrderItemDto(
                        item.Id,
                        item.ProductId,
                        item.Quantity,
                        item.UnitPrice,
                        item.TotalPrice
                    )).ToList());
        }
    }
}