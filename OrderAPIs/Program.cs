
using OrderAPIs.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOrderApiServices(builder.Configuration);

var app = builder.Build();

app.UseOrderApiMiddleware();

await app.ApplyDatabaseMigrationsAsync();

app.Run();