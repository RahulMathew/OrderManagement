namespace OrderManagement.Application.DTO
{
    public sealed record OrderItemDto(Guid Id,
        int ProductId,
        int Quantity,
        decimal UnitPrice,
        decimal TotalPrice
    );
}
