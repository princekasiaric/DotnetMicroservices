using Basket.API.GrpcServices;
using Basket.API.Repositories;
using Common.Logging;
using Discount.Grpc.Protos;
using HealthChecks.UI.Client;
using MassTransit;
using MassTransit.MultiBus;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Serilog;
using System.Reflection;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

var config = builder.Configuration;
// Redis Configuration
builder.Services.AddStackExchangeRedisCache(o => 
o.Configuration = config.GetValue<string>("CacheSettings:ConnectionString"));

// gRPC Configuration
builder.Services.AddGrpcClient<DiscountProtoService.DiscountProtoServiceClient>(o => 
o.Address = new Uri(config["GrpcSettings:DiscountUrl"]));

// MassTransit-RabbitMQ Configuration
builder.Services.AddMassTransit(x => {
    x.UsingRabbitMq((ctx, cfg) => {
        cfg.Host(config["EventBusSettings:HostAddress"]);
    });
});

builder.Services.AddAutoMapper(Assembly.GetExecutingAssembly());
builder.Services.AddScoped<DiscountGrpcService>();
builder.Services.AddScoped<IBasketRepository, BasketRepository>();
builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddHealthChecks()
        .AddRedis(config["CacheSettings:ConnectionString"], "Redis Health", HealthStatus.Degraded);

builder.Host.UseSerilog(SeriLogger.Configure);

var app = builder.Build();

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