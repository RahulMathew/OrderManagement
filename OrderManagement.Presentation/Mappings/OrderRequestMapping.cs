using OrderManagement.Application.Commands.Order;
using OrderManagement.Presentation.Requests;

namespace OrderManagement.Presentation.Mappings
{
    public static class OrderRequestMapping
    {
        public static CreateOrderCommand ToCommand(this CreateOrderRequest request, string idempotencyKey)
        {
            var command = new CreateOrderCommand(
                request.CustomerId,
                request.Items.Select(item =>
                    new CreateOrderItemCommand(
                        item.ProductId,
                        item.Quantity,
                        item.UnitPrice))
                    .ToList(),
                idempotencyKey);
            return command;
        }
    }
}
