using SharedLib.Domain.Entities;
using SharedLib.Domain.Enums;

namespace SharedLib.Domain.DTOs
{
    public class CreditCardDto : BaseEntity
    {
        public Guid ClientId { get; set; }
        public string CardNumber { get; set; }
        public string CardHolderName { get; set; }
        public CreditCardTypeEnum CardType { get; set; }
        public CreditCardStatusEnum Status { get; set; }
        public decimal Limit { get; set; }
        public decimal AvailableLimit { get; set; }
        public DateTime ExpiryDate { get; set; }
        public string Cvv { get; set; }
        
        // Construtor padrão para serialização
        public CreditCardDto()
        {
        }
    }
}
