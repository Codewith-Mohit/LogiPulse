using SharedContracts;
using MassTransit;
using Microsoft.EntityFrameworkCore;

namespace OrderAPIs.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddOrderApiServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddControllers();
        services.AddJwtAuthentication(configuration);

        services.AddCors(options =>
        {
            options.AddDefaultPolicy(policy =>
            {
                policy.AllowAnyOrigin()
                      .AllowAnyHeader()
                      .AllowAnyMethod();
            });
        });

        var connectionString = Environment.GetEnvironmentVariable("SQL_CONNECTIONSTRING")
            ?? configuration.GetConnectionString("SQL_CONNECTIONSTRING")
            ?? "Server=localhost,1433;Database=LogiPulseOrders;User Id=sa;Password=YourStrong@Password123;TrustServerCertificate=True;";

        Console.WriteLine($">>> [STARTUP] Utilizing Connection String: {connectionString}");

        services.AddDbContext<OrderDbContext>(options =>
            options.UseSqlServer(connectionString));

        services.AddMassTransit(x =>
        {
            x.AddConsumer<OrderPlacedConsumer>();
            x.AddConsumer<CheckoutRequestedConsumer>();
            x.AddConsumer<OrderDeliveredConsumer>();
            x.UsingRabbitMq((context, cfg) =>
            {
                var host = Environment.GetEnvironmentVariable("RABBITMQ_HOST") 
                ?? configuration.GetValue<string>("RabbitMQ_Host") 
                ?? "localhost";
                var user = configuration.GetValue<string>("RabbitMQ:Username") ?? "guest";
                var pass = configuration.GetValue<string>("RabbitMQ:Password") ?? "guest";

                cfg.Host(host, "/", h =>
                {
                    h.Username(user);
                    h.Password(pass);
                });

                cfg.Message<CheckoutRequestedEvent>(m => m.SetEntityName("logipulse-checkout-requested"));
                cfg.Message<OrderPlacedEvent>(m => m.SetEntityName("logipulse-order-placed"));
                cfg.Message<OrderDeliveredEvent>(m => m.SetEntityName("logipulse-order-delivered"));
                
                // cfg.ReceiveEndpoint("orderapi-checkout-requested-queue", e =>
                // {
                //     e.ConfigureConsumer<CheckoutRequestedConsumer>(context);
                // });

                // cfg.ReceiveEndpoint("orderapi-order-placed-queue", e =>
                // {
                //     e.ConfigureConsumer<OrderPlacedConsumer>(context);
                // });

                // cfg.ReceiveEndpoint("orderapi-order-delivered-queue", e =>
                // {
                //     e.ConfigureConsumer<OrderDeliveredConsumer>(context);
                // });

                cfg.ConfigureEndpoints(context);
            });
        });

        return services;
    }
}
