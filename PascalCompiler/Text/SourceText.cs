using System.Collections.Generic;
using System.Linq;

namespace PascalCompiler.Text
{
    public class SourceText
    {
        private readonly string _text;
        public char this[int index] => _text[index];
        public List<TextLine> Lines { get; }
        public int Length => _text.Length;

        public SourceText(string text)
        {
            _text = text;
            Lines = new List<TextLine>();
            ParseLines(text);
        }

        private void ParseLines(string text)
        {
            int position = 0;
            int lineStart = 0;

            while (position < text.Length)
            {
                int lineBreakLength = GetLineBreakLength(text, position);

                if (lineBreakLength == 0)
                {
                    position++;
                }
                else
                {
                    AddLine(position, lineStart, lineBreakLength);
                    var test1 = TextSubstr(Lines.Last());

                    position += lineBreakLength;
                    lineStart = position;
                }
            }

            if (position > lineStart)
            {
                AddLine(position, lineStart, 0);

                var test2 = TextSubstr(Lines.Last());
            }
        }

        private void AddLine(int position, int lineStart, int lineBreakWidth)
        {
            var lineLength = position - lineStart;
            var lineLengthIncludingLineBreak = lineLength + lineBreakWidth;
            var line = new TextLine(lineStart, lineLength, lineLengthIncludingLineBreak);
            Lines.Add(line);
        }

        private static int GetLineBreakLength(string text, int position)
        {
            char c = text[position];
            char l = position + 1 >= text.Length ? '\0' : text[position + 1];

            if (c == '\r' && l == '\n')
                return 2;

            if (c == '\r' || c == '\n')
                return 1;

            return 0;
        }

        public int GetLineIndex(int position)
        {
            //TODO подправить подсчет строки по позиции.
            var left = 0;
            var right = Lines.Count - 1;

            while (left <= right)
            {
                var index = (right + left) / 2;
                var start = Lines[index].Start;

                if (position == start)
                    return index;

                if (start > position)
                {
                    right = index - 1;
                }
                else
                {
                    left = index + 1;
                }
            }

            return left - 1;
        }

        public string TextSubstr(TextLine textLine) => _text.Substring(textLine.Start, textLine.Length);
        public string TextSubstr(int start, int length) => _text.Substring(start, length);
    }
}
