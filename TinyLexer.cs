/*
* Tiny Lexer v0.1.0
* github.com/lightspeedlucas/tinylexer
*/

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace TinyLexer
{
	public struct SourceCodePosition
    {
        public int Column { get; set; }
        public int Line { get; set; }
    }

    public class Token<TType>
    {
        public TType Type { get; set; }
        public string Value { get; set; }

        public static implicit operator TType(Token<TType> token) { return token.Type; }
    }

    public sealed class TokenDefinition<TType>
    {
        public Regex Regex { get; set; }
		public TType Type { get; set; }
		public bool Ignored { get; set; }
	}

    public sealed class TinyLexer<TType> : IDisposable where TType : struct
    {
        /// <summary>
        /// The defined tokens.
        /// </summary>
        private List<TokenDefinition<TType>> Definitions;

		/// <summary>
		/// The current position in the source code.
		/// </summary>
		private SourceCodePosition Position;

        /// <summary>
        /// The current token.
        /// </summary>
        public Token<TType> CurrentToken { get; private set; }

		/// <summary>
		/// Flag for reaching the end of file.
		/// </summary>
		public bool EOF { get { return CurrentToken == null; } }

        /// <summary>
        /// Buffered code.
        /// </summary>
        private string LineBuffer = string.Empty;

        /// <summary>
        /// The code reader.
        /// </summary>
        private TextReader Reader;

        /// <summary>
        /// Constructor.
        /// </summary>
        public TinyLexer(Stream code)
        {
			Definitions = new List<TokenDefinition<TType>>();
            Reader = new StreamReader(code);
        }

        /// <summary>
        /// Defines a token.
        /// </summary>
        public void Define(TType type, string regex, bool ignore = false)
        {
            Definitions.Add(new TokenDefinition<TType> { Type = type, Regex = new Regex(@"^" + regex), Ignored = ignore });
        }

		/// <summary>
		/// Starts processing.
		/// </summary>
		public void Start()
		{
			NextToken();
		}

        /// <summary>
        /// Fills the code buffer.
        /// </summary>
        string FillBuffer()
        {
            while (LineBuffer != null && LineBuffer.Length == 0)
            {
                ++Position.Line;
                Position.Column = 0;
                LineBuffer = Reader.ReadLine();
				if (LineBuffer == null) break;
				LineBuffer = LineBuffer.TrimStart();
            }

            return LineBuffer;
        }

        /// <summary>
        /// Gets the next token or null if EOF.
        /// </summary>
        private void NextToken()
        {
            for (CurrentToken = null; CurrentToken == null && FillBuffer() != null; )
            {
                Match match = null;
                var definition = Definitions.FirstOrDefault(def => (match = def.Regex.Match(LineBuffer)).Success);

                if (definition == null)
                    throw Error("Unable to match \"{0}\" against any tokens", LineBuffer);

                Position.Column += match.Length;
                LineBuffer = LineBuffer.Substring(match.Index + match.Length).TrimStart();

				Console.WriteLine(match.Groups[1].Value);

				if (!definition.Ignored)
					CurrentToken = new Token<TType> { Type = definition.Type, Value = match.Groups[1].Value };
            }
        }

		/// <summary>
		/// Matches a token.
		/// </summary>
		public bool Match(TType type, string value = null)
        {
            return !EOF && EqualityComparer<TType>.Default.Equals(type, CurrentToken.Type) && (value == null || CurrentToken.Value == value);
        }

		/// <summary>
		/// Consumes a token.
		/// </summary>
		public bool TryToConsume(TType type, string value = null)
        {
            if (!Match(type, value))
                return false;
            NextToken();
            return true;
        }

        /// <summary>
        /// Consumes a token.
        /// </summary>
        public string Consume(TType type, string value = null)
        {
            if (!Match(type, value))
                throw Error("Unexpected token \"{0}\"", CurrentToken.Value);
            value = CurrentToken.Value;
            NextToken();
            return value;
        }

        /// <summary>
        /// Dispose of the reader.
        /// </summary>
        public void Dispose()
        {
            Reader.Dispose();
        }

        /// <summary>
        /// Throws an error.
        /// </summary>
        Exception Error(string message, params string[] args)
        {
            throw new Exception(string.Format("At line {0} position {1}: {2}", Position.Line, Position.Column, string.Format(message, args)));
        }
    }
}
