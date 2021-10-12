using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PascalCompiler.Extensions;
using PascalCompiler.Text;
using PascalCompiler.Token;

namespace PascalCompiler
{
    class Lexer
    {
        private readonly SourceText _text;
        private int _position;
        private char Current 
        { 
            get
            {
                if (_position >= _text.Length)
                {
                    return '\0';
                }
                return _text[_position];
            }
        }

        private char NextChar
        {
            get
            {
                if (_position + 1 >= _text.Length)
                {
                    return '\0';
                }
                return _text[_position + 1];
            }
        }

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
            SpaceToken,

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
            _text = IO.sourceText;
            _position = 0;
        }

        private void ReadUnnecessary()
        {
            bool done = false;

            while (!done)
            {
                switch (Current)
                {
                    case '\0':
                        done = true;
                        break;
                    case '{':
                        ReadMultilineComment();
                        break;
                    case '\n':case '\r':
                        Next();
                        break;
                    case ' ': case '\t':
                        ReadWhiteSpace();
                        break;
                    default:
                        done = true;
                        break;
                }
            }
        }

        private void ReadMultilineComment()
        {
            int _start = _position;
            _position++;
            var done = false;

            while (!done)
            {
                if (Current == '\0')
                {
                    throw new Exception($"Многострочный комментарий не завершен. (Line:{_text.GetLineIndex(_start) + 1})");
                    done = true;
                }
                else if (Current == '}')
                {
                    done = true;
                    _position++;
                }
                _position++;
            }
        }

        private void ReadWhiteSpace()
        {
            var done = false;

            while (!done)
            {
                if (Current == ' ' || Current == '\t')
                    _position++;
                return;
            }
        }

        public SyntaxToken GetNextToken()
        {
            // 1. Чтение спец. символов '\n','\r','\t' итд; комментариев (одно/много-строчных)
            ReadUnnecessary();

            // 2. Чтение токена
            switch (Current)
            {
                case '\0':
                    return new SyntaxToken(TokenType.EndOfFileToken, '\0', _position);
                case '+':
                    return new SyntaxToken(TokenType.PlusToken, _position++);
                case '-':
                    return new SyntaxToken(TokenType.MinusToken, _position++);
                case '*':
                    return new SyntaxToken(TokenType.MultToken, _position++);
                case '/':
                    return new SyntaxToken(TokenType.DivisionToken, _position++);
                case '(':
                    return new SyntaxToken(TokenType.LeftRoundBracketToken, _position++);
                case ')':
                    return new SyntaxToken(TokenType.RightRoundBracketToken, _position++);
                case '<':
                    Next();
                    if (Current == '=')
                    {
                        return new SyntaxToken(TokenType.LessOrEqualToken, _position++); // '<='
                    }
                    else if (Current == '>')
                    {
                        return new SyntaxToken(TokenType.NotEqualToken, _position++); // '<>'
                    }
                    else
                    {
                        return new SyntaxToken(TokenType.LessToken, _position); // '<'
                    }
                case '>':
                    Next();
                    if (Current == '=')
                    {
                        return new SyntaxToken(TokenType.GreaterOrEqualToken, _position++); // '>='
                    }
                    else
                    {
                        return new SyntaxToken(TokenType.GreaterToken, _position); // '>'
                    }
                case '=':
                    return new SyntaxToken(TokenType.EqualToken, _position++);
                case ':':
                    Next();
                    if (Current == '=')
                    {
                        return new SyntaxToken(TokenType.AssignmentToken, _position++); // ':='
                    }
                    else
                    {
                        return new SyntaxToken(TokenType.ColonToken, _position); // ':'
                    }
                case ';':
                    return new SyntaxToken(TokenType.SemicolonToken, _position++);
                case '[':
                    return new SyntaxToken(TokenType.LeftSquareBracketToken, _position++);
                case ']':
                    return new SyntaxToken(TokenType.RightSquareBracketToken, _position++);
                case ',':
                    return new SyntaxToken(TokenType.CommaToken, _position++);
                case '.':
                    return new SyntaxToken(TokenType.DotToken, _position++);
                case '^':
                    return new SyntaxToken(TokenType.CaretToken, _position++);
                case '\'':
                    var sb = new StringBuilder();
                    int start = _position;
                    Next();
                    while (Current != '\'')
                    {
                        if (Current == '\0' || Current == '\n' || Current == '\r') 
                            throw new Exception($"Ошибка инициализации строковой константы (Line:{_text.GetLineIndex(_position) + 1})");

                        sb.Append(Current);
                        Next();
                    }
                    Next();
                    return new SyntaxToken(TokenType.StringConstToken, sb.ToString(), start);
                case ' ':
                    return new SyntaxToken(TokenType.SpaceToken, _position++);
            }

            // Сканируем идентификатор или ключевое слово
            if (Current.IsAsciiLetter())
            {
                int start = _position;
                
                int maxlength = 127;
                int length = 0;
                while ((Current.IsAsciiLetter() || Current.IsNumber())
                    && length < maxlength)
                {
                    length++;
                    //identifierName += Current;
                    Next();
                }
                string identifierName = _text.TextSubstr(start, length);

                // Проверка на ключевое слово
                if (KeyWords.TryGetValue(identifierName.ToLower(), out int wordNum))
                {
                    return new SyntaxToken((TokenType)wordNum, identifierName, start);
                }

                // Идентификатор
                return new SyntaxToken(TokenType.IdentifierToken, identifierName, start);
            }

            // Сканируем целую или вещественную константу
            if (char.IsNumber(Current))
            {
                int start = _position;
                int intgerLen = ReadNumLength();

                // Float part
                if (Current == '.')
                {
                    Next();
                    int realLen = intgerLen + 1 + ReadNumLength();
                    try
                    {
                        float realNum = float.Parse(_text.TextSubstr(start, realLen), System.Globalization.CultureInfo.InvariantCulture);
                        return new SyntaxToken(TokenType.FloatConstToken, realNum, start);

                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e.Message);
                    }
                }

                // Integer part
                try
                {
                    int intNum = int.Parse(_text.TextSubstr(start, intgerLen));
                    return new SyntaxToken(TokenType.IntConstToken, intNum, start);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                }
            }

            return new SyntaxToken(TokenType.BadToken, _position++);
        }

        private int ReadNumLength()
        {
            int length = 0;
            while (char.IsNumber(Current))
            {
                length++;
                Next();
            }
            return length;
        }

        private void Next()
        {
            _position++;
        }
    }
}
