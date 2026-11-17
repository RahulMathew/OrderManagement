namespace OrderManagement.Infrastructure.Idempotency
{
    public sealed class IdempotencyRecord
    {
        public Guid Id { get; private set; }
        public string Key { get; private set; } = string.Empty;
        public string RequestHash { get; private set; } = string.Empty;
        public Guid OrderId { get; private set; }
        public DateTime CreatedOnUtc { get; private set; }

        private IdempotencyRecord() { }

        public static IdempotencyRecord Create(string key,
            string requestHash,
            Guid orderId)
        {
            return new IdempotencyRecord
            {
                Id = Guid.NewGuid(),
                Key = key,
                RequestHash = requestHash,
                OrderId = orderId,
                CreatedOnUtc = DateTime.UtcNow
            };
        }
    }
}


