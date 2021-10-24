using System;
using System.Linq;
using System.Text;
using System.IO;
using PascalCompiler.Text;
using System.Collections.Generic;

namespace PascalCompiler
{

    class IOModule
    {
        public readonly SourceText sourceText;
        private Dictionary<int, List<CompilerError>> _errors;

        public IOModule(string filePath)
        {
            string text = File.ReadAllText(filePath);
            sourceText = new SourceText(text);
            
            _errors = new Dictionary<int, List<CompilerError>>();
            
        }

        public void AddError(int position, CompilerError error)
        {
            int lineNum = sourceText.GetLineIndex(position);
            if (_errors.TryGetValue(lineNum, out List<CompilerError> currentLineErros))
            {
                currentLineErros.Add(error);
            }
            else
            {
                _errors.Add(lineNum, new List<CompilerError> { error });
            }
        }

        public void PrintErrors(bool skipCleanLines)
        {
            for (int lineNum = 0; lineNum < sourceText.Lines.Count; lineNum++)
            {
                string line = sourceText.TextSubstr(sourceText.Lines[lineNum]);
                bool hasError = _errors.ContainsKey(lineNum);
                if (skipCleanLines && !hasError)
                    continue;
                
                if (hasError)
                    Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(line);
                Console.ForegroundColor = ConsoleColor.White;

                if (_errors.TryGetValue(lineNum, out List<CompilerError> currentLineErros))
                {
                    foreach (var error in currentLineErros)
                    {
                        Console.WriteLine($"(Line {lineNum + 1}): error[{(int)error}]: {_errorMessage[error]}");
                    }
                }

            }
        }

        private Dictionary<CompilerError, string> _errorMessage = new Dictionary<CompilerError, string>
        {
            {CompilerError.OverflowException, "выражение выходит за допустимые пределы" },
            {CompilerError.StringExceedsLine, "строка не закрыта" },
            {CompilerError.CommentWithoutEnd, "комментарий не закрыт" },
            {CompilerError.ConstError, "ошибка в константе" },
            {CompilerError.LexicalError, "lexical error" }
        };
    }

    public enum CompilerError
    {
        ConstError = 50,
        CommentWithoutEnd = 86,
        OverflowException = 308,
        StringExceedsLine,
        LexicalError
    }

    
}
