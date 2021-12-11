using System;
using System.Collections.Generic;
using PascalCompiler.Token;

namespace PascalCompiler.Syntax
{
    // Класс области видимости.
    // Хранит таблицу типов и имен данной области.
    class Scope
    {
        private List<CType> _typeTable; // Хранит информацию о типах.
        private Dictionary<string, IdentifierInfo> _identifierTable; // Хранит информацию о идентификаторах.

        public Scope()
        {
            _typeTable = new List<CType>();
            _identifierTable = new Dictionary<string, IdentifierInfo>();
        }

        // Добавляет новый идентификатор в данную область.
        public void AddIdentifier(string name, IdentifierInfo info)
        {
            if (ContainsIdent(name))
            {
                throw new Exception("Идентификатор с данным именем уже содержится");
            }
            _identifierTable.Add(name, info);
        }

        // Добавляет новый тип в данную область.
        public CType AddType(CType cType)
        {
            _typeTable.Add(cType);
            return cType;
        }

        // Проверяет сущ-т ли идентификатор в данной области.
        public bool ContainsIdent(string name)
        {
            return _identifierTable.ContainsKey(name);
        }

        public IdentifierInfo FindIdentifier(string name)
        {
            IdentifierInfo identInfo = null;
            _identifierTable.TryGetValue(name, out identInfo);
            return identInfo;
        }
    }

    enum IdentifierPurpose
    {
        Const,
        Variable,
        ProgType,
        Procedure,
        Function
    }

    // Хранит информацию о идентификаторе:
    // * назначение (константа, переменные, типы и т.д.)
    // * тип
    class IdentifierInfo
    {
        public IdentifierPurpose Purpose { get; }
        public CType Type { get; }

        public IdentifierInfo(IdentifierPurpose purpose, CType cType)
        {
            Purpose = purpose;
            Type = cType;
        }
    }

    partial class SyntaxAnalyzer
    {
        private Stack<Scope> _scopeTable;

        private Scope CurScope { get => _scopeTable.Peek(); }


        public void ScopeInit()
        {
            _scopeTable = new Stack<Scope>();

            // Добавление фиктивной области действия.
            var scope = new Scope();

            // Заполнение фиктивной области.
            var intType = scope.AddType(new IntType());
            var realType = scope.AddType(new RealType());
            var stringType = scope.AddType(new StringType());
            var booleanType = scope.AddType(new BooleanType());

            scope.AddIdentifier("integer", new IdentifierInfo(IdentifierPurpose.ProgType, intType));
            scope.AddIdentifier("real", new IdentifierInfo(IdentifierPurpose.ProgType, realType));
            scope.AddIdentifier("string", new IdentifierInfo(IdentifierPurpose.ProgType, stringType));
            scope.AddIdentifier("boolean", new IdentifierInfo(IdentifierPurpose.ProgType, booleanType));

            scope.AddIdentifier("true", new IdentifierInfo(IdentifierPurpose.Const, booleanType));
            scope.AddIdentifier("false", new IdentifierInfo(IdentifierPurpose.Const, booleanType));

            _scopeTable.Push(scope);

            // Добавление основной программы.

            var mainScope = new Scope();
            _scopeTable.Push(mainScope);
        }

        // Добавляет идентификатор из считываемой программы.
        // Проверяет сущ. ли данный идент-р: если да, то выбрасывает исключение.
        private void AddPrgmIdent(string name, IdentifierInfo info)
        {
            if (IsIdentifierAvailable(name))
            {
                _scopeTable.Peek().AddIdentifier(name, info);
            }
            throw new Exception("Идентификатор уже присутствует в одной из областей видимости.");
        }

        private bool IsIdentifierAvailable(string identName)
        {
            // Спускается по стеку scope
            // и проверяет доступен ли данный идентификатор.

            foreach (var s in _scopeTable)
            {
                if (s.ContainsIdent(identName))
                    return false;
            }
            return true;
        }

        private CType GetIdentTypeGlobally(string name)
        {
            foreach(var s in _scopeTable)
            {
                IdentifierInfo identifier = s.FindIdentifier(name);
                if (identifier is not null)
                {
                    return identifier.Type;
                }
            }

            throw new Exception($"Идентификатор с именем {name} не существует.");
        }
    }
}
