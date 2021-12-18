using PascalCompiler.Token;
using System;
using System.IO;
using System.Reflection;
using System.Reflection.Emit;

namespace PascalCompiler.Syntax
{
    class CodeGenerator
    {
        private AssemblyBuilder _assemblyBuilder;
        private TypeBuilder _typeBuilder;
        private ILGenerator _il;
        private MethodBuilder _methodBuilder;
        private string _assemblyPath;

        public CodeGenerator(string assemblyPath)
        {
            _assemblyPath = Path.Combine(assemblyPath, "app.dll");

            // Создание динамической сборки.
            string assemblyName = "PascalProgram";
            string typeName = "Program";
            string methodName = "Main";

            AssemblyName aName = new AssemblyName(assemblyName);

            //AppDomain domain = System.Threading.Thread.GetDomain();
            _assemblyBuilder = AssemblyBuilder.DefineDynamicAssembly(aName, AssemblyBuilderAccess.Run);

            ModuleBuilder module = _assemblyBuilder.DefineDynamicModule(aName.Name + ".dll");

            _typeBuilder = module.DefineType(typeName, TypeAttributes.Public | TypeAttributes.Class);
            _methodBuilder = _typeBuilder.DefineMethod(methodName, MethodAttributes.Static | MethodAttributes.Public,
              typeof(void), new Type[] { });


            ILGenerator ilGenerator = _methodBuilder.GetILGenerator();
            this._il = ilGenerator;
        }

        // Сохраняет сборку.
        public void SaveAssembly()
        {
            _il.Emit(OpCodes.Ret);
            Type t = _typeBuilder.CreateType(); // Создаем тип, чтобы сохранить в dll.
            //MethodInfo mi = t.GetMethod("Main");
            //object o1 = Activator.CreateInstance(t);
            //mi.Invoke(o1, new object[] { });

            var generator = new Lokad.ILPack.AssemblyGenerator();
            generator.GenerateAssembly(_assemblyBuilder, _assemblyPath);

            RunAssembly(_assemblyPath);
        }

        // Запуск сборки.
        public void RunAssembly(string asmPath)
        {
            Console.WriteLine("[Debug] Program starts from dll");
            Console.WriteLine("[Debug] Program output:\n");
            var assembly = Assembly.LoadFile(asmPath);
            var type = assembly.GetType("Program");
            var obj = Activator.CreateInstance(type);
            var method = type.GetMethod("Main");
            method.Invoke(obj, new object[] { });

            Console.WriteLine("\n[Debug] Program finished");
        }

        // IL команда инициализации переменной.
        public LocalBuilder DeclareVar(CType cType)
        {
            LocalBuilder v; // Хранит информацию о переменной.

            // Загружаем переменную в память со значением по умолчанию.
            switch (cType.pasType)
            {
                case PascalType.Integer:
                    v = _il.DeclareLocal(typeof(int));
                    _il.Emit(OpCodes.Ldc_I4_0);
                    _il.Emit(OpCodes.Stloc, v);
                    break;
                case PascalType.Real:
                    v = _il.DeclareLocal(typeof(double));
                    _il.Emit(OpCodes.Ldc_R8, 0.0);
                    _il.Emit(OpCodes.Stloc, v);
                    break;
                case PascalType.String:
                    v = _il.DeclareLocal(typeof(string));
                    _il.Emit(OpCodes.Ldstr, "");
                    _il.Emit(OpCodes.Stloc, v);
                    break;
                case PascalType.Boolean:
                    v = _il.DeclareLocal(typeof(bool));
                    _il.Emit(OpCodes.Ldc_I4_0);
                    _il.Emit(OpCodes.Stloc, v);
                    break;
                default:
                    throw new Exception("Тип не поддерживается");
            }

            return v;
        }

        // IL команда присвоения значения, лежащего на вершине стека,
        // указанной переменной.
        public void Assign(LocalBuilder lb)
        {
            _il.Emit(OpCodes.Stloc, lb);
        }

        // IL команда добавления константы/переменной на стек.
        public void AddValue(int value) => _il.Emit(OpCodes.Ldc_I4, value);
        public void AddValue(bool value) => _il.Emit(OpCodes.Ldc_I4, value ? 1 : 0);
        public void AddValue(double value) => _il.Emit(OpCodes.Ldc_R8, value);
        public void AddValue(string value) => _il.Emit(OpCodes.Ldstr, value);
        public void AddValue(LocalBuilder lb) => _il.Emit(OpCodes.Ldloc, lb);

