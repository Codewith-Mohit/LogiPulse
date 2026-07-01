namespace FleetService;
using MassTransit;
using SharedContracts;

public class OrderPlacedConsumer : IConsumer<OrderPlacedEvent>
{
    public Task Consume(ConsumeContext<OrderPlacedEvent> context)
    {
        Console.WriteLine($">>> [FLEET ENGINE] Order detected! Allocating automatic route delivery to: {context.Message.DeliveryAddress}");
        return Task.CompletedTask;
    }
}
