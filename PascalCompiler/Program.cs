using System;

namespace PascalCompiler
{
    class Program
    {
        static void Main(string[] args)
        {
            var io = new IOModule(@"C:\Users\gitbleidd\Desktop\test.pas");
            var lexer = new Lexer(io);

            while(lexer.GetNextToken() != null)
            {

            }
        }
    }
}
