using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CraftingInterpreters.CSLox.Core;

internal class LoxFunctionCallable : ILoxCallable
{

    readonly FunctionLoxStatement _statement;
    private readonly Environment _closure; 

    public LoxFunctionCallable(FunctionLoxStatement statement, Environment closure)
    {
        _statement = statement;
        _closure = closure;
    }

    public int Arity()
    {
        return _statement.Paramters.Count;
    }

    public object? Call(LoxInterpreter interpreter, List<object?> arguments)
    {
        Environment env = new Environment(_closure);
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
