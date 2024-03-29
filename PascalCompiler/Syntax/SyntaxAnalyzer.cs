﻿using PascalCompiler.Token;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PascalCompiler.Syntax
{
    partial class SyntaxAnalyzer
    {
        private IOModule _io; // Модуль IO для вывода ошибок.
        private Lexer _lexer; // Лексер для получения токенов.
        private LexicalToken _token; // Текущий токен.
        private ScopeManager _scopeManager; // Менеджер для работы с областями видимости.
        private CodeGenerator _cg;
        public SyntaxAnalyzer(IOModule io, Lexer lexer)
        {
            _io = io;
            _cg = new CodeGenerator(_io.CompileResPath);

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

        private void PrintError()
        {
            // Выводит ошибку перед исключением.
            Console.WriteLine($"Line {_io.CurrentLineNum}:");
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(_io.CurrentLine);
            Console.ForegroundColor = ConsoleColor.Gray;
        }

        private void Accept(SpecialSymbolType symbolType)
        {
            var specialSymbolToken = _token as SpecialSymbolToken;
            if (specialSymbolToken == null || specialSymbolToken.Type != symbolType)
            {
                PrintError();
                throw new Exception($"Ожидался {symbolType}.");
            }

            NextToken();
        }

        private void Accept<T>() where T: class
        {
            if (_token is not T)
            {
                PrintError();
                throw new Exception("Ожидалась другая лексема");
            }
            NextToken();
        }

        // Приводит левый тип к правому.
        // Если типы одинаковые, то возвращает тот же тип.
        private CType Cast(CType left, CType right)
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
                            PrintError();
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
                    PrintError();
                    throw new Exception($"Тип {left.pasType} не поддерживается.");
            }
        }

        // Проверяет приводимость типов и приводит, если возможно.
        private CType TryCast(CType left, CType right)
        {
            if (left.IsCastedTo(right))
            {
                return Cast(left, right);
            }
            else
            {
                PrintError();
                throw new Exception("Типы не приводимы друг к другу.");
            }
        }

        // Проверяет является ли типом переданном в аргументе,
        // иначе выбрасывает исключение.
        private void AcceptType(CType cType, PascalType type)
        {
            if (cType.pasType != type)
            {
                PrintError();
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
            PrintError();
            throw new Exception($"Ожидался один из типов: " + string.Join(", ", types));
        }

        public void Start()
        {
            NextToken(); // Получение первого токена.
            Program(); // Запуск анализа программы.

            if (!_io.HasErrors)
            {
                _cg.SaveAssembly();
            }
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
                var localBuilder = _cg.DeclareVar(cType);
                _scopeManager.AddPrgmIdent(vars.Dequeue(), new IdentifierInfo(IdentifierPurpose.Variable, cType, localBuilder));
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
                return _scopeManager.GetIdentInfo(identifierName).Type; // Выбросит исключение, если тип не сущ.
            }
            else
            {
                PrintError();
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

            if (_token is IdentifierToken identifier)
            {
                string identName = identifier.Name;
                NextToken();
                var specialSymbol = _token as SpecialSymbolToken;
                switch (specialSymbol.Type)
                {
                    case SpecialSymbolType.AssignmentToken:
                        AssignmentStatement(identifier); // <оператор присваивания>
                        break;
                    case SpecialSymbolType.LeftRoundBracketToken:
                        ProcedureStatement(identifier);
                        break;
                    default:
                        PrintError();
                        throw new Exception("Ожидался оператор присваивания или оператор процедуры");
                }
            }
            else if (_token is SpecialSymbolToken)
            {
                throw new Exception("Оператор goto не поддерживается");
            }
        }

        private void ProcedureStatement(IdentifierToken identifier)
        {
            // Никакие функции, кроме writeln(Type printArg) не поддерживаются!!!


            // Анализ конструкции <обозначение функции>
            // <обозначение функции>::= <имя функции>|<имя функции>(<фактический параметр>{,<фактический параметр>})

            string procName = identifier.Name;
            //var procInfo = _scopeManager.GetIdentInfo(procName); // Исключение, если процедуры нет.

            Accept(SpecialSymbolType.LeftRoundBracketToken);
            SpecialSymbolToken special;

            // 0 параметров
            special = _token as SpecialSymbolToken;
            if (special?.Type == SpecialSymbolType.RightRoundBracketToken)
            {
                Accept(SpecialSymbolType.RightRoundBracketToken);
                // TODO Вызвать процедуру.
                return;
            }

            // > 1 параметра
            special = _token as SpecialSymbolToken;
            CType cType = Expression();
            while (special?.Type == SpecialSymbolType.CommaToken)
            {
                Accept(SpecialSymbolType.CommaToken);
                cType = Expression(); // Добавить <фактический параметр>
                special = _token as SpecialSymbolToken;
            }
            Accept(SpecialSymbolType.RightRoundBracketToken);

            // Вызвать процедуру.
            _cg.PrintConst(cType);
        }

        private void AssignmentStatement(IdentifierToken identifier)
        {
            // Анализ конструкции <оператор присваивания>.
            // <оператор присваивания>::= <переменная>:= <выражение> | <имя функции>:=<выражение>
            // <переменная>::= <имя переменной> 

            // 1) Имя переменной
            string varName = identifier.Name;
            
            // 2) Оператор присваивания
            Accept(SpecialSymbolType.AssignmentToken);

            // 3) Выражение
            // Проверяем есть ли переменная в таблице переменных.
            // И проверяем можно ли присвоить переменной заданого типа полученное значение.
            var varInfo = _scopeManager.GetIdentInfo(varName); // Исключение, если переменной с данным именем нет.
            var leftType = varInfo.Type;
            var rightType = Expression();

            // Если слева переменная real, то справа может быть значение типа int, real.
            // Иначе типы должны быть равны.
            // Если условие не выполняется, то выбрасывается исключение.
            if (leftType.pasType == PascalType.Real)
            {
                AcceptType(rightType, PascalType.Real, PascalType.Integer);
            }
            else
                AcceptType(rightType, leftType.pasType);


            _cg.Assign(varInfo.LocalBuilder);
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
                        PrintError();
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
                        PrintError();
                        throw new Exception($"Операция - не допустима для типов: {l}, {r}");
                    }
                case SpecialSymbolType.OrToken:
                    if (l == PascalType.Boolean && r == PascalType.Boolean)
                    {
                        return left;
                    }
                    else
                    {
                        PrintError();
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
                        PrintError();
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
                        PrintError();
                        throw new Exception($"Операции div, mod не допустимы для типов: {l}, {r}");
                    }
                case SpecialSymbolType.AndToken:
                    if (l == PascalType.Boolean && r == PascalType.Boolean)
                    {
                        return left;
                    }
                    else
                    {
                        PrintError();
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
                        PrintError();
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
                        PrintError();
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
                        _cg.AddOperation(specialSymbolToken.Type);
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
                        break;
                }
            }
            CType left = Term(); // <слагаемое>

            if (sign != null && sign == SpecialSymbolType.MinusToken)
                _cg.NegValue();

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
                        if (left.pasType == PascalType.String)
                            _cg.ConcatString();
                        else
                        {
                            _cg.AddOperation(specialSymbolToken.Type);
                        }
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
                        left = CheckMultOperation(left, right, specialSymbolToken.Type);
                        _cg.AddOperation(specialSymbolToken.Type);
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
                        _cg.AddValue(identifierToken.Name == "true");
                        cType = new BooleanType();
                        break;
                    default:
                        // Смотрим тип переменной в таблице идентификаторов.
                        var identInfo = _scopeManager.GetIdentInfo(identifierToken.Name);
                        _cg.AddValue(identInfo.LocalBuilder);
                        cType = identInfo.Type;
                        break;
                }
                NextToken();
            }

            // <константа без знака>
            else if (_token is ConstToken<int> intToken) // число int без знака
            {
                _cg.AddValue(intToken.Value);
                cType = new IntType();
                NextToken();
            }
            else if (_token is ConstToken<double> doubleToken) // число real без знака
            {
                _cg.AddValue(doubleToken.Value);
                cType = new RealType();
                NextToken();
            }
            else if (_token is ConstToken<string> stringToken) // строка
            {
                _cg.AddValue(stringToken.Value);
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
                        // nil - типа указатель. Не обрабатывается.
                        Accept(SpecialSymbolType.NilToken);
                        throw new Exception("Поддержка nil не реализована.");
                    case SpecialSymbolType.NotToken:
                        Accept(SpecialSymbolType.NotToken);
                        cType = Factor();
                        if (cType.pasType != PascalType.Boolean)
                        {
                            PrintError();
                            throw new Exception("Ожидался тип Boolean.");
                        }
                        break;

                    // Выражение со скобками.
                    case SpecialSymbolType.LeftRoundBracketToken:
                        Accept(SpecialSymbolType.LeftRoundBracketToken);
                        cType = Expression();
                        Accept(SpecialSymbolType.RightRoundBracketToken);
                        break;
                    default:
                        PrintError();
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
                    PrintError();
                    throw new Exception("Ошибка конструкции <сложный оператор>.");
            }
        }

        private void WhileStatement()
        {
            // Анализ конструкции <цикл с предусловием>.
            // <цикл с предусловием>::= while <выражение> do <оператор>
            var endBranch = _cg.DefineLabel();
            var repeatLoopBranch = _cg.DefineLabel();

            Accept(SpecialSymbolType.WhileToken);
            _cg.MarkLabel(repeatLoopBranch); // Проверка условия цикла.
            CType left = Expression();
            AcceptType(left, PascalType.Boolean); // Проверка типа на boolean значение.
            _cg.TransferControlIfFalse(endBranch); // Если выражение в условии цикла false, то выходим из цикла.

            Accept(SpecialSymbolType.DoToken);
            Statement();
            _cg.TransferControl(repeatLoopBranch); // Выполнили операции в цикле, теперь идем проверять условие цикла.

            _cg.MarkLabel(endBranch); // Конец цикла.
        }

        private void IfStatement()
        {
            // Анализ конструкции <условный оператор>.
            // Анализ конструкции <условный оператор>::= if <выражение> then <оператор> | if <выражение> then <оператор> else <оператор>
            
            Accept(SpecialSymbolType.IfToken);

            var falseBranch = _cg.DefineLabel();
            var endBranch = _cg.DefineLabel();
            CType left = Expression(); // Вычисление выражения.
            AcceptType(left, PascalType.Boolean); // Проверка типа на boolean значение.
            Accept(SpecialSymbolType.ThenToken);

            _cg.TransferControlIfFalse(falseBranch); // Если выражение false, то прыгаем в falseBranch. (Если false, то идем в else или пропускаем if)
            Statement();
            _cg.TransferControl(endBranch); // Если выражение true, то прыгаем в endBranch. (Если true, то выполнили Statement() и перескочили else ветку)

            var specialSymbolToken = _token as SpecialSymbolToken;
            if (specialSymbolToken is not null && specialSymbolToken.Type == SpecialSymbolType.ElseToken)
            {
                Accept(SpecialSymbolType.ElseToken);
                _cg.MarkLabel(falseBranch); // Указываем, где начинается falseBranch.
                Statement();
            }
            else
            {
                _cg.MarkLabel(falseBranch);
            }
            _cg.MarkLabel(endBranch); // Указываем, где начинается endBranch.
        }
    }
}
