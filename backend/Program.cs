using Microsoft.EntityFrameworkCore;
using InventoryApi.Data;
using InventoryApi.Services;
using InventoryApi.Workers;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(
        builder.Configuration
            .GetConnectionString("DefaultConnection")
        ?? "Data Source=/data/inventory.db"));

// Service Bus
builder.Services.AddSingleton<ServiceBusPublisher>();
builder.Services.AddHostedService<OutOfStockWorker>();

// Kafka
builder.Services.AddSingleton<KafkaPublisher>();
builder.Services.AddHostedService<KafkaAnalyticsConsumer>();
builder.Services.AddHostedService<KafkaLoggingConsumer>();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddCors(options =>
    options.AddDefaultPolicy(policy =>
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader()));

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider
        .GetRequiredService<AppDbContext>();
    try { db.Database.EnsureCreated(); }
    catch (Exception ex) 
        when (ex.Message.Contains("already exists")) { }
}

app.UseSwagger();
app.UseSwaggerUI();
app.UseCors();
app.MapControllers();
app.Run();