using System;

namespace PascalCompiler
{
    // Базовый класс для описания типов Паскаля.
    // Хранит тип.
    // Определяет приводимость одних типов к другим.
    abstract class CType
    {
        public abstract bool IsCastedTo(CType type);
        public readonly PascalType pasType;

        public CType(PascalType pasType)
        {
            this.pasType = pasType;
        }
    }

    enum PascalType
    {
        Integer,
        Real,
        String,
        Boolean
    }

    class IntType : CType
    {
        public IntType() : base(PascalType.Integer) { }
        public override bool IsCastedTo(CType type)
        {
            switch (type.pasType)
            {
                case PascalType.Integer:
                case PascalType.Real:
                    return true;
                case PascalType.String:
                case PascalType.Boolean:
                    return false;
                default:
                    throw new Exception($"Тип {type.pasType} не поддерживается");
            }
        }
    }

    class RealType : CType
    {
        public RealType() : base(PascalType.Real) { }
        public override bool IsCastedTo(CType type)
        {
            switch (type.pasType)
            {
                case PascalType.Real:
                    return true;
                case PascalType.Integer:
                case PascalType.String:
                case PascalType.Boolean:
                    return false;
                default:
                    throw new Exception($"Тип {type.pasType} не поддерживается");
            }
        }
    }

    class StringType : CType
    {
        public StringType() : base(PascalType.String) { }
        public override bool IsCastedTo(CType type)
        {
            switch (type.pasType)
            {
                case PascalType.String:
                    return true;
                case PascalType.Integer:
                case PascalType.Real:
                case PascalType.Boolean:
                    return false;
                default:
                    throw new Exception($"Тип {type.pasType} не поддерживается");
            }
        }
    }

    class BooleanType : CType
    {
        public BooleanType() : base(PascalType.Boolean) { }
        public override bool IsCastedTo(CType type)
        {
            switch (type.pasType)
            {
                case PascalType.Boolean:
                    return true;
                case PascalType.Integer:
                case PascalType.Real:
                case PascalType.String:
                    return false;
                default:
                    throw new Exception($"Тип {type.pasType} не поддерживается");
            }
        }
    }
}
