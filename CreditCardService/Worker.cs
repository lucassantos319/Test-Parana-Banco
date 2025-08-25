using SharedLib.Domain.Entities;
using SharedLib.Domain.Enums;
using SharedLib.Domain.Interfaces.Bus;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SharedLib.Domain.DTOs;

namespace CreditCardService
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private readonly IEventBus _eventBus;
        private readonly IConfiguration _configuration;

        public Worker(ILogger<Worker> logger, IEventBus eventBus, IConfiguration configuration)
        {
            _logger = logger;
            _eventBus = eventBus;
            _configuration = configuration;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("CreditCardService Worker started");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    // O worker agora é apenas para manter o serviço rodando
                    // O processamento real é feito pelos handlers
                    await Task.Delay(5000, stoppingToken);
                }
                catch (OperationCanceledException)
                {
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error in CreditCardService Worker");
                    await Task.Delay(1000, stoppingToken);
                }
            }

            _logger.LogInformation("CreditCardService Worker stopped");
        }
    }
}
