using System.ComponentModel;

namespace SharedLib.Domain.Enums
{
    public enum CreditCardStatusEnum
    {
        [Description("Pendente")]
        Pending = 1,

        [Description("Aprovado")]
        Approved = 2,

        [Description("Rejeitado")]
        Rejected = 3,

        [Description("Em Processamento")]
        Processing = 4,

        [Description("Cancelado")]
        Cancelled = 5,

        [Description("Bloqueado")]
        Blocked = 6
    }
}
