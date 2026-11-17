namespace OrderManagement.Infrastructure.Messaging.Kafka.Abstractions
{
    public interface IKafkaPublisher
    {
        Task PublishAsync(Guid messageId, string eventType, string payload, CancellationToken cancellationToken = default);
    }
}
