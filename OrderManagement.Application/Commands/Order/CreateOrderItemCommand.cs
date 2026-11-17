namespace OrderManagement.Application.Commands.Order
{
    public record CreateOrderItemCommand(int ProductId, int Quantity, decimal UnitPrice);
}
