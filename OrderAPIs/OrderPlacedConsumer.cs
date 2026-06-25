using MassTransit;

namespace OrderAPIs;

public class OrderPlacedConsumer : IConsumer<OrderPlacedEvent>
{

private readonly ILogger<OrderPlacedConsumer> _logger;

public OrderPlacedConsumer(ILogger<OrderPlacedConsumer> logger)
    {
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<OrderPlacedEvent> context)
    {

        var message = context.Message;
        
        _logger.LogInformation(">>> [FLEET ENGINE] Received Order processing request for ID: {OrderId}", message.OrderId);
        _logger.LogInformation(">>> [FLEET ENGINE] Simulating driver allocation for route to: '{DeliveryAddress}'", message.DeliveryAddress);
    
    await Task.Delay(1500); // Simulate some processing time

        _logger.LogInformation(">>> [FLEET ENGINE] Driver allocated for Order ID: {OrderId}. Ready for dispatch.", message.OrderId);
    }
}