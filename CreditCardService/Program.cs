using CreditCardService;
using SharedLib.Domain.Entities;
using SharedLib.Domain.Interfaces.Bus;
using SharedLib.Infrastructure;
using Microsoft.Extensions.Configuration;

var builder = Host.CreateApplicationBuilder(args);
builder.Services.AddHostedService<Worker>();

builder.Services.AddMediatR(cfg =>
{
    cfg.RegisterServicesFromAssemblies(
        typeof(Program).Assembly,
        typeof(CreditCardRequestEvent).Assembly,
        typeof(RabbitMqClientBus).Assembly
    );
});

// Registrar configuração
builder.Services.AddSingleton<IConfiguration>(builder.Configuration);

builder.Services.AddTransient<CreditCardWorker>();
builder.Services.AddTransient<IEventBus, RabbitMqClientBus>();
builder.Services.AddTransient<IEventHandler<CreditCardRequestEvent>, CreditCardWorker>();

var host = builder.Build();

ConfigureEventBus(host);
host.Run();

void ConfigureEventBus(IHost host)
{
    var eventBus = host.Services.GetRequiredService<IEventBus>();
    eventBus.Subscribe<CreditCardRequestEvent, CreditCardWorker>();
}
