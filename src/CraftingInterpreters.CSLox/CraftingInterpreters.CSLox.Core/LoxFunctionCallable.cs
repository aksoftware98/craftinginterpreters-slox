using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CraftingInterpreters.CSLox.Core;

public class LoxFunctionCallable : ILoxCallable
{

    readonly FunctionLoxStatement _statement;

    public LoxFunctionCallable(FunctionLoxStatement statement)
    {
        _statement = statement;
    }

    public int Arity()
    {
        return _statement.Paramters.Count;
    }

    public object? Call(LoxInterpreter interpreter, List<object?> arguments)
    {
        Environment env = new Environment(interpreter.Globals);
        for (int i = 0; i < _statement.Paramters.Count; i++)
        {
            env.Define(_statement.Paramters[i].Lexeme, arguments[i]);
        }

        try
        {
            interpreter.ExecuteBlock(_statement.Body, env);
        }
        catch (ReturnException returnException) // We catch the return because it's treated as an exception.
        {
            return returnException.Value;
        }

        return null;
    }
    public override string ToString()
    {
        return $"<fn {_statement.Name.Lexeme}>";
    }
}
