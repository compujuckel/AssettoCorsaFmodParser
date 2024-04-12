namespace FmodParser;

public static class Program
{
    public static void Main(string[] args)
    {
        var inputFile = args[0];

        var chunk = RiffParser.Parse(inputFile);

        using var outFile = File.CreateText($"{inputFile}.out");
        RiffParser.Print(outFile, chunk);
    }
}
