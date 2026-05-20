using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using InventoryApi.Data;
using InventoryApi.Models;
using InventoryApi.Messages;
using InventoryApi.Services;

namespace InventoryApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProductsController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly ServiceBusPublisher _serviceBus;
    private readonly KafkaPublisher _kafka;

    public ProductsController(
        AppDbContext db,
        ServiceBusPublisher serviceBus,
        KafkaPublisher kafka)
    {
        _db = db;
        _serviceBus = serviceBus;
        _kafka = kafka;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll() =>
        Ok(await _db.Products.ToListAsync());

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var product = await _db.Products.FindAsync(id);
        return product is null ? NotFound() : Ok(product);
    }

    [HttpPost]
    public async Task<IActionResult> Create(Product product)
    {
        _db.Products.Add(product);
        await _db.SaveChangesAsync();

        // Kafka — fires on every create
        await _kafka.PublishProductEventAsync(new ProductEvent
        {
            ProductId = product.Id,
            ProductName = product.Name,
            Category = product.Category,
            Quantity = product.Quantity,
            Price = product.Price,
            EventType = "ProductCreated"
        });

        // Service Bus — only fires when out of stock
        if (product.Quantity == 0)
            await _serviceBus.PublishOutOfStockAsync(
                new OutOfStockEvent
                {
                    ProductId = product.Id,
                    ProductName = product.Name,
                    Category = product.Category
                });

        return CreatedAtAction(
            nameof(GetById),
            new { id = product.Id },
            product);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(
        int id, Product product)
    {
        if (id != product.Id) return BadRequest();
        _db.Entry(product).State = EntityState.Modified;
        await _db.SaveChangesAsync();

        // Kafka — fires on every update
        await _kafka.PublishProductEventAsync(new ProductEvent
        {
            ProductId = product.Id,
            ProductName = product.Name,
            Category = product.Category,
            Quantity = product.Quantity,
            Price = product.Price,
            EventType = "ProductUpdated"
        });

        // Service Bus — only fires when out of stock
        if (product.Quantity == 0)
            await _serviceBus.PublishOutOfStockAsync(
                new OutOfStockEvent
                {
                    ProductId = product.Id,
                    ProductName = product.Name,
                    Category = product.Category
                });

        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var product = await _db.Products.FindAsync(id);
        if (product is null) return NotFound();
        _db.Products.Remove(product);
        await _db.SaveChangesAsync();

        // Kafka — fires on every delete
        await _kafka.PublishProductEventAsync(new ProductEvent
        {
            ProductId = id,
            ProductName = product.Name,
            Category = product.Category,
            Quantity = 0,
            Price = product.Price,
            EventType = "ProductDeleted"
        });

        return NoContent();
    }
}