namespace OrderManagement.Presentation.Requests
{
    public sealed record CreateOrderItemRequest(int ProductId,
        int Quantity,
        decimal UnitPrice);
}
