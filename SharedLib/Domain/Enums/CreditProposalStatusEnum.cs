using System.ComponentModel;

namespace SharedLib.Domain.Enums
{
    public enum CreditProposalStatusEnum
    {
        [Description("Pre Aprovado")]
        PreApproved = 1,

        [Description("Aprovado")]
        Approved = 2,

        [Description("Rejeitado")]
        Rejected = 3,

        [Description("Pending")]
        Pending = 4

    }
}
