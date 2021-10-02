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

        public enum TokenType
        {
            // OperationToken
            LeftRoundBracketToken,
            RightRoundBracketToken,
            PlusToken,
            MinusToken,
            MultToken,
            DivToken,
            GreaterToken,
            LessToken,
            GreaterOrEqualToken,
            LessOrEqualToken,
            EqualToken,
            NotEqualToken,

            SemicolonToken,
            ColonToken,
            AssignmentToken,
            LeftSquareBracketToken,
            RightSquareBracketToken,
            LeftCurlyBracketToken,
            RightCurlyBracketToken,
            ModToken,


            // ConstToken
            IntToken,
            FloatToken,
            StringToken,

            // VarToken
            VarToken,
            BadToken
        }

        public Lexer(IOModule IO)
        {
            this.IO = IO;
            CurrentStr = IO.ReadNextLine();
            CurrentCharNum = 0;
            Ch = CurrentStr[CurrentCharNum];
        }

        public SyntaxToken GetNextToken()
        {
            if (CurrentStr == null)
                return null;

            while (Ch == ' ') GetNextChar();

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
                    GetNextChar();
                }

                // TODO Развилка -> проверяем является ли текущее слово ключевым.

                return new SyntaxToken(TokenType.VarToken, identifierName); // 'int num'
            }

            // Числовая константа
            // Сканируем целую или вещественную константу
            if (char.IsNumber(Ch))
            {
                // TODO вещественная константа
                int maxInt = 32767;
                int num = 0;
                while (char.IsNumber(Ch))
                {
                    int digit = Ch - '0';
                    if (num < maxInt / 10 || (num == maxInt / 10 && digit <= maxInt % 10))
                        num = 10 * num + digit;
                    else
                    {
                        num = 0;
                        throw new Exception("Константа превышает допустимый предел");
                    }
                    GetNextChar();
                }
                return new SyntaxToken(TokenType.IntToken, num); // 'int num'
            }

            switch (Ch)
            {
                case '+':
                    GetNextChar();
                    return new SyntaxToken(TokenType.PlusToken);
                case '-':
                    GetNextChar();
                    return new SyntaxToken(TokenType.MinusToken);
                case '*':
                    GetNextChar();
                    return new SyntaxToken(TokenType.MultToken);
                case '/':
                    GetNextChar();
                    return new SyntaxToken(TokenType.DivToken);
                case '(':
                    GetNextChar();
                    return new SyntaxToken(TokenType.LeftRoundBracketToken);
                case ')':
                    GetNextChar();
                    return new SyntaxToken(TokenType.RightRoundBracketToken);
                case '<':
                    GetNextChar();
                    if (Ch == '=')
                    {
                        GetNextChar();
                        return new SyntaxToken(TokenType.LessOrEqualToken); // '<='
                    }
                    else if (Ch == '>')
                    {
                        GetNextChar();
                        return new SyntaxToken(TokenType.NotEqualToken); // '<>'
                    }
                    else
                        return new SyntaxToken(TokenType.LessToken); // '<'
                case '>':
                    GetNextChar();
                    if (Ch == '=')
                    {
                        GetNextChar();
                        return new SyntaxToken(TokenType.GreaterOrEqualToken); // '>='
                    }
                    else
                        return new SyntaxToken(TokenType.GreaterToken); // '>'
                case '=':
                    GetNextChar();
                    return new SyntaxToken(TokenType.EqualToken);
                case ':':
                    GetNextChar();
                    if (Ch == '=')
                    {
                        GetNextChar();
                        return new SyntaxToken(TokenType.AssignmentToken); // ':='
                    }
                    else
                        return new SyntaxToken(TokenType.ColonToken); // ':'
                case ';':
                    GetNextChar();
                    return new SyntaxToken(TokenType.SemicolonToken); 
                case '[':
                    GetNextChar();
                    return new SyntaxToken(TokenType.LeftSquareBracketToken);
                case ']':
                    GetNextChar();
                    return new SyntaxToken(TokenType.RightSquareBracketToken);
                case '{':
                    GetNextChar();
                    return new SyntaxToken(TokenType.LeftCurlyBracketToken);
                case '}':
                    GetNextChar();
                    return new SyntaxToken(TokenType.RightCurlyBracketToken);
                case '%':
                    GetNextChar();
                    return new SyntaxToken(TokenType.ModToken);
            }

            return null;
        }

        private void GetNextChar()
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
