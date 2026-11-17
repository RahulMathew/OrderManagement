using MediatR;
using OrderManagement.Application.DTO;
using OrderManagement.Domain.Common;

namespace OrderManagement.Application.Queries.Order
{
    public sealed record GetOrderByIdQuery(Guid OrderId) : IRequest<Result<OrderDto>>;
}
