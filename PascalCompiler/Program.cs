using PascalCompiler.Token;
using System;
using System.IO;

namespace PascalCompiler
{
    class Program
    {
        static void Main(string[] args)
        {
            var io = new IOModule(@"C:\Users\gitbleidd\Desktop\othres\ФГИМТ\test-without-errors.pas");
            io = new IOModule(@"C:\Users\gitbleidd\Desktop\othres\ФГИМТ\test.pas");
            var lexer = new Lexer(io);
            ReadTokens(lexer); 
            return;

            var syntaxAnalyzer = new SyntaxAnalyzer(io, lexer);
            syntaxAnalyzer.Start();

            io.PrintErrors();
        }

        public static void ReadTokens(Lexer lexer)
        {
            Token.LexicalToken token = lexer.GetNextToken();
            bool done = false;
            string format = "{0,18}{1,40}";
            string output = "";
            Console.WriteLine(string.Format(format, "Token type", "Value"));
            while (!done)
            {
                switch (token)
                {
                    case IdentifierToken:
                        var identToken = (IdentifierToken)token;
                        output = string.Format(format, "IdentifierToken", $"{identToken.Name}");
                        //Console.WriteLine(output);
                        break;
                    case SpecialSymbolToken:
                        var specialSymbolToken = (SpecialSymbolToken)token;
                        output = string.Format(format, "SpecialSymbolToken", $"{specialSymbolToken.Type}");
                        break;
                    case ConstToken<int>:
                        var intToken = (ConstToken<int>)token;
                        output = string.Format(format, "Int token", $"{intToken.Value}");
                        break;
                    case ConstToken<double>:
                        var doubleToken = (ConstToken<double>)token;
                        output = string.Format(format, "Double token", $"{doubleToken.Value}");
                        break;
                    case ConstToken<string>:
                        var stringToken = (ConstToken<string>)token;
                        output = string.Format(format, "String token", $"{stringToken.Value}");
                        break;
                    case ConstToken<bool>:
                        var boolToken = (ConstToken<bool>)token;
                        output = string.Format(format, "Bool token", $"{boolToken.Value}");
                        break;
                    case TriviaToken:
                        var triviaT = (TriviaToken)token;
                        if (triviaT.Type == TriviaTokenType.EndOfFileToken)
                        {
                            done = true;
                        }
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
                        output = string.Format(format, "TriviaToken token", $"{triviaT.Type}");
                        break;
                    default:
                        break;
                }
                Console.WriteLine(output);
                token = lexer.GetNextToken();
            }
            Console.WriteLine("\n\n\n\n\n");
        }
    }
}
