using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace PascalCompiler
{
    class IOModule
    {
        private StreamReader Reader { get; set; }

        private string[] _textLines;
        private int _currentLineNum;

        public IOModule(string filePath)
        {
            Reader = new StreamReader(filePath);
            string text = File.ReadAllText(filePath);
            _textLines = text.Split("\r\n");
            _currentLineNum = 0;
        }

        public string ReadNextLine()
        {
            // Пропускаем пустые строки
            while (_currentLineNum < _textLines.Length && _textLines[_currentLineNum].Length == 0)
            {
                _currentLineNum++;
                continue;
            }

            // Если файл закончился
            if (_currentLineNum == _textLines.Length)
                return null;

            Console.WriteLine("\n" + _textLines[_currentLineNum]);
            return _textLines[_currentLineNum++];

            //string currentStr = Reader.ReadLine();
            //while (currentStr != null && currentStr.Length == 0)
            //    currentStr = Reader.ReadLine();

            //if (currentStr != null)
            //    Console.WriteLine("\n" + currentStr);
            //return currentStr;
        }

        public void CloseIO()
        {
            Reader.Close();
        }
    }
}
