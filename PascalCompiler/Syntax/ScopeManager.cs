using System;
using System.Collections.Generic;
using PascalCompiler.Token;

namespace PascalCompiler.Syntax
{
    class ScopeManager
    {
        private Stack<Scope> _scopeTable;
        private Scope CurScope { get => _scopeTable.Peek(); }

        public ScopeManager()
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

            // Добавление scope основной программы.

            var mainScope = new Scope();
            _scopeTable.Push(mainScope);
        }

        // Добавляет идентификатор из считываемой программы.
        // Проверяет сущ. ли данный идент-р: если да, то выбрасывает исключение.
        public void AddPrgmIdent(string name, IdentifierInfo info)
        {
            if (IsIdentifierAvailable(name))
            {
                _scopeTable.Peek().AddIdentifier(name, info);
            }
            else
            {
                throw new Exception("Идентификатор уже присутствует в одной из областей видимости.");
            }
        }


        // Пробегается по всем областям видимости в стеке
        // и проверяет доступен ли данный идентификатор.
        public bool IsIdentifierAvailable(string identName)
        {
            foreach (var s in _scopeTable)
            {
                if (s.ContainsIdent(identName))
                    return false;
            }
            return true;
        }

        public CType GetIdentTypeGlobally(string name)
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
