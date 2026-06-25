namespace FleetService;
using MassTransit;  
public class OrderPlacedConsumer : IConsumer<OrderPlacedEvent>
{
    public Task Consume(ConsumeContext<OrderPlacedEvent> context)
    {
        Console.WriteLine($">>> [FLEET ENGINE] Order detected! Allocating automatic route delivery to: {context.Message.DeliveryAddress}");
        return Task.CompletedTask;
    }
}

public record OrderPlacedEvent(Guid OrderId, string OrderNumber, string DeliveryAddress);