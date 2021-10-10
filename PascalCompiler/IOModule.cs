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
        public IOModule(string filePath)
        {
            Reader = new StreamReader(filePath);
        }

        public string ReadNextLine()
        {
            string currentStr = Reader.ReadLine();
            while (currentStr != null && currentStr.Length == 0)
                currentStr = Reader.ReadLine();

            if (currentStr != null)
                Console.WriteLine("\n" + currentStr);
            return currentStr;
        }

        public void CloseIO()
        {
            Reader.Close();
        }
    }
}
