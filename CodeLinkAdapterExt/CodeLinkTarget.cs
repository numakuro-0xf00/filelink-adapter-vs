namespace CodeLinkAdapterExt
{
    internal sealed class CodeLinkTarget
    {
        public CodeLinkTarget(string filePath, int line, int column)
        {
            FilePath = filePath;
            Line = line;
            Column = column;
        }

        public string FilePath { get; }

        public int Line { get; }

        public int Column { get; }
    }
}
