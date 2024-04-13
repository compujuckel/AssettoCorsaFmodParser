using CommunityToolkit.HighPerformance;
using FmodParser.Riff;
using FmodParser.Utils;

namespace FmodParser.FmodTypes;

[DataChunk("TLNB")]
public class TimelineChunk : DataChunk
{
    public Guid TimelineId { get; set; }
    public Guid EventId { get; set; }
    public Memory<byte> Unknown1 { get; set; }
    public List<TimelineModule> Modules { get; set; } = [];
    public Memory<byte> Unknown2 { get; set; }

    public TimelineChunk(Memory<byte> data)
    {
        var stream = data.AsStream();
        var reader = new BinaryReader(stream);

        TimelineId = reader.ReadGuid();
        EventId = reader.ReadGuid();
        Unknown1 = reader.ReadBytes(6);
        while (stream.Position + 24 < stream.Length)
        {
            Modules.Add(new TimelineModule
            {
                ModuleId = reader.ReadGuid(),
                Start = reader.ReadInt32(),
                Length = reader.ReadInt32()
            });
        }
        Unknown2 = reader.ReadBytes(6);
    }

    public override void ToWriter(TextWriter writer, int indent = 0)
    {
        writer.WriteIndented(indent, "Timeline");
        writer.WriteIndented(indent, $"Timeline ID: {TimelineId.ToKnownString()}");
        writer.WriteIndented(indent, $"Event ID: {EventId.ToKnownString()}");
        writer.WriteIndented(indent, Convert.ToHexString(Unknown1.Span));
        foreach (var module in Modules)
        {
            writer.WriteIndented(indent + 1, $"- Destination ID: {module.ModuleId} Start: {module.Start} Length: {module.Length}");
        }
        writer.WriteIndented(indent, Convert.ToHexString(Unknown2.Span));
    }
}
