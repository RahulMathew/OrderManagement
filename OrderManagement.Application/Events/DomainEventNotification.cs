using MediatR;
using OrderManagement.Domain.Events.Abstractions;

namespace OrderManagement.Application.Events
{
    public sealed record DomainEventNotification<TEvent>(
        TEvent DomainEvent
    ) : INotification where TEvent : IDomainEvent;
}