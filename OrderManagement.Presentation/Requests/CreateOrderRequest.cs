namespace OrderManagement.Presentation.Requests
{
    public sealed record CreateOrderRequest(int CustomerId, IReadOnlyCollection<CreateOrderItemRequest> Items);
}
