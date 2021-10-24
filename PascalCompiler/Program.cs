using System;

namespace PascalCompiler
{
    class Program
    {
        static void Main(string[] args)
        {
            var io = new IOModule(@"C:\Users\gitbleidd\Desktop\ФГИМТ\test.pas");
            var lexer = new Lexer(io);

            Token.SyntaxToken token = lexer.GetNextToken();
            while (token.Type != TokenType.EndOfFileToken)
            {
                if (token.Type == TokenType.BadToken)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                }

                if (token.Value != null)
                    Console.WriteLine($"{token.Type} | Value: {token.Value}");
                else
                    Console.WriteLine(token.Type);
                token = lexer.GetNextToken();

                Console.ForegroundColor = ConsoleColor.White;
            }

            Console.WriteLine("\n\n\n\n\n");
            io.PrintErrors(skipCleanLines: false);
        }
    }
}
