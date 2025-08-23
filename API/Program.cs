using API.Handlers;
using API.Services;
using API.Services.Interfaces;
using Microsoft.Extensions.Hosting;
using SharedLib.Domain.Entities;
using SharedLib.Domain.Interfaces.Bus;
using SharedLib.Infrastructure;
using SharedLib.Infrastructure.IoC;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddMediatR(cfg =>
{
    cfg.RegisterServicesFromAssemblies(
        typeof(Program).Assembly, // assembly atual
        typeof(RabbitMqClientBus).Assembly, // assembly infra
        typeof(CreditProposalEvent).Assembly // assembly de domínio
    );
});

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddTransient<IClientService, ClientService>();
builder.Services.AddSwaggerGen();

// Registrar apenas o EventBus (sem MediatR)
DependencyContainer.RegisterService(builder.Services);

// Registrar handlers após o EventBus
builder.Services.AddTransient<IEventHandler<CreditProposalResultEvent>, CreditProposalHandler>();

var app = builder.Build();

// Configurar inscrição de eventos
var eventBus = app.Services.GetRequiredService<IEventBus>();
eventBus.Subscribe<CreditProposalResultEvent, CreditProposalHandler>();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
