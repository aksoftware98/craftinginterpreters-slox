using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CraftingInterpreters.CSLox.Core;

internal interface ILoxCallable
{

    object? Call(LoxInterpreter interpreter, List<object?> arguments);

    /// <summary>
    /// Returns the actual nubmer of arguments declared on the function
    /// Used to compare with the number of arguments passed at runtime to make sure they match.
    /// </summary>
    /// <returns></returns>
    int Arity();

}
