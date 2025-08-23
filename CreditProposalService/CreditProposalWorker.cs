

using SharedLib.Domain.Entities;
using SharedLib.Domain.Enums;
using SharedLib.Domain.Interfaces.Bus;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SharedLib.Domain.DTOs;

namespace CreditProposalService
{
    public class CreditProposalWorker : IEventHandler<CreditProposalEvent>
    {
        private readonly IEventBus _eventBus;
        private readonly IConfiguration _configuration;
        private readonly ILogger<CreditProposalWorker> _logger;
        private readonly decimal MinThreshold ;

        public CreditProposalWorker(IConfiguration configuration, IEventBus eventBus, ILogger<CreditProposalWorker> logger) 
        { 
            _eventBus = eventBus;
            _configuration = configuration;
            _logger = logger;
            MinThreshold = decimal.Parse(_configuration["MinThreshold"] ?? "1200");
        }


        public Task Handle(CreditProposalEvent @event)
        {
            try
            {
                _logger.LogInformation("Processing credit proposal for client {ClientId} with income balance {IncomeBalance}", 
                    @event.ClientId, @event.IncomeBalance);

                var creditProposal = new CreditProposalDto
                {
                    Id = Guid.NewGuid(),
                    ClientId = @event.ClientId,
                    CreatedAt = DateTime.UtcNow
                };

                if (@event.IncomeBalance < MinThreshold)
                {
                    creditProposal.Status = CreditProposalStatusEnum.Rejected;
                    creditProposal.Observations = "Income balance below minimum threshold.";
                    creditProposal.ApprovedLimit = 0;
                    
                    _logger.LogInformation("Credit proposal REJECTED for client {ClientId}. Income {IncomeBalance} < Threshold {Threshold}", 
                        @event.ClientId, @event.IncomeBalance, MinThreshold);
                }
                else if (@event.IncomeBalance >= MinThreshold)
                {
                    var approvedLimit = @event.IncomeBalance * 2.5m; 

                    creditProposal.Status = CreditProposalStatusEnum.PreApproved;
                    creditProposal.ApprovedLimit = approvedLimit;
                    creditProposal.Observations = $"Pre Approved with a limit of {approvedLimit}.";
                    
                    _logger.LogInformation("Credit proposal PRE-APPROVED for client {ClientId}. Approved limit: {ApprovedLimit}", 
                        @event.ClientId, approvedLimit);
                }

                var @eventCreditProposalResult = new CreditProposalResultEvent(creditProposal);
                _eventBus.Publish(@event);

                _logger.LogInformation("CreditProposalResultEvent published for client {ClientId}", @event.ClientId);

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing credit proposal for client {ClientId}", @event.ClientId);
                throw; // Re-throw para que o sistema de eventos possa tratar adequadamente
            }
            
            return Task.CompletedTask;
        }
    }
}
