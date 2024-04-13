namespace FmodParser;

public static class Program
{
    public static void Main(string[] args)
    {
        var inputFile = args[0];

        GuidCache.LoadFile("GUIDs.txt");
        var fmodFile = RiffParser.Parse(inputFile);

        using (var file = File.Create($"roundtrip_{inputFile}"))
        {
            using var writer = new BinaryWriter(file);
            fmodFile.RootChunk.Write(writer);
        }

        using var outFile = File.CreateText($"{inputFile}.out");
        RiffParser.Print(outFile, fmodFile.RootChunk);

        using var outFileJson = File.Create($"{inputFile}.json");
        RiffParser.Print2(outFileJson, fmodFile.RootChunk);
    }
}
