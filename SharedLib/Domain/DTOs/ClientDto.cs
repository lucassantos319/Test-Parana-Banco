using SharedLib.Domain.Entities;
using SharedLib.Domain.Enums;

namespace SharedLib.Domain.DTOs
{
    public class ClientDto : BaseEntity
    {
        public string Name { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public StatusClientEnum Status { get; set; }
        public AccountTypeEnum Type { get; set; }
        public decimal Balance { get; set; } = 0.0m;
        public decimal IncomeAmount { get; set; } = 0.0m;
        public decimal Limit { get; set; } = 0.0m;
        public decimal AddicionalCreditLimit { get; set; } = 0.0m;
        public CreditProposalStatusEnum AddicionalCreditLimitStatus { get; set; } = CreditProposalStatusEnum.Pending;
    }
}
