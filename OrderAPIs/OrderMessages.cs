
using Microsoft.EntityFrameworkCore;
namespace OrderAPIs;


public record CreateOrderRequest(string DeliveryAddress);
public record OrderDto(System.Guid Id, string OrderNumber, string DeliveryAddress, string Status);
public record OrderPlacedEvent(System.Guid OrderId, string OrderNumber, string DeliveryAddress);


public class Order
{
    public Guid Id { get; set; }
    public string OrderNumber { get; set; } = string.Empty;
    public string DeliveryAddress { get; set; } = string.Empty;
    public string Status { get; set; } = "Pending";
}

public class OrderDbContext : DbContext
{
    public OrderDbContext(DbContextOptions<OrderDbContext> options) : base(options) { }
    public DbSet<Order> Orders => Set<Order>();
}