using System.Text;

namespace CraftingInterpreters.CSLox.Core
{
	public class Lox
	{

		static bool _hadError = false;

		public static void Main(string[] args)
		{
			if (args.Length > 1)
			{
				Console.WriteLine("Usage: cslox [script]");
				Environment.Exit(64);
			}
			else if (args.Length == 1)
			{
				RunFile(args[0]);
			}
			else
			{
				RunPrompt();
			}
		}

		private static void RunFile(string path)
		{
			byte[] bytes = File.ReadAllBytes(path);
			Run(Encoding.UTF8.GetString(bytes));

			// Indicate an error in the exit code.
			if (_hadError)
				Environment.Exit(65);
		}

		private static void RunPrompt()
		{
			for (; ; )
			{
				Console.Write("> ");
				string? line = Console.ReadLine();
				if (line == null) break;
				Run(line);
				_hadError = false;
			}
		}

		private static void Run(string source)
		{
			var scanner = new Scanner(source);
			var tokens = new

		}

		static void Error(int line, string message)
		{
			Report(line, "", message);
		}

		private static void Report(int line, string where, string message)
		{
			Console.WriteLine($"[Line: {line}] Error {where}: {message}");
			_hadError = true;
		}

	}

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

		/// <summary>
		/// Analyze the source code and extract a list of tokens.
		/// </summary>
		/// <returns></returns>
		public List<Token> ScanTokens()
		{
			while (!IsAtEnd())
			{
				_start = _current;
				ScanTokens();
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
				case '/':
					AddToken(TokenType.SLASH);
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
			}
		}

		private char Advance()
		{
			return _source[_current++];
		}

		private void AddToken(TokenType type)
		{
			AddToken(type, null);
		}

		private void AddToken(TokenType type, object? literal)
		{
			_tokens.Add(new(type, _source[_start.._current], literal, _line));
		}

		/// <summary>
		/// Determine if the current cursor is at the end of the source code.
		/// </summary>
		/// <returns></returns>
		private bool IsAtEnd()
			=> _current >= _source.Length;
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
		BANG, BANG_EQUAL, EQUAL, EQUAL_EUAL, GREATER, GREATER_EQUAL, LESS, LESS_EQUAL,

		// Literals
		IDENTIFIER, STRING, NUMBER,

		// Keywords
		AND, CLASS, ELSE, FALSE, FUN, FOR, IF, NIL, OR, PRINT, RETURN, SUPER, THIS, TRUE, VAR, WHILE,

		EOF
	}

}
