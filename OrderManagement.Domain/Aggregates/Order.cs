using OrderManagement.Domain.Common;
using OrderManagement.Domain.Entities;
using OrderManagement.Domain.Events.Order;

namespace OrderManagement.Domain.Aggregates
{
    public sealed class Order : BaseEntity
    {
        private readonly List<OrderItem> _items = new();

        public Guid Id { get; private set; }
        public int CustomerId { get; private set; }
        public IReadOnlyCollection<OrderItem> Items=> _items.AsReadOnly();

        // Indicates whether creation has been finalized
        public bool IsCreated { get; private set; }

        private Order() { }

        public static Result<Order> Create(int customerId)
        {
            if (customerId <= 0)
                return Result<Order>.Failure(new Error("Order.InvalidCustomer","Customer ID must be positive."));

            var order = new Order
            {
                Id = Guid.NewGuid(),
                CustomerId = customerId
            };

            return Result<Order>.Success(order);
        }

        public Result AddItem(int productId,
            int quantity,
            decimal unitPrice)
        {
            if (IsCreated)
                return Result.Failure(new Error("Order.AlreadyCreated", "Cannot add items after creation."));

            var itemResult = OrderItem.Create(Id,
                productId,
                quantity,
                unitPrice);

            if (itemResult.IsFailure)
                return Result.Failure(itemResult.Error!);

            _items.Add(itemResult.Value);

            return Result.Success();
        }

        public Result UpdateItemQuantity(Guid orderItemId, int quantity)
        {
            var item = _items.FirstOrDefault(
                x => x.Id == orderItemId);

            if (item is null)
                return Result.Failure(new Error("Order.ItemNotFound", "Order item was not found."));

            return item.UpdateQuantity(quantity);
        }

        public Result MarkCreated()
        {
            if (IsCreated)
                return Result.Failure(new Error("Order.AlreadyCreated","Order has already been created."));

            if (_items.Count == 0)
                return Result.Failure(new Error("Order.NoItems", "Order must contain at least one item."));

            IsCreated = true;

            AddDomainEvent(new OrderCreatedEvent(Id,CustomerId, DateTime.UtcNow));

            return Result.Success();
        }
    }
}
