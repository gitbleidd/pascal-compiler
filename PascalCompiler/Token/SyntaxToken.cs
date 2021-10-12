using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PascalCompiler.Token
{
    class SyntaxToken
    {
        public TokenType Type { get; private set; }
        public object Value { get; private set; }
        public int Position { get; private set; }

        public SyntaxToken(TokenType type, int position)
        {
            Type = type;
            Value = null;
            Position = position;
        }

        public SyntaxToken(TokenType type, object value, int position)
        {
            Type = type;
            Value = value;
            Position = position;
        }
    }
}
