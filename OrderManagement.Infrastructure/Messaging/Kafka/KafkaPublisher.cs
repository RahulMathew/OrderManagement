using Confluent.Kafka;
using Microsoft.Extensions.Configuration;
using OrderManagement.Infrastructure.Messaging.Kafka.Abstractions;

namespace OrderManagement.Infrastructure.Messaging.Kafka
{
    public sealed class KafkaPublisher : IKafkaPublisher, IDisposable
    {
        private readonly IProducer<string, string> _producer;
        private readonly string _topic;

        public KafkaPublisher(IConfiguration configuration)
        {
            var bootstrapServers =
                configuration["Kafka:BootstrapServers"]
                ?? throw new InvalidOperationException(
                    "Kafka bootstrap servers are missing.");

            _topic = configuration["Kafka:Topic"]
                ?? throw new InvalidOperationException(
                    "Kafka topic is missing.");

            var config = new ProducerConfig
            {
                BootstrapServers = bootstrapServers,
                Acks = Acks.All,
                EnableIdempotence = true
            };

            _producer = new ProducerBuilder<string, string>(
                config).Build();
        }

        public async Task PublishAsync(Guid messageId,
            string eventType,
            string payload,
            CancellationToken cancellationToken = default)
        {
            var kafkaMessage = new Message<string, string>
            {
                Key = messageId.ToString(),
                Value = payload,

                Headers = new Headers
                {
                    {
                        "event-type",
                        System.Text.Encoding.UTF8.GetBytes(eventType)
                    }
                }
            };

            await _producer.ProduceAsync(_topic, kafkaMessage, cancellationToken);
        }

        public void Dispose()
        {
            _producer.Dispose();
        }
    }
}
