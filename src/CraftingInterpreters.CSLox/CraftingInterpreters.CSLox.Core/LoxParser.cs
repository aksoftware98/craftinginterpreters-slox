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
	private readonly List<string> _errors = new();
	private bool _hadError = false;

	public IEnumerable<string> Errors => _errors;
	public bool HadError => _hadError;

	public LoxParser(List<Token> tokens)
	{
		_tokens = tokens;
	}

	/// <summary>
	/// Kick off the parsing process
	/// </summary>
	/// <returns></returns>
	public List<LoxStatement> Parse()
	{
		try
		{
			var statements = new List<LoxStatement>();
			while (!IsAtEnd())
				statements.Add(Declaration());

			return statements;
		}
		catch (LoxParserException ex)
		{
			// TODO: Synchronize the parser after an error
			return null;
		}
	}

	private LoxStatement Declaration()
	{
		if (Match(TokenType.VAR))
		{
			return VariableDeclaration();	
		}

		return Statement();
	}

	private LoxStatement VariableDeclaration()
	{
		var tokenName =	Consume(TokenType.IDENTIFIER, "Identifier expected");

		LoxExpression? expression = null;
        if (Match(TokenType.EQUAL))
        {
			expression = Expression();
        }

		Consume(TokenType.SEMICOLON, "; expected after variable decleration");
		return new VariableLoxStatement(tokenName, expression);
    }

	private LoxStatement Statement()
	{
		if (Match(TokenType.PRINT))
			return PrintStatement();

		return ExpressionStatement();
	}

	/// <summary>
	/// Parse a print statement
	/// </summary>
	/// <returns></returns>
	private LoxStatement PrintStatement()
	{
		var value = Expression();
		Consume(TokenType.SEMICOLON, "Expect ';' after value.");
		return new PrintLoxStatement(value);
	}

	/// <summary>
	/// Parse an expression statement
	/// </summary>
	/// <returns></returns>
	private LoxStatement ExpressionStatement()
	{
		var value = Expression();
		Consume(TokenType.SEMICOLON, "Expect ';' after value.");
		return new ExpressionLoxStatement(value);
	}

	// Translate each rule in the language
	public LoxExpression Expression()
	{
		return Assignment();
	}

	public LoxExpression Assignment()
	{
		var expression = Equality();

		if (Match(TokenType.EQUAL))
		{
			Token equals = Previous();
			var value = Assignment();

			if (expression is VariableLoxExpression)
			{
				Token name = ((VariableLoxExpression)expression).Name;
				return new AssignLoxExpression(name, value);
			}

			Error(equals, "Invalid assignment target");
		}

		return expression;
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
		if (Match(TokenType.IDENTIFIER))
			return new VariableLoxExpression(Previous());

		if (Match(TokenType.NUMBER, TokenType.STRING))
			return new LiteralLoxExpression(Previous().Literal);

		if (Match(TokenType.LEFT_PAREN))
		{
			var expression = Expression();
			Consume(TokenType.RIGHT_PAREN, "Expect ')' after expression.");
			return new GroupingLoxExpression(expression);
		}

		throw Error(Peek(), "Except expression.");
	}

	#region Panic-Mode (Error Handling)
	/// <summary>
	/// Check if the current token matches any of a given token and move the cursor ahead, otherwise throw a <see cref="LoxParserException"/> add add the error to the errors list
	/// </summary>
	/// <param name="type"></param>
	/// <param name="message"></param>
	/// <returns></returns>
	private Token Consume(TokenType type, string message)
	{
		if (Check(type)) 
			return Advance();

		throw Error(Peek(), message);
	}

	/// <summary>
	/// Discard the tokens after an error until we reach a token that has a statement boundary. After catching an error, we will call this until hopefully we are back on track to sync the remaning tokens
	/// </summary>
	private void Synchronize()
	{
		Advance();
		while(!IsAtEnd())
		{
			if (Previous().Type == TokenType.SEMICOLON)
				return;
			switch (Peek().Type)
			{
				case TokenType.CLASS:
				case TokenType.FUN:
				case TokenType.FOR:
				case TokenType.IF:
				case TokenType.PRINT:
				case TokenType.RETURN:
				case TokenType.VAR:
				case TokenType.WHILE:
					return;
			}
		}

		Advance();
	}

	/// <summary>
	/// Synchronize the parser after an error
	/// </summary>
	/// <param name="token"></param>
	/// <param name="message"></param>
	private LoxParserException Error(Token token, string message)
	{
		string errorMessage = string.Empty;
		if (token.Type == TokenType.EOF)
			errorMessage = AddError(token.Line, " at end", message);
		else
			errorMessage = AddError(token.Line, $" at '{token.Lexeme}'", message);

		return new LoxParserException(errorMessage);
	}

	/// <summary>
	/// Add an error to the list of errors and set the flag to indicate that we had an error
	/// </summary>
	/// <param name="line"></param>
	/// <param name="where"></param>
	/// <param name="message"></param>
	private string AddError(int line, string where, string message)
	{
		var errorMessage = $"[line {line}] Error{where}: {message}";
		_errors.Add(errorMessage);
		_hadError = true;
		return errorMessage;
	}
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
	private Token Advance()
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

/// <summary>
/// Exception thrown when the parser encounters an error
/// </summary>
public class LoxParserException : Exception
{
	public LoxParserException(string message) : base(message)
	{
	}
} 