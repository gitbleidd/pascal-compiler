using PascalCompiler.Token;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PascalCompiler.Syntax
{
    partial class SyntaxAnalyzer
    {
        private IOModule _io;
        private Lexer _lexer;
        private LexicalToken _token;
        private ScopeManager _scopeManager;

        public SyntaxAnalyzer(IOModule io, Lexer lexer)
        {
            _io = io;
            _lexer = lexer;
            _scopeManager = new ScopeManager();
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

        // Приводит левый тип к правому.
        // Если типы одинаковые, то возвращает тот же тип.
        public CType Cast(CType left, CType right)
        {
            switch (left.pasType)
            {
                // int -> int, real
                case PascalType.Integer:
                    switch (right.pasType)
                    {
                        case PascalType.Integer:
                            return left;
                        case PascalType.Real:
                            return right;
                        default:
                            throw new Exception($"Нельзя привести {left.pasType} к {right.pasType}.");
                    }

                // real -> real
                case PascalType.Real:
                    return left;

                // string -> string
                case PascalType.String:
                    return left;

                // bool -> bool
                case PascalType.Boolean:
                    return left;
                default:
                    throw new Exception($"Тип {left.pasType} не поддерживается.");
            }
        }

        // Проверяет приводимость типов и приводит, если возможно.
        public CType TryCast(CType left, CType right)
        {
            if (left.IsCastedTo(right))
            {
                return Cast(left, right);
            }
            else
            {
                throw new Exception("Типы не приводимы друг к другу.");
            }
        }

        // Проверяет является ли типом переданном в аргументе,
        // иначе выбрасывает исключение.
        private void AcceptType(CType cType, PascalType type)
        {
            if (cType.pasType != type)
            {
                throw new Exception($"Ожидался {type}");
            }
        }

        private void AcceptType(CType cType, params PascalType[] types)
        {
            foreach (var t in types)
            {
                if (cType.pasType == t)
                {
                    return;
                }
            }
            throw new Exception($"Ожидался один из типов: " + string.Join(", ", types));
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

            // Сохраняю имена переменных. Можно в очереди.
            var vars = new Queue<string>();
            var identToken = _token as IdentifierToken;
            Accept<IdentifierToken>();
            vars.Enqueue(identToken.Name);

            var specialSymbolToken = _token as SpecialSymbolToken;
            while (specialSymbolToken?.Type == SpecialSymbolType.CommaToken)
            {
                Accept(SpecialSymbolType.CommaToken);

                identToken = _token as IdentifierToken;
                Accept<IdentifierToken>();
                vars.Enqueue(identToken.Name);

                specialSymbolToken = _token as SpecialSymbolToken;
            }
            Accept(SpecialSymbolType.ColonToken);

            // Запоминаем тип.
            CType cType = Type();

            // Добавляю переменные в текущий Scope.
            while(vars.Count > 0)
            {
                // Если переменная с данным именем уже существует, то выкидывается исключение.
                _scopeManager.AddPrgmIdent(vars.Dequeue(), new IdentifierInfo(IdentifierPurpose.Variable, cType));
            }
        }

        private CType Type()
        {
            // Анализ конструкции <тип>.
            // <простой тип>::= <перечислимый тип> | <ограниченный тип> | <имя типа>
            // <перечислимый тип>::= (< имя >{,< имя >})
            // <ограниченный тип>::=<константа> .. <константа>

            if (_token is IdentifierToken)
            {
                var identifierName = (_token as IdentifierToken).Name;
                NextToken();
                return _scopeManager.GetIdentTypeGlobally(identifierName); // Выбросит исключение, если тип не сущ.
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
            
            // 2) Оператор присваивания
            Accept(SpecialSymbolType.AssignmentToken);

            // 3) Выражение
            // Проверяем есть ли переменная в таблице переменных.
            // И проверяем можно ли присвоить переменной заданого типа полученное значение.
            var leftType = Expression();
            var rightType = _scopeManager.GetIdentTypeGlobally(variable.Name); // Исключение, если переменной нет.
            
            if (leftType.pasType == PascalType.Real)
            {
                AcceptType(leftType, PascalType.Real, PascalType.Integer); // Исключение, если тип не совместимы.
            }
            else
                AcceptType(leftType, rightType.pasType); // Исключение, если типы разные.
        }

        private CType CheckAddOperation(CType left, CType right, SpecialSymbolType operation)
        {
            var l = left.pasType; // Левый тип.
            var r = right.pasType; // Правый тип.
            switch (operation)
            {
                case SpecialSymbolType.PlusToken:
                    if ((l == PascalType.Integer || l == PascalType.Real) && (r == PascalType.Integer || r == PascalType.Real))
                    {
                        // В результате может получиться integer или real. Поэтому пытаемся вернуть real.
                        return l == PascalType.Real ? left : right;
                    }
                    else if (l == PascalType.String && r == PascalType.String)
                    {
                        return left;
                    }
                    else
                    {
                        throw new Exception($"Операция + не допустима для типов: {l}, {r}");
                    }
                case SpecialSymbolType.MinusToken:
                    if ((l == PascalType.Integer || l == PascalType.Real) && (r == PascalType.Integer || r == PascalType.Real))
                    {
                        // В результате может получиться integer или real. Поэтому пытаемся вернуть real.
                        return l == PascalType.Real ? left : right;
                    }
                    else
                    {
                        throw new Exception($"Операция - не допустима для типов: {l}, {r}");
                    }
                case SpecialSymbolType.OrToken:
                    if (l == PascalType.Boolean && r == PascalType.Boolean)
                    {
                        return left;
                    }
                    else
                    {
                        throw new Exception($"Операция Or не допустима для типов: {l}, {r}");
                    }
                default:
                    throw new Exception($"{operation} не является аддитивной операцией");
            }
        }

        private CType CheckMultOperation(CType left, CType right, SpecialSymbolType operation)
        {
            var l = left.pasType; // Левый тип.
            var r = right.pasType; // Правый тип.
            switch (operation)
            {
                case SpecialSymbolType.MultToken:
                case SpecialSymbolType.DivisionToken:
                    if ((l == PascalType.Integer || l == PascalType.Real) && (r == PascalType.Integer || r == PascalType.Real))
                    {
                        // В результате может получиться integer или real. Поэтому пытаемся вернуть real.
                        return l == PascalType.Real ? left : right;
                    }
                    else
                    {
                        throw new Exception($"Операции *, / не допустимы для типов: {l}, {r}");
                    }
                case SpecialSymbolType.DivToken:
                case SpecialSymbolType.ModToken:
                    if (l == PascalType.Integer && r == PascalType.Integer)
                    {
                        return left;
                    }
                    else
                    {
                        throw new Exception($"Операции div, mod не допустимы для типов: {l}, {r}");
                    }
                case SpecialSymbolType.AndToken:
                    if (l == PascalType.Boolean && r == PascalType.Boolean)
                    {
                        return left;
                    }
                    else
                    {
                        throw new Exception($"Операция And не допустима для типов: {l}, {r}");
                    }
                default:
                    throw new Exception($"{operation} не является мультипликативной операцией");
            }
        }

        private BooleanType CheckRelationOperation(CType left, CType right, SpecialSymbolType operation)
        {
            var l = left.pasType; // Левый тип.
            var r = right.pasType; // Правый тип.
            switch (operation)
            {
                case SpecialSymbolType.NotEqualToken:
                case SpecialSymbolType.EqualToken:
                    bool canBeEqual =
                        ((l == PascalType.Integer || l == PascalType.Real) && (r == PascalType.Integer || r == PascalType.Real)) ||
                        (l == PascalType.Boolean && r == PascalType.Boolean) ||
                        (l == PascalType.String && r == PascalType.String);
                    if (!canBeEqual)
                    {
                        throw new Exception($"Операции =, <> не допустимы для типов: {l}, {r}");
                    }
                    break;
                case SpecialSymbolType.LessToken:
                case SpecialSymbolType.LessOrEqualToken:
                case SpecialSymbolType.GreaterToken:
                case SpecialSymbolType.GreaterOrEqualToken:
                    bool canBeGreaterOrLess =
                        ((l == PascalType.Integer || l == PascalType.Real) && (r == PascalType.Integer || r == PascalType.Real));
                    if (!canBeGreaterOrLess)
                    {
                        throw new Exception($"Операции <, <=, >, >= не допустимы для типов: {l}, {r}");
                    }
                    break;
                default:
                    throw new Exception($"{operation} не является операцией отношения");
            }
            return new BooleanType();
        }

        private CType Expression()
        {
            // Анализ конструкции <выражение>.
            // <выражение>::= <простое выражение> | <простое выражение> <операция отношения> <простое выражение>
            CType left = SimpleExpression();
            
            var specialSymbolToken = _token as SpecialSymbolToken;
            if (specialSymbolToken is not null)
            {
                // <операция отношения>::= <>|=|<|<=|>=|>|
                switch (specialSymbolToken.Type)
                {
                    case SpecialSymbolType.NotEqualToken:
                    case SpecialSymbolType.EqualToken:
                    case SpecialSymbolType.LessToken:
                    case SpecialSymbolType.LessOrEqualToken:
                    case SpecialSymbolType.GreaterToken:
                    case SpecialSymbolType.GreaterOrEqualToken:
                        NextToken();
                        CType right = SimpleExpression();
                        left = CheckRelationOperation(left, right, specialSymbolToken.Type);
                        break;
                    default:
                        break;
                }
            }
            return left;
        }

        private CType SimpleExpression()
        {
            // <простое выражение>::= [<знак>] <слагаемое> {<аддитивная операция><слагаемое>}
            // <аддитивная операция>::= +|-|or
            // <знак>::=+|-

            // Знак перед слагаемым.
            var specialSymbolToken = _token as SpecialSymbolToken;
            SpecialSymbolType? sign = null;
            if (specialSymbolToken != null)
            {
                switch (specialSymbolToken.Type)
                {
                    case SpecialSymbolType.PlusToken:
                    case SpecialSymbolType.MinusToken:
                        sign = specialSymbolToken.Type;
                        NextToken();
                        break;
                    default:
                        //throw new Exception("По БНФ <простое выражение> требуется знак!!!!");
                        break;
                }
            }
            CType left = Term(); // <слагаемое>

            // Знак может быть только перед integer, real.
            if (sign != null)
            {
                AcceptType(left, PascalType.Integer, PascalType.Real);
            }


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
                        CType right = Term();
                        left = CheckAddOperation(left, right, specialSymbolToken.Type); // Проверяем возможно ли выполнить операцию.
                        break;
                    default:
                        isAdditiveOper = false;
                        break;
                }
                specialSymbolToken = _token as SpecialSymbolToken;
            }
            return left;
        }

        private CType Term()
        {
            // Анализ конструкции <слагаемое>.
            // <слагаемое>::= <множитель>{<мультипликативная операция><множитель>}
            // <мультипликативная операция>::=*|/|div|mod|and
            CType left = Factor(); // <множитель>

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
                    case SpecialSymbolType.AndToken:
                        NextToken();
                        CType right = Factor();
                        left = TryCast(left, right);
                        left = CheckMultOperation(left, right, specialSymbolToken.Type);
                        break;
                    default:
                        isMultOper = false;
                        break;
                }
                specialSymbolToken = _token as SpecialSymbolToken;
            }
            return left;
        }

        private CType Factor()
        {
            // Анализ конструкции <множитель>.
            // <множитель>::= <переменная> | <обозначение функции> | <константа без знака> | (<выражение>) | not <множитель>
            // <константа без знака>::= <число без знака> | <строка> | <имя константы> | nil

            CType cType = null;
            // <переменная> | <обозначение функции> |
            // <имя константы> 
            // true, false - тоже идентификаторы.
            if (_token is IdentifierToken identifierToken)
            {
                switch (identifierToken.Name)
                {
                    // TODO true, false добавить в таблице идентификаторов при инициализации.
                    // Cмотреть таблица, тк true, false можно переопределить.
                    case "true":
                    case "false":
                        cType = new BooleanType();
                        break;
                    default:
                        // TODO Пока что все переменные REAL.
                        // Смотрим тип переменной в таблице идентификаторов.
                        cType = _scopeManager.GetIdentTypeGlobally(identifierToken.Name);
                        break;
                }
                NextToken();
            }

            // <константа без знака>
            else if (_token is ConstToken<int>) // число int без знака
            {
                cType = new IntType();
                NextToken();
            }
            else if (_token is ConstToken<double>) // число real без знака
            {
                cType = new RealType();
                NextToken();
            }
            else if (_token is ConstToken<string>) // строка
            {
                cType = new StringType();
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
                        // nil - типа указатель. Не обрабатывается.
                        throw new Exception("Поддержка nil не реализована.");
                    case SpecialSymbolType.NotToken:
                        Accept(SpecialSymbolType.NotToken);
                        cType = Factor();
                        if (cType.pasType != PascalType.Boolean)
                        {
                            throw new Exception("Ожидался тип Boolean.");
                        }
                        break;
                    case SpecialSymbolType.LeftRoundBracketToken:
                        Accept(SpecialSymbolType.LeftRoundBracketToken);
                        cType = Expression();
                        Accept(SpecialSymbolType.RightRoundBracketToken);
                        break;
                    default:
                        throw new Exception("Не удалось определить тип множителя.");
                }
            }
            return cType;
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
            CType left = Expression();
            AcceptType(left, PascalType.Boolean); // Проверка типа на boolean значение.

            Accept(SpecialSymbolType.DoToken);
            Statement();
        }

        private void IfStatement()
        {
            // Анализ конструкции <условный оператор>.
            // Анализ конструкции <условный оператор>::= if <выражение> then <оператор> | if <выражение> then <оператор> else <оператор>
            
            Accept(SpecialSymbolType.IfToken);
            CType left = Expression();
            AcceptType(left, PascalType.Boolean); // Проверка типа на boolean значение.

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
