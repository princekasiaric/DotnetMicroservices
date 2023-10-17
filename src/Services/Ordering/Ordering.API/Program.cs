using Common.Logging;
using EventBus.Messages.Common;
using HealthChecks.UI.Client;
using MassTransit;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Ordering.API.EventBusConsumer;
using Ordering.API.Extensions;
using Ordering.Application;
using Ordering.Infrastructure;
using Ordering.Infrastructure.Persistence;
using Serilog;
using System.Reflection;

var builder = WebApplication.CreateBuilder(args);
// Add services to the container.
var config = builder.Configuration;

builder.Services.AddApplicationServices();
builder.Services.AddInfrastructureServices(config);
// MassTransit-RabbitMQ Configuration
builder.Services.AddMassTransit(x =>
{
    x.AddConsumer<BasketCheckoutConsumer>();
    x.UsingRabbitMq((ctx, cfg) =>
    {
        cfg.Host(config["EventBusSettings:HostAddress"]);
        cfg.ReceiveEndpoint(EventBusConstants.BasketCheckoutQueue, c =>
        {
            c.ConfigureConsumer<BasketCheckoutConsumer>(ctx);
        });
    });
});
builder.Services.AddScoped<BasketCheckoutConsumer>();
builder.Services.AddAutoMapper(Assembly.GetExecutingAssembly());
builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddHealthChecks()
        .AddDbContextCheck<OrderContext>();

builder.Host.UseSerilog(SeriLogger.Configure);

var app = builder.Build().MigrateDatabase<OrderContext>((context, services) =>
{
    var logger = services.GetService<ILogger<OrderContextSeed>>();
    OrderContextSeed.SeedAsync(context, logger).Wait();
});
// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthorization();

app.MapControllers();
app.MapHealthChecks("/hc", new HealthCheckOptions() { Predicate = (check) => true, ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse });

app.Run();
