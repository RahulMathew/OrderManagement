using MediatR;
using OrderManagement.Application.DTO;
using OrderManagement.Application.Mappings;
using OrderManagement.Domain.Common;
using OrderManagement.Domain.Repositories.Abstractions;

namespace OrderManagement.Application.Queries.Order
{
    public sealed class GetOrderByIdQueryHandler : IRequestHandler<GetOrderByIdQuery, Result<OrderDto>>
    {
        private readonly IOrderRepository _orderRepository;

        public GetOrderByIdQueryHandler(IOrderRepository orderRepository)
        {
            _orderRepository = orderRepository;
        }

        public async Task<Result<OrderDto>> Handle(GetOrderByIdQuery request, CancellationToken cancellationToken)
        {
            // Step 1: Retrieve Order aggregate
            var order = await _orderRepository.GetByIdAsync(request.OrderId, cancellationToken);

            // Step 2: Handle order not found
            if (order is null)
                return Result<OrderDto>.Failure(new Error("Order.NotFound", "Order was not found."));

            // Step 3: Map Domain Aggregate to DTO
            var orderDto = OrderMapping.ToDto(order);

            // Step 4: Return DTO
            return Result<OrderDto>.Success(orderDto);
        }
    }
}
