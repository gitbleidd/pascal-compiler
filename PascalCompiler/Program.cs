using System;

namespace PascalCompiler
{
    class Program
    {
        static void Main(string[] args)
        {
            var io = new IOModule(@"C:\Users\gitbleidd\Desktop\test.pas");
            var lexer = new Lexer(io);

            Token.SyntaxToken token = lexer.GetNextToken();
            while (token != null)
            {
                if (token.Value != null)
                    Console.WriteLine($"{token.Type} | Value: {token.Value}");
                else
                    Console.WriteLine(token.Type);
                token = lexer.GetNextToken();
            }
        }
    }
}
