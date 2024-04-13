using FmodParser.FmodTypes;

namespace FmodParser;

public static class Program
{
    public static void Main(string[] args)
    {
        var inputFile = args[0];

        GuidCache.LoadFile("GUIDs.txt");
        var fmodFile = RiffParser.Parse(inputFile);

        var soundChunk = (SoundChunk)fmodFile.Root.SubChunks.First(c => c.Identifier.Span.SequenceEqual("SND "u8));
        soundChunk.Bank.Samples.First(s => s.Name == "horn").ReplaceAudio("Vine-boom-sound-effect.wav");

        using (var file = File.Create($"roundtrip_{inputFile}"))
        {
            using var writer = new BinaryWriter(file);
            fmodFile.Root.Write(writer);
        }

        using var outFile = File.CreateText($"{inputFile}.out");
        RiffParser.Print(outFile, fmodFile.Root);
    }
}
