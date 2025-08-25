using SharedLib.Domain.Entities;
using SharedLib.Domain.Interfaces;
using SharedLib.Domain.Interfaces.Bus;
using Microsoft.Extensions.Logging;

namespace API.Handlers
{
    public class CreditCardHandler : IEventHandler<CreditCardResultEvent>
    {
        private readonly IClientRepository _clientRepository;
        private readonly ILogger<CreditCardHandler> _logger;

        public CreditCardHandler(IClientRepository clientRepository, ILogger<CreditCardHandler> logger)
        {
            _clientRepository = clientRepository;
            _logger = logger;
        }

        public async Task Handle(CreditCardResultEvent @event)
        {
            try
            {
                _logger.LogInformation("Processing CreditCardResultEvent for client {ClientId} with card {CardNumber}", 
                    @event.ClientId, @event.CardNumber);

                var client = await _clientRepository.GetByIdAsync(@event.ClientId);
                if (client != null)
                {
                    _logger.LogInformation("Updating client {ClientId} with credit card status {Status} and limit {Limit}", 
                        client.Id, @event.Status, @event.Limit);

                    // Atualizar informações do cartão no cliente
                    // Aqui você pode adicionar lógica para armazenar múltiplos cartões
                    client.AddicionalCreditLimitStatus = (SharedLib.Domain.Enums.CreditProposalStatusEnum)@event.Status;
                    client.AddicionalCreditLimit = @event.Limit;
                    client.UpdatedAt = DateTime.UtcNow;

                    await _clientRepository.UpdateAsync(client);

                    _logger.LogInformation("Successfully updated client {ClientId} credit card information", client.Id);
                }
                else
                {
                    _logger.LogWarning("Client {ClientId} not found for credit card update", @event.ClientId);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error handling CreditCardResultEvent for client {ClientId}", @event.ClientId);
                throw;
            }
        }
    }
}
