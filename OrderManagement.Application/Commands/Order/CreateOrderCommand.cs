using MediatR;
using OrderManagement.Domain.Common;

namespace OrderManagement.Application.Commands.Order
{
    public sealed record CreateOrderCommand(int CustomerId,
        IReadOnlyCollection<CreateOrderItemCommand> Items,
        string IdempotencyKey
    ) : IRequest<Result<Guid>>;
}
