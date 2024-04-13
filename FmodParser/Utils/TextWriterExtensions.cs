namespace FmodParser.Utils;

public static class TextWriterExtensions
{
    public static void WriteIndented(this TextWriter writer, int indent, string str)
    {
        for (int i = 0; i < indent; i++)
        {
            writer.Write("    ");
        }

        writer.WriteLine(str);
    }
}