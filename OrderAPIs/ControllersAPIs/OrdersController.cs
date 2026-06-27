using MassTransit;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace OrderAPIs.ControllersAPIs;

[ApiController]
[Route("api/orders")]
[Authorize]
public class OrdersController : ControllerBase
{
    private readonly OrderDbContext _db;
    private readonly IPublishEndpoint _publishEndpoint;

    public OrdersController(OrderDbContext db, IPublishEndpoint publishEndpoint)
    {
        _db = db;
        _publishEndpoint = publishEndpoint;
    }

    [HttpGet]
    public async Task<IActionResult> GetOrders()
    {
        var orders = await _db.Orders.AsNoTracking().ToListAsync();
        return Ok(orders);
    }

    [HttpPost]
    public async Task<IActionResult> CreateOrder([FromBody] CreateOrderRequest request)
    {
        var newOrder = new Order
        {
            Id = Guid.NewGuid(),
            OrderNumber = $"LP-{DateTime.UtcNow.Ticks % 10000}",
            DeliveryAddress = request.DeliveryAddress,
            Status = "Pending"
        };

        _db.Orders.Add(newOrder);
        await _db.SaveChangesAsync();

        await _publishEndpoint.Publish(new OrderPlacedEvent(newOrder.Id, newOrder.OrderNumber, newOrder.DeliveryAddress));

        return Created($"/api/orders/{newOrder.Id}", newOrder);
    }
}
