using OrderManagement.Domain.Common;

namespace OrderManagement.Domain.Entities
{
    public sealed class OrderItem
    {
        public Guid Id { get; private set; }
        public Guid OrderId { get; private set; }
        public int ProductId { get; private set; }
        public int Quantity { get; private set; }
        public decimal UnitPrice { get; private set; }
        public decimal TotalPrice => Quantity * UnitPrice;

        // Required for EF Core materialization
        private OrderItem()
        {

        }

        // Only the Domain assembly can create OrderItems
        internal static Result<OrderItem> Create(Guid orderId,
            int productId,
            int quantity,
            decimal unitPrice)
        {
            if (orderId == Guid.Empty)
                return Result<OrderItem>.Failure(new Error("OrderItem.InvalidOrder", "Order ID cannot be empty."));

            if (productId <= 0)
                return Result<OrderItem>.Failure(new Error("OrderItem.InvalidProduct", "Product ID must be greater than zero."));

            if (quantity <= 0)
                return Result<OrderItem>.Failure(new Error("OrderItem.InvalidQuantity","Quantity must be greater than zero."));

            if (unitPrice < 0)
                return Result<OrderItem>.Failure(new Error("OrderItem.InvalidPrice", "Unit price cannot be negative."));

            var orderItem = new OrderItem
            {
                Id = Guid.NewGuid(),
                OrderId = orderId,
                ProductId = productId,
                Quantity = quantity,
                UnitPrice = unitPrice
            };

            return Result<OrderItem>.Success(orderItem);
        }

        // Called by Order aggregate root
        internal Result UpdateQuantity(int quantity)
        {
            if (quantity <= 0)
            {
                return Result.Failure(new Error("OrderItem.InvalidQuantity", "Quantity must be greater than zero."));
            }

            Quantity = quantity;

            return Result.Success();
        }
    }
}
