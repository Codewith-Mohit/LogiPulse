using MassTransit;
using FleetService;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddMassTransit(x =>
{
    x.AddConsumer<OrderPlacedConsumer>();

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

        cfg.ConfigureEndpoints(context);
    });
});

var app = builder.Build();
app.Run();

