using System.Collections;
using System.Reflection.Metadata.Ecma335;
using System.Text;

namespace CraftingInterpreters.CSLox.Core
{
	public class Scanner
	{
		private readonly string _source;
		private readonly List<Token> _tokens = new List<Token>();

		public Scanner(string source)
		{
			_source = source;
		}

		private int _start = 0;
		private int _current = 0;
		private int _line = 1;
		private static readonly Dictionary<string, TokenType> _keywords = new()
		{
			{ "and", TokenType.AND },
			{ "or", TokenType.OR },
			{ "var", TokenType.VAR },
			{ "fun", TokenType.FUN },
			{ "if", TokenType.IF },
			{ "else", TokenType.ELSE },
			{ "class", TokenType.CLASS },
			{ "nil", TokenType.NIL },
			{ "print", TokenType.PRINT },
			{ "super", TokenType.SUPER },
			{ "return", TokenType.RETURN },
			{ "this", TokenType.THIS },
			{ "true", TokenType.TRUE },
			{ "false", TokenType.FALSE },
			{ "for", TokenType.FOR },
			{ "while", TokenType.WHILE },

		};


		/// <summary>
		/// Analyze the source code and extract a list of tokens.
		/// </summary>
		/// <returns></returns>
		public List<Token> ScanTokens()
		{
			while (!IsAtEnd())
			{
				_start = _current;
				ScanToken();
			}

			_tokens.Add(new Token(TokenType.EOF, "", null, _line));
			return _tokens;
		}

		private void ScanToken()
		{
			var c = Advance();
			switch (c)
			{
				case '(':
					AddToken(TokenType.LEFT_PAREN);
					break;
				case ')':
					AddToken(TokenType.RIGHT_PAREN);
					break;
				case '+':
					AddToken(TokenType.PLUS);
					break;
				case '-':
					AddToken(TokenType.MINUS);
					break;
				case '{':
					AddToken(TokenType.LEFT_BRACE);
					break;
				case '}':
					AddToken(TokenType.RIGHT_BRACE);
					break;
				case '.':
					AddToken(TokenType.DOT);
					break;
				case ';':
					AddToken(TokenType.SEMICOLON);
					break;
				case '*':
					AddToken(TokenType.STAR);
					break;
				case ',':
					AddToken(TokenType.COMMA);
					break;
				case '!':
					AddToken(Match('=') ? TokenType.BANG_EQUAL : TokenType.BANG);
					break;
				case '=':
					AddToken(Match('=') ? TokenType.EQUAL_EQUAL : TokenType.EQUAL);
					break;
				case '<':
					AddToken(Match('=') ? TokenType.LESS_EQUAL : TokenType.EQUAL);
					break;
				case '>':
					AddToken(Match('=') ? TokenType.GREATER_EQUAL : TokenType.EQUAL);
					break;
				case '/':
					// Check if it matches a comment with two slashes, so keep moving until the end of the line
					if (Match('/'))
					{
						while (Peek() != '\n' && !IsAtEnd())
						{
							Advance();
						}
					}
					else
						AddToken(TokenType.SLASH);
					break;

				// Skip and process the useless characters 
				case ' ':
				case '\t':
				case '\r':
					break;

				// Handle new lines
				case '\n':
					_line++;
					break;

				// Handle String Literals
				case '"':
					HandleStringToken();
					break;
				default:
					if (IsDigit(c))
						HandleNumberToken();
					else if (IsAlpha(c))
						HandleIdentifierToken();
					else
						Lox.Error(_line, "Unexpected character.");
					break;
			}
		}

		/// <summary>
		/// Process a literal string token 
		/// </summary>
		private void HandleStringToken()
		{
			// Keep moving until reaching the close "
			while (Peek() != '"' && !IsAtEnd())
			{
				if (Peek() == '\n')
					_line++;
				Advance();
			}

			// Check if the current source file ends before reading the closing "
			if (IsAtEnd())
			{
				Lox.Error(_line, "Unterminated string");
				return;
			}

			// Move one step forward to jump on from the closing "
			Advance();

			// Read the string value from the start position up to the current position and skip the open and closing "
			var stringValue = _source[(_start + 1)..(_current - 1)];
			AddToken(TokenType.STRING, stringValue);
		}

