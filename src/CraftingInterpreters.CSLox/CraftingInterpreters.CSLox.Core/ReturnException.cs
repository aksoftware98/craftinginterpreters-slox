using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CraftingInterpreters.CSLox.Core;

internal class ReturnException : LoxRuntimeException
{

    public object? Value { get; private set; }

    public ReturnException(object? value) : base(null, string.Empty)
    {
        Value = value;
    }
}
