using MassTransit;
using SharedContracts;
using Microsoft.EntityFrameworkCore;

namespace OrderAPIs
{
    public class OrderDeliveredConsumer : IConsumer<OrderDeliveredEvent>
    {
        private readonly OrderDbContext _dbContext;
        private readonly ILogger<OrderDeliveredConsumer> _logger;

        public OrderDeliveredConsumer(OrderDbContext dbContext, ILogger<OrderDeliveredConsumer> logger)
        {
            _dbContext = dbContext;
            _logger = logger;
        }

        public async Task Consume(ConsumeContext<OrderDeliveredEvent> context)
        {
            var message = context.Message;
            _logger.LogInformation($"[OrderAPI] Processing delivery update for Order ID: {message.OrderId}");

            var order = await _dbContext.Orders.FirstOrDefaultAsync(o => o.Id == message.OrderId);
            if (order != null)
            {
                order.Status = message.Status; // Updates status to "Completed"
                await _dbContext.SaveChangesAsync();
                _logger.LogInformation($"[OrderAPI] Order {message.OrderId} state updated to Database successfully.");
            }
        }
    }
}