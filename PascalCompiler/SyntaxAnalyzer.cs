using PascalCompiler.Token;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PascalCompiler
{
    class SyntaxAnalyzer
    {
        private Lexer _lexer;
        private LexicalToken _token;

        public SyntaxAnalyzer(Lexer lexer)
        {
            _lexer = lexer;
        }

        private void NextToken()
        {
            _token = _lexer.GetNextToken();
        }

        private bool IsFileEnded()
        {
            if (_token is TriviaToken)
            {
                var triviaT = (TriviaToken)_token;
                return triviaT.Type == TriviaTokenType.EndOfFileToken;
            }
            return false;
        }

        private void Accept(SpecialSymbolType symbolType)
        {
            if (IsFileEnded())
            {

            }

            if (_token is SpecialSymbolToken)
            {
                var specialSymbolToken = (SpecialSymbolToken)_token;
                if (specialSymbolToken.Type != symbolType)
                    throw new Exception($"Ожидался {symbolType} вместо {specialSymbolToken.Type}");
            }
            else
            {
                throw new Exception();
            }

            NextToken();
        }

        private void Accept(HashSet<SpecialSymbolType> specialSymbols)
        {
            if (IsFileEnded())
            {

            }

            var specialSymbolToken = _token as SpecialSymbolToken;
            if (specialSymbolToken is null)
            {
                throw new Exception($"Ожидался тип токена вместо {_token.GetType()}");
            }
            else if (!specialSymbols.Contains(specialSymbolToken.Type))
            {
                throw new Exception($"Ожидался другой спец. символа вместо {specialSymbolToken.Type}");
            }

            NextToken();
        }

        public void Accept<T>()
        {
            if (IsFileEnded())
            {

            }

            if (_token is not T)
            {
                throw new Exception();
            }
            NextToken();
        }

        public void Start()
        {
            NextToken(); // Получение первого токена.
            Program(); // Запуск анализа программы.
        }

        private void Program()
        {
            Accept(SpecialSymbolType.ProgramToken);
            Accept<IdentifierToken>();
            Accept(SpecialSymbolType.SemicolonToken);
            Block();
            //Accept(SpecialSymbolType.DotToken);
        }

        private void Block()
        {
            // 1. Раздел меток
            // 2. Раздел констант
            // 3. Раздел типов (доп.)
            
            VariablePart(); // 4. Раздел переменных
            // 5. Раздел процедур и функций

            StatementPart(); // 6. Раздел операторов
        }

        private void VariablePart()
        {
            // Анализ конструкции <раздел переменных>.
            // <раздел переменных>::= var <описание однотипных переменных>; {< описание однотипных переменных>; } | < пусто >
            
            Accept(SpecialSymbolType.VarToken);
            
            do
            {
                VariableDeclaration();
                Accept(SpecialSymbolType.SemicolonToken);
            } while (_token is IdentifierToken);
        }

        private void VariableDeclaration()
        {
            // Анализ конструкции <описание однотипных переменных>.
            // <описание однотипных переменных>::= <имя>{,<имя>}:<тип>
            Accept<IdentifierToken>();
            
            while(_token is SpecialSymbolToken && ((SpecialSymbolToken)_token).Type == SpecialSymbolType.CommaToken)
            {
                NextToken();
                Accept<IdentifierToken>();
            }
            Accept(SpecialSymbolType.ColonToken);

            Type();
        }

        private void Type()
        {
            // Анализ конструкции <тип>.
            // <простой тип>::= <перечислимый тип> | <ограниченный тип> | <имя типа>
            // <перечислимый тип>::= (< имя >{,< имя >})
            // <ограниченный тип>::=<константа> .. <константа>

            IdentifierToken identifierToken = null;
            if (_token is IdentifierToken)
            {
                identifierToken = (IdentifierToken)_token;

                switch (identifierToken.Name)
                {
                    case "integer":
                        break;
                    case "real":
                        break;
                    case "string":
                        break;
                    case "boolean":
                        break;
                    default:
                        // TODO проверка на типы, описанные пользователем.
                        throw new Exception("Ожидалось имя типа");
                        break;
                }
                NextToken();
            }
            else
            {
                throw new Exception("Ожидалось имя типа");
            }
        }

        private void StatementPart()
        {
            // Анализ конструкции <раздел операторов>::= <составной оператор>.
            // <составной оператор>::= begin <оператор>{;<оператор>} end
            Accept(SpecialSymbolType.BeginToken);
            Statement();

            bool isSemicolon = _token is SpecialSymbolToken && ((SpecialSymbolToken)_token).Type == SpecialSymbolType.SemicolonToken;
            while (isSemicolon)
            {
                NextToken();
                Statement();
                isSemicolon = _token is SpecialSymbolToken && ((SpecialSymbolToken)_token).Type == SpecialSymbolType.SemicolonToken;
            }
            Accept(SpecialSymbolType.EndToken);
        }

        private void Statement()
        {
            // Анализ конструкции <оператор>.
            // <оператор>::= <простой оператор>|<сложный оператор>
            
            // TODO Оператор присваивания и составной оператор.
            SpecialSymbolToken specialSymbolToken = _token as SpecialSymbolToken;
            if (specialSymbolToken == null)
            {
                SimpleStatement();
            }
            else if (specialSymbolToken.Type == SpecialSymbolType.BeginToken)
            {

            }
        }

        private void SimpleStatement()
        {
            // Анализ конструкции <простой оператор>.
            // <простой оператор>::= <оператор присваивания> | <оператор процедуры> | <оператор перехода> | <пустой оператор>
            
            AssignmentStatement(); // <оператор присваивания>
        }

        private void AssignmentStatement()
        {
            // Анализ конструкции <оператор присваивания>.
            // <оператор присваивания>::= <переменная>:= <выражение> | <имя функции>:=<выражение>

            // <переменная>::= <имя переменной> | <компонента переменной> | <указанная переменная>
            // (Не надо) 2. <компонента переменной>::=<индексированная переменная> |<обозначение поля>|<буфер файла>
            // (Не надо) 3. <указанная переменная>::=<переменная-ссылка>

            // 1) Имя переменной
            var variable = _token as IdentifierToken;
            if (variable is null)
                throw new Exception("Ожидалось имя переменной");
            else
                NextToken();
            // TODO проверить есть ли переменная в таблице переменный!
            
            // 2) Оператор присваивания
            Accept(SpecialSymbolType.AssignmentToken);

            // 3) Выражение
            // TODO в зависимости от типа переменной, которой мы присваиваем значение
            // выбрать соответствующий вариант метода SimpleExpression: double(int), string, bool
            double value = SimpleExpression();

            // <выражение>::= <простое выражение> | <простое выражение><операция отношения><простое выражение>

            // <операция отношения>::==|<>|<|<=|>=|>|in
        }

        private double SimpleExpression()
        {

            // <простое выражение>::=<знак><слагаемое> {<аддитивная операция><слагаемое>}
            // <аддитивная операция>::=+|-|or

            // Знак перед слагаемым.
            int sign = 1; 
            var specialSymbolToken = _token as SpecialSymbolToken;
            if (specialSymbolToken != null)
            {
                switch (specialSymbolToken.Type)
                {
                    case SpecialSymbolType.PlusToken:
                        sign = 1;
                        NextToken();
                        break;
                    case SpecialSymbolType.MinusToken:
                        sign = -1;
                        NextToken();
                        break;
                    default:
                        break;
                }
            }
            double left = Term() * sign; // Слагаемое.


            // {<аддитивная операция><слагаемое>}
            specialSymbolToken = _token as SpecialSymbolToken;
            bool isAdditiveOper = true;
            while(specialSymbolToken != null && isAdditiveOper)
            {
                switch (specialSymbolToken.Type)
                {
                    case SpecialSymbolType.PlusToken:
                        NextToken();
                        left += Term();
                        break;
                    case SpecialSymbolType.MinusToken:
                        NextToken();
                        left -= Term();
                        break;
                    default:
                        isAdditiveOper = false;
                        break;
                }
                specialSymbolToken = _token as SpecialSymbolToken;
            }
            return left;
        }

        private double Term()
        {
            // Анализ конструкции <слагаемое>.
            // <слагаемое>::= <множитель>{<мультипликативная операция><множитель>}
            // <мультипликативная операция>::=*|/|div|mod|and
            double left = Factor(); // Множитель.

            var specialSymbolToken = _token as SpecialSymbolToken;
            bool isMultOper = true;
            while (specialSymbolToken != null && isMultOper)
            {
                switch (specialSymbolToken.Type)
                {
                    case SpecialSymbolType.MultToken:
                        NextToken();
                        left *= Factor();
                        break;
                    case SpecialSymbolType.DivisionToken:
                    case SpecialSymbolType.DivToken:
                        NextToken();
                        left /= Factor();
                        break;
                    case SpecialSymbolType.ModToken:
                        NextToken();
                        left %= Factor();
                        break;
                    default:
                        isMultOper = false;
                        break;
                }
                specialSymbolToken = _token as SpecialSymbolToken;
            }
            return left;
        }

        private double Factor()
        {
            // <множитель>::= <переменная> | <константа без знака> | (<выражение>) | <обозначение функции> | <множество> | not <множитель>
            // <константа без знака>::= <число без знака> | <строка> | <имя константы> | nil

            double? left = null;

            if (_token is IdentifierToken)
            {
                // TODO <переменная> | <обозначение функции>
            }

            // Константы
            else if (_token is ConstToken<int>) // число int без знака
            {
                left = ((ConstToken<int>)_token).Value;
                NextToken();
            }
            else if (_token is ConstToken<double>) // число real без знака
            {
                left = ((ConstToken<double>)_token).Value;
                NextToken();
            }
            else if (_token is ConstToken<string>) // строка
            {
                throw new Exception("Ожидалась числовая константа");
            }
            // TODO? имя константы
            // TODO? nil
            else if (_token is SpecialSymbolToken)
            {
                // (<выражение>) | not <множитель>

                Accept(SpecialSymbolType.LeftRoundBracketToken);
                left = SimpleExpression();
                Accept(SpecialSymbolType.RightRoundBracketToken);
            }

            if (left.HasValue)
                return left.Value;
            else
            {
                throw new Exception("Токен не удовлетворяет ни одному из перечисленных: " +
                    "<переменная> | <константа без знака> | (<выражение>) | <обозначение функции> | <множество> | not <множитель>");
            }
        }
    }
}
