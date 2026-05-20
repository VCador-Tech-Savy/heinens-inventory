using Confluent.Kafka;
using InventoryApi.Messages;
using System.Text.Json;

namespace InventoryApi.Workers;

public class KafkaAnalyticsConsumer : BackgroundService
{
    private readonly IConfiguration _config;
    private readonly ILogger<KafkaAnalyticsConsumer> _logger;
    private readonly bool _isEnabled;

    public KafkaAnalyticsConsumer(IConfiguration config,
        ILogger<KafkaAnalyticsConsumer> logger)
    {
        _config = config;
        _logger = logger;
        var servers = config["Kafka:BootstrapServers"];
        _isEnabled = !string.IsNullOrEmpty(servers)
            && servers != "localhost:9092";
    }

    protected override async Task ExecuteAsync(
        CancellationToken stoppingToken)
    {
        if (!_isEnabled)
        {
            _logger.LogInformation(
                "[LOCAL KAFKA] AnalyticsConsumer waiting " +
                "— no Kafka broker configured");
            while (!stoppingToken.IsCancellationRequested)
                await Task.Delay(5000, stoppingToken);
            return;
        }

        var config = new ConsumerConfig
        {
            BootstrapServers = _config["Kafka:BootstrapServers"],
            GroupId = "analytics-consumer-group",
            AutoOffsetReset = AutoOffsetReset.Earliest
        };

        using var consumer = new ConsumerBuilder<string, string>(
            config).Build();
        consumer.Subscribe(_config["Kafka:Topic"]);

        while (!stoppingToken.IsCancellationRequested)
        {
            var result = consumer.Consume(stoppingToken);
            var evt = JsonSerializer
                .Deserialize<ProductEvent>(result.Message.Value);

            _logger.LogInformation(
                "[ANALYTICS] Event={EventType} " +
                "Product={ProductName} " +
                "Category={Category} Qty={Quantity}",
                evt?.EventType,
                evt?.ProductName,
                evt?.Category,
                evt?.Quantity);
        }
    }
}