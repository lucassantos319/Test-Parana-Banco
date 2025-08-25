using CreditProposalService;
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
        typeof(CreditProposalEvent).Assembly,
        typeof(CreditProposalResultEvent).Assembly,
        typeof(RabbitMqClientBus).Assembly
    );
});

// Registrar configuração
builder.Services.AddTransient<IEventBus, RabbitMqClientBus>();
builder.Services.AddSingleton<IConfiguration>(builder.Configuration);

builder.Services.AddTransient<CreditProposalWorker>();
builder.Services.AddTransient<IEventHandler<CreditProposalRequestEvent>, CreditProposalWorker>();

var host = builder.Build();

ConfigureEventBus(host);
host.Run();

void ConfigureEventBus(IHost host)
{
    var eventBus = host.Services.GetRequiredService<IEventBus>();
    eventBus.Subscribe<CreditProposalRequestEvent, CreditProposalWorker>();
}

