using MediatR;
using OrderManagement.Domain.Common;
using OrderManagement.Domain.Repositories.Abstractions;
using OrderManagement.Application.Abstractions.Persistence;
using OrderManagement.Application.Idempotency.Order;

namespace OrderManagement.Application.Commands.Order
{
    public sealed class CreateOrderCommandHandler: IRequestHandler<CreateOrderCommand, Result<Guid>>
    {
        private readonly IOrderRepository _orderRepository;
        private readonly IIdempotencyRepository _idempotencyRepository;
        private readonly IUnitOfWork _unitOfWork;

        public CreateOrderCommandHandler(IOrderRepository orderRepository, 
            IIdempotencyRepository idempotencyRepository,
            IUnitOfWork unitOfWork)
        {
            _orderRepository = orderRepository;
            _idempotencyRepository = idempotencyRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<Result<Guid>> Handle(CreateOrderCommand request, CancellationToken cancellationToken)
        {
            // STEP 1: Validate idempotency key
            if (string.IsNullOrWhiteSpace(request.IdempotencyKey))
                return Result<Guid>.Failure(new Error("Idempotency.KeyRequired", "Idempotency key is required."));

            // STEP 2: Hash the order request
            var requestHash = OrderRequestHasher.Compute(request);

            // STEP 3: Check whether key already exists
            var existing = await _idempotencyRepository.GetByKeyAsync(request.IdempotencyKey, cancellationToken);

            if (existing is not null)
            {
                // Same key, different request
                if (existing.RequestHash != requestHash)
                    return Result<Guid>.Failure(new Error("Idempotency.Conflict", "Key already used with different order data."));

                // Same key, same request
                // Return existing OrderId without creating another order
                return Result<Guid>.Success(existing.OrderId);
            }

            // STEP 4: Create new order
            var orderCreationResult = Domain.Aggregates.Order.Create(request.CustomerId);

            if (orderCreationResult.IsFailure)
                return Result<Guid>.Failure(orderCreationResult.Error!);

            var order = orderCreationResult.Value;

            foreach (var item in request.Items)
            {
                var itemResult = order.AddItem(item.ProductId,
                    item.Quantity,
                    item.UnitPrice);

                if (itemResult.IsFailure)
                    return Result<Guid>.Failure(itemResult.Error!);
            }

            var createdResult = order.MarkCreated();

            if (createdResult.IsFailure)
                return Result<Guid>.Failure(createdResult.Error!);

            // STEP 5: Stage the order
            await _orderRepository.AddAsync(order, cancellationToken);

            // STEP 6: Stage the idempotency key and OrderId
            await _idempotencyRepository.AddAsync(request.IdempotencyKey,
                requestHash,
                order.Id,
                cancellationToken);

            // STEP 7: Save all changes together
            // Order + IdempotencyRecord + OutboxMessage
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Result<Guid>.Success(order.Id);
        }
    }
}
