
using System.ComponentModel;

namespace SharedLib.Domain.Enums
{
    public enum StatusClientEnum
    {
        [Description("Ativo")]
        Active = 1,

        [Description("Inativo")]
        Inactive = 2,

        [Description("Pendente")]
        Pending = 3,

        [Description("Bloqueado")]
        Blocked = 4
    }
}
