namespace OrderManagement.Application.DTO
{
    public sealed record OrderDto(Guid Id,
        int CustomerId,
        IReadOnlyCollection<OrderItemDto> Items
    );
}
