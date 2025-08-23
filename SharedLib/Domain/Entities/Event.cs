using SharedLib.Domain.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharedLib.Domain.Entities
{
    public abstract class Event
    {
        public DateTime Timestamp { get; protected set; }
        protected Event()
        {
            Timestamp = DateTime.Now;
        }
    }

    // Exemplo de event concreto
    public class CreditCardCreationEvent : Event
    {
        public Guid ClientId { get; set; }

        public CreditCardCreationEvent(Guid clientId)
        {
            ClientId = clientId;
        }
    }

    public class CreditProposalEvent : Event
    {
        public Guid ClientId { get; set; }
        public decimal IncomeBalance { get; set; }

        public CreditProposalEvent(Guid clientId, decimal incomeBalance)
        {
            ClientId = clientId;
            IncomeBalance = incomeBalance;
        }
    }

    public class CreditProposalResultEvent : Event
    {
        public CreditProposalDto CreditProposal { get; set; }
        public CreditProposalResultEvent(CreditProposalDto creditProposal)
        {
            CreditProposal = creditProposal;
        }
        
        // Construtor padrão para serialização
        public CreditProposalResultEvent()
        {
        }
    }
}
