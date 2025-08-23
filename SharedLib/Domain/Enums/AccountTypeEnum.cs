using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharedLib.Domain.Enums
{
    public enum AccountTypeEnum
    {
        [Description("Poupança")]
        Saving = 1,

        [Description("Corrente")]
        Current = 2
    }
}
