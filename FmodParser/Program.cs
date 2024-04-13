namespace FmodParser;

public static class Program
{
    public static void Main(string[] args)
    {
        var inputFile = args[0];

        GuidCache.LoadFile("GUIDs.txt");
        var chunk = RiffParser.Parse(inputFile);

        using var outFile = File.CreateText($"{inputFile}.out");
        RiffParser.Print(outFile, chunk);

        using var outFileJson = File.Create($"{inputFile}.json");
        RiffParser.Print2(outFileJson, chunk);
    }
}
