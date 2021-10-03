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
        private char Ch 
        { 
            get
            {
                if (CurrentCharNum >= CurrentStr.Length)
                {
                    CurrentStr = IO.ReadNextLine();
                    if (CurrentStr == null)
                    {
                        return '\0';
                    }
                    CurrentCharNum = 0;
                }
                return CurrentStr[CurrentCharNum];
            }
        }

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
            DivisionToken,
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

            // ConstToken
            IntConstToken,
            FloatConstToken,
            StringConstToken,

            // VarToken
            IdentifierToken,
            BadToken,
            CommaToken,
            DotToken,
            CaretToken,

            EndOfFileToken,

            // ReversedWords
            AbsoluteToken = 100,
            AndToken,
            ArrayToken,
            AsmToken,
            BeginToken,
            CaseToken,
            ConstToken,
            ConstructorToken,
            DestructorToken,
            DivToken,
            DoToken,
            DowntoToken,
            ElseToken,
            EndToken,
            FileToken,
            ForToken,
            FunctionToken,
            GotoToken,
            IfToken,
            ImplementationToken,
            InToken,
            InheritedToken,
            InlineToken,
            InterfaceToken,
            LabelToken,
            ModToken,
            NilToken,
            NotToken,
            ObjectToken,
            OfToken,
            OperatorToken,
            OrToken,
            PackedToken,
            ProcedureToken,
            ProgramToken,
            RecordToken,
            ReintroduceToken,
            RepeatToken,
            SelfToken,
            SetToken,
            ShlToken,
            ShrToken,
            StringToken,
            ThenToken,
            ToToken,
            TypeToken,
            UnitToken,
            UntilToken,
            UsesToken,
            VarToken,
            WhileToken,
            WithToken,
            XorToken
        }

        public Dictionary<string, int> KeyWords { get; private set; } = new Dictionary<string, int>()
        {
            {"absolute", 100},
            {"and", 101},
            {"array", 102},
            {"asm", 103},
            {"begin", 104},
            {"case", 105},
            {"const", 106},
            {"constructor", 107},
            {"destructor", 108},
            {"div", 109},
            {"do", 110},
            {"downto", 111},
            {"else", 112},
            {"end", 113},
            {"file", 114},
            {"for", 115},
            {"function", 116},
            {"goto", 117},
            {"if", 118},
            {"implementation", 119},
            {"in", 120},
            {"inherited", 121},
            {"inline", 122},
            {"interface", 123},
            {"label", 124},
            {"mod", 125},
            {"nil", 126},
            {"not", 127},
            {"object", 128},
            {"of", 129},
            {"operator", 130},
            {"or", 131},
            {"packed", 132},
            {"procedure", 133},
            {"program", 134},
            {"record", 135},
            {"reintroduce", 136},
            {"repeat", 137},
            {"self", 138},
            {"set", 139},
            {"shl", 140},
            {"shr", 141},
            {"string", 142},
            {"then", 143},
            {"to", 144},
            {"type", 145},
            {"unit", 146},
            {"until", 147},
            {"uses", 148},
            {"var", 149},
            {"while", 150},
            {"with", 151},
            {"xor", 152}
        };

        public Lexer(IOModule IO)
        {
            this.IO = IO;
            CurrentStr = IO.ReadNextLine();
            CurrentCharNum = 0;
        }

        public SyntaxToken GetNextToken()
        {
            if (Ch == '\0')
                return new SyntaxToken(TokenType.EndOfFileToken, '\0', CurrentCharNum); //EndOfFileToken

            while (Ch == ' ') GetNextChar();

            // Идентификатор
            // Сканируем идентификатор или ключевое слово
            if (Ch.IsAsciiLetter())
            {
                int start = CurrentCharNum;

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

                // Проверяем является ли текущее слово ключевым.
                if (KeyWords.TryGetValue(identifierName.ToLower(), out int wordNum))
                {
                    return new SyntaxToken((TokenType) wordNum, identifierName, start);
                }

                // Идентификатор
                return new SyntaxToken(TokenType.IdentifierToken, identifierName, start);
            }

            // Числовая константа
            // Сканируем целую или вещественную константу
            if (char.IsNumber(Ch))
            {
                int start = CurrentCharNum;

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
                return new SyntaxToken(TokenType.IntConstToken, num, start); // 'int num'
            }

            switch (Ch)
            {
                case '+':
                    return new SyntaxToken(TokenType.PlusToken, CurrentCharNum++);
                case '-':
                    return new SyntaxToken(TokenType.MinusToken, CurrentCharNum++);
                case '*':
                    return new SyntaxToken(TokenType.MultToken, CurrentCharNum++);
                case '/':
                    return new SyntaxToken(TokenType.DivisionToken, CurrentCharNum++);
                case '(':
                    return new SyntaxToken(TokenType.LeftRoundBracketToken, CurrentCharNum++);
                case ')':
                    return new SyntaxToken(TokenType.RightRoundBracketToken, CurrentCharNum++);
                case '<':
                    GetNextChar();
                    if (Ch == '=')
                    {
                        return new SyntaxToken(TokenType.LessOrEqualToken, CurrentCharNum++); // '<='
                    }
                    else if (Ch == '>')
                    {
                        return new SyntaxToken(TokenType.NotEqualToken, CurrentCharNum++); // '<>'
                    }
                    else
                    {
                        return new SyntaxToken(TokenType.LessToken, CurrentCharNum++); // '<'
                    }
                case '>':
                    GetNextChar();
                    if (Ch == '=')
                    {
                        return new SyntaxToken(TokenType.GreaterOrEqualToken, CurrentCharNum++); // '>='
                    }
                    else
                    {
                        return new SyntaxToken(TokenType.GreaterToken, CurrentCharNum++); // '>'
                    }
                case '=':
                    return new SyntaxToken(TokenType.EqualToken, CurrentCharNum++);
                case ':':
                    GetNextChar();
                    if (Ch == '=')
                    {
                        return new SyntaxToken(TokenType.AssignmentToken, CurrentCharNum++); // ':='
                    }
                    else
                    {
                        return new SyntaxToken(TokenType.ColonToken, CurrentCharNum++); // ':'
                    }
                case ';':
                    return new SyntaxToken(TokenType.SemicolonToken, CurrentCharNum++);
                case '[':
                    return new SyntaxToken(TokenType.LeftSquareBracketToken, CurrentCharNum++);
                case ']':
                    return new SyntaxToken(TokenType.RightSquareBracketToken, CurrentCharNum++);
                case '{':
                    return new SyntaxToken(TokenType.LeftCurlyBracketToken, CurrentCharNum++);
                case '}':
                    return new SyntaxToken(TokenType.RightCurlyBracketToken, CurrentCharNum++);
                case ',':
                    return new SyntaxToken(TokenType.CommaToken, CurrentCharNum++);
                case '.':
                    return new SyntaxToken(TokenType.DotToken, CurrentCharNum++);
                case '^':
                    return new SyntaxToken(TokenType.CaretToken, CurrentCharNum++);
            }

            return new SyntaxToken(TokenType.BadToken, CurrentCharNum++);
        }

        private void GetNextChar()
        {
            CurrentCharNum++;
        }
    }
}
