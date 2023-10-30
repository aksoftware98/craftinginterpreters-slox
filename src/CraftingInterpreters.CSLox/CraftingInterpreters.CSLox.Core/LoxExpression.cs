using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CraftingInterpreters.CSLox.Core;

public abstract class LoxExpression
{
}

public class BinaryLoxExpression : LoxExpression
{
	public BinaryLoxExpression(LoxExpression left, Token @operator, LoxExpression right)
	{
		Left = left;
		Operator = @operator;
		Right = right;
	}

	public LoxExpression Left { get; }
	public Token Operator { get; }
	public LoxExpression Right { get; }
}
