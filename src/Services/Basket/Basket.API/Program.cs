using Basket.API.GrpcServices;
using Basket.API.Repositories;
using Discount.Grpc.Protos;
using MassTransit;
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
        //cfg.UseHealthCheck(ctx);
    });
});

builder.Services.AddAutoMapper(Assembly.GetExecutingAssembly());
builder.Services.AddScoped<DiscountGrpcService>();
builder.Services.AddScoped<IBasketRepository, BasketRepository>();
builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthorization();

app.MapControllers();

app.Run();
