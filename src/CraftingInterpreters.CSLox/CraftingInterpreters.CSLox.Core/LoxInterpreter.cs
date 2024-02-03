using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CraftingInterpreters.CSLox.Core;

public class LoxInterpreter : ILoxExpressionVisitor<object?>, ILoxStatementVisitor<object?>
{

	private List<string> _errors = new();
	private Environment _environment = new();
	public void Interpret(List<LoxStatement> statements)
	{
		try
		{
			foreach (var statement in statements)
			{
				Execute(statement);
			}

		}
		catch (LoxRuntimeException ex)
		{

		}
	}


	private void Execute(LoxStatement statement)
		=> statement.Accept(this);

	public object? VisitBinaryLoxExpression(BinaryLoxExpression loxExpression)
	{
		var left = Evaluate(loxExpression.Left);
		var right = Evaluate(loxExpression.Right);

		switch (loxExpression.Operator.Type)
		{

			case TokenType.MINUS:
				CheckNumberOperands(loxExpression.Operator, left, right);
				return Number(left) - Number(right);
			case TokenType.SLASH:
				CheckNumberOperands(loxExpression.Operator, left, right);
				return Number(left) / Number(right);
			case TokenType.STAR:
				CheckNumberOperands(loxExpression.Operator, left, right);
				return Number(left) * Number(right);

			// Sum
			case TokenType.PLUS:
				return SumTwoObjects(loxExpression.Operator, left, right);

			// Comparison
			case TokenType.GREATER:
				CheckNumberOperands(loxExpression.Operator, left, right);
				return Number(left) > Number(right);
			case TokenType.GREATER_EQUAL:
				CheckNumberOperands(loxExpression.Operator, left, right);
				return Number(left) >= Number(right);
			case TokenType.LESS:
				CheckNumberOperands(loxExpression.Operator, left, right);
				return Number(left) < Number(right);
			case TokenType.LESS_EQUAL:
				CheckNumberOperands(loxExpression.Operator, left, right);
				return Number(left) <= Number(right);

			default:
				return null;
		}
	}

	private object? Evaluate(LoxExpression expression)
		=> expression.Accept(this);

	public object? VisitGroupingLoxExpression(GroupingLoxExpression loxExpression)
	{
		return Evaluate(loxExpression.Expression);
	}

	public object? VisitLiteralLoxExpression(LiteralLoxExpression loxExpression)
	{
		return loxExpression.Value;
	}

	public object? VisitUnaryLoxExpression(UnaryLoxExpression loxExpression)
	{
		var right = Evaluate(loxExpression.Right);

		switch (loxExpression.Operator.Type)
		{
			case TokenType.MINUS:
				CheckNumberOperand(loxExpression.Operator, right);
				return -(double)(right ?? 0);
			case TokenType.BANG:
				return !IsTruthy(right);
			default:
				return null;
		}
	}

	/// <summary>
	/// Convert a nullable object into a boolean value at runtime.
	/// </summary>
	/// <param name="value"></param>
	/// <returns></returns>
	private bool IsTruthy(object? value)
	{
		if (value == null)
			return false;

		return value is bool ? (bool)value : true;
	}

	/// <summary>
	/// Check if two objects are equal
	/// </summary>
	/// <param name="left"></param>
	/// <param name="right"></param>
	/// <returns></returns>
	private bool IsEquals(object? left, object? right)
	{
		if (left == null && right == null)
			return true;
		if (left == null || right == null) return false;
		return left.Equals(right);
	}

	/// <summary>
	/// Process a two objects to apply the sum operation on them, if the both objects are strings, then combine the result, if both objects are double then sum them and return the result, otherwise return null
	/// </summary>
	/// <param name="left"></param>
	/// <param name="right"></param>
	/// <returns></returns>
	private object? SumTwoObjects(Token token, object? left, object? right)
	{
		if (left == null)
			return right;
		if (right == null)
			return left;

		// Check if they are string
		if (left is string && right is string)
			return left.ToString() + right.ToString();

		if (left is double && right is double)
			return (double)(left ?? 0) + (double)(right ?? 0);

		throw new LoxRuntimeException(token, $"Cannot sum two objects from two different types. Object to be sum must either be numbers or strings.");

	}

	/// <summary>
	/// Translate a nullable object into a double number
	/// </summary>
	/// <param name="left"></param>
	/// <returns></returns>
	private double Number(object? left)
	{
		return (double)(left ?? 0);
	}


	#region Error Checks
	/// <summary>
	/// Check if the operand is a number, if not throw a runtime exception <see cref="LoxRuntimeException"/>"/>
	/// </summary>
	/// <param name="token"></param>
	/// <param name="operand"></param>
	/// <exception cref="LoxRuntimeException"></exception>
	private void CheckNumberOperand(Token token, object? operand)
	{
		if (operand is double)
			return;

		throw new LoxRuntimeException(token, "Operand must be a number.");
	}

	/// <summary>
	/// Check if left and right operands are both valid numbers otherwise throw a runtime exception <see cref="LoxRuntimeException"/>"/>
	/// </summary>
	/// <param name="token"></param>
	/// <param name="left"></param>
	/// <param name="right"></param>
	/// <exception cref="LoxRuntimeException"></exception>
	private void CheckNumberOperands(Token token, object? left, object? right)
	{
		if (left is double && right is double)
			return;

		throw new LoxRuntimeException(token, "Operands must be numbers.");
	}

	#region Statement Interpreter
	public object? VisitExpressionLoxStatement(ExpressionLoxStatement loxExpression)
	{
		Evaluate(loxExpression.Expression);
		return null;
	}

	public object? VisitPrintLoxStatement(PrintLoxStatement loxExpression)
	{
		var value = Evaluate(loxExpression.Expression);
		Console.WriteLine(Stringify(value));
		return null;
	}
	#endregion
	#endregion

	private string Stringify(object? value)
	{
		if (value == null)
			return "nil";

		if (value is double)
		{
			var text = value.ToString() ?? string.Empty;
			if (text.EndsWith(".0"))
			{
				text = text.Substring(0, text.Length - 2);
			}

			return text;
		}

		return value.ToString() ?? string.Empty;
	}

	public object? VisitVariableLoxExpression(VariableLoxExpression loxExpression)
	{
		return _environment.Get(loxExpression.Name);
	}

	public object? VisitVariableLoxStatement(VariableLoxStatement loxExpression)
	{
		object? value = null;
		if (loxExpression.Initializer != null)
		{
			value = Evaluate(loxExpression.Initializer);
		}

		_environment.Define(loxExpression.Name.Lexeme, value);
		return null;
	}

	public object? VisitAssignLoxExpression(AssignLoxExpression loxExpression)
	{
		var value = Evaluate(loxExpression.Value);
		_environment.Assign(loxExpression.Name, value);
		return value;
	}
}

/// <summary>
/// Exception thrown when a runtime error occurs
/// </summary>
public class LoxRuntimeException : Exception
{
	public Token Token { get; }
	public LoxRuntimeException(Token token, string message) : base(message)
	{
		Token = token;
	}
}