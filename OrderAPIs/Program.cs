
using MassTransit;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OrderAPIs;

var builder = WebApplication.CreateBuilder(args);

// 1. Enable CORS
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy => 
    {
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

string connectionstring = builder.Configuration.GetConnectionString("SQL_CONNECTIONSTRING") 
?? "Server=localhost,1433;Database=LogiPulseOrders;User Id=sa;Password=YourStrong@Password123;TrustServerCertificate=True;";

builder.Services.AddDbContext<OrderDbContext>(options =>
    options.UseSqlServer(connectionstring));

// 2. Configure MassTransit with an explicit, hardcoded topology name
builder.Services.AddMassTransit(x =>
{

x.AddConsumer<OrderPlacedConsumer>();
x.AddConsumer<CheckoutRequestedConsumer>();

        x.UsingRabbitMq((context, cfg) =>
        {
            string host = builder.Configuration.GetValue<string>("RabbitMQ:Host") ?? "localhost";
            string user = builder.Configuration.GetValue<string>("RabbitMQ:Username") ?? "guest";
            string pass = builder.Configuration.GetValue<string>("RabbitMQ:Password") ?? "guest";

            cfg.Host(host, "/", h =>
            {
                h.Username(user);
                h.Password(pass);
            });

            // This force-names the exchange, completely bypassing namespace validation!
            cfg.Message<OrderPlacedEvent>(m => m.SetEntityName("logipulse-order-placed"));

            cfg.ConfigureEndpoints(context);
        });
});

var app = builder.Build();

app.UseCors();

// 3. In-Memory Data Store
var orders = new List<OrderDto>();

// 4. Minimal API Endpoints
app.MapGet("/api/orders", async (OrderDbContext db) => 
{
    var orders = await db.Orders.AsNoTracking().ToListAsync();
    return Results.Ok(orders);
});

app.MapPost("/api/orders", async ([FromBody] CreateOrderRequest request, OrderDbContext db, IPublishEndpoint publishEndpoint) =>
{
    // Create actual Entity class instance
    var newOrder = new Order
    {
        Id = Guid.NewGuid(),
        OrderNumber = $"LP-{DateTime.UtcNow.Ticks % 10000}",
        DeliveryAddress = request.DeliveryAddress,
        Status = "Pending"
    };

    // Save to SQL Server via EF Core Context
    db.Orders.Add(newOrder);
    await db.SaveChangesAsync();

    // Publish event to RabbitMQ
    await publishEndpoint.Publish(new OrderPlacedEvent(newOrder.Id, newOrder.OrderNumber, newOrder.DeliveryAddress));

    return Results.Created($"/api/orders/{newOrder.Id}", newOrder);
});

app.Run();