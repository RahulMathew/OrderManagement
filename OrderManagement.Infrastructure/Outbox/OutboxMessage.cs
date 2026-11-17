namespace OrderManagement.Infrastructure.Outbox
{
    public sealed class OutboxMessage
    {
        public Guid Id { get; private set; }
        public string EventType { get; private set; } = string.Empty;
        public string Payload { get; private set; } = string.Empty;
        public DateTime CreatedOnUtc { get; private set; }
        public DateTime? ProcessedOnUtc { get; private set; }

        // Required by Entity Framework Core
        private OutboxMessage()
        {

        }

        public static OutboxMessage Create(Guid messageId,
            string eventType,
            string payload)
        {
            if (messageId == Guid.Empty)
                throw new ArgumentException("Message ID cannot be empty.", nameof(messageId));

            ArgumentException.ThrowIfNullOrWhiteSpace(eventType);
            ArgumentException.ThrowIfNullOrWhiteSpace(payload);

            return new OutboxMessage
            {
                Id = messageId,
                EventType = eventType,
                Payload = payload,
                CreatedOnUtc = DateTime.UtcNow,
                ProcessedOnUtc = null
            };
        }

        public void MarkProcessed()
        {
            if (ProcessedOnUtc.HasValue)
                return;

            ProcessedOnUtc = DateTime.UtcNow;
        }
    }
}
