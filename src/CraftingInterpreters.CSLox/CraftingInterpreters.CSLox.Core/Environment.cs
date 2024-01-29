using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CraftingInterpreters.CSLox.Core;

internal class Environment
{

	private readonly Dictionary<string, object?> _values = new();

	public void Define(string name, object? value)
	{
		if (_values.ContainsKey(name))
			_values[name] = value;
		_values.Add(name, value);
	}

	public object? Get(Token token)
	{
		if (_values.ContainsKey(token.Lexeme)) return _values[token.Lexeme];

		throw new LoxRuntimeException(token, $"Undefined variable '{token.Lexeme}'.");
	}



}
