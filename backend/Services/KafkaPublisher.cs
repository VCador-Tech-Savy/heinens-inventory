using Confluent.Kafka;
using InventoryApi.Messages;
using System.Text.Json;

namespace InventoryApi.Services;

public class KafkaPublisher
{
    private readonly IProducer<string, string>? _producer;
    private readonly ILogger<KafkaPublisher> _logger;
    private readonly string _topic;
    private readonly bool _isEnabled;

    public KafkaPublisher(IConfiguration config,
        ILogger<KafkaPublisher> logger)
    {
        _logger = logger;
        _topic = config["Kafka:Topic"] ?? "product-events";
        var bootstrapServers = config["Kafka:BootstrapServers"];

        if (string.IsNullOrEmpty(bootstrapServers)
            || bootstrapServers == "localhost:9092")
        {
            _isEnabled = false;
            _logger.LogWarning(
                "Kafka not configured — running in local mode");
            return;
        }

        _isEnabled = true;
        var producerConfig = new ProducerConfig
        {
            BootstrapServers = bootstrapServers
        };
        _producer = new ProducerBuilder<string, string>(
            producerConfig).Build();
    }

    public async Task PublishProductEventAsync(ProductEvent evt)
    {
        if (!_isEnabled)
        {
            _logger.LogInformation(
                "[LOCAL KAFKA] {EventType}: {ProductName} " +
                "qty={Quantity} at {Time}",
                evt.EventType,
                evt.ProductName,
                evt.Quantity,
                evt.OccurredAt);
            return;
        }

        var json = JsonSerializer.Serialize(evt);
        await _producer!.ProduceAsync(_topic,
            new Message<string, string>
            {
                Key = evt.ProductId.ToString(),
                Value = json
            });

        _logger.LogInformation(
            "Published {EventType} for {ProductName} to Kafka",
            evt.EventType, evt.ProductName);
    }
}