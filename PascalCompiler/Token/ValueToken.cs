using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PascalCompiler.Token
{
    class ValueToken<T> : IToken
    {
        public Lexer.TokenType Type { get; set; }
        public T Value { get; set; }
    }
}
