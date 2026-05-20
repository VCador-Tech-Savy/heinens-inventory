using Azure.Messaging.ServiceBus;
using InventoryApi.Messages;
using System.Text.Json;

namespace InventoryApi.Workers;

public class OutOfStockWorker : BackgroundService
{
    private readonly IConfiguration _config;
    private readonly ILogger<OutOfStockWorker> _logger;
    private ServiceBusProcessor? _processor;
    private readonly bool _isEnabled;

    public OutOfStockWorker(IConfiguration config,
        ILogger<OutOfStockWorker> logger)
    {
        _config = config;
        _logger = logger;
        _isEnabled = !string.IsNullOrEmpty(
            config["ServiceBus:ConnectionString"]);
    }

    protected override async Task ExecuteAsync(
        CancellationToken stoppingToken)
    {
        if (!_isEnabled)
        {
            _logger.LogInformation(
                "[LOCAL] OutOfStockWorker running — " +
                "no Service Bus configured, waiting for events...");

            while (!stoppingToken.IsCancellationRequested)
            {
                await Task.Delay(5000, stoppingToken);
            }
            return;
        }

        var connectionString = _config["ServiceBus:ConnectionString"];
        var queueName = _config["ServiceBus:QueueName"] 
            ?? "out-of-stock";

        var client = new ServiceBusClient(connectionString);
        _processor = client.CreateProcessor(queueName);

        _processor.ProcessMessageAsync += async args =>
        {
            var body = args.Message.Body.ToString();
            var evt = JsonSerializer
                .Deserialize<OutOfStockEvent>(body);

            _logger.LogWarning(
                "ALERT: {ProductName} (Category: {Category}) " +
                "is OUT OF STOCK at {Time}",
                evt?.ProductName,
                evt?.Category,
                evt?.OccurredAt);

            await args.CompleteMessageAsync(args.Message);
        };

        _processor.ProcessErrorAsync += args =>
        {
            _logger.LogError(
                "Service Bus error: {Error}",
                args.Exception.Message);
            return Task.CompletedTask;
        };

        await _processor.StartProcessingAsync(stoppingToken);

        while (!stoppingToken.IsCancellationRequested)
        {
            await Task.Delay(1000, stoppingToken);
        }

        await _processor.StopProcessingAsync();
    }
}