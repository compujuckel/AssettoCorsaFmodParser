namespace FmodParser;

public static class GuidCache
{
    public static readonly Dictionary<Guid, string> KnownGuids = new();

    public static void LoadFile(string path)
    {
        foreach (var line in File.ReadLines(path))
        {
            var guidStart = line.IndexOf('{') + 1;
            var guidEnd = line.IndexOf('}');

            var guid = new Guid(line[guidStart..guidEnd]);
            var name = line[(guidEnd + 2)..];

            KnownGuids.Add(guid, name);
        }
    }
}