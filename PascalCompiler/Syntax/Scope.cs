using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
}
