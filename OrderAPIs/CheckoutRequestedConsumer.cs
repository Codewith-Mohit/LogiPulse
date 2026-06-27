using MassTransit;
using SharedContracts;

namespace OrderAPIs;

public class CheckoutRequestedConsumer : IConsumer<CheckoutRequestedEvent>
{
    private readonly OrderDbContext _db;
    private readonly IPublishEndpoint _publishEndpoint;

    public CheckoutRequestedConsumer(OrderDbContext db, IPublishEndpoint publishEndpoint)
    {
        _db = db;
        _publishEndpoint = publishEndpoint;
    }

    public async Task Consume(ConsumeContext<CheckoutRequestedEvent> context)
    {
        var newOrder = new Order
        {
            Id = Guid.NewGuid(),
            OrderNumber = $"LP-{DateTime.UtcNow.Ticks % 10000}",
            DeliveryAddress = context.Message.DeliveryAddress,
            Status = "Processing"
        };

        _db.Orders.Add(newOrder);
        await _db.SaveChangesAsync();

        await _publishEndpoint.Publish(new OrderPlacedEvent(newOrder.Id, newOrder.OrderNumber, newOrder.DeliveryAddress));
    }
}