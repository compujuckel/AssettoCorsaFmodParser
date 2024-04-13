using FmodParser.Riff;
using FmodParser.Utils;

namespace FmodParser.FmodTypes;

[DataChunk("EVTB")]
public class EventChunk : RiffChunkBase
{
    public Guid EventId { get; set; }
    public byte[] Unknown1 { get; set; }
    public Guid TimelineId { get; set; }
    public Guid MixerInputId { get; set; }
    public Guid EventMixerMasterId { get; set; }
    public byte[] Unknown2 { get; set; }

    public EventChunk(BinaryReader reader)
    {
        EventId = reader.ReadGuid();
        Unknown1 = reader.ReadBytes(16);
        TimelineId = reader.ReadGuid();
        MixerInputId = reader.ReadGuid();
        EventMixerMasterId = reader.ReadGuid();
        Unknown2 = reader.ReadToEnd();
    }

    protected override void WriteData(BinaryWriter writer)
    {
        writer.WriteStruct(EventId);
        writer.Write(Unknown1);
        writer.WriteStruct(TimelineId);
        writer.WriteStruct(MixerInputId);
        writer.WriteStruct(EventMixerMasterId);
        writer.Write(Unknown2);
    }

    public override void ToTextWriter(TextWriter writer, int indent = 0)
    {
        writer.WriteIndented(indent + 1, "Event");
        writer.WriteIndented(indent + 1, $"Event ID: {EventId.ToKnownString()}");
        writer.WriteIndented(indent + 1, Convert.ToHexString(Unknown1));
        writer.WriteIndented(indent + 1, $"Timeline ID: {TimelineId.ToKnownString()}");
        writer.WriteIndented(indent + 1, $"Mixer Input ID: {MixerInputId.ToKnownString()}");
        writer.WriteIndented(indent + 1, $"Event Mixer Master ID: {EventMixerMasterId.ToKnownString()}");
        writer.WriteIndented(indent + 1, Convert.ToHexString(Unknown2));
    }
}