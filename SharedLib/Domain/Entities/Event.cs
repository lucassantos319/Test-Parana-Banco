using SharedLib.Domain.DTOs;
using SharedLib.Domain.Enums;
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
        public Guid CorrelationId { get; set; } = Guid.NewGuid();
        
        protected Event()
        {
            Timestamp = DateTime.Now;
        }
    }

    public class ClientCreatedEvent : Event
    {
        public Guid ClientId { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public decimal IncomeAmount { get; set; }
        public AccountTypeEnum AccountType { get; set; }

        public ClientCreatedEvent(Guid clientId, string name, string email, decimal incomeAmount, AccountTypeEnum accountType)
        {
            ClientId = clientId;
            Name = name;
            Email = email;
            IncomeAmount = incomeAmount;
            AccountType = accountType;
        }

        public ClientCreatedEvent()
        {
        }
    }

    public class CreditProposalRequestEvent : Event
    {
        public Guid ClientId { get; set; }
        public decimal IncomeAmount { get; set; }
        public Guid RequestId { get; set; }

        public CreditProposalRequestEvent(Guid clientId, decimal incomeAmount, Guid requestId)
        {
            ClientId = clientId;
            IncomeAmount = incomeAmount;
            RequestId = requestId;
        }

        public CreditProposalRequestEvent()
        {
        }
    }

    public class CreditProposalResultEvent : Event
    {
        public CreditProposalDto CreditProposal { get; set; }
        
        public CreditProposalResultEvent(CreditProposalDto creditProposal)
        {
            CreditProposal = creditProposal;
        }
        
        public CreditProposalResultEvent()
        {
        }
    }

    // Eventos de Cartão de Crédito
    public class CreditCardRequestEvent : Event
    {
        public Guid ClientId { get; set; }
        public CreditCardTypeEnum CardType { get; set; }
        public Guid RequestId { get; set; }

        public CreditCardRequestEvent(Guid clientId, CreditCardTypeEnum cardType, Guid requestId)
        {
            ClientId = clientId;
            CardType = cardType;
            RequestId = requestId;
        }

        public CreditCardRequestEvent()
        {
        }
    }

    public class CreditCardResultEvent : Event
    {
        public Guid ClientId { get; set; }
        public Guid CardId { get; set; }
        public string CardNumber { get; set; }
        public CreditCardStatusEnum Status { get; set; }
        public decimal Limit { get; set; }
        public CreditCardTypeEnum CardType { get; set; }

        public CreditCardResultEvent(Guid clientId, Guid cardId, string cardNumber, CreditCardStatusEnum status, decimal limit, CreditCardTypeEnum cardType)
        {
            ClientId = clientId;
            CardId = cardId;
            CardNumber = cardNumber;
            Status = status;
            Limit = limit;
            CardType = cardType;
        }

        public CreditCardResultEvent()
        {
        }
    }

    // Eventos de Falha e Resiliência
    public class ProcessFailureEvent : Event
    {
        public Guid ClientId { get; set; }
        public string ServiceName { get; set; }
        public string ErrorMessage { get; set; }
        public int RetryCount { get; set; }
        public string OriginalEventType { get; set; }
        public string OriginalEventData { get; set; }

        public ProcessFailureEvent(Guid clientId, string serviceName, string errorMessage, int retryCount, string originalEventType, string originalEventData)
        {
            ClientId = clientId;
            ServiceName = serviceName;
            ErrorMessage = errorMessage;
            RetryCount = retryCount;
            OriginalEventType = originalEventType;
            OriginalEventData = originalEventData;
        }

        public ProcessFailureEvent()
        {
        }
    }

    // Eventos Legacy (mantidos para compatibilidade)
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
}
