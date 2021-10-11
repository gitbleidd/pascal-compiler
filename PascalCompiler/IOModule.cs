using System;
using System.Linq;
using System.Text;
using System.IO;
using PascalCompiler.Text;

namespace PascalCompiler
{

    class IOModule
    {
        public readonly SourceText sourceText;
        private int _currentLineNum;

        public IOModule(string filePath)
        {
            string text = File.ReadAllText(filePath);
            sourceText = new SourceText(text);
            _currentLineNum = 0;
        }

        public string GetNextLexeme()
        {
            foreach (var textLine in sourceText.Lines)
            {
                string line = sourceText.TextSubstr(textLine);
                Console.WriteLine(line);
                //line.Split(' ', '\t')
                //TODO разбивка строки на лексемы
                //' ', '\t', итд
                // Обработка комментариев
            }
            return null;
        }
    }
}
