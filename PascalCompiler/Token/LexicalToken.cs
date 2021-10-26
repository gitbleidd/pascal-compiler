using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PascalCompiler.Token
{
    abstract class LexicalToken
    {
        public int Position { get; }
        public LexicalToken(int position)
        {
            Position = position;
        }
    }

    class IdentifierToken : LexicalToken
    {
        public string Name { get; }
        public IdentifierToken(int position, string name) : base(position)
        {
            Name = name;
        }
    }

    class SpecialSymbolToken : LexicalToken
    {
        public SpecialSymbolType Type { get; }
        public SpecialSymbolToken(int position, SpecialSymbolType type) : base(position)
        {
            Type = type;
        }
    }

    class ConstToken<T> : LexicalToken
    {
        public T Value { get; }
        public ConstToken(int position, T value) : base(position)
        {
            Value = value;
        }
    }

    class TriviaToken : LexicalToken
    {
        public TriviaTokenType Type { get; }
        public TriviaToken(int position, TriviaTokenType type) : base(position)
        {
            Type = type;
        }
    }
}
