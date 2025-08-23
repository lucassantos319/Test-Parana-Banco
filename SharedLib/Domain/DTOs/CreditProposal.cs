using SharedLib.Domain.Entities;
using SharedLib.Domain.Enums;

namespace SharedLib.Domain.DTOs
{
    public class CreditProposalDto : BaseEntity
    {
        public Guid ClientId { get; set; }
        public decimal ApprovedLimit { get; set; }
        public CreditProposalStatusEnum Status { get; set; }
        public string Observations { get; set; }
        
        // Construtor padrão para serialização
        public CreditProposalDto()
        {
        }
    }
}
