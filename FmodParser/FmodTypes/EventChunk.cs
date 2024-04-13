using System.Runtime.InteropServices;
using FmodParser.Riff;
using FmodParser.Utils;

namespace FmodParser.FmodTypes;

[DataChunk("EVTB")]
public class EventChunk : DataChunk
{
    public Guid EventId { get; set; }
    public Memory<byte> Unknown1 { get; set; }
    public Guid TimelineId { get; set; }
    public Guid MixerInputId { get; set; }
    public Guid EventMixerMasterId { get; set; }
    public Memory<byte> Unknown2 { get; set; }

    public EventChunk(Memory<byte> data)
    {
        var span = data.Span;
        EventId = MemoryMarshal.Read<Guid>(span);
        // 16
        Unknown1 = data[16..32];
        // 32
        TimelineId = MemoryMarshal.Read<Guid>(span[32..]);
        // 48
        MixerInputId = MemoryMarshal.Read<Guid>(span[48..]);
        // 64
        EventMixerMasterId = MemoryMarshal.Read<Guid>(span[64..]);
        // 80
        Unknown2 = data[80..];
    }
    
    public override void ToWriter(TextWriter writer, int indent = 0)
    {
        writer.WriteIndented(indent + 1, "Event");
        writer.WriteIndented(indent + 1, $"Event ID: {EventId.ToKnownString()}");
        writer.WriteIndented(indent + 1, Convert.ToHexString(Unknown1.Span));
        writer.WriteIndented(indent + 1, $"Timeline ID: {TimelineId.ToKnownString()}");
        writer.WriteIndented(indent + 1, $"Mixer Input ID: {MixerInputId.ToKnownString()}");
        writer.WriteIndented(indent + 1, $"Event Mixer Master ID: {EventMixerMasterId.ToKnownString()}");
        writer.WriteIndented(indent + 1, Convert.ToHexString(Unknown2.Span));
    }
}