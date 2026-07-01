using MassTransit;
using SharedContracts;
namespace NotifierService;
// The Asynchronous Consumer Logic
public class OrderPlacedConsumer : IConsumer<OrderPlacedEvent>
{
    private readonly ILogger<OrderPlacedConsumer> _logger;

    public OrderPlacedConsumer(ILogger<OrderPlacedConsumer> logger)
    {
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<OrderPlacedEvent> context)
    {
        var order = context.Message;
        _logger.LogInformation("[Notification Engine] Order {OrderNumber} received. Preparing shipment path for {DeliveryAddress}...", order.OrderNumber, order.DeliveryAddress);

        // Simulate the 1-minute transit/delivery delay asynchronously
        await Task.Delay(TimeSpan.FromMinutes(1));

        // 1. Simulate sending the email notification
        _logger.LogInformation("[DELIVERY NOTICE] Order {OrderNumber} for {DeliveryAddress} has been successfully delivered!", order.OrderNumber, order.DeliveryAddress);

        // 2. Publish the completion status back to the broker ecosystem
        await context.Publish(new OrderDeliveredEvent(order.OrderId, "Completed"));
        _logger.LogInformation("[Notification Engine] Published OrderDeliveredEvent for Order {OrderId}", order.OrderId);
    }
}
