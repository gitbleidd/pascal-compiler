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

        public void Accept<T>()
        {
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
            // Оператор присваивания и составной оператор.
        }
    }
}
