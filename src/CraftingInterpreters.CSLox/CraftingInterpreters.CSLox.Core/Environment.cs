using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CraftingInterpreters.CSLox.Core;

internal class Environment
{

	private readonly Environment? _enclosing;
	private readonly Dictionary<string, object?> _values = new();

    public Environment()
    {
        _enclosing = null;
    }

	public Environment(Environment enclosing)
	{
		_enclosing = enclosing;
	}

    public void Define(string name, object? value)
	{
		if (_values.ContainsKey(name))
			_values[name] = value;
		_values.Add(name, value);
	}

	public object? Get(Token token)
	{
		if (_values.ContainsKey(token.Lexeme)) 
			return _values[token.Lexeme];

		if (_enclosing != null) 
			return _enclosing.Get(token);

		throw new LoxRuntimeException(token, $"Undefined variable '{token.Lexeme}'.");
	}

	public void Assign(Token name, object? value)
	{
		if (_values.ContainsKey(name.Lexeme))
		{
			_values[name.Lexeme] = value;
			return;
		}

		if (_enclosing != null)
		{
			_enclosing.Assign(name, value);
			return;
		}
		throw new LoxRuntimeException(name, $"Undefine variable '{name.Lexeme}'.");
	}


}
