// Path: CraftingInterpreters.CSLox.Core/LoxExpression.cs
// This file was generated by the tool at CraftingInterpreters.CSLox.Tools
// Do not edit this file directly.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CraftingInterpreters.CSLox.Core;

public abstract class LoxExpression
{
	public abstract T Accept<T>(ILoxExpressionVisitor<T> visitor);
}

public interface ILoxExpressionVisitor<T>
{
	T VisitBinaryLoxExpression(BinaryLoxExpression loxExpression);
	T VisitGroupingLoxExpression(GroupingLoxExpression loxExpression);
	T VisitLiteralLoxExpression(LiteralLoxExpression loxExpression);
	T VisitVariableLoxExpression(VariableLoxExpression loxExpression);
	T VisitUnaryLoxExpression(UnaryLoxExpression loxExpression);
}

public class BinaryLoxExpression : LoxExpression
{
	public BinaryLoxExpression(LoxExpression left, Token @operator, LoxExpression right)
	{
		this.Left = left;
		this.Operator = @operator;
		this.Right = right;
	}

	public override T Accept<T>(ILoxExpressionVisitor<T> visitor)
	{
		return visitor.VisitBinaryLoxExpression(this);
	}

	public LoxExpression Left { get; set; }
	public Token Operator { get; set; }
	public LoxExpression Right { get; set; }

}

public class GroupingLoxExpression : LoxExpression
{
	public GroupingLoxExpression(LoxExpression expression)
	{
		this.Expression = expression;
	}

	public override T Accept<T>(ILoxExpressionVisitor<T> visitor)
	{
		return visitor.VisitGroupingLoxExpression(this);
	}

	public LoxExpression Expression { get; set; }

}

public class LiteralLoxExpression : LoxExpression
{
	public LiteralLoxExpression(object value)
	{
		this.Value = value;
	}

	public override T Accept<T>(ILoxExpressionVisitor<T> visitor)
	{
		return visitor.VisitLiteralLoxExpression(this);
	}

	public object Value { get; set; }

}

public class VariableLoxExpression : LoxExpression
{
	public VariableLoxExpression(Token name)
	{
		this.Name = name;
	}

	public override T Accept<T>(ILoxExpressionVisitor<T> visitor)
	{
		return visitor.VisitVariableLoxExpression(this);
	}

	public Token Name { get; set; }

}

public class UnaryLoxExpression : LoxExpression
{
	public UnaryLoxExpression(Token @operator, LoxExpression right)
	{
		this.Operator = @operator;
		this.Right = right;
	}

	public override T Accept<T>(ILoxExpressionVisitor<T> visitor)
	{
		return visitor.VisitUnaryLoxExpression(this);
	}

	public Token Operator { get; set; }
	public LoxExpression Right { get; set; }

}


