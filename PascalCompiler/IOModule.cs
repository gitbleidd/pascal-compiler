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
        private StreamReader _sr;
        private bool _isStreamReaderClosed = false;
        private string _line = "";
        private int _lineNum = 0;
        private int _position = -1; // Позиция в текущей строке.

        // Словарь с ошибками: key - номер строки с ошибкой. Value - кортеж с самой строкой и списоком ошибок в ней.
        private Dictionary<int, (string errorLine, List<CompilerError> lineErrors)> _errors; 

        public char LookaheadChar
        {
            get
            {
                return _line[_position + 1];
            }
        }

        public char CurrentChar
        {
            get => _line[_position];
        }

        public int Position { get => _position; }

        public char NextChar()
        {
            if (_isStreamReaderClosed)
                return _line[_position];

            _position++;
            if (_position >= _line.Length || _position == -1)
            {
                _line = _sr.ReadLine();
                _lineNum++;

                if (_line == null)
                {
                    _line = "\0";
                    _sr.Close();
                    _isStreamReaderClosed = true;
                }
                    
                else
                    _line += "\n\r";

                _position = 0;
            }

            return _line[_position];
        }

        public string TextSubstr(int start, int length) => _line.Substring(start, length);

        public IOModule(string filePath)
        {
            _sr = new StreamReader(filePath);
            _errors = new Dictionary<int, (string, List<CompilerError>)>();
            NextChar(); // Считываем первый символ
        }

        public void AddError(int position, CompilerError error)
        {
            if (_errors.TryGetValue(_lineNum, out (string, List<CompilerError>) currentLineErros))
            {
                currentLineErros.Item2.Add(error);
            }
            else
            {
                _errors.Add(_lineNum, (_line, new List<CompilerError> { error }));
            }
        }

        /// <summary>
        /// Вывод всех ошибок, содержащихся в буфере, в консоль.
        /// </summary>
        public void PrintErrors()
        {
            int errorCount = 0;
            foreach (var line in _errors)
            {
                errorCount += line.Value.lineErrors.Count;

                Console.ForegroundColor = ConsoleColor.Red;
                string l = line.Value.errorLine;
                Console.WriteLine(l.Substring(0, l.Length - 2));
                Console.ForegroundColor = ConsoleColor.Gray;

                foreach (var error in line.Value.lineErrors)
                {
                    Console.WriteLine($"(Line {line.Key + 1}): error[{(int)error}]: {_errorMessage[error]}");
                }
                Console.WriteLine();
            }
            Console.WriteLine($"Кол-во ошибок: {errorCount}");
        }

        private Dictionary<CompilerError, string> _errorMessage = new Dictionary<CompilerError, string>
        {
            {CompilerError.OverflowException, "выражение выходит за допустимые пределы" },
            {CompilerError.StringExceedsLine, "строка не закрыта" },
            {CompilerError.CommentWithoutEnd, "комментарий не закрыт" },
            {CompilerError.ConstError, "ошибка в константе" },
            {CompilerError.LexicalError, "не удалось считать символ" }
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
