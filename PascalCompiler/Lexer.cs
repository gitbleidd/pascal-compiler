using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using PascalCompiler.Extensions;
using PascalCompiler.Token;

namespace PascalCompiler
{
    class Lexer
    {
        private int CurrentCharNum { get; set; }
        private char Ch { get; set; }

        private string CurrentStr { get; set; }
        private int CurrentStrNum { get; set; }

        private IOModule IO { get; }

        public enum Symbols
        {
            LeftBracket,
            RightBracket,

            Plus,
            Minus,
            Mult,
            Div,
            
            Greater,
            Less,
            GreaterOrEqual,
            LessOrEqual,
            Equal
        }

        public enum TokenType
        {
            ConstToken,
            OperationToken,
            IdentToken
        }

        public Lexer(IOModule IO)
        {
            this.IO = IO;
            CurrentStr = IO.ReadNextLine();
            CurrentCharNum = 0;
            Ch = CurrentStr[CurrentCharNum];
        }

        public GenericToken GetNextToken()
        {
            if (CurrentStr == null)
                return null;

            while (Ch == ' ') NextChar();

            // Идентификатор
            // Сканируем идент. или ключевое слово
            if (Ch.IsAsciiLetter())
            {
                string identifierName = "";
                int maxIndentLen = 127;
                int letterCounter = 0;
                while ((Ch.IsAsciiLetter() || Ch.IsNumber())
                    && letterCounter < maxIndentLen)
                {
                    letterCounter++;
                    identifierName += Ch;
                    NextChar();
                }
                return new GenericToken(); // 'int num'
            }

            // Числовая константа
            // Сканируем целую или вещественную константу
            if (char.IsNumber(Ch))
            {
                // int const:
                int maxInt = 32767;
                int num = 0;
                while (char.IsNumber(Ch))
                {
                    int digit = Ch - '0';
                    if (num < maxInt / 10 || (num == maxInt / 10 && digit <= maxInt % 10))
                        num = 10 * num + digit;
                    else
                    {
                        throw new Exception("Константа превышает допустимый предел");
                        num = 0;
                    }
                    NextChar();
                }
                return new GenericToken(); // 'int num'
            }

            switch (Ch)
            {
                case '+':
                    NextChar();
                    return new GenericToken();
                case '-':
                    NextChar();
                    return new GenericToken();
                case '*':
                    NextChar();
                    return new GenericToken();
                case '/':
                    NextChar();
                    return new GenericToken();
                case '(':
                    NextChar();
                    return new GenericToken();
                case ')':
                    NextChar();
                    return new GenericToken();
                case '<':
                    NextChar();
                    if (Ch == '=')
                    {
                        NextChar();
                        return new GenericToken(); // '<='
                    }
                    else if (Ch == '>')
                    {
                        NextChar();
                        return new GenericToken(); // '<>'
                    }
                    else
                        return new GenericToken(); // '<'
                case '>':
                    NextChar();
                    if (Ch == '=')
                    {
                        NextChar();
                        return new GenericToken(); // '>='
                    }
                    else
                        return new GenericToken(); // '>'
                case ':':
                    NextChar();
                    if (Ch == '=')
                    {
                        NextChar();
                        return new GenericToken(); // ':='
                    }
                    else
                        return new GenericToken(); // ':'
                case ';':
                    NextChar();
                    return new GenericToken(); // ';'
                
            }

            return null;
        }

        private void NextChar()
        {
            if (CurrentCharNum == CurrentStr.Length - 1)
            {
                CurrentStr = IO.ReadNextLine();
                if (CurrentStr == null)
                {
                    Ch = '\0';
                    return;
                }

                CurrentStrNum++;
                CurrentCharNum = 0;
            }

            CurrentCharNum++;
            Ch = CurrentStr[CurrentCharNum];
        }
    }
}
