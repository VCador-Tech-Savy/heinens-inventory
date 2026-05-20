namespace InventoryApi.Messages;

public class ProductEvent
{
    public int ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string EventType { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal Price { get; set; }
    public DateTime OccurredAt { get; set; } = DateTime.UtcNow;
}