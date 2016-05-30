using System;
using System.IO;
using TinyLexer;

namespace Sample
{
	class Program
	{
		static void Main(string[] args)
		{
			Console.WriteLine("Enter simple arithmetic operations of two positive numbers, e.g. 47 + 11.");
			Console.WriteLine("Type exit to stop.\n");

			using (var stdin = Console.OpenStandardInput())
			{
				using (var interpreter = new Interpreter(stdin))
					interpreter.Execute();
			}
		}
	}

	/// <summary>
	/// Types of tokens.
	/// </summary>
	public enum Token
	{
		Decimal, Operator, Reserved
	}

	/// <summary>
	/// Sample interpreter.
	/// </summary>
	class Interpreter : IDisposable
	{
		/// <summary>
		/// The lexer.
		/// </summary>
		private TinyLexer<Token> Lexer;

		/// <summary>
		/// Constructor.
		/// </summary>
		public Interpreter(Stream code)
		{
			Lexer = new TinyLexer<Token>(code);
			Lexer.Define(Token.Decimal, @"(\d+(?:\.\d+)?)\b");
			Lexer.Define(Token.Operator, @"(\+|-|\*|\/)");
			Lexer.Define(Token.Reserved, @"(exit)");
		}

		/// <summary>
		/// Executes the parser.
		/// </summary>
		public void Execute()
		{
			while (!Lexer.Match(Token.Reserved, "exit"))
			{
				try
				{
					ParseOperation();
				}
				catch (Exception ex)
				{
					Console.WriteLine(ex.Message);
					Lexer.Discard();
				}
			}
		}

		/// <summary>
		/// Parses an operation.
		/// </summary>
		void ParseOperation()
		{
			var a = decimal.Parse(Lexer.Consume(Token.Decimal));
			var op = Lexer.Consume(Token.Operator);
			var b = decimal.Parse(Lexer.Consume(Token.Decimal));

			Console.WriteLine("Result: " + ExecuteOperation(a, b, op));
		}

		/// <summary>
		/// Execute an operation.
		/// </summary>
		decimal ExecuteOperation(decimal a, decimal b, string op)
		{
			switch (op)
			{
				case "+": return a + b;
				case "-": return a - b;
				case "*": return a * b;
				case "/": return a / b;
				default: return 0;
			}
		}

		/// <summary>
		/// Dispose of the lexer.
		/// </summary>
		public void Dispose()
		{
			Lexer.Dispose();
		}
	}
}
