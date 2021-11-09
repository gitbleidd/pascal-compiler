using PascalCompiler.Token;
using System;

namespace PascalCompiler
{
    class Program
    {
        static void Main(string[] args)
        {
            var io = new IOModule(@"C:\Users\gitbleidd\Desktop\ФГИМТ\test.pas");
            var lexer = new Lexer(io);

            Token.LexicalToken token = lexer.GetNextToken();
            bool done = false;
            while (!done)
            {
                switch (token)
                {
                    case IdentifierToken:
                        var identToken = (IdentifierToken)token;
                        Console.WriteLine($"Ident token | Name: {identToken.Name}");
                        break;
                    case SpecialSymbolToken:
                        var specialSymbolToken = (SpecialSymbolToken)token;
                        Console.WriteLine($"Special symbol token | Name: {specialSymbolToken.Type}");
                        break;
                    case ConstToken<int>:
                        var intToken = (ConstToken<int>)token;
                        Console.WriteLine($"Int token | Value: {intToken.Value}");
                        break;
                    case ConstToken<double>:
                        var doubleToken = (ConstToken<double>)token;
                        Console.WriteLine($"Double token | Value: {doubleToken.Value}");
                        break;
                    case ConstToken<string>:
                        var stringToken = (ConstToken<string>)token;
                        Console.WriteLine($"String token | Value: {stringToken.Value}");
                        break;
                    case ConstToken<bool>:
                        var boolToken = (ConstToken<bool>)token;
                        Console.WriteLine($"Bool token | Value: {boolToken.Value}");
                        break;
                    case TriviaToken:
                        var triviaT = (TriviaToken)token;
                        if (triviaT.Type == TriviaTokenType.EndOfFileToken)
                            done = true;
                        else if (triviaT.Type == TriviaTokenType.BadToken)
                        {
                            Console.ForegroundColor = ConsoleColor.Red;
                            Console.WriteLine("Bad token");
                            Console.ForegroundColor = ConsoleColor.White;
                        }
                        else
                        {
                            Console.ForegroundColor = ConsoleColor.Blue;
                            Console.WriteLine(triviaT.Type);
                            Console.ForegroundColor = ConsoleColor.White;
                        }

                        break;
                    default:
                        break;
                }
                token = lexer.GetNextToken();
            }

            Console.WriteLine("\n\n\n\n\n");
            io.PrintErrors();
        }
    }
}
