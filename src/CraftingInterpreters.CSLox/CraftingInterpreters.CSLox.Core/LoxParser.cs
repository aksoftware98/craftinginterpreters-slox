using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace CraftingInterpreters.CSLox.Core;

/// <summary>
/// Lox parses.
/// Following the are the rules of the language.
/// expression ==> equality ;
/// equality   ==> comparison ( ( "!=" | "==" ) comparison )* ;
/// comparison ==> term ( ( ">" | "<" | "<=" | ">=" ) term )* ;
/// term       ==> factor ( ( "+" | "-" ) factor )* ;
/// factor     ==> unary ( ( "/" | "*" ) unary )* ;
/// unary      ==> ( "!" | "-" ) unary | primary ;
/// primary    ==> NUMBER | STRING | "true" | "false" | "nil" | "(" expression ")"
/// </summary>
public class LoxParser
{
	private readonly List<Token> _tokens;
	private int _current = 0;

	public LoxParser(List<Token> tokens)
	{
		_tokens = tokens;
	}

	// Translate each rule in the language
	public LoxExpression Expression()
	{
		return Equality();
	}
	public LoxExpression Equality()
	{
		var expression = Comparison();
		while (Match(TokenType.BANG_EQUAL, TokenType.EQUAL_EQUAL))
		{
			var @operator = Previous();
			var right = Comparison();
			expression = new BinaryLoxExpression(expression, @operator, right);
		}

		return expression;
	}

	public LoxExpression Comparison()
	{
		var expression = Term();

		while (Match(TokenType.GREATER, TokenType.GREATER_EQUAL, TokenType.LESS, TokenType.LESS_EQUAL)) 
		{
			var @operator = Previous();
			var right = Term();
			expression = new BinaryLoxExpression(expression, @operator, right);
		}

		return expression;
	}

	public LoxExpression Term()
	{
		var expression = Factor();

		while (Match(TokenType.PLUS, TokenType.MINUS))
		{
			var @operator = Previous();
			var right = Factor();
			expression = new BinaryLoxExpression(expression, @operator, right);
		}

		return expression;
	}

	public LoxExpression Factor()
	{
		var expression = Unary();

		while (Match(TokenType.SLASH, TokenType.STAR))
		{
			var @operator = Previous();
			var right = Unary();
			expression = new BinaryLoxExpression(expression, @operator, right);
		}

		return expression;
	}

	public LoxExpression Unary()
	{
		if (Match(TokenType.BANG, TokenType.MINUS))
		{
			var @operator = Previous();
			var right = Unary();
			return new UnaryLoxExpression(@operator, right);
		}

		return Primary();
	}

	public LoxExpression Primary()
	{
		if (Match(TokenType.TRUE))
			return new LiteralLoxExpression(true);
		if (Match(TokenType.FALSE))
			return new LiteralLoxExpression(false);
		if (Match(TokenType.NIL))
			return new LiteralLoxExpression(null);

		if (Match(TokenType.NUMBER, TokenType.STRING))
			return new LiteralLoxExpression(Previous().Literal);

		if (Match(TokenType.LEFT_PAREN))
		{
			var expression = Expression();
			// Consume(TokenType.RIGHT_PAREN, "Expect ')' after expression.");
			return new GroupingLoxExpression(expression);
		}

		// TODO: Implement the error part
		throw new NotImplementedException();
	}

	#region Panic-Mode (Error Handling)
	
	#endregion

	#region Infrastrcture 
	/// <summary>
	/// Check if the current token matches any of a given token and move the cursor ahead for each matching attempt
	/// </summary>
	/// <param name="tokens"></param>
	/// <returns></returns>
	private bool Match(params TokenType[] tokens)
	{
		foreach (var item in tokens)
		{
			if (Check(item))
			{
				Advance();
				return true;
			}
		}

		return false;
	}

	/// <summary>
	/// Validates if the current token type matches a given token type
	/// </summary>
	/// <param name="tokenType"></param>
	/// <returns></returns>
	private bool Check(TokenType tokenType)
	{
		if (IsAtEnd())
			return false;

		return Peek().Type == tokenType;
	}

	/// <summary>
	/// Return the current token and move the cursor forward
	/// </summary>
	/// <returns></returns>
	private Token? Advance()
	{
		if (!IsAtEnd())
			_current++;

		return Previous();
	}

	/// <summary>
	/// Check if the parser reached the last token and we ran out of tokens
	/// </summary>
	/// <returns></returns>
	private bool IsAtEnd()
	{
		return Peek().Type == TokenType.EOF;
	}

	/// <summary>
	/// Return the current token without moving the cursor ahead
	/// </summary>
	/// <returns></returns>
	private Token Peek()
	{
		return _tokens[_current];
	}

	/// <summary>
	/// Return the previous token without moving the cursor
	/// </summary>
	/// <returns></returns>
	private Token Previous()
	{
		return _tokens[_current - 1];
	}
	#endregion
}
