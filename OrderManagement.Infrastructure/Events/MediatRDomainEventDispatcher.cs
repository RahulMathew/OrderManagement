using MediatR;
using OrderManagement.Application.Abstractions.Events;
using OrderManagement.Application.Events;
using OrderManagement.Domain.Events.Abstractions;

namespace OrderManagement.Infrastructure.Events
{
    public sealed class MediatRDomainEventDispatcher : IDomainEventDispatcher
    {
        private readonly IPublisher _publisher;

        public MediatRDomainEventDispatcher(IPublisher publisher)
        {
            _publisher = publisher;
        }

        public async Task DispatchAsync(IDomainEvent domainEvent, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(domainEvent);

            var notificationType = typeof(DomainEventNotification<>)
                .MakeGenericType(domainEvent.GetType());

            var notification = (INotification)Activator.CreateInstance(
                notificationType,
                domainEvent)!;

            await _publisher.Publish(
                notification,
                cancellationToken);
        }
    }
}
