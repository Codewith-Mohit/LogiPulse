using MassTransit;
using Microsoft.AspNetCore.Mvc;
using CatalogService;
using LogiPulse.SharedContracts;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddCors(options => options.AddDefaultPolicy(p => p.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod()));

builder.Services.AddMassTransit(x =>
{
    x.UsingRabbitMq((context, cfg) =>
    {
        string host = builder.Configuration.GetValue<string>("RabbitMQ_Host") ?? "localhost";
            string user = builder.Configuration.GetValue<string>("RabbitMQ:Username") ?? "guest";
            string pass = builder.Configuration.GetValue<string>("RabbitMQ:Password") ?? "guest";

            cfg.Host(host, "/", h =>
            {
                h.Username(user);
                h.Password(pass);
            });

        cfg.Message<CheckoutRequestedEvent>(m => m.SetEntityName("logipulse-checkout-requested"));
    });
});

var app = builder.Build();
app.UseCors();

// Dummy product catalog database substitute
var products = new List<ProductDto>
{
    new(Guid.NewGuid(), "Industrial Drone Battery", 299.99m),
    new(Guid.NewGuid(), "GPS Tracking Node v4", 89.50m),
    new(Guid.NewGuid(), "Rugged Fleet Cargo Box", 145.00m)
};

app.MapGet("/api/products", () => Results.Ok(products));

app.MapPost("/api/cart/checkout", async ([FromBody] CheckoutRequest request, IPublishEndpoint publishEndpoint) =>
{
    // Fire event to tell OrderService to pick this up and build the persistent order
    await publishEndpoint.Publish(new CheckoutRequestedEvent(request.DeliveryAddress, request.TotalAmount));
    return Results.Accepted();
});

app.Run();

