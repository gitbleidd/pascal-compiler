namespace PascalCompiler.Text
{
    public class TextLine
    {
        public TextLine(int start, int length, int lengthWithLineBreak)
        {
            Start = start;
            Length = length;
            LengthWithLineBreak = lengthWithLineBreak;
        }

        public int Start { get; }
        public int Length { get; }
        public int End => Start + Length;
        public int LengthWithLineBreak { get; }
    }
}
