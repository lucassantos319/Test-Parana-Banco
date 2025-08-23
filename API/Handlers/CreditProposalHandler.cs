using SharedLib.Domain.Entities;
using SharedLib.Domain.Interfaces;
using SharedLib.Domain.Interfaces.Bus;
using Microsoft.Extensions.Logging;

namespace API.Handlers
{
    public class CreditProposalHandler : IEventHandler<CreditProposalResultEvent>
    {
        private readonly IClientRepository _clientRepository;
        private readonly ILogger<CreditProposalHandler> _logger;

        public CreditProposalHandler(IClientRepository clientRepository, ILogger<CreditProposalHandler> logger)
        {
            _clientRepository = clientRepository;
            _logger = logger;
        }

        public async Task Handle(CreditProposalResultEvent @event)
        {
            try
            {
                _logger.LogInformation("Processing CreditProposalResultEvent for client {ClientId}", @event.CreditProposal.ClientId);
                
                var client = await _clientRepository.GetByIdAsync(@event.CreditProposal.ClientId);
                if (client != null)
                {
                    _logger.LogInformation("Updating client {ClientId} with credit proposal status {Status} and limit {Limit}", 
                        client.Id, @event.CreditProposal.Status, @event.CreditProposal.ApprovedLimit);
                    
                    client.AddicionalCreditLimitStatus = @event.CreditProposal.Status;
                    client.AddicionalCreditLimit = @event.CreditProposal.ApprovedLimit;
                    client.UpdatedAt = DateTime.UtcNow;

                    await _clientRepository.UpdateAsync(client);
                    
                    _logger.LogInformation("Successfully updated client {ClientId} credit proposal", client.Id);
                }
                else
                {
                    _logger.LogWarning("Client {ClientId} not found for credit proposal update", @event.CreditProposal.ClientId);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error handling CreditProposalResultEvent for client {ClientId}", @event.CreditProposal.ClientId);
                throw; // Re-throw para que o sistema de eventos possa tratar adequadamente
            }
        }
    }
}
