using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PascalCompiler.Token
{
    class SyntaxToken
    {
        public Lexer.TokenType Type { get; private set; }
        public object Value { get; private set; }

        public SyntaxToken(Lexer.TokenType type)
        {
            Type = type;
            Value = null;
        }

        public SyntaxToken(Lexer.TokenType type, object value)
        {
            Type = type;
            Value = value;
        }
    }
}
