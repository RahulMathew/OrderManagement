using OrderManagement.Application.Common.Hasher;
using OrderManagement.Application.Commands.Order;

namespace OrderManagement.Application.Idempotency
{
    public static class OrderRequestHasher
    {
        public static string Compute(CreateOrderCommand command)
        {
            ArgumentNullException.ThrowIfNull(command);

            var request = new
            {
                command.CustomerId,

                Items = command.Items.Select(item => new
                {
                    item.ProductId,
                    item.Quantity,
                    item.UnitPrice
                }).ToArray()
            };

            return RequestHasher.Compute(request);
        }
    }
}
