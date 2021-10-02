using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PascalCompiler.Token
{
    class GenericToken
    {
        public Lexer.TokenType TType { get; private set; }
        public Lexer.Symbols TSymbol { get; private set; }


        public GenericToken()
        {

        }
    }
}
