using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CraftingInterpreters.CSLox.Core;

public class AbstractSyntaxTreePrinter : ILoxExpressionVisitor<string>
{
	public string VisitBinaryLoxExpression(BinaryLoxExpression loxExpression)
		=> Parenthesize(loxExpression.Operator.Lexeme, loxExpression.Left, loxExpression.Right);

	public string VisitGroupingLoxExpression(GroupingLoxExpression loxExpression)
		=> Parenthesize("group", loxExpression.Expression);

	public string VisitLiteralLoxExpression(LiteralLoxExpression loxExpression)
		=> loxExpression.Value?.ToString() ?? "nil";

	public string VisitUnaryLoxExpression(UnaryLoxExpression loxExpression)
		=> Parenthesize(loxExpression.Operator.Lexeme, loxExpression.Right);

	public string Print(LoxExpression expression)
	{
		return expression.Accept(this);
	}

	private string Parenthesize(string name, params LoxExpression[] expressions)
	{
		var builder = new StringBuilder();

		builder.Append($"(").Append(name);
		foreach ( var expression in expressions )
		{
			builder.Append(" ");
			builder.Append(expression.Accept(this));
		}
		builder.Append(")");

		return builder.ToString();
	}

	public string VisitVariableLoxExpression(VariableLoxExpression loxExpression)
	{
		throw new NotImplementedException();
	}

	public string VisitAssignLoxExpression(AssignLoxExpression loxExpression)
	{
		throw new NotImplementedException();
	}

	public string VisitLogicalLoxExpression(LogicalLoxExpression loxExpression)
	{
		throw new NotImplementedException();
	}

	public string VisitSteppingLoxExpression(SteppingLoxExpression loxExpression)
	{
		throw new NotImplementedException();
	}
}
