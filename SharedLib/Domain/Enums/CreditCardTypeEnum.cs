using System.ComponentModel;

namespace SharedLib.Domain.Enums
{
    public enum CreditCardTypeEnum
    {
        [Description("Básico")]
        Basic = 1,

        [Description("Gold")]
        Gold = 2,

        [Description("Platinum")]
        Platinum = 3,

        [Description("Black")]
        Black = 4,

        [Description("Infinite")]
        Infinite = 5
    }
}
