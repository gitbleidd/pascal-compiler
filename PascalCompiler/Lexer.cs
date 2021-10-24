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

        private IOModule _io { get; }

        public Dictionary<string, int> KeyWords { get; } = new Dictionary<string, int>()
        {
            {"nil", 100},
            {"not", 101},
            {"and", 102},
            {"div", 103},

            {"packed", 104},
            {"array", 105},
            {"of", 106},
            {"file", 107},
            {"set", 108},
            {"record", 109},
            {"end", 110},
            {"case", 111},

            {"or", 112},
            {"function", 113},
            {"var", 114},
            {"procedure", 115},

            {"begin", 116},
            {"if", 117},
            {"then", 118},
            {"else", 119},
            {"while", 120},
            {"do", 121},
            {"repeat", 122},
            {"until", 123},
            {"for", 124},
            {"to", 125},
            {"downto", 126},
            {"with", 127},
            {"goto", 128},

            {"label", 129},
            {"const", 130},
            {"type", 131},
            {"program", 132},
            {"mod", 133},
            {"in", 134}
        };

        public Lexer(IOModule IO)
        {
            _io = IO;
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
            // 1. Чтение спец. символов '\n','\r','\t' итд, а также комментариев (одно/многострочных)
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
                    _position++;
                    if (Current == '.')
                    {
                        return new SyntaxToken(TokenType.DoubleDotToken, _position++);
                    }
                    else
                    {
                        return new SyntaxToken(TokenType.DotToken, _position);
                    }
                case '\'':
                    var sb = new StringBuilder();
                    int start = _position;
                    Next();
                    while (Current != '\'')
                    {
                        if (Current == '\0' || Current == '\n' || Current == '\r')
                        {
                            //TODO ошибка строка не закрыта
                            Console.WriteLine($"Ошибка инициализации строковой константы (Line:{_text.GetLineIndex(start) + 1})");
                            return new SyntaxToken(TokenType.BadToken, start);
                        }

                        sb.Append(Current);
                        Next();
                    }
                    Next();
                    return new SyntaxToken(TokenType.StringConstToken, sb.ToString(), start);
                case ' ':
                case '\t':
                    return new SyntaxToken(TokenType.SpaceToken, _position++);
            }

            // Сканируем идентификатор или ключевое слово
            if (Current.IsAsciiLetter())
            {
                int start = _position;
                int length = 0;

                // Макс. длина не ограничена
                while (Current.IsAsciiLetter() || char.IsNumber(Current))
                {
                    length++;
                    Next();
                }
                string identifierName = _text.TextSubstr(start, length);

                // Ключевое слово
                if (KeyWords.TryGetValue(identifierName.ToLower(), out int wordNum))
                {
                    return new SyntaxToken((TokenType)wordNum, identifierName, start);
                }

                // Идентификатор
                return new SyntaxToken(TokenType.IdentifierToken, identifierName, start);
            }

            // Сканируем целую или вещественную беззнаковую константу
            if (char.IsNumber(Current))
            {
                int start = _position;
                while (char.IsDigit(Current))
                    _position++;
                int integerLen = _position - start;
                int realLen = -1;

                // Read unsigned float const
                if (Current == '.' && NextChar != '.')
                {
                    // Считываем вещ. конст
                    int startReal = _position;
                    _position++;

                    while (char.IsDigit(Current))
                        _position++;

                    if (_position - (startReal + 1) == 0)
                    {
                        //TODO ошибка инициализации вещ. константы
                        Console.WriteLine($"Ошибка инициализации вещественной константы (Line:{_text.GetLineIndex(_position) + 1})");
                        return new SyntaxToken(TokenType.BadToken, _position++);
                    }

                    // Считываем порядок вещ. константы (Scale factor)
                    if (Current == 'e' || Current == 'E')
                    {
                        _position++;
                        if (!(Current == '-' || Current == '+' || char.IsDigit(Current)))
                        {
                            //TODO ошибка инициализации вещ. константы
                            Console.WriteLine($"Ошибка инициализации вещественной константы (Line:{_text.GetLineIndex(_position) + 1})");
                            return new SyntaxToken(TokenType.BadToken, _position++);
                        }
                        _position++;

                        while (char.IsDigit(Current))
                            _position++;
                    }

                    realLen = integerLen + (_position - startReal);
                }
                else if (Current == 'e' || Current == 'E')
                {
                    int startReal = _position;
                    _position++;
                    
                    while (char.IsDigit(Current)) // Считываем порядок
                        _position++;
                    realLen = integerLen + (_position - startReal);
                }

                // Parse float const
                if (realLen > 0 && realLen != integerLen)
                {
                    try
                    {
                        float realNum = float.Parse(_text.TextSubstr(start, realLen), System.Globalization.CultureInfo.InvariantCulture);
                        return new SyntaxToken(TokenType.FloatConstToken, realNum, start);

                    }
                    catch (Exception e)
                    {
                        //TODO ошибка float константа превысила допустимый предел
                        Console.WriteLine($"{e.Message} (Line:{_text.GetLineIndex(_position) + 1})");
                    }
                }

                // Parse unsigned integer const
                try
                {
                    int intNum = int.Parse(_text.TextSubstr(start, integerLen));
                    return new SyntaxToken(TokenType.IntConstToken, intNum, start);
                }
                catch (Exception e)
                {
                    //TODO ошибка int константа превысила допустимый предел
                    Console.WriteLine($"{e.Message} (Line:{_text.GetLineIndex(_position) + 1})");
                }
            }

            return new SyntaxToken(TokenType.BadToken, _position++);
        }

        private void Next() => _position++;
    }
}
