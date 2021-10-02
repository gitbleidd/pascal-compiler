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
            return Reader.ReadLine();
        }

        public void CloseIO()
        {
            Reader.Close();
        }
    }
}
