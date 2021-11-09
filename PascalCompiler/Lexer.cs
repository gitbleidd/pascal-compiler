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
        private int _position
        {
            get => _io.Position;
        }

        private char Current 
        {
            get => _io.CurrentChar;
        }

        private char LookaheadChar
        {
            get =>  _io.LookaheadChar;
        }

        private IOModule _io { get; }

        public Dictionary<string, SpecialSymbolType> KeyWords { get; } = new Dictionary<string, SpecialSymbolType>()
        {
            {"nil", SpecialSymbolType.NilToken},
            {"not", SpecialSymbolType.NotToken},
            {"and", SpecialSymbolType.AndToken},
            {"div", SpecialSymbolType.DivToken},

            {"packed", SpecialSymbolType.PackedToken},
            {"array", SpecialSymbolType.ArrayToken},
            {"of", SpecialSymbolType.OfToken},
            {"file", SpecialSymbolType.FileToken},
            {"set", SpecialSymbolType.SetToken},
            {"record", SpecialSymbolType.RecordToken},
            {"end", SpecialSymbolType.EndToken},
            {"case", SpecialSymbolType.CaseToken},

            {"or", SpecialSymbolType.OrToken},
            {"function", SpecialSymbolType.FunctionToken},
            {"var", SpecialSymbolType.VarToken},
            {"procedure", SpecialSymbolType.ProcedureToken},

            {"begin", SpecialSymbolType.BeginToken},
            {"if", SpecialSymbolType.IfToken},
            {"then", SpecialSymbolType.ThenToken},
            {"else", SpecialSymbolType.ElseToken},
            {"while", SpecialSymbolType.WhileToken},
            {"do", SpecialSymbolType.DoToken},
            {"repeat", SpecialSymbolType.RepeatToken},
            {"until", SpecialSymbolType.UntilToken},
            {"for", SpecialSymbolType.ForToken},
            {"to", SpecialSymbolType.ToToken},
            {"downto", SpecialSymbolType.DowntoToken},
            {"with", SpecialSymbolType.WithToken},
            {"goto", SpecialSymbolType.GotoToken},

            {"label", SpecialSymbolType.LabelToken},
            {"const", SpecialSymbolType.ConstToken},
            {"type", SpecialSymbolType.TypeToken},
            {"program", SpecialSymbolType.ProgramToken},
            {"mod", SpecialSymbolType.ModToken},
            {"in",SpecialSymbolType.InToken}
        };

        public Lexer(IOModule IO)
        {
            _io = IO;
            _io.NextChar(); // Считываем первый символ
        }

        public void NextPos()
        {
            _io.NextChar();
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
                        NextPos();
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
            NextPos();
            var done = false;

            while (!done)
            {
                if (Current == '\0' || Current == '{')
                {
                    _io.AddError(_start, CompilerError.CommentWithoutEnd);
                    done = true;
                }
                else if (Current == '}')
                {
                    done = true;
                }

                NextPos();
            }
        }

        private void ReadWhiteSpace()
        {
            var done = false;

            while (!done)
            {
                if (Current == ' ' || Current == '\t')
                    NextPos();
                return;
            }
        }

        public LexicalToken GetNextToken()
        {
            // 1. Чтение спец. символов '\n','\r','\t' итд, а также комментариев (одно/многострочных)
            ReadUnnecessary();

            LexicalToken token = null;

            // 2. Чтение токена
            switch (Current)
            {
                case '\0':
                    token = new TriviaToken(_position, TriviaTokenType.EndOfFileToken);
                    break;
                case '+':
                    token = new SpecialSymbolToken(_position, SpecialSymbolType.PlusToken);
                    NextPos();
                    break;
                case '-':
                    token = new SpecialSymbolToken(_position, SpecialSymbolType.MinusToken);
                    NextPos();
                    break;
                case '*':
                    token = new SpecialSymbolToken(_position, SpecialSymbolType.MultToken);
                    NextPos();
                    break;
                case '/':
                    token = new SpecialSymbolToken(_position, SpecialSymbolType.DivisionToken);
                    NextPos();
                    break;
                case '(':
                    token = new SpecialSymbolToken(_position, SpecialSymbolType.LeftRoundBracketToken);
                    NextPos();
                    break;
                case ')':
                    token = new SpecialSymbolToken(_position, SpecialSymbolType.RightRoundBracketToken);
                    NextPos();
                    break;
                case '<':
                    NextPos();
                    if (Current == '=')
                    {
                        token = new SpecialSymbolToken(_position, SpecialSymbolType.LessOrEqualToken); // '<='
                        NextPos();
                    }
                    else if (Current == '>')
                    {
                        token = new SpecialSymbolToken(_position, SpecialSymbolType.NotEqualToken); // '<>'
                        NextPos();
                    }
                    else
                    {
                        token = new SpecialSymbolToken(_position, SpecialSymbolType.LessToken); // '<'
                    }
                    break;
                case '>':
                    NextPos();
                    if (Current == '=')
                    {
                        token = new SpecialSymbolToken(_position, SpecialSymbolType.GreaterOrEqualToken); // '>='
                        NextPos();
                    }
                    else
                    {
                        token = new SpecialSymbolToken(_position, SpecialSymbolType.GreaterToken); // '>'
                    }
                    break;
                case '=':
                    token = new SpecialSymbolToken(_position, SpecialSymbolType.EqualToken);
                    NextPos();
                    break;
                case ':':
                    NextPos();
                    if (Current == '=')
                    {
                        token = new SpecialSymbolToken(_position, SpecialSymbolType.AssignmentToken); // ':='
                        NextPos();
                    }
                    else
                    {
                        token = new SpecialSymbolToken(_position, SpecialSymbolType.ColonToken); // ':'
                    }
                    break;
                case ';':
                    token = new SpecialSymbolToken(_position, SpecialSymbolType.SemicolonToken);
                    NextPos();
                    break;
                case '[':
                    token = new SpecialSymbolToken(_position, SpecialSymbolType.LeftSquareBracketToken);
                    NextPos();
                    break;
                case ']':
                    token = new SpecialSymbolToken(_position, SpecialSymbolType.RightSquareBracketToken);
                    NextPos();
                    break;
                case ',':
                    token = new SpecialSymbolToken(_position, SpecialSymbolType.CommaToken);
                    NextPos();
                    break;
                case '.':
                    NextPos();
                    if (Current == '.')
                    {
                        token = new SpecialSymbolToken(_position, SpecialSymbolType.DoubleDotToken);
                        NextPos();
                    }
                    else
                    {
                        token = new SpecialSymbolToken(_position, SpecialSymbolType.DotToken);
                    }
                    break;
                case '\'':
                    var sb = new StringBuilder();
                    int start = _position;
                    NextPos();
                    bool hasError = false;
                    while (Current != '\'')
                    {
                        hasError = Current == '\0' || Current == '\n' || Current == '\r';
                        if (hasError)
                        {
                            _io.AddError(start, CompilerError.StringExceedsLine);
                            token = new TriviaToken(start, TriviaTokenType.BadToken);
                            break;
                        }

                        sb.Append(Current);
                        NextPos();
                    }
                    if (!hasError)
                    {
                        NextPos();
                        token = new ConstToken<string>(start, sb.ToString());
                    }
                    break;
                case ' ':
                case '\t':
                    token = new TriviaToken(_position, TriviaTokenType.SpaceToken);
                    NextPos();
                    break;
                default:
                    if (char.IsDigit(Current))
                    {
                        token = ReadNum();
                    }
                    else if (Current.IsAsciiLetter())
                    {
                        token = ReadIdentifier();
                    }
                    else
                    {
                        _io.AddError(_position, CompilerError.LexicalError);
                        token = new TriviaToken(_position, TriviaTokenType.UnknownSymbol);
                        NextPos();
                    }
                    break;
                    
            }
            return token;
        }

        // Считывает целую или вещественную беззнаковую константу
        private LexicalToken ReadNum()
        {
            int start = _position;
            while (char.IsDigit(Current))
                NextPos();
            int integerLen = _position - start;
            int realLen = -1;

            // Read unsigned float const
            if (Current == '.' && LookaheadChar != '.')
            {
                // Считываем вещ. конст
                int startReal = _position;
                NextPos();

                while (char.IsDigit(Current))
                    NextPos();

                if (_position - (startReal + 1) == 0)
                {
                    _io.AddError(start, CompilerError.ConstError);
                    return new TriviaToken(start, TriviaTokenType.BadToken);
                }

                // Считываем порядок вещ. константы (Scale factor)
                if (Current == 'e' || Current == 'E')
                {
                    NextPos();
                    if (!(Current == '-' || Current == '+' || char.IsDigit(Current)))
                    {
                        _io.AddError(start, CompilerError.ConstError);
                        return new TriviaToken(start, TriviaTokenType.BadToken);
                    }
                    NextPos();

                    while (char.IsDigit(Current))
                        NextPos();
                }

                realLen = integerLen + (_position - startReal);
            }
            else if (Current == 'e' || Current == 'E')
            {
                int startReal = _position;
                NextPos();

                if (!char.IsDigit(Current))
                {
                    _io.AddError(startReal, CompilerError.ConstError);
                    return new TriviaToken(start, TriviaTokenType.BadToken);
                }

                while (char.IsDigit(Current)) // Считываем порядок
                    NextPos();

                realLen = integerLen + (_position - startReal);
            }

            // Parse double const
            if (realLen > 0 && realLen != integerLen)
            {
                try
                {
                    double doubleValue = double.Parse(_io.TextSubstr(start, realLen), System.Globalization.CultureInfo.InvariantCulture);
                    return new ConstToken<double>(start, doubleValue);

                }
                catch (Exception)
                {
                    _io.AddError(start, CompilerError.OverflowException);
                    return new TriviaToken(start, TriviaTokenType.BadToken);
                }
            }

            // Parse unsigned integer const
            try
            {
                int intValue = int.Parse(_io.TextSubstr(start, integerLen));
                return new ConstToken<int>(start, intValue);
            }
            catch (Exception)
            {
                _io.AddError(start, CompilerError.OverflowException);
                return new TriviaToken(start, TriviaTokenType.BadToken);
            }
        }

        // Считывает идентификатор или ключевое слово
        private LexicalToken ReadIdentifier()
        {
            int start = _position;
            int length = 0;

            // Макс. длина не ограничена
            while (Current.IsAsciiLetter() || char.IsNumber(Current))
            {
                length++;
                NextPos();
            }
            string identifierName = _io.TextSubstr(start, length);

            // Ключевое слово
            if (KeyWords.TryGetValue(identifierName.ToLower(), out SpecialSymbolType type))
            {
                return new SpecialSymbolToken(start, type);
            }

            // Идентификатор
            return new IdentifierToken(start, identifierName);
        }
    }
}
