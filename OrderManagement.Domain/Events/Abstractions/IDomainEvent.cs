namespace OrderManagement.Domain.Events.Abstractions
{
    public interface IDomainEvent
    {
        DateTime OccurredOnUtc { get; }
    }
}
