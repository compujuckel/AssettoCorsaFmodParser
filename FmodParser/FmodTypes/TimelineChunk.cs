using FmodParser.Riff;
using FmodParser.Utils;

namespace FmodParser.FmodTypes;

[DataChunk("TLNB")]
public class TimelineChunk : RiffChunkBase
{
    public Guid TimelineId { get; set; }
    public Guid EventId { get; set; }
    public byte[] Unknown1 { get; set; }

    public TimelineChunk(BinaryReader reader)
    {
        TimelineId = reader.ReadGuid();
        EventId = reader.ReadGuid();
        Unknown1 = reader.ReadToEnd();
    }

    protected override void WriteData(BinaryWriter writer)
    {
        writer.WriteStruct(TimelineId);
        writer.WriteStruct(EventId);
        writer.Write(Unknown1);
    }

    public override void ToTextWriter(TextWriter writer, int indent = 0)
    {
        writer.WriteIndented(indent, "Timeline");
        writer.WriteIndented(indent, $"Timeline ID: {TimelineId.ToKnownString()}");
        writer.WriteIndented(indent, $"Event ID: {EventId.ToKnownString()}");
        writer.WriteIndented(indent, Convert.ToHexString(Unknown1));
    }
}
