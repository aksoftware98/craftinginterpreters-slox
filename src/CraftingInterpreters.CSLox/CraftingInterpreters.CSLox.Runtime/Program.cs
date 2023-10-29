// See https://aka.ms/new-console-template for more information
using CraftingInterpreters.CSLox.Core;
using System.Text;

Console.WriteLine("Hello, World!");

if (args.Length > 1)
{
	Console.WriteLine("Usage: cslox [script]");
	Environment.Exit(64);
}
else if (args.Length == 1)
{
	Lox.RunFile(args[0]);
}
else
{
	Lox.RunPrompt();
}


class Lox
{
	public static void RunFile(string path)
	{
		byte[] bytes = File.ReadAllBytes(path);
		Run(Encoding.UTF8.GetString(bytes));

		// Indicate an error in the exit code.
		if (_hadError)
			Environment.Exit(65);
	}

	public static void RunPrompt()
	{
		for (; ; )
		{
			Console.Write("> ");
			string? line = Console.ReadLine();
			if (line == null) break;
			RunFile(line);
			_hadError = false;
		}
	}

	public static void Run(string source)
	{
		var scanner = new Scanner(source);
		var tokens = scanner.ScanTokens();
		_hadError = scanner.HadError;

		if (_hadError)
		{
			Console.WriteLine($"Failed to process the source file.\r\n{scanner.Errors.Count()} errors have been found");
			foreach (var item in scanner.Errors)
			{
				Console.WriteLine(item);
			}
		}
		else
		{
			foreach (var token in tokens)
			{
				Console.WriteLine(token);
			}
		}
	}
	static bool _hadError = false;
}
