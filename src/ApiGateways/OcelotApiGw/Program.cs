using Ocelot.DependencyInjection;
using Ocelot.Middleware;

var builder = WebApplication.CreateBuilder(args);

builder.Host
    .ConfigureAppConfiguration((hostContext, config) =>
    {
        config.AddJsonFile($"ocelot.{hostContext.HostingEnvironment.EnvironmentName}.json", true, true);
    })
    .ConfigureLogging((hostContext, loggingBuilder) =>
    {
        loggingBuilder.AddConfiguration(builder.Configuration.GetSection("Logging"));
        loggingBuilder.AddConsole();
        loggingBuilder.AddDebug();
    });

builder.Services.AddOcelot();

var app = builder.Build();

await app.UseOcelot();

app.MapGet("/", () => "Hello World!");

app.Run();