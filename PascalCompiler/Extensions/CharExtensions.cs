using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PascalCompiler.Extensions
{
    public static class CharExtensions
    {
        public static bool IsAsciiLetter(this char ch)
        {
            return (ch >= 'a' && ch <= 'z') || (ch >= 'A' && ch <= 'Z');
        }

        public static bool IsNumber(this char ch)
        {
            return char.IsNumber(ch) ;
        }
    }
}
