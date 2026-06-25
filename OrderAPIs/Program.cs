
using MassTransit;
using Microsoft.AspNetCore.Mvc;
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

// 2. Configure MassTransit with an explicit, hardcoded topology name
builder.Services.AddMassTransit(x =>
{
    x.UsingRabbitMq((context, cfg) =>
    {
        cfg.Host("localhost", "/", h =>
        {
            h.Username("guest");
            h.Password("guest");
        });

        // This force-names the exchange, completely bypassing namespace validation!
        cfg.Message<OrderPlacedEvent>(m => m.SetEntityName("logipulse-order-placed"));
    });
});

var app = builder.Build();

app.UseCors();

// 3. In-Memory Data Store
var orders = new List<OrderDto>();

// 4. Minimal API Endpoints
app.MapGet("/api/orders", () => Results.Ok(orders));

app.MapPost("/api/orders", async ([FromBody] CreateOrderRequest request, IPublishEndpoint publishEndpoint) =>
{
    var newOrder = new OrderDto(
        Id: Guid.NewGuid(),
        OrderNumber: $"LP-{DateTime.UtcNow.Ticks % 10000}",
        DeliveryAddress: request.DeliveryAddress,
        Status: "Pending"
    );

    orders.Add(newOrder);

    await publishEndpoint.Publish(new OrderPlacedEvent(newOrder.Id, newOrder.OrderNumber, newOrder.DeliveryAddress));

    return Results.Created($"/api/orders/{newOrder.Id}", newOrder);
});

app.Run();