using MassTransit;
using SharedContracts;
using NotifierService;


var builder = WebApplication.CreateBuilder(args);

// Read RabbitMQ host from environment variables (Twelve-Factor App rule)
string rabbitMqHost = Environment.GetEnvironmentVariable("RABBITMQ_HOST") ?? "localhost";

builder.Services.AddMassTransit(x =>
{
    x.SetEndpointNameFormatter(new KebabCaseEndpointNameFormatter("notifier", false));
    x.AddConsumer<OrderPlacedConsumer>();

    x.UsingRabbitMq((context, cfg) =>
    {
        cfg.Host(rabbitMqHost, "/");

        // Explicit topology mapping to match your current system setup
        cfg.Message<OrderPlacedEvent>(m => m.SetEntityName("logipulse-order-placed"));
        cfg.Message<OrderDeliveredEvent>(m => m.SetEntityName("logipulse-order-delivered"));

        cfg.ConfigureEndpoints(context);
    });
});

var app = builder.Build();
app.Run();
