using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PascalCompiler.Extensions;
using PascalCompiler.Token;

namespace PascalCompiler
{
    class Lexer
    {
        private int CurrentCharPosition { get; set; }
        private char Current 
        { 
            get
            {
                if (CurrentStr == null)
                    return '\0';

                if (CurrentCharPosition >= CurrentStr.Length)
                {
                    CurrentStr = IO.ReadNextLine();
                    if (CurrentStr == null)
                        return '\0';
                    CurrentCharPosition = 0;
                }
                return CurrentStr[CurrentCharPosition];
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
            CurrentCharPosition = 0;
        }

        public SyntaxToken GetNextToken()
        {
            if (Current == '\0')
                return new SyntaxToken(TokenType.EndOfFileToken, '\0', CurrentCharPosition); //EndOfFileToken

            while (Current == ' ') Next();

            // Обработка комментариев, без обработки экранированных скобок.
            if (Current == '{')
            {
                while (Current != '}')
                {
                    if (Current == '\0')
                        throw new Exception("Ошибка объявления комментария.");
                    Next();
                }
                Next(); // Пропускаем '}'
            }

            // Идентификатор
            // Сканируем идентификатор или ключевое слово
            if (Current.IsAsciiLetter())
            {
                int start = CurrentCharPosition;

                string identifierName = "";
                int maxIndentLen = 127;
                int letterCounter = 0;
                while ((Current.IsAsciiLetter() || Current.IsNumber())
                    && letterCounter < maxIndentLen)
                {
                    letterCounter++;
                    identifierName += Current;
                    Next();
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
            if (char.IsNumber(Current))
            {
                int start = CurrentCharPosition;

                int maxValue = 32767; // Max integer
                int num = 0;
                while (char.IsNumber(Current))
                {
                    int digit = Current - '0';
                    if (num < maxValue / 10 || (num == maxValue / 10 && digit <= maxValue % 10))
                        num = 10 * num + digit;
                    else
                    {
                        num = 0;
                        throw new Exception("Константа превышает допустимый предел");
                    }
                    Next();
                }

                if (Current == '.')
                {
                    Next();
                    // Обработка вещественной константы
                    int num2 = 0;
                    while (char.IsNumber(Current))
                    {
                        int digit = Current - '0';
                        if (num2 < maxValue / 10 || (num2 == maxValue / 10 && digit <= maxValue % 10))
                            num2 = 10 * num2 + digit;
                        else
                        {
                            num2 = 0;
                            throw new Exception("Константа превышает допустимый предел");
                        }
                        Next();
                    }
                    
                    float realNum = float.Parse($"{num}.{num2}", System.Globalization.CultureInfo.InvariantCulture);
                    return new SyntaxToken(TokenType.FloatConstToken, realNum, start);
                }
                else
                    return new SyntaxToken(TokenType.IntConstToken, num, start);
            }

            switch (Current)
            {
                case '+':
                    return new SyntaxToken(TokenType.PlusToken, CurrentCharPosition++);
                case '-':
                    return new SyntaxToken(TokenType.MinusToken, CurrentCharPosition++);
                case '*':
                    return new SyntaxToken(TokenType.MultToken, CurrentCharPosition++);
                case '/':
                    return new SyntaxToken(TokenType.DivisionToken, CurrentCharPosition++);
                case '(':
                    return new SyntaxToken(TokenType.LeftRoundBracketToken, CurrentCharPosition++);
                case ')':
                    return new SyntaxToken(TokenType.RightRoundBracketToken, CurrentCharPosition++);
                case '<':
                    Next();
                    if (Current == '=')
                    {
                        return new SyntaxToken(TokenType.LessOrEqualToken, CurrentCharPosition++); // '<='
                    }
                    else if (Current == '>')
                    {
                        return new SyntaxToken(TokenType.NotEqualToken, CurrentCharPosition++); // '<>'
                    }
                    else
                    {
                        return new SyntaxToken(TokenType.LessToken, CurrentCharPosition); // '<'
                    }
                case '>':
                    Next();
                    if (Current == '=')
                    {
                        return new SyntaxToken(TokenType.GreaterOrEqualToken, CurrentCharPosition++); // '>='
                    }
                    else
                    {
                        return new SyntaxToken(TokenType.GreaterToken, CurrentCharPosition); // '>'
                    }
                case '=':
                    return new SyntaxToken(TokenType.EqualToken, CurrentCharPosition++);
                case ':':
                    Next();
                    if (Current == '=')
                    {
                        return new SyntaxToken(TokenType.AssignmentToken, CurrentCharPosition++); // ':='
                    }
                    else
                    {
                        return new SyntaxToken(TokenType.ColonToken, CurrentCharPosition); // ':'
                    }
                case ';':
                    return new SyntaxToken(TokenType.SemicolonToken, CurrentCharPosition++);
                case '[':
                    return new SyntaxToken(TokenType.LeftSquareBracketToken, CurrentCharPosition++);
                case ']':
                    return new SyntaxToken(TokenType.RightSquareBracketToken, CurrentCharPosition++);
                case ',':
                    return new SyntaxToken(TokenType.CommaToken, CurrentCharPosition++);
                case '.':
                    return new SyntaxToken(TokenType.DotToken, CurrentCharPosition++);
                case '^':
                    return new SyntaxToken(TokenType.CaretToken, CurrentCharPosition++);
                case '\'':
                    string text = "";
                    int start = CurrentCharPosition;
                    Next();
                    while (Current != '\'')
                    {
                        if (Current == '\0') 
                            throw new Exception("Ошибка инициализации строковой константы");

                        text += Current;
                        Next();
                    }
                    Next();
                    return new SyntaxToken(TokenType.StringConstToken, text, start);
            }

            return new SyntaxToken(TokenType.BadToken, CurrentCharPosition++);
        }

        private void Next()
        {
            CurrentCharPosition++;
        }
    }
}
