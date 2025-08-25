using SharedLib.Domain.Entities;
using SharedLib.Domain.Enums;
using SharedLib.Domain.Interfaces.Bus;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SharedLib.Domain.DTOs;

namespace CreditCardService
{
    public class CreditCardWorker : IEventHandler<CreditCardRequestEvent>
    {
        private readonly IEventBus _eventBus;
        private readonly IConfiguration _configuration;
        private readonly ILogger<CreditCardWorker> _logger;

        public CreditCardWorker(IConfiguration configuration, IEventBus eventBus, ILogger<CreditCardWorker> logger)
        {
            _eventBus = eventBus;
            _configuration = configuration;
            _logger = logger;
        }

        public async Task Handle(CreditCardRequestEvent @event)
        {
            try
            {
                _logger.LogInformation("Processing credit card request for client {ClientId} with card type {CardType}", 
                    @event.ClientId, @event.CardType);

                // Simular processamento de cartão
                await SimulateCardProcessing(@event);

                // Gerar número do cartão
                var cardNumber = GenerateCardNumber(@event.CardType);
                var cardId = Guid.NewGuid();
                var limit = CalculateCardLimit(@event.CardType);

                var creditCard = new CreditCardDto
                {
                    Id = cardId,
                    ClientId = @event.ClientId,
                    CardNumber = cardNumber,
                    CardHolderName = "CARD HOLDER", // Seria obtido do cliente
                    CardType = @event.CardType,
                    Status = CreditCardStatusEnum.Approved,
                    Limit = limit,
                    AvailableLimit = limit,
                    ExpiryDate = DateTime.UtcNow.AddYears(5),
                    Cvv = GenerateCVV(),
                    CreatedAt = DateTime.UtcNow
                };

                // Publicar resultado
                var resultEvent = new CreditCardResultEvent(
                    @event.ClientId,
                    cardId,
                    cardNumber,
                    CreditCardStatusEnum.Approved,
                    limit,
                    @event.CardType
                );

                _eventBus.Publish(resultEvent);
                _logger.LogInformation("CreditCardResultEvent published for client {ClientId} with card {CardNumber}", 
                    @event.ClientId, cardNumber);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing credit card request for client {ClientId}", @event.ClientId);
                throw;
            }
        }

        private async Task SimulateCardProcessing(CreditCardRequestEvent @event)
        {
            // Simular tempo de processamento
            var processingTime = Random.Shared.Next(1000, 3000);
            await Task.Delay(processingTime);

            // Simular falha ocasional (5% de chance)
            if (Random.Shared.Next(1, 100) <= 5)
            {
                throw new Exception($"Simulated processing failure for client {@event.ClientId}");
            }
        }

        private string GenerateCardNumber(CreditCardTypeEnum cardType)
        {
            // Gerar número de cartão baseado no tipo
            var prefix = cardType switch
            {
                CreditCardTypeEnum.Basic => "4000",
                CreditCardTypeEnum.Gold => "5000",
                CreditCardTypeEnum.Platinum => "6000",
                CreditCardTypeEnum.Black => "7000",
                CreditCardTypeEnum.Infinite => "8000",
                _ => "4000"
            };

            var random = new Random();
            var middleDigits = string.Join("", Enumerable.Range(0, 12).Select(_ => random.Next(0, 10)));
            return $"{prefix}{middleDigits}";
        }

        private decimal CalculateCardLimit(CreditCardTypeEnum cardType)
        {
            return cardType switch
            {
                CreditCardTypeEnum.Basic => 2000m,
                CreditCardTypeEnum.Gold => 5000m,
                CreditCardTypeEnum.Platinum => 15000m,
                CreditCardTypeEnum.Black => 50000m,
                CreditCardTypeEnum.Infinite => 100000m,
                _ => 2000m
            };
        }

        private string GenerateCVV()
        {
            var random = new Random();
            return random.Next(100, 1000).ToString();
        }
    }
}
