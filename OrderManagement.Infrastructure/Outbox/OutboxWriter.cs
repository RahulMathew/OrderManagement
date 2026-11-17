using OrderManagement.Application.Abstractions.Messaging;
using OrderManagement.Infrastructure.Persistence.Repositories.Abstractions;

namespace OrderManagement.Infrastructure.Outbox
{
    public sealed class OutboxWriter : IOutboxWriter
    {
        private readonly IOutboxRepository _repository;

        public OutboxWriter(IOutboxRepository repository)
        {
            _repository = repository;
        }

        public async Task AddAsync(Guid messageId,
            string eventType,
            string payload,
            CancellationToken cancellationToken = default)
        {
            var message = OutboxMessage.Create(messageId,
                eventType,
                payload);

            await _repository.AddAsync(message, cancellationToken);

            // Do not call SaveChangesAsync here.
            // UnitOfWork commits the order and outbox together.
        }
    }
}
