using Azure.Messaging.ServiceBus;
using InventoryApi.Messages;
using System.Text.Json;

namespace InventoryApi.Services;

public class ServiceBusPublisher
{
    private readonly ServiceBusClient? _client;
    private readonly ServiceBusSender? _sender;
    private readonly ILogger<ServiceBusPublisher> _logger;
    private readonly bool _isEnabled;

    public ServiceBusPublisher(IConfiguration config, ILogger<ServiceBusPublisher> logger)
    {
        _logger = logger;
        var connectionString = config["ServiceBus:ConnectionString"];
        var queueName = config["ServiceBus:QueueName"] ?? "out-of-stock";

        if(string.IsNullOrEmpty(connectionString))
        {
            _isEnabled = false;
            _logger.LogWarning("Service Bus not configured - running in local mode");
            return;
        }

        _isEnabled = true;
        _client = new ServiceBusClient(connectionString);
        _sender = _client.CreateSender(queueName);
    }

    public async Task PublishOutOfStockAsync(OutOfStockEvent evt)
    {
        if(!_isEnabled)
        {
            _logger.LogInformation("[LOCAL] OutOfStockEvent: {ProductName} at {Time}", evt.ProductName, evt.OccurredAt);
            return;
        }

        var json = JsonSerializer.Serialize(evt);
        var message = new ServiceBusMessage(json)
        {
            ContentType = "application/json",
            Subject = "OutOfStockEvent"
        };

        await _sender!.SendMessageAsync(message);
        _logger.LogInformation("Published OutOfStockEvent for {ProductName} to Service Bus", evt.ProductName);
    }
}