        public void NegValue() => _il.Emit(OpCodes.Neg);

        // IL команды операций.
        public void AddOperation(SpecialSymbolType oper)
        {
            switch (oper)
            {
                case SpecialSymbolType.PlusToken:
                    _il.Emit(OpCodes.Add);
                    break;
                case SpecialSymbolType.MinusToken:
                    _il.Emit(OpCodes.Sub);
                    break;
                case SpecialSymbolType.MultToken:
                    _il.Emit(OpCodes.Mul);
                    break;
                case SpecialSymbolType.DivisionToken:
                case SpecialSymbolType.DivToken:
                    _il.Emit(OpCodes.Div);
                    break;
                case SpecialSymbolType.GreaterToken:
                    _il.Emit(OpCodes.Cgt);
                    break;
                case SpecialSymbolType.LessToken:
                    _il.Emit(OpCodes.Clt);
                    break;
                case SpecialSymbolType.GreaterOrEqualToken:
                    _il.Emit(OpCodes.Clt);
                    _il.Emit(OpCodes.Ldc_I4_0);
                    _il.Emit(OpCodes.Ceq);
                    break;
                case SpecialSymbolType.LessOrEqualToken:
                    _il.Emit(OpCodes.Cgt);
                    _il.Emit(OpCodes.Ldc_I4_0);
                    _il.Emit(OpCodes.Ceq);
                    break;
                case SpecialSymbolType.EqualToken:
                    _il.Emit(OpCodes.Ceq);
                    break;
                case SpecialSymbolType.NotEqualToken:
                    _il.Emit(OpCodes.Ceq);
                    _il.Emit(OpCodes.Ldc_I4_0);
                    _il.Emit(OpCodes.Ceq);
                    break;
                case SpecialSymbolType.NotToken:
                    _il.Emit(OpCodes.Not);
                    break;
                case SpecialSymbolType.AndToken:
                    _il.Emit(OpCodes.And);
                    break;
                case SpecialSymbolType.OrToken:
                    _il.Emit(OpCodes.Or);
                    break;
                case SpecialSymbolType.ModToken:
                    _il.Emit(OpCodes.Rem);
                    break;
                default:
                    throw new Exception("Операция не поддерживается");
            }
        }

        // IL команда конкатенации строк.
        public void ConcatString()
        {
            var concat = typeof(string).GetMethod("Concat", new Type[] { typeof(string), typeof(string) });
            _il.Emit(OpCodes.Call, concat);
        }

        // IL команда вывода константы в консоль.
        public void PrintConst(LocalBuilder lb)
        {
            _il.EmitWriteLine(lb);
        }

        public void PrintConst(CType cType)
        {
            MethodInfo fnWriteLine;

            if (cType.pasType == PascalType.Integer)
            {
                fnWriteLine = typeof(Console).GetMethod("WriteLine", new Type[] { typeof(int) });
            }
            else if (cType.pasType == PascalType.Real)
            {
                fnWriteLine = typeof(Console).GetMethod("WriteLine", new Type[] { typeof(double) });
            }
            else if (cType.pasType == PascalType.Boolean)
            {
                fnWriteLine = typeof(Console).GetMethod("WriteLine", new Type[] { typeof(bool) });
            }
            else if (cType.pasType == PascalType.String)
            {
                fnWriteLine = typeof(Console).GetMethod("WriteLine", new Type[] { typeof(string) });
            }
            else
            {
                throw new Exception("Тип не поддерживается");
            }
            _il.Emit(OpCodes.Call, fnWriteLine);
        }

        public Label DefineLabel() => _il.DefineLabel();
        public void MarkLabel(Label label) => _il.MarkLabel(label);

        // Передает управление заданной инструкции, если значение на стеке равно false, null или ноль.
        public void TransferControlIfFalse(Label label)
        {
            _il.Emit(OpCodes.Brfalse, label);
        }

        // Передает управление заданной инструкции.
        public void TransferControl(Label label)
        {
            _il.Emit(OpCodes.Br, label);
        }
    }
}
