using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CraftingInterpreters.CSLox.Core;

public class LoxInterpreter : IVisitor<object?>
{
	public object? VisitBinaryLoxExpression(BinaryLoxExpression loxExpression)
	{
		var left = Evaluate(loxExpression.Left);
		var right = Evaluate(loxExpression.Right);

		return loxExpression.Operator.Type switch
		{
			// Arithmetic
			TokenType.MINUS => Number(left) - Number(right),
			TokenType.STAR => Number(left) * Number(right),
			TokenType.SLASH => Number(left) / Number(right),
			// PLUS special handling two either combine two numbers or concatenate two strings
			TokenType.PLUS => SumTwoObjects(left, right),

			// Comparison
			TokenType.GREATER => Number(left) > Number(right),
			TokenType.LESS => Number(left) < Number(right),
			TokenType.GREATER_EQUAL => Number(left) >= Number(right),
			TokenType.LESS_EQUAL => Number(left) <= Number(right),

			// Equality
			TokenType.BANG_EQUAL => !IsEquals(left, right),
			TokenType.EQUAL_EQUAL => IsEquals(left, right),
			_ => null
		};
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

		return loxExpression.Operator.Type switch
		{
			TokenType.BANG => !IsTruthy(right),
			TokenType.MINUS => -(double)(right ?? 0),
			_ => null
		};
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
	private object? SumTwoObjects(object? left, object? right)
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

		// TODO: Handle when on the two objects are from a two different types
		return null;
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


	
}
