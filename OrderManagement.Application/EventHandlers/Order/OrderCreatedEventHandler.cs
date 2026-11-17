using System.Text.Json;
using MediatR;
using OrderManagement.Domain.Events.Order;
using OrderManagement.Application.Abstractions.Messaging;
using OrderManagement.Application.Events;
using OrderManagement.Application.IntegrationEvents.Order;

namespace OrderManagement.Application.EventHandlers.Order
{
    public sealed class OrderCreatedEventHandler: INotificationHandler<DomainEventNotification<OrderCreatedEvent>>
    {
        private readonly IOutboxWriter _outboxWriter;

        public OrderCreatedEventHandler(IOutboxWriter outboxWriter)
        {
            _outboxWriter = outboxWriter;
        }

        public async Task Handle(DomainEventNotification<OrderCreatedEvent> notification, CancellationToken cancellationToken)
        {
            // Step 1: Retrieve the domain event
            var domainEvent = notification.DomainEvent;

            // Step 2: Create the integration event
            var integrationEvent =
                new OrderCreatedIntegrationEvent(
                    domainEvent.OrderId,
                    domainEvent.CustomerId,
                    domainEvent.OccurredOnUtc);

            // Step 3: Serialize the integration event
            var payload = JsonSerializer.Serialize(integrationEvent);

            // Step 4: Add message to the Outbox
            await _outboxWriter.AddAsync(Guid.NewGuid(),
                nameof(OrderCreatedIntegrationEvent),
                payload,
                cancellationToken);

            // Do not call SaveChangesAsync here.
            // Do not publish directly to Kafka.
        }
    }
}
