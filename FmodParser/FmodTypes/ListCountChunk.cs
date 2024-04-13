using FmodParser.Riff;
using FmodParser.Utils;

namespace FmodParser.FmodTypes;

[DataChunk("LCNT")]
public class ListCountChunk : RiffChunkBase
{
    public int ListCount { get; set; }

    public ListCountChunk(BinaryReader reader)
    {
        ListCount = reader.ReadInt32();
    }

    protected override void WriteData(BinaryWriter writer)
    {
        writer.Write(ListCount);
    }

    public override void ToTextWriter(TextWriter writer, int indent = 0)
    {
        writer.WriteIndented(indent + 1, $"List count: {ListCount}");
    }
}