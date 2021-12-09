using PascalCompiler.Token;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PascalCompiler.Syntax
{
    class SyntaxAnalyzer
    {
        private IOModule _io;
        private Lexer _lexer;
        private LexicalToken _token;

        public SyntaxAnalyzer(IOModule io, Lexer lexer)
        {
            _io = io;
            _lexer = lexer;
        }

        private void NextToken()
        {
            _token = _lexer.GetNextToken();
            
            // Пропускаем ошибочные токены.
            var triviaT = (_token as TriviaToken);
            while (triviaT != null && triviaT.Type != TriviaTokenType.EndOfFileToken)
            {
                _token = _lexer.GetNextToken();
                triviaT = (_token as TriviaToken);
            }
        }

        // Проверяет является ли текущий токен последним.
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
            var specialSymbolToken = _token as SpecialSymbolToken;
            if (specialSymbolToken == null || specialSymbolToken.Type != symbolType)
            {
                throw new Exception($"Ожидался {symbolType}");
            }

            NextToken();
        }

        /*
        private void Accept(params SpecialSymbolType[] specialSymbols)
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
        */

        private void Accept<T>() where T: class
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
            Accept(SpecialSymbolType.DotToken);
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

            var specialSymbolToken = _token as SpecialSymbolToken;
            while (specialSymbolToken?.Type == SpecialSymbolType.CommaToken)
            {
                Accept(SpecialSymbolType.CommaToken);
                Accept<IdentifierToken>();
                specialSymbolToken = _token as SpecialSymbolToken;
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

            if (_token is IdentifierToken)
            {
                var identifierToken = _token as IdentifierToken;
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
                        throw new Exception("Тип не является одним из встроенных: integer, real, string, boolean");
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

            var specialSymbolToken = _token as SpecialSymbolToken;
            bool isSemicolon = specialSymbolToken?.Type == SpecialSymbolType.SemicolonToken;
            while (isSemicolon)
            {
                Accept(SpecialSymbolType.SemicolonToken);
                specialSymbolToken = _token as SpecialSymbolToken;

                if (specialSymbolToken?.Type == SpecialSymbolType.EndToken)
                {
                    Accept(SpecialSymbolType.EndToken);
                    return;
                }

                Statement();
                specialSymbolToken = _token as SpecialSymbolToken;
                isSemicolon = specialSymbolToken?.Type == SpecialSymbolType.SemicolonToken;
            }
            Accept(SpecialSymbolType.SemicolonToken);
        }

        private void Statement()
        {
            // Анализ конструкции <оператор>.
            // <оператор>::= <простой оператор>|<сложный оператор>
            
            SpecialSymbolToken specialSymbolToken = _token as SpecialSymbolToken;
            if (specialSymbolToken == null)
            {
                SimpleStatement();
            }
            else
            {
                CompoundStatement();
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
            // <переменная>::= <имя переменной> 

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
            Expression();
        }

        private void Expression()
        {
            // Анализ конструкции <выражение>.
            // <выражение>::= <простое выражение> | <простое выражение> <операция отношения> <простое выражение>
            SimpleExpression();
            
            var specialSymbolToken = _token as SpecialSymbolToken;
            if (specialSymbolToken is not null)
            {
                // <операция отношения>::= <>|=|<|<=|>=|>|in
                switch (specialSymbolToken.Type)
                {
                    case SpecialSymbolType.NotEqualToken:
                    case SpecialSymbolType.EqualToken:
                    case SpecialSymbolType.LessToken:
                    case SpecialSymbolType.LessOrEqualToken:
                    case SpecialSymbolType.GreaterToken:
                    case SpecialSymbolType.GreaterOrEqualToken:
                    case SpecialSymbolType.InToken:
                        NextToken();
                        SimpleExpression();
                        break;
                    default:
                        break;
                }
            }
        }

        private void SimpleExpression()
        {
            // <простое выражение>::= <знак><слагаемое> {<аддитивная операция><слагаемое>}
            // <аддитивная операция>::= +|-|or

            // Знак перед слагаемым.
            var specialSymbolToken = _token as SpecialSymbolToken;
            if (specialSymbolToken != null)
            {
                switch (specialSymbolToken.Type)
                {
                    case SpecialSymbolType.PlusToken:
                    case SpecialSymbolType.MinusToken:
                    case SpecialSymbolType.OrToken:
                        NextToken();
                        break;
                    default:
                        //throw new Exception("По БНФ <простое выражение> требуется знак!!!!");
                        break;
                }
            }
            Term(); // <слагаемое>

            // {<аддитивная операция><слагаемое>}
            specialSymbolToken = _token as SpecialSymbolToken;
            bool isAdditiveOper = true;
            while(specialSymbolToken != null && isAdditiveOper)
            {
                switch (specialSymbolToken.Type)
                {
                    // +|-|or
                    case SpecialSymbolType.PlusToken:
                    case SpecialSymbolType.MinusToken:
                    case SpecialSymbolType.OrToken:
                        NextToken();
                        Term();
                        break;
                    default:
                        isAdditiveOper = false;
                        break;
                }
                specialSymbolToken = _token as SpecialSymbolToken;
            }
        }

        private void Term()
        {
            // Анализ конструкции <слагаемое>.
            // <слагаемое>::= <множитель>{<мультипликативная операция><множитель>}
            // <мультипликативная операция>::=*|/|div|mod|and
            Factor(); // <множитель>

            var specialSymbolToken = _token as SpecialSymbolToken;
            bool isMultOper = true;
            while (specialSymbolToken != null && isMultOper)
            {
                switch (specialSymbolToken.Type)
                {
                    case SpecialSymbolType.MultToken:
                    case SpecialSymbolType.DivisionToken:
                    case SpecialSymbolType.DivToken:
                    case SpecialSymbolType.ModToken:
                        NextToken();
                        Factor();
                        break;
                    default:
                        isMultOper = false;
                        break;
                }
                specialSymbolToken = _token as SpecialSymbolToken;
            }
        }

        private void Factor()
        {
            // Анализ конструкции <множитель>.
            // <множитель>::= <переменная> | <обозначение функции> | <константа без знака> | (<выражение>) | not <множитель>
            // <константа без знака>::= <число без знака> | <строка> | <имя константы> | nil


            // <переменная> | <обозначение функции> |
            // <имя константы> 
            // true, false - тоже идентификаторы.
            if (_token is IdentifierToken)
            {
                NextToken();
            }

            // <константа без знака>
            else if (_token is ConstToken<int>) // число int без знака
            {
                NextToken();
            }
            else if (_token is ConstToken<double>) // число real без знака
            {
                NextToken();
            }
            else if (_token is ConstToken<string>) // строка
            {
                NextToken();
            }

            // nil | not <множитель> | (<выражение>)
            else if (_token is SpecialSymbolToken)
            {
                var specialSymbolToken = _token as SpecialSymbolToken;
                switch (specialSymbolToken.Type)
                {
                    case SpecialSymbolType.NilToken:
                        Accept(SpecialSymbolType.NilToken);
                        break;
                    case SpecialSymbolType.NotToken:
                        Accept(SpecialSymbolType.NotToken);
                        Factor();
                        break;
                    case SpecialSymbolType.LeftRoundBracketToken:
                        Accept(SpecialSymbolType.LeftRoundBracketToken);
                        SimpleExpression();
                        Accept(SpecialSymbolType.RightRoundBracketToken);
                        break;
                    default:
                        break;
                }
            }
        }

        private void CompoundStatement()
        {
            // Анализ конструкции <сложный оператор>.
            // <сложный оператор>::= <составной оператор> | <выбирающий оператор> | <оператор цикла> | <оператор присоединения>

            var specialSymbolToken = _token as SpecialSymbolToken; // Не может быть null, тк обязательно проверится в анализе конструкции <оператор>.
            switch (specialSymbolToken.Type)
            {
                // <составной оператор>
                case SpecialSymbolType.BeginToken:
                    //CompositeStatement();
                    StatementPart(); // Анализ конструкции <составной оператор>.
                    break;

                // <выбирающий оператор>
                case SpecialSymbolType.IfToken:
                    IfStatement();
                    break;
                case SpecialSymbolType.CaseToken:
                    throw new Exception("Оператор варианта case - не поддерживается.");

                // <оператор цикла>
                case SpecialSymbolType.WhileToken:
                    WhileStatement();
                    break;
                case SpecialSymbolType.RepeatToken:
                    throw new Exception("Оператор цикла с постусловием repeat - не поддерживается.");
                case SpecialSymbolType.ForToken:
                    throw new Exception("Оператор цикла с параметром for - не поддерживается.");

                // <оператор присоединения>
                case SpecialSymbolType.WithToken:
                    throw new Exception("Оператор присоединения with - не поддерживается.");

                default: 
                    throw new Exception("Ошибка конструкции <сложный оператор>.");
            }
        }

        private void WhileStatement()
        {
            // Анализ конструкции <цикл с предусловием>.
            // <цикл с предусловием>::= while <выражение> do <оператор>

            Accept(SpecialSymbolType.WhileToken);
            Expression();
            Accept(SpecialSymbolType.DoToken);
            Statement();
        }

        private void IfStatement()
        {
            // Анализ конструкции <условный оператор>.
            // Анализ конструкции <условный оператор>::= if <выражение> then <оператор> | if <выражение> then <оператор> else <оператор>
            
            Accept(SpecialSymbolType.IfToken);
            Expression();
            Accept(SpecialSymbolType.ThenToken);
            Statement();

            var specialSymbolToken = _token as SpecialSymbolToken;
            if (specialSymbolToken is not null && specialSymbolToken.Type == SpecialSymbolType.ElseToken)
            {
                Accept(SpecialSymbolType.ElseToken);
                Statement();
            }
        }
    }
}
