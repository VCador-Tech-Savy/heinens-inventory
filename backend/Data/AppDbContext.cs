using Microsoft.EntityFrameworkCore;
using InventoryApi.Models;

namespace InventoryApi.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Product> Products => Set<Product>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Product>().HasData(
            new Product { Id = 1, Name = "Organic Whole Milk", Category = "Dairy", Quantity = 142, Price = 4.99m },
            new Product { Id = 2, Name = "Sourdough Bread", Category = "Bakery", Quantity = 38, Price = 6.49m },
            new Product { Id = 3, Name = "Atlantic Salmon", Category = "Seafood", Quantity = 24, Price = 14.99m },
            new Product { Id = 4, Name = "Baby Spinach", Category = "Produce", Quantity = 0, Price = 3.99m },
            new Product { Id = 5, Name = "Sharp Cheddar", Category = "Dairy", Quantity = 87, Price = 7.49m }
        );
    }
}
