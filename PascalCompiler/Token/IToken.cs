using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PascalCompiler.Token
{
    interface IToken
    {
        public Lexer.TokenType Type { get; set; }
    }
}