		/// <summary>
		/// Process a literal number token
		/// </summary>
		private void HandleNumberToken()
		{
			while (IsDigit(Peek()))
				Advance();

			if (Peek() == '.' && IsDigit(PeekNext()))
			{
				while (IsDigit(Peek()))
					Advance();
			}

			var stringValue = _source[_start.._current];
			var doubleValue = double.Parse(stringValue);
			AddToken(TokenType.NUMBER, doubleValue);
		}

		private void HandleIdentifierToken()
		{
			while (IsAlphaNumeric(Peek()))
				Advance();

			var stringValue = _source[_start.._current];
			var isKeyword = _keywords.TryGetValue(stringValue, out var type);
			if (!isKeyword)
				type = TokenType.IDENTIFIER;

			AddToken(type);
		}

		/// <summary>
		/// Determine if the current character is a digit or no
		/// </summary>
		/// <returns></returns>
		private bool IsDigit(char c) => c >= '0' && c <= '9';

		/// <summary>
		/// Determine if the current cursor is at the end of the source code.
		/// </summary>
		/// <returns></returns>
		private bool IsAtEnd()
			=> _current >= _source.Length;

		/// <summary>
		/// Read the upcoming character and move the cursor a single char forward
		/// </summary>
		/// <returns></returns>
		private char Advance()
		{
			return _source[_current++];
		}

		/// <summary>
		/// Read the current character without moving the cursor a single char forward
		/// </summary>
		/// <returns></returns>
		private char Peek()
		{
			if (IsAtEnd())
				return '\0';

			return _source[_current];
		}

		/// <summary>
		/// Read the next character without moving the cursor a single char forward
		/// </summary>
		/// <returns></returns>
		private char PeekNext()
		{
			if (_current + 1 >= _source.Length)
				return '\0';

			return _source[_current + 1];
		}
		/// <summary>
		/// Peek at the upcoming character and check if it matches an expected value, if it does move the cursor forward, otherwise leave it where it is.
		/// </summary>
		/// <param name="expected"></param>
		/// <returns></returns>
		private bool Match(char expected)
		{
			if (IsAtEnd())
				return false;

			if (_source[_current] == expected)
			{
				_current++;
				return true;
			}

			return false;
		}

		/// <summary>
		/// Add a token to the list of tokens with no literal value
		/// </summary>
		/// <param name="type"></param>
		private void AddToken(TokenType type)
		{
			AddToken(type, null);
		}

		/// <summary>
		/// Add a token to the list of tokens with a literal value
		/// </summary>
		/// <param name="type"></param>
		/// <param name="literal"></param>
		private void AddToken(TokenType type, object? literal)
		{
			_tokens.Add(new(type, _source[_start.._current], literal, _line));
		}

		/// <summary>
		/// Determine if a character is a alphabetical character or an underscore 
		/// </summary>
		/// <param name="c"></param>
		/// <returns></returns>
		private bool IsAlpha(char c)
		{
			return c >= 'A' && c <= 'Z' || c >= 'a' && c <= 'z' || c == '_';
		}

		/// <summary>
		/// Determine if a character is an alphanumerical
		/// </summary>
		/// <param name="c"></param>
		/// <returns></returns>
		private bool IsAlphaNumeric(char c)
			=> IsDigit(c) || IsAlpha(c);

	}

	public class Token
	{
		public Token(TokenType type, string lexeme, object? literal, int line)
		{
			Type = type;
			Lexeme = lexeme;
			Literal = literal;
			Line = line;
		}

		public TokenType Type { get; }
		public string Lexeme { get; }
		public object? Literal { get; }
		public int Line { get; }

		public override string ToString()
		 => $"{Type} {Lexeme} {Literal}";
	}

	/// <summary>
	/// Enumeration of all possible token types supported by the scanner in the Lox language
	/// </summary>
	public enum TokenType
	{
		// Single--character tokens.
		LEFT_PAREN, RIGHT_PAREN, LEFT_BRACE, RIGHT_BRACE, COMMA, DOT, MINUS, PLUS, SEMICOLON, SLASH, STAR,

		// One or two character tokens.
		BANG, BANG_EQUAL, EQUAL, EQUAL_EQUAL, GREATER, GREATER_EQUAL, LESS, LESS_EQUAL,

		// Literals
		IDENTIFIER, STRING, NUMBER,

		// Keywords
		AND, CLASS, ELSE, FALSE, FUN, FOR, IF, NIL, OR, PRINT, RETURN, SUPER, THIS, TRUE, VAR, WHILE,

		EOF
	}

}
